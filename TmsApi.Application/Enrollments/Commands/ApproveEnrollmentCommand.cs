
using MediatR;

namespace TmsApi.Application.Enrollments.Commands;

public record ApproveEnrollmentCommand(int ID) : IRequest<bool>;