using System.Collections.Generic;

namespace TripleMatch.Core
{
    public static class EventBus<T> where T : IEvent
    {
        static readonly HashSet<IEventBinding<T>> bindings = new();
        static bool tracked;

        public static void Register(EventBinding<T> binding)
        {
            bindings.Add(binding);
            if (!tracked)
            {
                EventBusRegistry.TrackBus(typeof(T), Clear);
                tracked = true;
            }
        }

        public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

        public static void Raise(T @event)
        {
            var snapshot = new HashSet<IEventBinding<T>>(bindings);
            foreach (var binding in snapshot)
            {
                if (bindings.Contains(binding))
                {
                    binding.OnEvent.Invoke(@event);
                    binding.OnEventNoArgs.Invoke();
                }
            }
        }

        public static void Clear()
        {
            bindings.Clear();
        }
    }
}