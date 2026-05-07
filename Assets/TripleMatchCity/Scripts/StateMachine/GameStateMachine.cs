using System;
using TripleMatch.Core;
using TripleMatch.Level;
using VContainer.Unity;

namespace TripleMatch.StateMachine
{
    /// <summary>
    /// Actually thats not a state machine, but its simple approach to define stuffs as state
    /// and react to events to transit between states
    /// </summary>
    
    // TODO: If the game has more complex states and transitions, conditions and predications, we should implement a more formal state machine pattern.
    public class GameStateMachine : IGameStateMachine, IStartable, IDisposable
    {
        private readonly ITimerManager _timerManager;

        private EventBinding<GoalCompletedEvent> _goalBinding;
        private EventBinding<TimerExpiredEvent> _timerBinding;
        private EventBinding<DeckFullEvent> _deckFullBinding;

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public bool IsPlaying => CurrentState == GameState.Playing;

        public GameStateMachine(ITimerManager timerManager)
        {
            _timerManager = timerManager;
        }

        public void Start()
        {
            CurrentState = GameState.Playing;

            _goalBinding = new EventBinding<GoalCompletedEvent>(_ => TransitionTo(GameState.Won));
            EventBus<GoalCompletedEvent>.Register(_goalBinding);

            _timerBinding = new EventBinding<TimerExpiredEvent>(_ => TransitionTo(GameState.Failed));
            EventBus<TimerExpiredEvent>.Register(_timerBinding);

            _deckFullBinding = new EventBinding<DeckFullEvent>(_ => TransitionTo(GameState.Failed));
            EventBus<DeckFullEvent>.Register(_deckFullBinding);
        }

        public void Dispose()
        {
            if (_goalBinding != null)
            {
                EventBus<GoalCompletedEvent>.Deregister(_goalBinding);
                _goalBinding = null;
            }
            if (_timerBinding != null)
            {
                EventBus<TimerExpiredEvent>.Deregister(_timerBinding);
                _timerBinding = null;
            }
            if (_deckFullBinding != null)
            {
                EventBus<DeckFullEvent>.Deregister(_deckFullBinding);
                _deckFullBinding = null;
            }
        }

        private void TransitionTo(GameState next)
        {
            if (CurrentState != GameState.Playing) return;

            CurrentState = next;
            _timerManager?.Pause();

            if (next == GameState.Won)
                EventBus<GameWonEvent>.Raise(new GameWonEvent());
            else if (next == GameState.Failed)
                EventBus<GameFailedEvent>.Raise(new GameFailedEvent());
        }
    }
}
