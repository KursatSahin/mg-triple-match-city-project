using UnityEngine;

namespace TripleMatch.Core.Data
{
    /// <summary>
    /// Centralized manager for game data persistence. Holds the single GameSaveData instance,
    /// (de)serializes via JsonUtility, and writes to whatever IDataProvider was passed in.
    /// Dirty tracking avoids redundant writes.
    /// </summary>
    public sealed class DataManager : IDataManager
    {
        private const string SaveKey = "tmc_game_save";

        private IDataProvider _provider;
        private bool _isDirty;

        public GameSaveData SaveData { get; private set; } = new GameSaveData();
        public bool IsLoaded { get; private set; }

        public void Initialize(IDataProvider provider)
        {
            _provider = provider;
        }

        public void Load()
        {
            if (_provider == null)
            {
                Debug.LogError("[DataManager] Provider not initialized.");
                IsLoaded = false;
                return;
            }

            string json = _provider.Load(SaveKey);
            if (string.IsNullOrEmpty(json))
            {
                SaveData = new GameSaveData();
                Debug.Log("[DataManager] No saved data found - using defaults.");
            }
            else
            {
                try
                {
                    SaveData = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
                }
                catch
                {
                    Debug.LogError("[DataManager] Failed to parse save data. Resetting to defaults.");
                    SaveData = new GameSaveData();
                }
            }

            _isDirty = false;
            IsLoaded = true;
        }

        public void Save()
        {
            if (_provider == null) return;
            if (!_isDirty) return;

            string json = JsonUtility.ToJson(SaveData);
            _provider.Save(SaveKey, json);
            _isDirty = false;
        }

        public void MarkDirty(bool forcePersist = false)
        {
            _isDirty = true;
            if (forcePersist) Save();
        }

        public void ResetToDefaults()
        {
            SaveData = new GameSaveData();
            _isDirty = true;
            Save();
        }
    }
}
