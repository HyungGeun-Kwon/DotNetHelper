using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotNetHelper.LifeTime.Bootstrap
{
    public class DisposeManager : IDisposeManager
    {
        private readonly ConcurrentBag<IDisposable> _disposables = new();

        private int _disposeState;
        private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;

        public void AddDisposeValue(IDisposable disposable)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _disposables.Add(disposable);
        }
        public void DisposeResources()
        {
            while (_disposables.TryTake(out var d))
            {
                try { d.Dispose(); }
                catch { }
            }
        }

        public void Dispose()
        {
            // 최초 1개 스레드만 Dispose 로직 실행
            if (Interlocked.Exchange(ref _disposeState, 1) == 1)
                return;

            DisposeResources();
        }
    }
}
