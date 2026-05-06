using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Level
{
    /// <summary>
    /// Project scope level management service.
    /// Owns level progression states
    ///     - current level,
    ///     - last completed level
    ///     - loop management
    ///     - TODO: Provide persistence with PlayerPrefs and win event handling.
    /// </summary>
    public class LevelManager : ILevelManager
    {
        private readonly LevelContainerSO _levelContainer;
        private readonly LevelManagerConfigSO _config;

        private int _currentDisplayIndex = 1;
        private int _lastCompletedDisplayIndex = 0; // If zero it means player have completed no level

        public int CurrentDisplayIndex => _currentDisplayIndex;
        public int LastCompletedDisplayIndex => _lastCompletedDisplayIndex;

        public int CurrentActualIndex => CalculateActualIndex(_currentDisplayIndex);

        public LevelDataSO CurrentLevel
        {
            get
            {
                if (_levelContainer == null || _levelContainer.Levels == null) return null;
                int index = CurrentActualIndex;
                if (index < 0 || index >= _levelContainer.Levels.Count) return null;
                return _levelContainer.Levels[index];
            }
        }

        public LevelManager(LevelContainerSO levelContainer, LevelManagerConfigSO config)
        {
            _levelContainer = levelContainer;
            _config = config;
        }

        public void LoadLevel(int displayIndex)
        {
            if (displayIndex < 1)
            {
                Debug.LogWarning($"[LevelManager] Display index must be >= 1. Got {displayIndex}.");
                return;
            }
            _currentDisplayIndex = displayIndex;
        }

        public void OnLevelCompleted()
        {
            _lastCompletedDisplayIndex = _currentDisplayIndex;
            _currentDisplayIndex++;
        }

        // formula: actual = loopStart + (display - loopStart) % (total - loopStart).
        // display index is starting from 1
        // actual index is starting from 0
        private int CalculateActualIndex(int displayIndex)
        {
            if (_levelContainer == null || _levelContainer.Levels == null) return -1;
            
            int total = _levelContainer.Levels.Count;
            
            if (total == 0) return -1;

            int displayZeroBased = displayIndex - 1;
            int loopStart = _config != null ? Mathf.Max(0, _config.LoopStartIndex - 1) : 0;

            if (displayZeroBased < loopStart) return Mathf.Clamp(displayZeroBased, 0, total - 1);
            
            if (loopStart >= total) return total - 1;

            int loopRange = total - loopStart;
            
            return loopStart + ((displayZeroBased - loopStart) % loopRange);
        }
    }
}
