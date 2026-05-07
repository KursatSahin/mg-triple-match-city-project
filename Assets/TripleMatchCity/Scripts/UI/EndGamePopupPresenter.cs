using System;
using TripleMatch.Core;
using TripleMatch.Level;
using UnityEngine;
using VContainer.Unity;

namespace TripleMatch.UI
{
    /// <summary>
    /// failed = 0
    /// won, remaining time [0,20) = 1
    /// won, remaining time [20, 60) = 2
    /// won, remaining time [60,100] = 3
    /// </summary>
    public class EndGamePopupPresenter : IStartable, IDisposable
    {
        private const string DoneText = "Done";
        private const string FailedText = "Failed";

        private readonly EndGamePopupView _view;
        private readonly ITimerManager _timerManager;
        private readonly ILevelManager _levelManager;
        private readonly IEventBus _eventBus;

        private EventBinding<GameWonEvent> _wonBinding;
        private EventBinding<GameFailedEvent> _failedBinding;

        public EndGamePopupPresenter(EndGamePopupView view, ITimerManager timerManager, ILevelManager levelManager, IEventBus eventBus)
        {
            _view = view;
            _timerManager = timerManager;
            _levelManager = levelManager;
            _eventBus = eventBus;
        }

        public void Start()
        {
            _wonBinding = new EventBinding<GameWonEvent>(_ => Show(true));
            _eventBus.Subscribe(_wonBinding);

            _failedBinding = new EventBinding<GameFailedEvent>(_ => Show(false));
            _eventBus.Subscribe(_failedBinding);
        }

        public void Dispose()
        {
            if (_wonBinding != null)
            {
                _eventBus.Unsubscribe(_wonBinding);
                _wonBinding = null;
            }

            if (_failedBinding != null)
            {
                _eventBus.Unsubscribe(_failedBinding);
                _failedBinding = null;
            }
        }

        private void Show(bool isWon)
        {
            if (_view == null) return;

            string title = isWon ? DoneText : FailedText;
            int stars = CalculateStars(isWon);

            _view.Show(title, stars, OnReturnHomeRequested);
        }

        private int CalculateStars(bool isWon)
        {
            if (!isWon) return 0;

            var level = _levelManager?.CurrentLevel;
            float limit = level != null ? level.TimeLimitSeconds : 0f;
            
            if (limit <= 0f) return 3;

            float remaining = _timerManager != null ? _timerManager.TimeRemaining : 0f;
            float fraction = Mathf.Clamp01(remaining / limit);

            if (fraction < 0.2f) return 1;
            if (fraction < 0.6f) return 2;
            
            return 3;
        }

        private void OnReturnHomeRequested()
        {
            _eventBus.Raise(new MainMenuRequestedEvent());
        }
    }
}
