using System;

namespace CvsSIService.LifeTime.Bootstrap
{
    public interface IDisposeManager : IDisposable
    {
        void AddDisposeValue(IDisposable disposable);
        void DisposeResources();
    }
}
