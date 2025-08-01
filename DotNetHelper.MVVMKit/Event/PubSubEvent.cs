namespace DotNetHelper.MVVMKit.Event
{
    public class PubSubEvent<T> : EventBase
    {
        private readonly List<Action<T>> _handlers = [];
        private readonly Lock _lockObj = new();
        public void Publish(T parameter)
        {
            Action<T>[] snapshot;
            lock (_lockObj) snapshot = _handlers.ToArray();
            foreach (var handler in snapshot)
            {
                handler?.Invoke(parameter);
            }
        }
        public void Subscribe(Action<T> action)
        {
            lock (_lockObj) _handlers.Add(action);
        }
        public void Unsubscribe(Action<T> action)
        {
            if (action == null) return;
            lock (_lockObj) _handlers.Remove(action);
        }
    }
    public class PubSubEvent : EventBase
    {
        private readonly List<Action> _handlers = [];
        private readonly Lock _lock = new();
        public void Publish()
        {
            Action[] snapshot;
            lock (_lock) snapshot = _handlers.ToArray();
            foreach (var handler in snapshot)
            {
                handler?.Invoke();
            }
        }
        public void Subscribe(Action action)
        {
            lock (_lock) _handlers.Add(action);
        }
        public void Unsubscribe(Action action)
        {
            if (action == null) return;
            lock (_lock) _handlers.Remove(action);
        }
    }
}
