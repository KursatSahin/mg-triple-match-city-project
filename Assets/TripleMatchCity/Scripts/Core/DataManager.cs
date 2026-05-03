using Cysharp.Threading.Tasks;

namespace TripleMatch.Core
{
    /// <summary>
    /// Mockup class for Data Management System
    /// </summary>
    public class DataManager : IDataManager
    {
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Load persistent data
        /// </summary>
        public async UniTask LoadData()
        {
            // TODO: Load PlayerPrefs, saved progress, etc.
            await UniTask.Yield();
            IsLoaded = true;
        }

        /// <summary>
        /// Save persistent data
        /// </summary>
        public async UniTask SaveData()
        {
            // TODO: Save PlayerPrefs, saved progress, etc.
            await UniTask.Yield();
        }
    }
}
