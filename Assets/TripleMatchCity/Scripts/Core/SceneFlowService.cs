using System;
using Cysharp.Threading.Tasks;
using TripleMatch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace TripleMatch.Runtime
{
    /// <summary>
    /// Single point of control for scene transitions.
    /// Loads the target scene additively, sets it active, then unloads every other scene
    /// (except itself). Listens to MainMenuRequestedEvent and GameSceneRequestedEvent so any
    /// system can request a transition without depending on this service directly.
    /// </summary>
    public class SceneFlowService : ISceneFlowService, IStartable, IDisposable
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string GameSceneName = "Game";

        private EventBinding<MainMenuRequestedEvent> _mainMenuBinding;
        private EventBinding<GameSceneRequestedEvent> _gameSceneBinding;
        private bool _isTransitioning;

        public void Start()
        {
            _mainMenuBinding = new EventBinding<MainMenuRequestedEvent>(_ => GoToScene(MainMenuSceneName).Forget());
            EventBus<MainMenuRequestedEvent>.Register(_mainMenuBinding);

            _gameSceneBinding = new EventBinding<GameSceneRequestedEvent>(_ => GoToScene(GameSceneName).Forget());
            EventBus<GameSceneRequestedEvent>.Register(_gameSceneBinding);
        }

        public void Dispose()
        {
            if (_mainMenuBinding != null)
            {
                EventBus<MainMenuRequestedEvent>.Deregister(_mainMenuBinding);
                _mainMenuBinding = null;
            }
            if (_gameSceneBinding != null)
            {
                EventBus<GameSceneRequestedEvent>.Deregister(_gameSceneBinding);
                _gameSceneBinding = null;
            }
        }

        public async UniTask GoToScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            if (_isTransitioning) return;

            _isTransitioning = true;
            try
            {
                Scene target = SceneManager.GetSceneByName(sceneName);
                if (!target.isLoaded)
                    await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                target = SceneManager.GetSceneByName(sceneName);
                if (target.IsValid()) SceneManager.SetActiveScene(target);

                // Unload every other loaded scene, walking backwards so indices stay valid.
                for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (!s.isLoaded) continue;
                    if (s.name == sceneName) continue;
                    await SceneManager.UnloadSceneAsync(s);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneFlowService] Transition to '{sceneName}' failed: {ex}");
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
