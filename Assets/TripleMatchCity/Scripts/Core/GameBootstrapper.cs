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
        private readonly IEventBus _eventBus;

        public GameBootstrapper(IDataManager dataManager, ISceneFlowService sceneFlowService, IEventBus eventBus)
        {
            _dataManager = dataManager;
            _sceneFlowService = sceneFlowService;
            _eventBus = eventBus;
        }

        public void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            Application.targetFrameRate = 60;

            _dataManager.Initialize();
            _dataManager.Load();
            
            _eventBus?.Raise(new MainMenuRequestedEvent());
        }
    }
}
