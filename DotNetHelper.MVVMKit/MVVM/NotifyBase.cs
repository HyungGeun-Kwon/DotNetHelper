using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotNetHelper.MVVMKit.MVVM
{
    public abstract partial class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _argCache = new();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is null) return;
            var args = _argCache.GetOrAdd(propertyName, static n => new PropertyChangedEventArgs(n));
            PropertyChanged.Invoke(this, args);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
