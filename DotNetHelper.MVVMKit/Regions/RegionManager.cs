using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using DotNetHelper.MVVMKit.DI;
using DotNetHelper.MVVMKit.MVVM;

namespace DotNetHelper.MVVMKit.Regions
{
    public class RegionManager(Container container) : IRegionManager
    {
        private readonly ConcurrentDictionary<string, ContentControl> _regions = [];
        private readonly Container _container = container;

        /// <summary>
        /// XAML 코드비하인드에서 Region 이름과 <see cref="ContentControl"/> 을 등록.
        /// 동일 이름이 이미 있으면 새 컨트롤로 교체.
        /// </summary>
        internal void RegisterRegionName(string regionName, ContentControl regionControl)
            => _regions.AddOrUpdate(regionName, regionControl, (_, _) => regionControl);
        
        public void RequestNavigate(string regionName, string viewKey)
            => RequestNavigate(regionName, _container.GetNavigationViewType(viewKey));

        /// <summary> 지정 Region에 ViewType을 로드하여 보여줌. </summary>
        public void RequestNavigate(string regionName, Type viewType)
        {
            if (!_regions.TryGetValue(regionName, out var target))
                throw new ArgumentException($"Region '{regionName}' not found.", nameof(regionName));


            // 현재 View/ViewModel이 INavigationAware면 OnNavigatedFrom 호출
            if (target.Content is FrameworkElement currentElement)
            {
                if (currentElement.DataContext != null && currentElement.DataContext is INavigationAware currentAware)
                {
                    currentAware.OnNavigatedFrom();
                }
            }

            if (_container.Resolve(viewType) is not FrameworkElement view)
            {
                throw new InvalidOperationException($"Resolved view '{viewType.FullName}' is not a FrameworkElement.");
            }

            if (ViewModelLocator.IsMapping(viewType))
            {
                Type viewModelType = ViewModelLocator.GetViewModelTypeForView(viewType);
                object viewModel = _container.Resolve(viewModelType);
                view.DataContext = viewModel;

                if (viewModel is INavigationAware nowAware)
                    nowAware.OnNavigatedTo();
            }

            target.Content = view;
        }
    }
}
