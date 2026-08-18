namespace TmsApi.Application.Interfaces;

using Dtos;
using Domain.Entities;
public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto1?> GetByIdEnrolmentAsync(int id, CancellationToken ct);

    Task<bool> ApproveAsync(int id, CancellationToken ct);
    Task<List<EnrollmentResponseDto1>> GetEnrollmentByIdAsync(int courseId, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByStudentIdAsync(int studentId, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<IReadOnlyList<Enrollment>> GetAllAsync();
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task<bool> DeleteAsync(int id);
    Task<Enrollment> AddAsync(Enrollment request, CancellationToken ct);
    Task<bool> UpdateBulk(CancellationToken ct);

}