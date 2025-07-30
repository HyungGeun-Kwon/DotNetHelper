using DotNetHelper.Logger.Enums;

namespace DotNetHelper.Logger.Models
{
    public sealed class LogEntry
    {
        public ELogLevel LogLevel { get; }
        public string Message { get; }
        public string Keyword { get; }
        public Exception Exception { get; }
        public string ThreadName { get; }
        public DateTime Timestamp { get; }

        public LogEntry(ELogLevel level, string message, string keyword = default, Exception ex = null)
        {
            LogLevel = level;
            Keyword = keyword == default ? ELogKeyword.Default.GetDescription() : keyword;
            Message = message;
            Exception = ex;
            ThreadName = Thread.CurrentThread.Name ?? $"Thread-{Thread.CurrentThread.ManagedThreadId}";
            Timestamp = DateTime.Now;
        }
        public LogEntry(ELogLevel level, string message, ELogKeyword keyword = ELogKeyword.Default, Exception ex = null)
        {
            LogLevel = level;
            Keyword = keyword.GetDescription();
            Message = message;
            Exception = ex;
            ThreadName = Thread.CurrentThread.Name ?? $"Thread-{Thread.CurrentThread.ManagedThreadId}";
            Timestamp = DateTime.Now;
        }
        public LogEntry(ELogLevel logLevel, string message, string keyword, Exception exception, string threadName, DateTime timestamp)
        {
            LogLevel = logLevel;
            Message = message;
            Keyword = keyword;
            Exception = exception;
            ThreadName = threadName;
            Timestamp = timestamp;
        }
    }
}
