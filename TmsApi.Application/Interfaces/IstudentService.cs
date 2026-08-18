namespace TmsApi.Application.Interfaces;

using Dtos;
public interface IStudentService
{
    Task<StudentResponseDto> CreateAsync(RegisterStudentDto student, CancellationToken ct);

    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<bool> DeleteStudentAsync(int id);
    Task<bool> UpdateStudentAsync(UpdatestudentRequest request, CancellationToken ct);
    Task<bool?> SoftDeleteAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<StudentResponseDto?>> ShowDeletedAsync();
    Task<bool> CodeExistsAsync(string registrationNumber, CancellationToken ct);


}