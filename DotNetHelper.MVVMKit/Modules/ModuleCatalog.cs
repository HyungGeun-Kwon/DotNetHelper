using DotNetHelper.MVVMKit.DI;

namespace DotNetHelper.MVVMKit.Modules
{
    public class ModuleCatalog : IModuleCatalog
    {
        private readonly List<IModule> _modules = [];
        private readonly Lock _lock = new();

        public void AddModule<T>() where T : IModule, new()
        {
            lock(_lock) _modules.Add(new T());
        }

        public void InitializeModules(IContainerRegistry registry, IContainerProvider provider)
        {
            IModule[] snapshot;
            lock (_lock) snapshot = _modules.ToArray();

            foreach (var module in snapshot)
            {
                module.RegisterTypes(registry);
            }

            foreach (var module in snapshot)
            {
                module.OnInitialized(provider);
            }
        }
    }
}
