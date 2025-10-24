namespace Feramor.CanliDoviz.Models;

public class Log
{
    public LogLevel LogLevel { get; protected set; }
    public DateTime Date { get; protected set; }
    public string Message { get; protected set; }
    public Exception? Exception { get; protected set; }

    public Log(DateTime date, string message, LogLevel logLevel = LogLevel.Info, Exception? exception = null)
    {
        LogLevel = logLevel;
        Date = date;
        Message = message;
        Exception = exception;
    }
}