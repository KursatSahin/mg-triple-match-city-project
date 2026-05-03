using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace TripleMatch.Core
{
    /// <summary>
    /// App bootstrapper, not a pure c# DI solution but it's supported by vcontainer
    /// We can use [RuntimeInitializeOnLoadMethod] instead of this without vcontainer
    /// </summary>
    public class GameBootstrapper : IStartable
    {
        private readonly IDataManager _dataManager;

        public GameBootstrapper(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        /// <summary>
        /// VContainer entry point
        /// Starts the async initialization flow
        /// </summary>
        public void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            await _dataManager.LoadData();

            await SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive).ToUniTask();

            await SceneManager.UnloadSceneAsync("Bootstrap").ToUniTask();
        }
    }
}
