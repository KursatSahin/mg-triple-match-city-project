using System;
using System.Collections.Generic;

namespace TripleMatch.Core
{
    /// <summary>
    /// DI-injectable event bus. Replaces the previous static EventBus pattern so that
    /// subscribers can be injected, mocked in tests, and bound to a scope's lifetime.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, object> _bindingsByType = new();

        public void Subscribe<T>(EventBinding<T> binding) where T : IEvent
        {
            if (binding == null) return;
            GetOrCreateSet<T>().Add(binding);
        }

        public void Unsubscribe<T>(EventBinding<T> binding) where T : IEvent
        {
            if (binding == null) return;
            if (_bindingsByType.TryGetValue(typeof(T), out var existing))
                ((HashSet<IEventBinding<T>>)existing).Remove(binding);
        }

        public void Raise<T>(T evt) where T : IEvent
        {
            if (!_bindingsByType.TryGetValue(typeof(T), out var existing)) return;

            var bindings = (HashSet<IEventBinding<T>>)existing;
            // Snapshot so handlers can subscribe / unsubscribe mid-dispatch without breaking iteration.
            var snapshot = new HashSet<IEventBinding<T>>(bindings);
            foreach (var binding in snapshot)
            {
                if (!bindings.Contains(binding)) continue;
                binding.OnEvent.Invoke(evt);
                binding.OnEventNoArgs.Invoke();
            }
        }

        private HashSet<IEventBinding<T>> GetOrCreateSet<T>() where T : IEvent
        {
            if (_bindingsByType.TryGetValue(typeof(T), out var existing))
                return (HashSet<IEventBinding<T>>)existing;

            var set = new HashSet<IEventBinding<T>>();
            _bindingsByType[typeof(T)] = set;
            return set;
        }
    }
}
