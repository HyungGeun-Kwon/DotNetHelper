using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Services.LogWriters
{
    public class FileLogWriter : ILogWriter
    {
        private bool _disposed;

        private readonly object _lockObj = new object();
        private readonly ILogFormatter _logFormatter;
        private readonly ILogPathFormatter _fileFullPathFormatter;
        private readonly LogStreamManager _streamManager;

        public ELogLevel LogLevel { get; set; } = ELogLevel.Verbose;

        public string LogRootPath => _fileFullPathFormatter.RootPath;

        /// <summary>
        /// 로그를 Wirte하는 클래스
        /// StreamWriter를 가지고있으며, 파일 변경 전까진 Stream을 닫지 않는다.
        /// </summary>
        /// <param name="fileFullPathFormatter">파일 전체 경로 포멧(파일경로 + 파일명)</param>
        /// <param name="logFormatter">로깅 시 사용할 포멧</param>
        public FileLogWriter(ILogPathFormatter fileFullPathFormatter, ILogFormatter logFormatter)
        {
            _logFormatter = logFormatter;
            _fileFullPathFormatter = fileFullPathFormatter;
            _streamManager = new LogStreamManager(fileFullPathFormatter);
        }

        public void Write(LogEntry entry)
        {
            if ((int)entry.LogLevel < (int)LogLevel)
                return; // 레벨이 낮으면 무시

            string line = _logFormatter.Format(entry);
            lock (_lockObj)
            {
                var streamWriter = _streamManager.GetStream(entry);
                streamWriter.WriteLine(line);
                streamWriter.Flush();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (_lockObj)
                    {
                        _streamManager.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}
