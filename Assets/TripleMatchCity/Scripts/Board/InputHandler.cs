using TripleMatch.Command;
using TripleMatch.Deck;
using UnityEngine;
using VContainer;

namespace TripleMatch.Board
{
    /// <summary>
    /// Tracks tap input on the board. Each accepted tap is wrapped in a CollectItemCommand and dispatched immediately.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        private IBoardManager _boardManager;
        private IDeckManager _deckManager;
        private ICommandQueue _commandQueue;

        [Inject]
        public void Construct(IBoardManager boardManager, IDeckManager deckManager, ICommandQueue commandQueue)
        {
            _boardManager = boardManager;
            _deckManager = deckManager;
            _commandQueue = commandQueue;
        }

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null) return;
            if (!Input.GetMouseButtonDown(0)) return;

            Vector3 screenPos = Input.mousePosition;
            Vector2 worldPoint = worldCamera.ScreenToWorldPoint(screenPos);

            CollectibleItemView picked = PickTopItem(worldPoint);

            if (picked == null) return;
            if (_deckManager.IsFull) return;

            _commandQueue.Enqueue(new CollectItemCommand(_boardManager, _deckManager, picked));
        }

        /// <summary>
        /// Picks the topmost collectible item under the given world point, if any.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        private CollectibleItemView PickTopItem(Vector2 worldPoint)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
            if (hits == null || hits.Length == 0) return null;
            
            CollectibleItemView top = null;
            int topOrder = int.MinValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit == null) continue;

                var view = hit.GetComponentInParent<CollectibleItemView>();
                if (view == null) continue;
                if (view.IsRemovedFromBoard) continue;

                int order = view.SortingOrder;
                if (order > topOrder)
                {
                    topOrder = order;
                    top = view;
                }
            }

            return IsTappable(top) ? top : null;
        }

        private static bool IsTappable(CollectibleItemView view)
        {
            if (view == null) return false;
            
            if (view.IsRemovedFromBoard) return false;

            // Check if its collectible or not
            var data = view.InstanceData;
            
            if (data == null || !data.IsCollectible) return false;

            // Check if parent item covers its child
            var parent = view.ParentItem;
            
            if (parent != null && !parent.IsRemovedFromBoard) return false;

            return true;
        }
    }
}