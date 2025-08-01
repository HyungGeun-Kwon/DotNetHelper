using System.Collections.Concurrent;

namespace DotNetHelper.LifeTime.Bootstrap
{
    public class BootstrapManager : IBootstrapManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, IBootstrapper> _bootstrappers = new();
        private readonly CancellationTokenSource _cts = new();

        private int _disposeState;
        private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;

        public bool IsFail { get; private set; } = false;

        public event EventHandler<Exception>? BootstrapperInitExceptionEvent;
        public event EventHandler<BootstrapperInfo>? BootstrapperInitStartingEvent;
        public event EventHandler? AllBootstrappersCompletedEvent;

        public void AddBootstrap(string key, IBootstrapper bootstrapper)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            _bootstrappers[key] = bootstrapper;
        }

        public void RemoveBootstrap(string key)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _bootstrappers.TryRemove(key, out _);
        }
        public Task InitializeAllAsync(bool stopOnError = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            // 외부 토큰과 내부 중단 토큰을 결합
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            var token = linkedCts.Token;

            IsFail = false;
            int total = _bootstrappers.Count;
            int index = 0;

            return Task.Run(() =>
            {
                int i = 0;
                foreach (var (key, bootstrapper) in _bootstrappers)
                {
                    token.ThrowIfCancellationRequested();

                    var bootInfo = bootstrapper.BootstrapperInfo;
                    bootInfo.SetProgressPercent(total, index);
                    BootstrapperInitStartingEvent?.Invoke(this, bootInfo);

                    try
                    {
                        InitBootstrapper(key);
                    }
                    catch (Exception ex)
                    {
                        IsFail = true;
                        BootstrapperInitExceptionEvent?.Invoke(this, new BootstrapException(bootInfo.Name, ex));
                        if (stopOnError) { break; }
                    }
                    i++;
                }
                AllBootstrappersCompletedEvent?.Invoke(this, EventArgs.Empty);
            });
        }

        public void InitBootstrapper(string key) => _bootstrappers[key].Initialize();
        public void StopInitialize() => _cts.Cancel();
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeState, 1) == 1)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _bootstrappers.Clear();
        }
    }
}
