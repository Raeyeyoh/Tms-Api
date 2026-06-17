namespace TmsApi.Models;

public class CourseModel
{
    public CourseModel(string ID, string courseCode, string title, int capacity, int enrolledCount)
    {
        id = ID;
        Code = courseCode;
        Title = title;
        Capacity = capacity;
        EnrolledCount = enrolledCount;
    }
    public string id { get; init; }
    public string Code { get; init; }
    public string Title
    {
        get;
        set => field = !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException("Title cannot be empty or whitespace.", nameof(value));
    }
    public int Capacity
    {
        get;
        set => field = value > 0
        ? value
        : throw new ArgumentOutOfRangeException(nameof(value), "System constraint: Capacity must begreater than zero.");
    }
    public int EnrolledCount { get; set; }
}

