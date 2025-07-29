using DotNetHelper.Scheduler.Interfaces;

namespace DotNetHelper.Scheduler.Services
{
    public class MidnightOffsetScheduler : IScheduler
    {
        private readonly Lock _startLock = new();
        private Timer _timer = new(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        private CancellationTokenSource _cts = new();
        private Action<CancellationToken>? _action;
        private DateTime _lastRunDate;
        private int _running; // o : 실행X(false) 1 : 실행(true)
        private readonly ManualResetEventSlim _runCompleted = new(true);

        /// <summary>
        /// 타이머가 매우 정확하지는 않으므로 자정보다 미세하게 일찍 실행될 수 있어 안정성을 위해 Offset 설정
        /// 물론 과거 타이머 실행 날짜와 현재 실행 날짜가 동일하다면 타이머 시간 다시 계산해서 활성화
        /// 기본값 = 10sec
        /// </summary>
        public TimeSpan MidnightOffset { get; set; } = TimeSpan.FromSeconds(10);
        public bool IsTimerRunning => Volatile.Read(ref _running) == 1;

        public event EventHandler<Exception>? TimerExceptionEvent;

        public void Start(Action action, bool runImmediately = true) => Start(_ => action(), runImmediately);

        public void Start(Action<CancellationToken> action, bool runImmediately = true)
        {
            ArgumentNullException.ThrowIfNull(action);

            lock (_startLock)
            {
                if (IsTimerRunning) 
                    throw new InvalidOperationException("Scheduler already started.");

                var oldCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                oldCts.Dispose();

                // 타이머 내부에서 다음 스케줄링
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

            _runCompleted.Reset(); // 콜백 시작
            try
            {
                // 동일한날에 실행되었다면 다음날 정확히 실행되도록 타이머 변경
                if (_cts.IsCancellationRequested) return; // Stop()중일 경우 Pass

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
                _ = Interlocked.Exchange(ref _running, 0);
                _runCompleted.Set(); // 콜백 종료
            }
        }

        private TimeSpan GetDelayUntilNext()
        {
            DateTime now = DateTime.Now;
            return now.Date.AddDays(1).Add(MidnightOffset) - now; // 자정 + offset
        }

        /// <summary>
        /// 현재 콜백이 끝날 때까지 차단식으로 대기한 뒤 스케줄러를 종료.
        /// 타임아웃 발생 시 : TimeoutException("Stop() timed-out while waiting for timer callback to finish.")
        /// </summary>
        public void Stop(TimeSpan? timeout = null)
        {
            // Timer / CTS 를 새 객체로 교체해 이후 접근을 안전하게 만든다.
            var oldTimer = Interlocked.Exchange(ref _timer,
                           new Timer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan));
            var oldCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());

            oldCts.Cancel();
            oldTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (!_runCompleted.Wait(timeout ?? Timeout.InfiniteTimeSpan))
                throw new TimeoutException("Stop() timed‑out while waiting for timer callback to finish.");

            oldTimer.Dispose();
            oldCts.Dispose();
        }

        /// <summary>
        /// 비동기로 중지하고 콜백이 끝나면 완료되는 Task 반환
        /// 타임아웃 발생 시 : TimeoutException("AsyncStop() timed-out while waiting for timer callback to finish.")
        /// </summary>
        /// <returns></returns>
        public async Task AsyncStop(TimeSpan? timeout = null)
        {
            var oldTimer = Interlocked.Exchange(ref _timer,
                           new Timer(_ => { }, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan));
            var oldCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());

            oldCts.Cancel();
            oldTimer.Change(Timeout.Infinite, Timeout.Infinite);

            var waitTask = Task.Run(() => _runCompleted.Wait(timeout ?? Timeout.InfiniteTimeSpan));

            if (!await waitTask.ConfigureAwait(false))
                throw new TimeoutException("AsyncStop() timed‑out while waiting for timer callback to finish.");

            oldTimer.Dispose();
            oldCts.Dispose();
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
            _cts.Dispose();
        }
    }
}
