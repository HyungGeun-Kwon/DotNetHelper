using System.Windows;
using System.Windows.Controls;
using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.RegionServices
{
    public interface IRegionService
    {
        void RegisterRegionName(string regionName, ContentControl regionControl);
        void UnregisterRegionName(string regionName);

        void AddRegionView<TView, TRegionViewModel>(string viewKey = "") where TView : FrameworkElement where TRegionViewModel : class, IRegionViewModel;
        void AddRegionView<TView>(string viewKey = "") where TView : FrameworkElement;

        void AddOrUpdateRegionView<TView, TRegionViewModel>(string viewKey = "") where TView : FrameworkElement where TRegionViewModel : class, IRegionViewModel;
        void AddOrUpdateRegionView<TView>(string viewKey = "") where TView : FrameworkElement;

        bool TryRemoveRegionView(string viewKey = "");
        bool TryRemoveRegionView<TView>() where TView : FrameworkElement;

        void SetView(string regionName, string viewKey, Parameters? parameters = null);
    }
}
