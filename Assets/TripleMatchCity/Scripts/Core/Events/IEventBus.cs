namespace TripleMatch.Core
{
    /// <summary>
    /// DI-injectable event bus. Replaces the previous static EventBus pattern so that
    /// subscribers can be injected, mocked in tests, and bound to a scope's lifetime.
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<T>(EventBinding<T> binding) where T : IEvent;
        void Unsubscribe<T>(EventBinding<T> binding) where T : IEvent;
        void Raise<T>(T evt) where T : IEvent;
    }
}