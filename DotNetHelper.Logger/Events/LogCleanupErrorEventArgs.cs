namespace DotNetHelper.Logger.Events
{
    public sealed class LogCleanupErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public LogCleanupErrorEventArgs(Exception ex) => Exception = ex;
    }
}
