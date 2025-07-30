using System.Text;
using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Models;

namespace DotNetHelper.Logger.Services.LogWriters
{
    internal class LogStreamManager : IDisposable
    {
        private readonly ILogPathFormatter _pathFormatter;
        private StreamWriter? _streamWriter;
        private string _currentPath = string.Empty;
        private bool disposedValue;

        internal LogStreamManager(ILogPathFormatter pathFormatter)
        {
            _pathFormatter = pathFormatter;
        }

        internal StreamWriter GetStream(LogEntry entry)
        {
            string newPath = _pathFormatter.Format(entry);
            if (_streamWriter == null || newPath != _currentPath)
            {
                string? directoryName = Path.GetDirectoryName(newPath);
                ArgumentNullException.ThrowIfNull(directoryName);
                _streamWriter?.Dispose();
                Directory.CreateDirectory(directoryName);
                _streamWriter = new StreamWriter(newPath, append: true, Encoding.UTF8);
                _currentPath = newPath;
            }
            return _streamWriter;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _streamWriter?.Dispose();
                    _streamWriter = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
