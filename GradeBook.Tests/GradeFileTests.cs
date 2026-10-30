namespace GradeBook.Tests;

[TestClass]
public class GradeFileTests
{
    private string folder = "";
    private LogService errorLog = null!;
    private GradeFile grades = null!;

    [TestInitialize]
    public void MakeFolder()
    {
        folder = Path.Combine(Path.GetTempPath(), "GradeBookTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        errorLog = new LogService(Path.Combine(folder, "errors"), new FakeClock(new DateTime(2026, 3, 14, 9, 0, 0)));
        grades = new GradeFile(errorLog);
    }

    [TestCleanup]
    public void RemoveFolder()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private string WriteFile(params string[] lines)
    {
        string path = Path.Combine(folder, "grades.csv");
        File.WriteAllLines(path, lines);
        return path;
    }

    private string[] ErrorLogLines()
    {
        string path = Path.Combine(folder, "errors", "20260314.log");
        return File.Exists(path) ? File.ReadAllLines(path) : new string[0];
    }

    [TestMethod]
    public void Read_GoodFile_ReturnsEveryStudentAfterTheHeader()
    {
        string path = WriteFile("firstname,lastname,gpa", "Emily,Wilson,3.91", "John,Smith,2.55");

        List<Student> students = grades.Read(path);

        Assert.HasCount(2, students);
        Assert.AreEqual("Emily", students[0].FirstName);
        Assert.AreEqual("Wilson", students[0].LastName);
        Assert.AreEqual(3.91m, students[0].Gpa);
        Assert.AreEqual("John", students[1].FirstName);
    }

    [TestMethod]
    public void Read_SpacesAroundValues_AreIgnored()
    {
        string path = WriteFile("firstname,lastname,gpa", "Emily , Wilson , 3.91");

        List<Student> students = grades.Read(path);

        Assert.HasCount(1, students);
        Assert.AreEqual("Emily", students[0].FirstName);
        Assert.AreEqual("Wilson", students[0].LastName);
        Assert.AreEqual(3.91m, students[0].Gpa);
    }

    [TestMethod]
    public void Read_BlankLines_AreSkippedWithoutLogging()
    {
        string path = WriteFile("firstname,lastname,gpa", "Emily,Wilson,3.91", "", "John,Smith,2.55", "");

        List<Student> students = grades.Read(path);

        Assert.HasCount(2, students);
        Assert.IsEmpty(ErrorLogLines());
    }

    [TestMethod]
    public void Read_BadLines_AreSkippedAndLogged()
    {
        string path = WriteFile("firstname,lastname,gpa", "Emily,Wilson,3.91", "John,Smith", "Anna,Williams,n/a", "Jane,Garcia,3.96");

        List<Student> students = grades.Read(path);

        Assert.HasCount(2, students);
        Assert.AreEqual("Jane", students[1].FirstName);
        CollectionAssert.AreEqual(
            new[]
            {
                "2026-03-14 09:00:00 grades.csv line 3: can't read 'John,Smith'",
                "2026-03-14 09:00:00 grades.csv line 4: can't read 'Anna,Williams,n/a'"
            },
            ErrorLogLines());
    }

    [TestMethod]
    public void Read_MissingFile_ReturnsNoStudentsAndLogsIt()
    {
        List<Student> students = grades.Read(Path.Combine(folder, "nobody.csv"));

        Assert.IsEmpty(students);
        CollectionAssert.AreEqual(new[] { "2026-03-14 09:00:00 nobody.csv not found" }, ErrorLogLines());
    }

    [TestMethod]
    public void WriteHonorRoll_Students_WritesTheHeaderAndThoseAtTheMinimum()
    {
        string path = Path.Combine(folder, "honor_roll.csv");
        List<Student> students = new List<Student>
        {
            new Student("Emily", "Wilson", 3.91m),
            new Student("John", "Smith", 2.55m),
            new Student("Chris", "Martinez", 3.5m)
        };

        grades.WriteHonorRoll(path, students, 3.5m);

        CollectionAssert.AreEqual(
            new[] { "firstname,lastname,gpa", "Emily,Wilson,3.91", "Chris,Martinez,3.50" },
            File.ReadAllLines(path));
    }

    [TestMethod]
    public void WriteHonorRoll_FileAlreadyThere_ReplacesIt()
    {
        string path = Path.Combine(folder, "honor_roll.csv");
        File.WriteAllLines(path, new[] { "old,file,1.00", "more,old,2.00" });

        grades.WriteHonorRoll(path, new List<Student> { new Student("Jane", "Garcia", 3.96m) }, 3.5m);

        CollectionAssert.AreEqual(new[] { "firstname,lastname,gpa", "Jane,Garcia,3.96" }, File.ReadAllLines(path));
    }

    [TestMethod]
    public void AddStudent_FileAlreadyThere_AddsOneLineAtTheEnd()
    {
        string path = WriteFile("firstname,lastname,gpa", "Emily,Wilson,3.91");

        grades.AddStudent(path, new Student("Zoe", "Whitehouse", 3.9m));

        CollectionAssert.AreEqual(
            new[] { "firstname,lastname,gpa", "Emily,Wilson,3.91", "Zoe,Whitehouse,3.90" },
            File.ReadAllLines(path));
    }

    [TestMethod]
    public void AddStudent_NoFileYet_WritesTheHeaderFirst()
    {
        string path = Path.Combine(folder, "new_class.csv");

        grades.AddStudent(path, new Student("Zoe", "Whitehouse", 3.95m));

        CollectionAssert.AreEqual(new[] { "firstname,lastname,gpa", "Zoe,Whitehouse,3.95" }, File.ReadAllLines(path));
    }

    [TestMethod]
    public void AddStudent_ThenRead_ReadsTheStudentBack()
    {
        string path = Path.Combine(folder, "new_class.csv");

        grades.AddStudent(path, new Student("Zoe", "Whitehouse", 3.95m));
        grades.AddStudent(path, new Student("Sam", "Lee", 3.1m));
        List<Student> students = grades.Read(path);

        Assert.HasCount(2, students);
        Assert.AreEqual("Sam", students[1].FirstName);
        Assert.AreEqual(3.1m, students[1].Gpa);
    }
}
