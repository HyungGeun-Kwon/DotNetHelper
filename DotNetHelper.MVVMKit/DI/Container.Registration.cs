namespace DotNetHelper.MVVMKit.DI
{
    public sealed partial class Container
    {
        public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface
            => RegisterSingleton(typeof(TInterface), typeof(TImplementation));
        public void RegisterSingleton<TImplementation>() where TImplementation : class
            => RegisterSingleton<TImplementation, TImplementation>();
        public void RegisterSingleton<TInterface, TImplementation>(string key) where TImplementation : class, TInterface
            => RegisterSingletonNamed(key, typeof(TInterface), typeof(TImplementation));
        public void RegisterSingleton<TImplementation>(string key) where TImplementation : class
            => RegisterSingletonNamed(key, typeof(TImplementation), typeof(TImplementation));
        public void RegisterInstance<T>(T instance) where T : class
            => RegisterInstance(typeof(T), instance);
        public void RegisterInstance<T>(T instance, string key) where T : class
            => RegisterInstanceNamed(key, typeof(T), instance);
        public void RegisterTransient<TInterface, TImplementation>() where TImplementation : class, TInterface
            => RegisterTransient(typeof(TInterface), typeof(TImplementation));
        public void RegisterTransient<T>() where T : class
            => RegisterTransient<T, T>();
        public void RegisterTransient<TInterface, TImplementation>(string key) where TImplementation : class, TInterface
            => RegisterTransientNamed(key, typeof(TInterface), typeof(TImplementation));
        public void RegisterTransient<T>(string key) where T : class
            => RegisterTransientNamed(key, typeof(T), typeof(T));

        private void RegisterSingleton(Type service, Type implementation)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _singletons.AddOrUpdate(service, _ => WrapLazySingleton(implementation), (_, old) => ReplaceLazy(old, WrapLazySingleton(implementation)));
        }
        private void RegisterSingletonNamed(string key, Type service, Type implementation)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _namedSingletons.AddOrUpdate((service, key), _ => WrapLazySingleton(implementation), (_, old) => ReplaceLazy(old, WrapLazySingleton(implementation)));
        }
        private void RegisterInstance(Type service, object instance)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _singletons.AddOrUpdate(service, _ => WrapLazyInstance(instance), (_, old) => ReplaceLazy(old, WrapLazyInstance(instance)));
        }
        private void RegisterInstanceNamed(string key, Type service, object instance)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _namedSingletons.AddOrUpdate((service, key), _ => WrapLazyInstance(instance), (_, old) => ReplaceLazy(old, WrapLazyInstance(instance)));
        }
        private void RegisterTransient(Type service, Type implementation)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _transients.AddOrUpdate(service, _ => () => CreateWithConstructor(implementation), (_, __) => () => CreateWithConstructor(implementation));
        }
        private void RegisterTransientNamed(string key, Type service, Type implementation)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _namedTransients.AddOrUpdate((service, key), _ => () => CreateWithConstructor(implementation), (_, __) => () => CreateWithConstructor(implementation));
        }

        private Lazy<object> ReplaceLazy(Lazy<object> oldValue, Lazy<object> newValue)
        {
            if (oldValue.IsValueCreated && oldValue.Value is IDisposable d) d.Dispose();
            return newValue;
        }
    }
}
