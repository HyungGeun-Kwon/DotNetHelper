using DotNetHelper.Logger.Enums;
using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Services
{
    public static class LogLevelShortcuts
    {
        public static void Verbose(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Verbose, message, logKeyword));

        public static void Debug(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Debug, message, logKeyword));

        public static void Info(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Info, message, logKeyword));

        public static void Warning(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default, Exception ex = null)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Warning, message, logKeyword, ex));

        public static void Error(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default, Exception ex = null)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Error, message, logKeyword, ex));

        public static void Fatal(this ILogManager logManager, string key, string message, ELogKeyword logKeyword = ELogKeyword.Default, Exception ex = null)
            => logManager.WriteLog(key, new LogEntry(ELogLevel.Fatal, message, logKeyword, ex));
    }
}
