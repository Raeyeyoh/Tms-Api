public readonly record struct GradeRecord
{
     public GradeRecord(string code, decimal score, DateTime gradetat)
     {
          CourseCode = code;
          if (score >= 0 && score <= 100)
          {
               Score = score;
          }
          else
          {
               throw new ArgumentOutOfRangeException();
          }

          GradeAt = gradetat;
     }
     readonly String CourseCode;
     readonly decimal Score;
     private readonly DateTime GradeAt;


}