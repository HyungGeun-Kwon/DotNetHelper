using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.RegionServices
{
    public interface IRegionViewModel
    {
        void OnRegionActivated(Parameters? parameters);
        void OnRegionDeactivated();
    }
}
