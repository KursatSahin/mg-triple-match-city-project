using TripleMatch.Core.Data;

namespace TripleMatch.Core
{
    /// <summary>
    /// Centralized data persistence service. Holds the cached GameSaveData and routes reads
    /// and writes through an IDataProvider.
    /// </summary>
    public interface IDataManager
    {
        /// <summary>Cached game save data. Mutate, then call MarkDirty.</summary>
        GameSaveData SaveData { get; }

        /// <summary>Whether Load has run successfully.</summary>
        bool IsLoaded { get; }

        /// <summary>Initialize with a storage provider. Must be called before Load.</summary>
        void Initialize(IDataProvider provider);

        /// <summary>Load cached data from the provider.</summary>
        void Load();

        /// <summary>Persist if dirty.</summary>
        void Save();

        /// <summary>
        /// Mark cached data as dirty so the next Save call writes it.
        /// If forcePersist is true, persist immediately.
        /// </summary>
        void MarkDirty(bool forcePersist = false);

        /// <summary>Reset cache to defaults and persist.</summary>
        void ResetToDefaults();
    }
}
