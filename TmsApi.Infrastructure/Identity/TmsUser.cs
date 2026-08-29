using Microsoft.AspNetCore.Identity;
namespace TmsApi.Infrastructure.Identity;

public class TmsUser : IdentityUser
{
    // public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
}