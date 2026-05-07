using System;
using Cysharp.Threading.Tasks;
using TripleMatch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace TripleMatch.Runtime
{
    /// <summary>
    /// Handles scene transitions between Game and MainMenu. In future this service should provide to manage all scene transitions
    /// </summary>
    public class SceneFlowService : IStartable, IDisposable
    {
        private const string MainMenuSceneName = "MainMenu";
        private const string GameSceneName = "Game";

        private EventBinding<MainMenuRequestedEvent> _mainMenuBinding;
        private bool _isTransitioning;

        public void Start()
        {
            _mainMenuBinding = new EventBinding<MainMenuRequestedEvent>(_ => GoToMainMenu().Forget());
            EventBus<MainMenuRequestedEvent>.Register(_mainMenuBinding);
        }

        public void Dispose()
        {
            if (_mainMenuBinding != null)
            {
                EventBus<MainMenuRequestedEvent>.Deregister(_mainMenuBinding);
                _mainMenuBinding = null;
            }
        }

        private async UniTaskVoid GoToMainMenu()
        {
            if (_isTransitioning) return;
            
            _isTransitioning = true;

            try
            {
                Scene gameScene = SceneManager.GetSceneByName(GameSceneName);
                Scene menuScene = SceneManager.GetSceneByName(MainMenuSceneName);

                if (!menuScene.isLoaded)
                    await SceneManager.LoadSceneAsync(MainMenuSceneName, LoadSceneMode.Additive);

                if (gameScene.isLoaded)
                    await SceneManager.UnloadSceneAsync(gameScene);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneFlowService] Scene transition to MainMenu failed: {ex}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
