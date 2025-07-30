namespace CvsSIService.LifeTime.Bootstrap
{
    public interface IBootstrapper
    {
        BootstrapperInfo BootstrapperInfo { get; }
        void Initialize();
    }
}
