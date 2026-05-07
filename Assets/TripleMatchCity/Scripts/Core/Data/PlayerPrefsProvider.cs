using UnityEngine;

namespace TripleMatch.Core.Data
{
    /// <summary>
    /// IDataProvider backed by UnityEngine.PlayerPrefs. Stores all data as a single JSON
    /// string per key.
    /// </summary>
    public sealed class PlayerPrefsProvider : IDataProvider
    {
        public string Load(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return null;
            return PlayerPrefs.GetString(key);
        }

        public void Save(string key, string jsonData)
        {
            PlayerPrefs.SetString(key, jsonData);
            PlayerPrefs.Save();
        }

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }
}
