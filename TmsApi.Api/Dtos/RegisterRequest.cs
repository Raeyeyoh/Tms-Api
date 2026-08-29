namespace TmsApi.Api.Dtos;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Role);