using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit.RegionServices
{
    internal class RegionHost(ContentControl host)
    {
        private readonly WeakReference<ContentControl> _hostRef = new(host);
        internal IServiceScope? Scope { get; set; }
        internal FrameworkElement? CurrentView { get; set; }
        internal ContentControl? Host => _hostRef.TryGetTarget(out var h) ? h : null;
        internal void DeactivateAndDispose(bool clearHost = false)
        {
            try
            {
                if (CurrentView?.DataContext is IRegionViewModel vm)
                    vm.OnRegionDeactivated();

                if (clearHost && Host is ContentControl h)
                    h.Content = null;
            }
            finally
            {
                CurrentView = null;
                try { Scope?.Dispose(); } catch { }
                Scope = null;
            }
        }
    }
}
