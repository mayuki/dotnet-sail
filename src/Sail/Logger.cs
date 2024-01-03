namespace Sail;

public class Logger(LogLevel logLevel)
{
    public void Trace(string message)
    {
        if (logLevel <= LogLevel.Trace)
        {
            Console.WriteLine($"[{DateTimeOffset.Now:s}][Trace] {message}");
        }
    }

    public void Information(string message)
    {
        if (logLevel <= LogLevel.Information)
        {
            Console.WriteLine($"[{DateTimeOffset.Now:s}][Info] {message}");
        }
    }

    public void Error(string message)
    {
        if (logLevel <= LogLevel.Error)
        {
            Console.WriteLine($"[{DateTimeOffset.Now:s}][Error] {message}");
        }
    }
}

public enum LogLevel
{
    Trace,
    Information,
    Error,
    None = 99,
}
