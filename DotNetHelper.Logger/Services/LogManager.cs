using System.Collections.Concurrent;
using DotNetHelper.Cleaner.Interfaces;
using DotNetHelper.Logger.Bulders;
using DotNetHelper.Logger.Events;
using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Models;
using DotNetHelper.Scheduler.Interfaces;
using DotNetHelper.Scheduler.Services;

namespace DotNetHelper.Logger.Services
{
    public class LogManager : ILogManager
    {
        private readonly ConcurrentDictionary<string, ILogWriter> _writers = new();
        private ICleaner _logCleaner;
        private IScheduler _logCleanScheduler;

        // 0 = 중지, 1 = 실행 중.
        private int _schedulerRunning;
        private readonly Lock _sync = new();

        // 0 = 사용 중 / 1 = Disposed
        private int _disposeState;
        private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;

        public event EventHandler<LogCleanupErrorEventArgs>? LogFileCleanSchedulerException;
        public event EventHandler? LogFileCleanerStart;
        public event EventHandler? LogFileCleanerEnd;
        
        /// <summary>
        /// 삭제 스케줄러 : 자정마다 실행
        /// 클리너 : 삭제 수행하지 않음.
        /// </summary>
        public LogManager()
            : this(new MidnightOffsetScheduler(), new DefaultLogCleanerBuilder().Build()) { }
        /// <summary>
        /// 삭제 스케줄러 : 자정마다 실행
        /// 클리너 : 파일 = 수정 시간 기준 삭제, 폴더 = 생성시간 + Empty여부 확인하여 삭제
        /// </summary>
        /// <param name="retentionTime">설정한 시간보다 오래된 파일 삭제</param>
        public LogManager(TimeSpan retentionTime)
            : this(new MidnightOffsetScheduler(), new DefaultLogCleanerBuilder().Build(retentionTime)) { }
        /// <summary>
        /// 직접 IScheduler와 ICleaner를 사용하여 생성
        /// </summary>
        public LogManager(IScheduler logScheduler, ICleaner logCleaner)
        {
            _logCleanScheduler = logScheduler ?? throw new ArgumentNullException(nameof(logScheduler));
            _logCleaner = logCleaner ?? throw new ArgumentNullException(nameof(logCleaner));
        }

        public void WriteLog(string key, LogEntry entry)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            GetLogWriter(key).Write(entry);
        }
        public void AddWriter(string key, ILogWriter writer)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _writers[key] = writer;
        }
        public bool RemoveWriter(string key)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (_writers.TryRemove(key, out var writer))
            {
                writer?.Dispose();
                return true;
            }
            return false;
        }
        public bool IsContainKey(string key)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            return _writers.ContainsKey(key);
        }
        public void SetLogLevel(string key, ELogLevel logLevel)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            GetLogWriter(key).LogLevel = logLevel;
        }
        public void SetScheduler(IScheduler logScheduler)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            lock (_sync)
            {
                // 실행 중이면 중지 후 교체
                if (Interlocked.Exchange(ref _schedulerRunning, 0) == 1)
                    _logCleanScheduler.Stop();

                _logCleanScheduler.Dispose();
                _logCleanScheduler = logScheduler;
            }
        }
        public void SetCleaner(ICleaner logCleaner)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            lock (_sync) _logCleaner = logCleaner;
        }
        /// <summary>로그 청소 스케줄러를 시작한다. 이미 실행 중이면 false.</summary>
        public bool StartCleanupScheduler(bool runImmediately = true)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (Interlocked.CompareExchange(ref _schedulerRunning, 1, 0) == 1)
                return false;

            try
            {
                _logCleanScheduler.Start(CleanLogFiles, runImmediately);
                return true;
            }
            catch
            {
                Interlocked.Exchange(ref _schedulerRunning, 0);
                throw;
            }
        }
        /// <summary>스케줄러를 중지한다. 실행 중이 아니면 false.</summary>
        public bool StopCleanupScheduler()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (Interlocked.CompareExchange(ref _schedulerRunning, 0, 1) == 0)
                return false; // 이미 중지됨

            _logCleanScheduler.Stop();
            return true;
        }
        
        private void CleanLogFiles(CancellationToken token)
        {
            LogFileCleanerStart?.Invoke(this, EventArgs.Empty);
            try
            {
                foreach (var kv in _writers.ToArray())
                {
                    token.ThrowIfCancellationRequested();
                    _logCleaner.Cleanup(kv.Value.LogRootPath);
                }
            }
            catch (OperationCanceledException) { } // 토큰취소
            catch (Exception ex) { LogFileCleanSchedulerException?.Invoke(this, new LogCleanupErrorEventArgs(ex)); }
            finally
            {
                LogFileCleanerEnd?.Invoke(this, EventArgs.Empty);
            }
        }
        private ILogWriter GetLogWriter(string key)
        {
            if (!_writers.TryGetValue(key, out ILogWriter? writer))
                throw new KeyNotFoundException($"Log writer with key '{key}' not found.");

            ArgumentNullException.ThrowIfNull(writer);
            return writer;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeState, 1) == 1)
                return;

            StopCleanupScheduler();
            _logCleanScheduler.Dispose();

            foreach (var writer in _writers.Values)
                writer.Dispose();

            _writers.Clear();
        }
    }
}
