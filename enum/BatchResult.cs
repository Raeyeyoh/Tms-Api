using TmsApi.Models;
public record BatchResult(IReadOnlyList<EnrollmentRecord> Successes, IReadOnlyList<string> Errors);