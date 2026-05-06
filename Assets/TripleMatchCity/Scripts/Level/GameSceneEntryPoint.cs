using Cysharp.Threading.Tasks;
using TripleMatch.Board;
using UnityEngine;
using VContainer.Unity;

namespace TripleMatch.Level
{
    /// <summary>
    /// VContainer entry point for the Game scene. On scene start it asks the LevelManager
    /// for the current level and asks the BoardManager to build it under the scene parent.
    /// </summary>
    public class GameSceneEntryPoint : IStartable
    {
        private readonly ILevelManager _levelManager;
        private readonly IBoardManager _boardManager;
        private readonly Transform _sceneParent;

        public GameSceneEntryPoint(
            ILevelManager levelManager,
            IBoardManager boardManager,
            Transform sceneParent)
        {
            _levelManager = levelManager;
            _boardManager = boardManager;
            _sceneParent = sceneParent;
        }

        public void Start()
        {
            BuildAsync().Forget();
        }

        private async UniTaskVoid BuildAsync()
        {
            var level = _levelManager.CurrentLevel;
            if (level == null)
            {
                Debug.LogError("[GameSceneEntryPoint] LevelManager.CurrentLevel is null. Check LevelContainer setup.");
                return;
            }
            await _boardManager.BuildBoard(level, _sceneParent);
        }
    }
}