using DotNetHelper.MVVMKit.DI;

namespace DotNetHelper.MVVMKit.Modules
{
    public interface IModule
    {
        void OnInitialized(IContainerProvider containerProvider);
        void RegisterTypes(IContainerRegistry containerRegistry);
    }
}
