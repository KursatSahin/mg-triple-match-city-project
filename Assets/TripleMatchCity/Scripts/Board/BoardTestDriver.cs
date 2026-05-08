using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TripleMatch.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace TripleMatch.Board
{
    /// <summary>
    /// Test driver for board generation
    /// </summary>
    public class BoardTestDriver : MonoBehaviour
    {
        [SerializeField] private LevelDataSO level;
        [SerializeField] private ItemPoolConfigSO itemPoolConfig;
        [SerializeField] private GameObject levelRootPrefab;
        [SerializeField] private CollectibleItemView itemViewPrefab;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private bool autoBuildOnStart = true;

        private ItemFactory _factory;
        private BoardManager _boardManager;

        void Start()
        {
            if (autoBuildOnStart) BuildBoard();
        }

        void OnDestroy()
        {
            _boardManager?.ClearBoard();
            _factory?.Dispose();
        }

        [Button("Build Board")]
        void BuildBoard()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[BoardTestDriver] Only works in Play Mode.");
                return;
            }
            
            if (level == null || levelRootPrefab == null || itemViewPrefab == null)
            {
                Debug.LogError("[BoardTestDriver] Assign Level, Level Root Prefab, and Host Prefab in the Inspector.");
                return;
            }

            InitializeDependencies();
            BuildBoardAsync().Forget();
        }

        [Button("Clear Board")]
        void ClearBoard()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[BoardTestDriver] Only works in Play Mode.");
                return;
            }
            _boardManager?.ClearBoard();
        }

        void InitializeDependencies()
        {
            if (_factory != null) return;
            
            _factory = new ItemFactory(itemViewPrefab, itemPoolConfig);
            _boardManager = new BoardManager(_factory, levelRootPrefab, cameraController);
        }

        async UniTaskVoid BuildBoardAsync()
        {
            await _boardManager.BuildBoard(level, transform);
            Debug.Log($"[BoardTestDriver] Build complete: {_boardManager.ActiveItems.Count} active items.");
        }
    }
}
