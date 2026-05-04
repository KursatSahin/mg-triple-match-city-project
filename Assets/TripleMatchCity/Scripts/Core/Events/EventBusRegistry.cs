using System;
using System.Collections.Generic;

namespace TripleMatch.Core
{
    public static class EventBusRegistry
    {
        static readonly Dictionary<Type, Action> _clearActions = new();

        public static void TrackBus(Type eventType, Action clearAction)
        {
            _clearActions.TryAdd(eventType, clearAction);
        }

        public static void ClearAll()
        {
            foreach (var clear in _clearActions.Values)
                clear();
        }
    }
}