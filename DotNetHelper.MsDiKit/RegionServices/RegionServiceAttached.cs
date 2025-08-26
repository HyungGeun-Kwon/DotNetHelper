using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit.RegionServices
{
    public static class RegionServiceAttached
    {
        private static Func<IServiceProvider>? _getServiceProvider;

        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached(
                "RegionName",
                typeof(string),
                typeof(RegionServiceAttached),
                new PropertyMetadata(null, OnRegionNameChanged));

        public static void Configure(Func<IServiceProvider> getServiceProvider) => _getServiceProvider = getServiceProvider;

        private static IRegionService GetRegionService()
        {
            var sp = _getServiceProvider?.Invoke() ?? throw new InvalidOperationException("ServiceProvider not configured.");
            return sp.GetRequiredService<IRegionService>();
        }

        public static IRegionService? TryGetRegionService()
        {
            var sp = _getServiceProvider?.Invoke();
            return sp?.GetService<IRegionService>();
        }

        public static void SetRegionName(DependencyObject element, string value) =>
            element.SetValue(RegionNameProperty, value);

        public static string GetRegionName(DependencyObject element) =>
            (string)element.GetValue(RegionNameProperty);

        private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (d is not ContentControl host)
                return;

            // 이전 이름 해제
            if (e.OldValue is string oldName && !string.IsNullOrWhiteSpace(oldName))
                TryUnregister(oldName);

            // 새 이름 등록
            if (e.NewValue is string newName && !string.IsNullOrWhiteSpace(newName))
                RegisterWhenLoaded(newName, host);
        }

        private static void RegisterWhenLoaded(string regionName, ContentControl host)
        {
            Register(regionName, host);

            // Host가 내려가면 정리
            host.Unloaded += OnHostUnloaded;

            void OnHostUnloaded(object? s, RoutedEventArgs e)
            {
                host.Unloaded -= OnHostUnloaded;
                TryUnregister(regionName);
            }
        }

        private static void Register(string regionName, ContentControl host)
        {
            IRegionService svc = GetRegionService();
            svc.RegisterRegionName(regionName, host);
        }

        private static void TryUnregister(string regionName)
        {
            // ServiceProvider가 아직 안 준비된 초기 시점 등을 고려해 Try 패턴
            IRegionService? svc = TryGetRegionService();
            svc?.UnregisterRegionName(regionName);
        }
    }
}
