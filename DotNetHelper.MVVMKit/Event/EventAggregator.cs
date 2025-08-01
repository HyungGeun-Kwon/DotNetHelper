using System.Collections.Concurrent;

namespace DotNetHelper.MVVMKit.Event
{
    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, EventBase> _events = new();

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            if (!_events.TryGetValue(typeof(TEventType), out var existingEvent))
            {
                var newEvent = new TEventType();
                _events.TryAdd(typeof(TEventType), newEvent);
                return newEvent;
            }
            return (TEventType)existingEvent;
        }
    }
}
