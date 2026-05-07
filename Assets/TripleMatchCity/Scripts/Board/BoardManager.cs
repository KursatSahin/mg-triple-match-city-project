using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public class BoardManager : IBoardManager
    {
        private readonly IItemFactory _itemFactory;
        private readonly GameObject _levelRootPrefab;
        private readonly List<CollectibleItemView> _activeItems = new();

        private LevelRootView _levelRootInstance;

        public IReadOnlyList<CollectibleItemView> ActiveItems => _activeItems;

        public BoardManager(IItemFactory itemFactory, GameObject levelRootPrefab)
        {
            _itemFactory = itemFactory;
            _levelRootPrefab = levelRootPrefab;
        }

        public async UniTask BuildBoard(LevelDataSO level, Transform sceneParent)
        {
            ClearBoard();

            if (level == null)
            {
                Debug.LogError("[BoardManager] Level data is null.");
                return;
            }
            if (_levelRootPrefab == null)
            {
                Debug.LogError("[BoardManager] Level root prefab is not assigned.");
                return;
            }

            var instance = Object.Instantiate(_levelRootPrefab, sceneParent);
            _levelRootInstance = instance.GetComponent<LevelRootView>();
            
            if (_levelRootInstance == null)
            {
                Debug.LogError("[BoardManager] Level root prefab is missing LevelRootView component.");
                Object.Destroy(instance);
            
                return;
            }

            ApplyBackground(level.Background);
            
            SpawnList(level.CollectibleItems, _levelRootInstance.CollectibleItems);
            SpawnList(level.NonCollectibleItems, _levelRootInstance.NonCollectibleItems);

            await UniTask.Yield();
        }

        public void RemoveItem(CollectibleItemView view)
        {
            if (view == null) return;

            int index = _activeItems.IndexOf(view);

            if (index < 0) return;

            DetachLiveChildren(view);

            view.MarkRemovedFromBoard();

            _activeItems.RemoveAt(index);
            _itemFactory.Despawn(view);
        }

        /// <summary>
        /// Detaches the item from the board without despawning it.
        /// </summary>
        /// <param name="view"></param>
        public void DetachItem(CollectibleItemView view)
        {
            if (view == null) return;

            int index = _activeItems.IndexOf(view);
            if (index < 0) return;

            DetachLiveChildren(view);
            
            view.MarkRemovedFromBoard();
            view.ClearParent();
            
            _activeItems.RemoveAt(index);
        }

        private void DetachLiveChildren(CollectibleItemView view)
        {
            // Detach (re-parent) child collectible item from its parent
            Transform reparentTo = view.transform.parent;
            Transform t = view.transform;

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Transform child = t.GetChild(i);
                CollectibleItemView childView = child.GetComponent<CollectibleItemView>();
                
                if (childView == null) continue;
                
                if (childView.IsRemovedFromBoard) continue;

                child.SetParent(reparentTo, worldPositionStays: true);
                childView.ClearParent();
            }
        }

        public void ClearBoard()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                _itemFactory.Despawn(_activeItems[i]);
            }
            
            _activeItems.Clear();

            if (_levelRootInstance != null)
            {
                Object.Destroy(_levelRootInstance.gameObject);
                
                _levelRootInstance = null;
            }
        }

        void ApplyBackground(BackgroundData background)
        {
            if (_levelRootInstance.Background == null)
            {
                Debug.LogWarning("[BoardManager] LevelRootView has no Background SpriteRenderer reference.");
                return;
            }
            
            if (background == null) return;

            var sr = _levelRootInstance.Background;
            
            if (background.Sprite != null) sr.sprite = background.Sprite;
            if (background.Size != Vector2.zero) sr.size = background.Size;
            
            sr.transform.localPosition = background.Position;
        }

        void SpawnList(List<CollectibleItemInstanceData> items, Transform container)
        {
            if (items == null || container == null) return;

            var indexToView = new Dictionary<int, CollectibleItemView>();

            for (int i = 0; i < items.Count; i++)
            {
                var data = items[i];
                
                if (data == null || data.Item == null) continue;

                Transform parentTransform = container;
                CollectibleItemView parentView = null;

                if (data.CollectibleParentIndex >= 0 && indexToView.TryGetValue(data.CollectibleParentIndex, out var pView))
                {
                    parentView = pView;
                    parentTransform = pView.transform;
                }

                var view = _itemFactory.Spawn(data, parentTransform, parentView);

                _activeItems.Add(view);
                indexToView[i] = view;
            }
        }
    }
}
