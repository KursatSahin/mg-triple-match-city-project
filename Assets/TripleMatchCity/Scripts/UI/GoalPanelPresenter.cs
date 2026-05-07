using System;
using TripleMatch.Core;
using TripleMatch.Level;
using VContainer.Unity;

namespace TripleMatch.UI
{
    /// <summary>
    /// Binds the goal panel view to the goal manager.
    /// Sets the slots from the current goals then update them with GoalUpdatedEvent
    /// </summary>
    public class GoalPanelPresenter : IStartable, IDisposable
    {
        private readonly IGoalManager _goalManager;
        private readonly GoalPanelView _view;
        private readonly IEventBus _eventBus;

        private EventBinding<GoalUpdatedEvent> _goalUpdatedBinding;

        public GoalPanelPresenter(IGoalManager goalManager, GoalPanelView view, IEventBus eventBus)
        {
            _goalManager = goalManager;
            _view = view;
            _eventBus = eventBus;
        }

        public void Start()
        {
            if (_view == null) return;

            _view.Build(_goalManager.Goals);

            _goalUpdatedBinding = new EventBinding<GoalUpdatedEvent>(OnGoalUpdated);
            _eventBus.Subscribe(_goalUpdatedBinding);
        }

        public void Dispose()
        {
            if (_goalUpdatedBinding != null)
            {
                _eventBus.Unsubscribe(_goalUpdatedBinding);
                _goalUpdatedBinding = null;
            }
        }

        private void OnGoalUpdated(GoalUpdatedEvent evt)
        {
            if (_view == null) return;
            _view.UpdateGoal(evt.ItemData, evt.Current);
        }
    }
}
