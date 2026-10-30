namespace GradeBook;

// Tells the time. It's finished. The app uses a SystemClock; the tests use a fake clock they can move forward.
public interface IClock
{
    DateTime Now { get; }
}
