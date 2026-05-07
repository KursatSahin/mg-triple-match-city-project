using TripleMatch.Data;

namespace TripleMatch.Core
{
    public struct ItemCollectedEvent : IEvent
    {
        public CollectibleItemData ItemData;
    }

    public struct MatchCompletedEvent : IEvent
    {
        public CollectibleItemData ItemData;
        public int Count;
    }

    public struct DeckFullEvent : IEvent { }
    public struct GoalCompletedEvent : IEvent { }
    public struct TimerExpiredEvent : IEvent { }
    public struct GameWonEvent : IEvent { }
    public struct GameFailedEvent : IEvent { }
    public struct MainMenuRequestedEvent : IEvent { }
    public struct GameSceneRequestedEvent : IEvent { }

    public struct GoalUpdatedEvent : IEvent
    {
        public CollectibleItemData ItemData;
        public int Current;
    }
}