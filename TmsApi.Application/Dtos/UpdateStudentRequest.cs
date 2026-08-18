namespace TmsApi.Application.Dtos;

public record UpdatestudentRequest(
    int IdNo,
    string Name,
    decimal Gpa,
    uint Version);