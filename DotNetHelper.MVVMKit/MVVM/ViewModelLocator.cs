using System.Collections.Concurrent;
using System.Windows;

namespace DotNetHelper.MVVMKit.MVVM
{
    public static class ViewModelLocator
    {
        private static readonly ConcurrentDictionary<Type, Type> _map = [];

        /// <summary>
        /// View와 ViewModel을 연결 등록
        /// </summary>
        public static void WireViewViewModel<TView, TViewModel>() where TView : FrameworkElement
        {
            _map.AddOrUpdate(typeof(TView), _ => typeof(TViewModel), (_, __) => typeof(TViewModel));
        }

        public static bool IsMapping(Type viewType) => _map.ContainsKey(viewType);

        /// <summary>
        /// 매핑된 ViewModel 타입을 옴.
        /// 매핑이 없으면 <see cref="InvalidOperationException"/>.
        /// </summary>
        public static Type GetViewModelTypeForView(Type viewType)
            => _map.TryGetValue(viewType, out Type? vm)
                ? vm
                : throw new InvalidOperationException($"No ViewModel registered for {viewType.FullName}");

        /// <summary>매핑 해제. 매핑이 없으면 <see langword="false"/>.</summary>
        public static bool Unwire<TView>() where TView : FrameworkElement
            => _map.TryRemove(typeof(TView), out _);
    }
}
