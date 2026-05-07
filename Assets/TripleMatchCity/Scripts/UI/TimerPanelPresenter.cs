using System;
using TripleMatch.Level;
using VContainer.Unity;

namespace TripleMatch.UI
{
    /// <summary>
    /// Binds the timer panel view to the timer manager.
    /// Listens to OnTimeChanged then update the view.
    /// </summary>
    public class TimerPanelPresenter : IStartable, IDisposable
    {
        private readonly ITimerManager _timerManager;
        private readonly TimerPanelView _view;

        public TimerPanelPresenter(ITimerManager timerManager, TimerPanelView view)
        {
            _timerManager = timerManager;
            _view = view;
        }

        public void Start()
        {
            if (_view == null || _timerManager == null) return;
            _view.SetTime(_timerManager.TimeRemaining);
            _timerManager.OnTimeChanged += OnTimeChanged;
        }

        public void Dispose()
        {
            if (_timerManager != null)
                _timerManager.OnTimeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(float seconds)
        {
            if (_view == null) return;
            _view.SetTime(seconds);
        }
    }
}
