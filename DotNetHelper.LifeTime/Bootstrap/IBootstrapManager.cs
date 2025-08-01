namespace DotNetHelper.LifeTime.Bootstrap
{
    public interface IBootstrapManager
    {
        void AddBootstrap(string key, IBootstrapper bootstrapper);
        void RemoveBootstrap(string key);
        Task InitializeAllAsync(bool stopOnError = false, CancellationToken cancellationToken = default);
        void InitBootstrapper(string key);
        void StopInitialize();
    }
}
