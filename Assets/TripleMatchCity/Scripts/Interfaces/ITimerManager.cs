using System;

namespace TripleMatch.Level
{
    public interface ITimerManager
    {
        float TimeRemaining { get; }
        bool IsRunning { get; }
        bool HasExpired { get; }

        event Action<float> OnTimeChanged;

        void Pause();
        void Resume();
    }
}
