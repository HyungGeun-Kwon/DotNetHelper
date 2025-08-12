using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using DotNetHelper.MsDiKit.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit.RegionServices
{
    public class RegionService : IRegionService
    {
        private readonly IServiceProvider _rootSp;

        private readonly ConcurrentDictionary<string, (Type viewType, Type? viewModelType)> _map = [];
        private readonly ConcurrentDictionary<string, RegionHost> _regions = [];

        public RegionService(IServiceProvider rootSp, IEnumerable<IRegionViewDescriptor> descriptors)
        {
            _rootSp = rootSp;
            foreach (var d in descriptors)
            {
                if (!_map.TryAdd(d.Key, (d.View, d.ViewModel)))
                    throw new InvalidOperationException($"Duplicate region key '{d.Key}' from {d.View.FullName}.");
            }
        }

        public void RegisterRegionName(string regionName, ContentControl regionControl)
        {
            var regionHost = new RegionHost(regionControl);
            if (!_regions.TryAdd(regionName, regionHost))
                throw new InvalidOperationException($"Region '{regionName}' already registered.");

            regionControl.Unloaded += OnHostUnloaded;
            void OnHostUnloaded(object? _, RoutedEventArgs __)
            {
                regionControl.Unloaded -= OnHostUnloaded;
                CleanupRegion(regionName);
            }
        }

        public void UnregisterRegionName(string regionName) => CleanupRegion(regionName);

        public void AddRegionView<TView, TRegionViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TRegionViewModel : class, IRegionViewModel
            => AddRegionView(GetViewKey<TView>(viewKey), typeof(TView), typeof(TRegionViewModel));

        public void AddRegionView<TView>(string viewKey = "")
            where TView : FrameworkElement
            => AddRegionView(GetViewKey<TView>(viewKey), typeof(TView));

        private void AddRegionView(string viewKey, Type view, Type? viewModel = null)
        {
            if (!_map.TryAdd(viewKey, (view, viewModel)))
                throw new InvalidOperationException($"Duplicate RegionView key: '{viewKey}'");
        }

        public void AddOrUpdateRegionView<TView, TRegionViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TRegionViewModel : class, IRegionViewModel
            => AddOrUpdateRegionView(GetViewKey<TView>(viewKey), typeof(TView), typeof(TRegionViewModel));

        public void AddOrUpdateRegionView<TView>(string viewKey = "")
            where TView : FrameworkElement
            => AddOrUpdateRegionView(GetViewKey<TView>(viewKey), typeof(TView));

        private void AddOrUpdateRegionView(string viewKey, Type view, Type? viewModel = null)
            => _map.AddOrUpdate(viewKey, (view, viewModel), (_, _) => (view, viewModel));

        public bool TryRemoveRegionView(string viewKey = "") => _map.TryRemove(viewKey, out _);
        public bool TryRemoveRegionView<TView>() where TView : FrameworkElement => TryRemoveRegionView(typeof(TView).Name);

        private string GetViewKey<TView>(string viewKey) => string.IsNullOrEmpty(viewKey) ? typeof(TView).Name : viewKey;

        public void SetView(string regionName, string viewKey, Parameters? parameters = null)
        {
            RunOnUiThread(() =>
            {
                if (!_regions.TryGetValue(regionName, out var regionHost))
                    throw new KeyNotFoundException($"Region '{regionName}' not found. RegisterHost first.");
                if (!_map.TryGetValue(viewKey, out var entry))
                    throw new KeyNotFoundException($"View '{viewKey}' is not registered. AddView first.");

                // 이전 View/Scope 정리
                regionHost.DeactivateAndDispose();

                // Host 상태 먼저 확인(없거나 아직 로드 안 됐으면 그냥 종료)
                var host = regionHost.Host;
                if (host is null || !host.IsLoaded)
                    return;

                var newScope = _rootSp.CreateScope();
                try
                {
                    var sp = newScope.ServiceProvider;
                    var ksp = sp as IKeyedServiceProvider;

                    object? viewObj =
                        ksp?.GetKeyedService(entry.viewType, viewKey) ??
                        sp.GetService(entry.viewType);

                    if (viewObj is not FrameworkElement fe)
                        throw new InvalidOperationException(
                            $"Resolved view '{viewKey}' is not a FrameworkElement. Actual: {viewObj?.GetType().FullName ?? "null"}");

                    // VM: 키 우선 → 일반 폴백 (DataContext 비었을 때만)
                    if (entry.viewModelType is not null && fe.DataContext is null)
                    {
                        object? vmObj =
                            ksp?.GetKeyedService(entry.viewModelType, viewKey) ??
                            sp.GetService(entry.viewModelType);

                        if (vmObj is null)
                            throw new InvalidOperationException(
                                $"ViewModel '{entry.viewModelType.FullName}' not found (keyed '{viewKey}' or unkeyed).");

                        fe.DataContext = vmObj;
                    }

                    // 성공 시 regionHost에 반영
                    host.Content = fe;
                    regionHost.CurrentView = fe;
                    regionHost.Scope = newScope; // 소유권 이전

                    (fe.DataContext as IRegionViewModel)?.OnRegionActivated(parameters);
                }
                finally
                {
                    newScope?.Dispose();
                }
            });
        }

        private void CleanupRegion(string regionName)
        {
            if (_regions.TryRemove(regionName, out var regionHost))
                regionHost.DeactivateAndDispose(true);
        }

        private static void RunOnUiThread(Action action)
        {
            var d = Application.Current?.Dispatcher;
            if (d is null) { action(); return; }
            if (d.CheckAccess()) action(); else d.Invoke(action);
        }
    }
}
