namespace GradeBook.Tests;

[TestClass]
public class LogServiceTests
{
    private string folder = "";

    [TestInitialize]
    public void MakeFolder()
    {
        folder = Path.Combine(Path.GetTempPath(), "GradeBookTests", Guid.NewGuid().ToString());
    }

    [TestCleanup]
    public void RemoveFolder()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void CurrentFilePath_AnyDay_IsThatDaysFileInTheFolder()
    {
        LogService log = new LogService(folder, new FakeClock(new DateTime(2026, 3, 14, 21, 5, 9)));

        Assert.AreEqual(Path.Combine(folder, "20260314.log"), log.CurrentFilePath);
    }

    [TestMethod]
    public void Log_OneEntry_WritesTheTimeAndTheMessage()
    {
        LogService log = new LogService(folder, new FakeClock(new DateTime(2026, 3, 14, 21, 5, 9)));

        log.Log("Started");

        CollectionAssert.AreEqual(new[] { "2026-03-14 21:05:09 Started" }, File.ReadAllLines(Path.Combine(folder, "20260314.log")));
    }

    [TestMethod]
    public void Log_TwoEntries_KeepsBoth()
    {
        FakeClock clock = new FakeClock(new DateTime(2026, 3, 14, 9, 0, 0));
        LogService log = new LogService(folder, clock);

        log.Log("Started");
        clock.Now = clock.Now.AddMinutes(5);
        log.Log("Finished");

        CollectionAssert.AreEqual(
            new[] { "2026-03-14 09:00:00 Started", "2026-03-14 09:05:00 Finished" },
            File.ReadAllLines(Path.Combine(folder, "20260314.log")));
    }

    [TestMethod]
    public void Log_FolderNotThereYet_CreatesIt()
    {
        string nested = Path.Combine(folder, "logs", "errors");
        LogService log = new LogService(nested, new FakeClock(new DateTime(2026, 3, 14, 9, 0, 0)));

        log.Log("Started");

        Assert.IsTrue(File.Exists(Path.Combine(nested, "20260314.log")));
    }

    [TestMethod]
    public void Log_NextDay_StartsANewFile()
    {
        FakeClock clock = new FakeClock(new DateTime(2026, 3, 14, 23, 59, 0));
        LogService log = new LogService(folder, clock);

        log.Log("Late");
        clock.Now = clock.Now.AddMinutes(2);
        log.Log("Early");

        CollectionAssert.AreEqual(new[] { "2026-03-14 23:59:00 Late" }, File.ReadAllLines(Path.Combine(folder, "20260314.log")));
        CollectionAssert.AreEqual(new[] { "2026-03-15 00:01:00 Early" }, File.ReadAllLines(Path.Combine(folder, "20260315.log")));
    }
}

// A fake clock: it's whatever time a test sets.
public class FakeClock : IClock
{
    public DateTime Now { get; set; }

    public FakeClock(DateTime now)
    {
        Now = now;
    }
}
