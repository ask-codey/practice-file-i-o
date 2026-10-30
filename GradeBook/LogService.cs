namespace GradeBook;

// Writes time-stamped entries to a log file, one file per day.
//
// Requirements:
// - The service is given the folder its files go in, and an IClock it asks for the time.
// - CurrentFilePath is today's file in that folder, named for the date as yyyyMMdd.log,
//   such as logs\errors\20260314.log.
// - Log adds one line to the end of today's file: the date and time as yyyy-MM-dd HH:mm:ss, a space,
//   then the message, such as "2026-03-14 21:05:09 Started". It creates the folder and the file when
//   they aren't there yet, and keeps every entry already in the file.
// - Logging must never crash the app: if the file can't be written (an IOException), Log does nothing.
public class LogService
{
    private readonly string directory;
    private readonly IClock clock;

    public LogService(string directory, IClock clock)
    {
        this.directory = directory;
        this.clock = clock;
    }

    public string CurrentFilePath => "";

    public void Log(string message)
    {
    }
}
