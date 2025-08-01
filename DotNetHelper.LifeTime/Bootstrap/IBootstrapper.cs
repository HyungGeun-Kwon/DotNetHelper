namespace DotNetHelper.LifeTime.Bootstrap
{
    public interface IBootstrapper
    {
        BootstrapperInfo BootstrapperInfo { get; }
        void Initialize();
    }
}
