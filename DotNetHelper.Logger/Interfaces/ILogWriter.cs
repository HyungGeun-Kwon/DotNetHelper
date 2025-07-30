using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Interfaces
{
    public interface ILogWriter : IDisposable
    {
        ELogLevel LogLevel { get; set; }
        string LogRootPath { get; }
        void Write(LogEntry entry);
    }
}
