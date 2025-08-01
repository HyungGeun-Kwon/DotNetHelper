using System.Collections.Concurrent;

namespace DotNetHelper.MVVMKit.DI
{
    public sealed partial class Container : IContainerProvider, IContainerRegistry, IFrameworkContainerProvider
    {
        // 타입별 생성자 함수 등록
        private readonly ConcurrentDictionary<Type, Func<object>> _transients = [];
        // 싱글톤 인스턴스 보관소 (한 번만 생성된 객체 저장)
        private readonly ConcurrentDictionary<Type, Lazy<object>> _singletons = [];

        // string key 기반 타입별 생성자 함수 등록
        private readonly ConcurrentDictionary<(Type, string), Func<object>> _namedTransients = [];
        // strign key 기반 싱글톤 인스턴스 보관소 (한 번만 생성된 객체 저장)
        private readonly ConcurrentDictionary<(Type, string), Lazy<object>> _namedSingletons = [];

        // RegionManager에서 사용하는 NavigationView 매핑
        private readonly ConcurrentDictionary<string, Type> _navigationViews = [];
        // DialogService에서 사용하는 DialogView 매핑
        private readonly ConcurrentDictionary<string, Type> _dialogViews = [];

        private int _disposeState;
        private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;

        private Lazy<object> WrapLazySingleton(Type impl) => new(() => CreateWithConstructor(impl), LazyThreadSafetyMode.ExecutionAndPublication);
        private Lazy<object> WrapLazyInstance(object obj) => new(() => obj, LazyThreadSafetyMode.ExecutionAndPublication);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposeState, 1) == 1) return;

            foreach (var lazy in _singletons.Values)
                if (lazy.IsValueCreated && lazy.Value is IDisposable d) d.Dispose();

            foreach (var lazy in _namedSingletons.Values)
                if (lazy.IsValueCreated && lazy.Value is IDisposable d) d.Dispose();

            _singletons.Clear();
            _namedSingletons.Clear();
            _transients.Clear();
            _namedTransients.Clear();
            _navigationViews.Clear();
            _dialogViews.Clear();
        }
    }
}
