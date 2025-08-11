using System.Collections.Concurrent;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit
{
    public interface IRegionViewModel
    {
    }
    public class RegionService(IServiceProvider rootSp)
    {
        private readonly ConcurrentDictionary<string, (Type viewType, Type viewModelType)> _map = [];
        private readonly IServiceProvider _rootSp = rootSp;

        public void AddRegion<TView, TViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TViewModel : class
        {

        }
    }

    

   


}
