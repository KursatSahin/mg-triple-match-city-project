namespace TripleMatch.Core.Data
{
    /// <summary>
    /// DataManager handles serialization; provider only reads/writes raw strings.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>Loads the JSON string for the given key, or null if missing.</summary>
        string Load(string key);

        /// <summary>Stores a JSON string with the given key.</summary>
        void Save(string key, string jsonData);

        /// <summary>True if the key exists.</summary>
        bool HasKey(string key);

        /// <summary>Removes the key and its data.</summary>
        void DeleteKey(string key);
    }
}
