using DotNetHelper.Logger.Enums;
using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Services.LogFormatters
{
    /// <summary>
    /// 기본 문자열 템플릿 기반 포맷터
    /// "{DateTime:yyyy-MM-dd HH:mm:ss} [{LogLevel}] - [{ThreadName}] {Message}{IfException:\r\n}{Exception}"
    /// </summary>
    public class LogFormatter : ILogFormatter
    {
        private readonly bool _useShortLogLevelName;

        public string Template { get; }

        public static readonly string LogLevel = "LogLevel";
        public static readonly string Message = "Message";
        public static readonly string ThreadName = "ThreadName";
        public static readonly string Exception = "Exception";
        public static readonly string IfException = "IfException";
        public static readonly string DateTime = "DateTime";
        public static readonly string Keyword = "Keyword";

        protected readonly HashSet<string> DynamicTokens = new HashSet<string>()
        {
            LogLevel, Message, ThreadName, Exception, IfException, DateTime, Keyword
        };

        public LogFormatter(string template, bool useShortLogLevelName = true)
        {
            Template = template;
            _useShortLogLevelName = useShortLogLevelName;
        }

        protected bool IsDynamic(string segment)
        {
            foreach (var token in DynamicTokens)
            {
                string pattern = "{" + token;
                if (segment.IndexOf(pattern) >= 0)
                    return true;
            }

            return false;
        }

        public string Format(LogEntry entry)
        {
            string logLevelStr = _useShortLogLevelName ? entry.LogLevel.GetDescription() : entry.LogLevel.ToString();
            string text = Template
                .Replace($"{{{LogLevel}}}", logLevelStr)
                .Replace($"{{{Message}}}", entry.Message)
                .Replace($"{{{ThreadName}}}", entry.ThreadName ?? string.Empty)
                .Replace($"{{{Keyword}}}", entry.Keyword ?? string.Empty)
                .Replace($"{{{Exception}}}", entry.Exception?.ToString() ?? string.Empty);

            text = ReplaceDateTimeToken(text, entry.Timestamp);
            text = ReplaceIfExceptionToken(text, entry.Exception != null);

            return text;
        }

        private string ReplaceIfExceptionToken(string text, bool hasException)
        {
            string tokenStart = $"{{{IfException}:";
            int startIndex = text.IndexOf(tokenStart);
            while (startIndex >= 0)
            {
                int endIndex = text.IndexOf('}', startIndex);
                if (endIndex < 0) break; // 잘못된 패턴

                int insideStart = startIndex + tokenStart.Length;
                string inside = text.Substring(insideStart, endIndex - insideStart);


                text = hasException
                     ? text.Substring(0, startIndex) + inside + text.Substring(endIndex + 1)
                     : text.Substring(0, startIndex) + text.Substring(endIndex + 1);

                startIndex = text.IndexOf(tokenStart, Math.Min(startIndex + inside.Length, text.Length));

            }
            return text;
        }
        private string ReplaceDateTimeToken(string text, DateTime dt)
        {
            // {DateTime:yyyy-MM-dd HH:mm:ss} 형식을 인식해서 실제 값으로 치환

            string tokenStart = $"{{{DateTime}:";

            int startIndex = text.IndexOf(tokenStart);
            while (startIndex >= 0)
            {
                int endIndex = text.IndexOf('}', startIndex);
                if (endIndex < 0) break; // 잘못된 패턴

                int formatStart = startIndex + tokenStart.Length;
                var dtFormat = text.Substring(formatStart, endIndex - formatStart);
                var formatted = dt.ToString(dtFormat);

                text = text.Substring(0, startIndex) + formatted + text.Substring(endIndex + 1);
                startIndex = text.IndexOf(tokenStart, startIndex + formatted.Length);
            }
            return text;
        }
    }
}
