namespace DotNetHelper.LifeTime.Bootstrap
{
    public interface IDisposeManager : IDisposable
    {
        void AddDisposeValue(IDisposable disposable);
        void DisposeResources();
    }
}
