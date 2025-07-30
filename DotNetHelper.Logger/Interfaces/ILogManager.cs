using DotNetHelper.Cleaner.Interfaces;
using DotNetHelper.Logger.Events;
using DotNetHelper.Logger.Models;
using DotNetHelper.Scheduler.Interfaces;

namespace DotNetHelper.Logger.Interfaces
{
    public interface ILogManager : IDisposable
    {
        event EventHandler<LogCleanupErrorEventArgs> LogFileCleanSchedulerException;
        void WriteLog(string key, LogEntry entry);
        void AddWriter(string key, ILogWriter writer);
        bool RemoveWriter(string key);
        bool IsContainKey(string key);
        void SetLogLevel(string key, ELogLevel logLevel);
        void SetScheduler(IScheduler locScheduler);
        void SetCleaner(ICleaner logCleaner);
        bool StartCleanupScheduler(bool runImmediately = true);
        bool StopCleanupScheduler();
    }
}
