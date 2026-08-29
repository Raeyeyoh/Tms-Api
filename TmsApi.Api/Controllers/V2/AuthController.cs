
using TmsApi.Api.Dtos;

using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence.Data;
using TmsApi.Infrastructure.Services;
using TmsApi.Domain.Entities;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace TmsApi.Api.Controllers.V2;




[ApiController]
[Route("api/v{version:apiVersion}/Auth")]
[ApiVersion("2.0")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;
    public AuthController(
    UserManager<TmsUser> userManager,
    RoleManager<IdentityRole> roleManager,
    TmsDbContext context,
    TokenService tokenService)
    {

        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }
    [EnableRateLimiting("AuthLimiter")]

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Dtos.LoginRequest request, [FromServices] IWebHostEnvironment env)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized(new { detail = "Invalid credentials." });

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail = "Account locked due to multiple failed login attempts."
            });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateJwt(user, roles);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id.ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });

        Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            message = "Login successful."
        });
    }
    // public async Task<IActionResult> Login([FromBody] Dtos.LoginRequest request)
    // {
    //     var user = await _userManager.FindByEmailAsync(request.Email);
    //     if (user == null) return Unauthorized(new
    //     {
    //         detail = "Invalid credentials."
    //     });
    //     if (await _userManager.IsLockedOutAsync(user))
    //     {
    //         return StatusCode(423, new
    //         {
    //             detail = "Account locked due to multiple failed login attempts."
    //         });
    //     }
    //     var validPassword = await _userManager.CheckPasswordAsync(user,
    //     request.Password);
    //     if (!validPassword)
    //     {
    //         await _userManager.AccessFailedAsync(user);
    //         return Unauthorized(new
    //         {
    //             detail = "Invalid  credentials."
    //         });
    //     }
    //     await _userManager.ResetAccessFailedCountAsync(user);
    //     var roles = await _userManager.GetRolesAsync(user);
    //     var accessToken = _tokenService.GenerateJwt(user, roles);
    //     // Issue initial Refresh Token
    //     var refreshToken = new RefreshToken
    //     {
    //         Token = Guid.NewGuid().ToString("N"),
    //         UserId = user.Id.ToString(),
    //         ExpiresAt = DateTime.UtcNow.AddDays(7),
    //         IsUsed = false,
    //         IsRevoked = false
    //     };
    //     _context.RefreshTokens.Add(refreshToken);
    //     await _context.SaveChangesAsync();
    //     return Ok(new
    //     {
    //         accessToken,
    //         refreshToken = refreshToken.Token
    //     });
    // }
    public record RefreshRequest(string RefreshToken);
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(rt => rt.Token ==
        request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        if (storedToken.IsUsed)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();
            foreach (var t in userTokens)
            {
                t.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return Unauthorized(new
            {
                detail = "Token theft detected.  All user sessions revoked."
            });
        }
        if (storedToken.IsRevoked || storedToken.ExpiresAt <
            DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }
        // Mark current token as used
        storedToken.IsUsed = true;
        // Issue brand-new Refresh Token pair
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();
        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        var roles = await _userManager.GetRolesAsync(user!);
        var newAccessToken = _tokenService.GenerateJwt(user!, roles);
        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }






    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = $"User not found for ID: {userId}"
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            email = user.Email,
            displayName = user.FirstName,
            role = roles.FirstOrDefault() ?? ""
        });
    }








    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await
            _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {

            return Ok(new
            {
                message = "Registration request received."
            });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        var result = await _userManager.CreateAsync(user,
            request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(new
                IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(user, request.Role);
        return Ok(new { message = "Registration successful." });
    }
}


//     [HttpPost("login")]
//     public async Task<IActionResult> Login([FromBody] Dtos.LoginRequest request)
//     {
//         var user = await _userManager.FindByEmailAsync(request.Email);
//         if (user == null)
//         {
//             return Unauthorized(new { detail = "Invalid credentials." });

//         }

//         if (await _userManager.IsLockedOutAsync(user))
//         {
//             return StatusCode(423, new
//             {
//                 detail = "Account locked due to multiple failed login attempts.Try again in 15 minutes."
//             });
//         }

//         var validPassword = await _userManager.CheckPasswordAsync(user,
//             request.Password);
//         if (!validPassword)
//         {
//             await _userManager.AccessFailedAsync(user);
//             return Unauthorized(new { detail = "Invalid credentials." });
//         }

//         await _userManager.ResetAccessFailedCountAsync(user);
//         return Ok(new
//         {
//             userId = user.Id,
//             email = user.Email,
//             firstName = user.FirstName,
//             lastName = user.LastName
//         });
//     }
// }



// public class AuthController : ControllerBase
// {
//     [HttpPost("login")]
//     public IActionResult Login(
//     [FromBody] LoginRequest request,
//     [FromServices] IWebHostEnvironment env)
//     {

//         if (request.Username == "admin" && request.Password ==
//         "Password123!")
//         {
//             var dummyJwt = "header.payload.signature-demo-token";

//             Response.Cookies.Append("tms_auth", dummyJwt, new
//             CookieOptions
//             {
//                 HttpOnly = true,
//                 Secure = !env.IsDevelopment(),
//                 SameSite = SameSiteMode.Strict,
//                 Expires = DateTimeOffset.UtcNow.AddHours(2)
//             });
//             return Ok(new UserProfileDto("System Admin",
//             "Admin"));
//         }
//         return Unauthorized(new
//         {
//             detail = "Invalid username orpassword."
//         });
//     }
//     [HttpGet("me")]
//     public IActionResult GetCurrentUser()
//     {

//         if (Request.Cookies.TryGetValue("tms_auth", out _))
//         {
//             return Ok(new UserProfileDto("System Admin",
//             "Admin"));
//         }
//         return Unauthorized(new
//         {
//             detail = "Session expired or missing authentication cookie."
//         });
//     }
// }