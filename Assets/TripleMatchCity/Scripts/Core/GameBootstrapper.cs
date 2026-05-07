using Cysharp.Threading.Tasks;
using TripleMatch.Core.Data;
using TripleMatch.Runtime;
using UnityEngine;
using VContainer.Unity;

namespace TripleMatch.Core
{
    /// <summary>
    /// App bootstrapper. Initializes the data manager with a PlayerPrefs provider, loads the
    /// save, and asks the scene flow service to switch from Bootstrap to MainMenu.
    /// </summary>
    public class GameBootstrapper : IStartable
    {
        private readonly IDataManager _dataManager;
        private readonly ISceneFlowService _sceneFlowService;

        public GameBootstrapper(IDataManager dataManager, ISceneFlowService sceneFlowService)
        {
            _dataManager = dataManager;
            _sceneFlowService = sceneFlowService;
        }

        public void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            _dataManager.Initialize(new PlayerPrefsProvider());
            _dataManager.Load();

            await _sceneFlowService.GoToScene(SceneFlowService.MainMenuSceneName);
        }
    }
}
