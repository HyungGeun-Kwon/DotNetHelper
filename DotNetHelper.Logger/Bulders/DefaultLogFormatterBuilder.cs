using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Services.LogFormatters;

namespace DotNetHelper.Logger.Bulders
{
    public class DefaultLogFormatterBuilder : ILogFormatterBuilder
    {
        //  yyyy-MM-dd_HH:mm:ss.fff [LogLevel] {Keyword} <ThreadName> Message\r\n    Exception
        public ILogFormatter Build()
        {
            return new LogFormatter(
                $"{{{LogFormatter.DateTime}:yyyy-MM-dd_HH:mm:ss.fff}} " +
                $"[{{{LogFormatter.LogLevel}}}] " +
                $"{{{{{LogFormatter.Keyword}}}}} " +
                $"<{{{LogFormatter.ThreadName}}}> " +
                $"{{{LogFormatter.Message}}}" +
                $"{{{LogFormatter.IfException}:\r\n    }}" +
                $"{{{LogFormatter.Exception}}}");
        }
    }
}
