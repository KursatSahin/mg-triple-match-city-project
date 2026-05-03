using Cysharp.Threading.Tasks;

namespace TripleMatch.Core
{
    /// <summary>
    /// Mockup class for Data Management System
    /// </summary>
    public class DataManager
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
    }
}
