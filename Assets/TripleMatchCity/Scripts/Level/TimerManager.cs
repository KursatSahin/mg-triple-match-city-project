using System;
using TripleMatch.Core;
using UnityEngine;
using VContainer.Unity;

namespace TripleMatch.Level
{
    /// <summary>
    /// Counts down the level time with ticks every frame via VContainer's ITickable
    /// </summary>
    public class TimerManager : ITimerManager, IStartable, ITickable, IDisposable
    {
        private readonly ILevelManager _levelManager;
        private readonly IEventBus _eventBus;

        private float _timeRemaining;
        private bool _isPaused;
        private bool _hasExpired;

        public float TimeRemaining => _timeRemaining;
        public bool IsRunning => !_isPaused && !_hasExpired;
        public bool HasExpired => _hasExpired;

        public event Action<float> OnTimeChanged;

        public TimerManager(ILevelManager levelManager, IEventBus eventBus)
        {
            _levelManager = levelManager;
            _eventBus = eventBus;
        }

        public void Start()
        {
            var level = _levelManager?.CurrentLevel;
            float limit = level != null ? level.TimeLimitSeconds : 0f;

            _timeRemaining = Mathf.Max(0f, limit);
            _isPaused = false;
            _hasExpired = _timeRemaining <= 0f;

            OnTimeChanged?.Invoke(_timeRemaining);

            if (_hasExpired) RaiseExpired();
        }

        public void Tick()
        {
            if (!IsRunning) return;

            _timeRemaining -= Time.deltaTime;
            
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _hasExpired = true;
                
                OnTimeChanged?.Invoke(_timeRemaining);
                
                RaiseExpired();
                
                return;
            }

            OnTimeChanged?.Invoke(_timeRemaining);
        }

        public void Pause()
        {
            if (_hasExpired) return;
            
            _isPaused = true;
        }

        public void Resume()
        {
            if (_hasExpired) return;
            
            _isPaused = false;
        }

        public void Dispose()
        {
            OnTimeChanged = null;
        }

        private void RaiseExpired()
        {
            _eventBus.Raise(new TimerExpiredEvent());
        }
    }
}
