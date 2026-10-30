namespace GradeBook;

// One student and their grade point average. It's finished.
public class Student
{
    public string FirstName { get; }
    public string LastName { get; }
    public decimal Gpa { get; }

    public Student(string firstName, string lastName, decimal gpa)
    {
        FirstName = firstName;
        LastName = lastName;
        Gpa = gpa;
    }
}
