using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Interfaces
{
    public interface ILogFormatter
    {
        string Template { get; }
        string Format(LogEntry entry);
    }
}
