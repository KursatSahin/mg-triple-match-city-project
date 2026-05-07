using TripleMatch.Core;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Level
{
    /// <summary>
    /// Project scope level management service.
    /// Owns level progression and persists it through IDataManager:
    ///   - current level
    ///   - last completed level
    ///   - loop management
    /// </summary>
    public class LevelManager : ILevelManager
    {
        private readonly LevelContainerSO _levelContainer;
        private readonly LevelManagerConfigSO _config;
        private readonly IDataManager _dataManager;

        public int CurrentDisplayIndex => _dataManager.SaveData.CurrentDisplayIndex;
        public int LastCompletedDisplayIndex => _dataManager.SaveData.LastCompletedDisplayIndex;
        public int CurrentActualIndex => CalculateActualIndex(CurrentDisplayIndex);

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

        public LevelManager(LevelContainerSO levelContainer, LevelManagerConfigSO config, IDataManager dataManager)
        {
            _levelContainer = levelContainer;
            _config = config;
            _dataManager = dataManager;
        }

        public void LoadLevel(int displayIndex)
        {
            if (displayIndex < 1)
            {
                Debug.LogWarning($"[LevelManager] Display index must be >= 1. Got {displayIndex}.");
                return;
            }

            _dataManager.SaveData.CurrentDisplayIndex = displayIndex;
            _dataManager.MarkDirty(forcePersist: true);
        }

        public void OnLevelCompleted()
        {
            var data = _dataManager.SaveData;
            data.LastCompletedDisplayIndex = data.CurrentDisplayIndex;
            data.CurrentDisplayIndex++;
            _dataManager.MarkDirty(forcePersist: true);
        }

        public void ResetProgress()
        {
            _dataManager.ResetToDefaults();
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
