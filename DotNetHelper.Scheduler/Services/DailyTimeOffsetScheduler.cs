using DotNetHelper.Scheduler.Interfaces;

namespace DotNetHelper.Scheduler.Services
{
    public sealed class DailyTimeOffsetScheduler : IScheduler
    {
        private readonly Lock _startLock = new();
        private readonly ManualResetEventSlim _runCompleted = new(initialState: true);

        // non‑null 필드
        private Timer _timer = new(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        private CancellationTokenSource _cts = new();

        private Action<CancellationToken>? _action;
        private DateTime _lastRunDate;
        private int _running;

        private readonly TimeSpan _runTimeOfDay;

        /// <summary>runTimeOfDay 뒤에 더해지는 오프셋.</summary>
        public TimeSpan Offset { get; set; }

        public bool IsTimerRunning => Volatile.Read(ref _running) == 1;

        public event EventHandler<Exception>? TimerExceptionEvent;

        /// <param name="runTimeOfDay">00:00:00 ≤ 24:00:00</param>
        /// <param name="offset">Timer 오차 보정(기본 10초)</param>
        public DailyTimeOffsetScheduler(TimeSpan runTimeOfDay, TimeSpan? offset = null)
        {
            if (runTimeOfDay < TimeSpan.Zero ||
                runTimeOfDay >= TimeSpan.FromDays(1))
                throw new ArgumentOutOfRangeException(nameof(runTimeOfDay));

            _runTimeOfDay = runTimeOfDay;
            Offset = offset ?? TimeSpan.FromSeconds(10);
        }

        public void Start(Action action, bool runImmediately = true) => Start(_ => action(), runImmediately);

        public void Start(Action<CancellationToken> action, bool runImmediately = true)
        {
            ArgumentNullException.ThrowIfNull(action);

            lock (_startLock)
            {
                if (IsTimerRunning)
                    throw new InvalidOperationException("Scheduler already started.");

                // CTS 교체
                var oldCts = Interlocked.Exchange(ref _cts,
                                new CancellationTokenSource());
                oldCts.Dispose();

                // Timer 교체
                var due = runImmediately ? TimeSpan.Zero : GetDelayUntilNext();
                var newTimer = new Timer(OnTimerTick, null, due, Timeout.InfiniteTimeSpan);

                var oldTimer = Interlocked.Exchange(ref _timer, newTimer);
                oldTimer.Dispose();

                _action = action;
            }
        }

        private void OnTimerTick(object? _)
        {
            // 재진입 방지
            if (Interlocked.Exchange(ref _running, 1) == 1) return;

            _runCompleted.Reset();
            try
            {
                // 동일한날에 실행되었다면 다음날 정확히 실행되도록 타이머 변경
                if (_cts.IsCancellationRequested) return;

                var today = DateTime.Today;
                if (_lastRunDate == today)
                {
                    _timer.Change(GetDelayUntilNext(), Timeout.InfiniteTimeSpan);
                    return;
                }

                _lastRunDate = today;
                _action?.Invoke(_cts.Token);

                if (_cts.IsCancellationRequested) return;

                // 다음 스케줄 계산 (자정 + offset)
                _timer.Change(GetDelayUntilNext(), Timeout.InfiniteTimeSpan);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { TimerExceptionEvent?.Invoke(this, ex); }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
                _runCompleted.Set();
            }
        }

        private TimeSpan GetDelayUntilNext()
        {
            var now = DateTime.Now;
            var target = now.Date + _runTimeOfDay + Offset;
            if (target <= now) target = target.AddDays(1);
            return target - now;
        }

        public void Stop(TimeSpan? timeout = null) =>
            StopAsyncCore(timeout).GetAwaiter().GetResult();

        public Task AsyncStop(TimeSpan? timeout = null) =>
            StopAsyncCore(timeout);

        private async Task StopAsyncCore(TimeSpan? timeout)
        {
            var oldTimer = Interlocked.Exchange(ref _timer,
                           new Timer(_ => { }, null,
                                     Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan));
            var oldCts = Interlocked.Exchange(ref _cts,
                           new CancellationTokenSource());

            oldCts.Cancel();
            oldTimer.Change(Timeout.Infinite, Timeout.Infinite);

            var waitTask = Task.Run(() => _runCompleted
                          .Wait(timeout ?? Timeout.InfiniteTimeSpan));

            if (!await waitTask.ConfigureAwait(false))
                throw new TimeoutException("Stop/AsyncStop timed‑out while waiting for timer callback to finish.");

            oldTimer.Dispose();
            oldCts.Dispose();
        }

        public void Dispose()
        {
            try { Stop(); } catch { }

            _timer.Dispose();
            _cts.Dispose();
        }
    }
}
