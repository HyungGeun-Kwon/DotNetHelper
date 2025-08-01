using DotNetHelper.MVVMKit.DI;

namespace DotNetHelper.MVVMKit.Modules
{
    public interface IModuleCatalog
    {
        void AddModule<T>() where T : IModule, new();
        void InitializeModules(IContainerRegistry registry, IContainerProvider provider);
    }
}
