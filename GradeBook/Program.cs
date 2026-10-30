using GradeBook;

LogService errorLog = new LogService(Path.Combine("logs", "errors"), new SystemClock());
GradeFile grades = new GradeFile(errorLog);

List<Student> students = grades.Read(Path.Combine("data", "student_grades.csv"));
Console.WriteLine($"Read {students.Count} students");

grades.WriteHonorRoll("honor_roll.csv", students, 3.5m);
if (File.Exists("honor_roll.csv"))
{
    Console.WriteLine(File.ReadAllText("honor_roll.csv"));
}

if (File.Exists(errorLog.CurrentFilePath))
{
    Console.WriteLine($"Problems logged in {errorLog.CurrentFilePath}:");
    Console.WriteLine(File.ReadAllText(errorLog.CurrentFilePath));
}
