using TripleMatch.Data;

namespace TripleMatch.Core
{
    public struct ItemCollectedEvent : IEvent
    {
        public CollectableItemData ItemData;
    }

    public struct MatchCompletedEvent : IEvent
    {
        public CollectableItemData ItemData;
        public int Count;
    }

    public struct DeckFullEvent : IEvent { }
    public struct GoalCompletedEvent : IEvent { }
    public struct TimerExpiredEvent : IEvent { }

    public struct GoalUpdatedEvent : IEvent
    {
        public CollectableItemData ItemData;
        public int Current;
    }
}