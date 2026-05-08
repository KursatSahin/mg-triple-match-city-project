using TripleMatch.Command;
using TripleMatch.Core;
using TripleMatch.Deck;
using TripleMatch.StateMachine;
using UnityEngine;
using VContainer;

namespace TripleMatch.Board
{
    /// <summary>
    /// Tracks tap input on the board, Each accepted tap is wrapped in a CollectItemCommand and dispatched immediately.
    /// Tracks drag input on the board (with avoiding collect item)
    /// Tracks pinch input on the board (with avoiding collect item)
    /// Ignored while the game state machine is not in Playing.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float tapMaxScreenMovement = 20f;
        [SerializeField] private float tapMaxDuration = 0.4f;

        private IBoardManager _boardManager;
        private IDeckManager _deckManager;
        private ICommandQueue _commandQueue;
        private IGameStateMachine _gameStateMachine;
        private IEventBus _eventBus;

        private Vector2 _touchStartScreen;
        private float _touchStartTime;
        private bool _touchActive;
        private bool _multiTouchOccurred;

        [Inject]
        public void Construct(
            IBoardManager boardManager,
            IDeckManager deckManager,
            ICommandQueue commandQueue,
            IGameStateMachine gameStateMachine,
            IEventBus eventBus)
        {
            _boardManager = boardManager;
            _deckManager = deckManager;
            _commandQueue = commandQueue;
            _gameStateMachine = gameStateMachine;
            _eventBus = eventBus;
        }

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null) return;
            if (_gameStateMachine != null && !_gameStateMachine.IsPlaying) return;

            // Track multi-touch even on intermediate frames; pinch must invalidate the tap.
            if (_touchActive && Input.touchCount > 1) _multiTouchOccurred = true;

            if (Input.GetMouseButtonDown(0))
            {
                _touchStartScreen = Input.mousePosition;
                _touchStartTime = Time.time;
                _touchActive = true;
                _multiTouchOccurred = Input.touchCount > 1;
                return;
            }

            if (!Input.GetMouseButtonUp(0)) return;
            if (!_touchActive) return;

            _touchActive = false;

            if (_multiTouchOccurred) return;

            float duration = Time.time - _touchStartTime;
            if (duration > tapMaxDuration) return;

            Vector2 endScreen = Input.mousePosition;
            float distance = Vector2.Distance(_touchStartScreen, endScreen);
            if (distance > tapMaxScreenMovement) return;

            Vector2 worldPoint = worldCamera.ScreenToWorldPoint(endScreen);
            CollectibleItemView picked = PickTopItem(worldPoint);

            if (picked == null) return;
            if (_deckManager.IsFull) return;

            _commandQueue.Enqueue(new CollectItemCommand(_boardManager, _deckManager, _eventBus, picked));
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