using System.Windows;
using DotNetHelper.MVVMKit.DI;
using DotNetHelper.MVVMKit.Dialogs;
using DotNetHelper.MVVMKit.Event;
using DotNetHelper.MVVMKit.Modules;
using DotNetHelper.MVVMKit.Regions;

namespace DotNetHelper.MVVMKit.App
{
    public abstract class MVVMKitApplication : Application
    {
        public IContainerProvider Container { get; private set; } = null!;

        private IContainerRegistry _containerRegistry = null!;
        private IFrameworkContainerProvider _frameworkContainerProvider = null!;
        private bool _initialized;

        protected abstract Window CreateShell(IFrameworkContainerProvider frameworkContainerProvider);
        protected abstract void RegisterTypes(IContainerRegistry containerRegistry);
        protected abstract void AddModule(IModuleCatalog moduleCatalog);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            EnsureNotInitialized();
            Initialize();
            MainWindow?.Show();
        }
        private void Initialize()
        {
            ContainerDefaultSetting();

            RegisterTypes(_containerRegistry);

            RegisterModules();

            ConfigureShellWindow();

            _initialized = true;
        }
        
        private void ContainerDefaultSetting()
        {
            var container = new Container();
            Container = container;
            _containerRegistry = container;
            _frameworkContainerProvider = container;

            _containerRegistry.RegisterInstance<IDialogService>(new DialogService(container));
            _containerRegistry.RegisterInstance<IRegionManager>(new RegionManager(container));
            _containerRegistry.RegisterInstance<IContainerProvider>(container);
            _containerRegistry.RegisterInstance<IFrameworkContainerProvider>(container);

            _containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
        }
        private void RegisterModules()
        {
            IModuleCatalog moduleCatalog = new ModuleCatalog();
            AddModule(moduleCatalog);
            moduleCatalog.InitializeModules(_containerRegistry, Container);
        }
        private void ConfigureShellWindow()
        {
            var shell = CreateShell(_frameworkContainerProvider);
            if (shell is not null)
                MainWindow = shell;
        }

        private void EnsureNotInitialized()
        {
            if (_initialized)
                throw new InvalidOperationException("Application has already been initialized.");
        }
    }
}
