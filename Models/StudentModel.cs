
using System.Xml;

namespace TmsApi.Models;

public class StudentModel
{


    public StudentModel(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {

            throw new ArgumentException();


        }
        else
        {
            Name = name;

        }

        Id = id;

    }
    private readonly List<GradeRecord> _grades = new();

    public IReadOnlyList<GradeRecord> Grades => _grades.AsReadOnly();

    public string Id { get; }
    public string Name { get; }
    public decimal GPA { get; set; }
    public int Age { get; set; }

    public void AddGrade(GradeRecord grade) => _grades.Add(grade);
}

















// public StudentModel(String id, string idno, string name, int age, decimal gpa)
// {
//     Id = id;
//     IdNo = idno;
//     Name = name;
//     Age = age;
//     GPA = gpa;
// }


// public string Id { get; init; }
// public string IdNo { get; set; }
// public string Name
// {
//     get;
//     set => field = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
// }
// public int Age
// {
//     get;
//     set => field = value is >= 16 and <= 100 ? value : throw new ArgumentOutOfRangeException(nameof(value), "Age must be between 16 and 100.");
// }
// public decimal GPA
// {
//     get;
//     set => field = value is >= 0.0m and <= 4.0m ? value : throw new ArgumentOutOfRangeException(nameof(value), "GPA must be between 0.0and 4.0.");
// }