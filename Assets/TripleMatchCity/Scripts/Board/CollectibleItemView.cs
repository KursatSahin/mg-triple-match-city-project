using System;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public class CollectibleItemView : MonoBehaviour
    {
        // TODO : AnimState will be refactored after StateMachine implementation
        public enum AnimState
        {
            Idle,
            MovingToSlot,
            Clearing
        }

        private const float SettleDistanceTreshold = 0.0001f;
        private const float SettleScaleTreshold = 0.0001f;

        [SerializeField] private CollectibleItemView parentItem;

        private GameObject visualInstance;
        private SpriteRenderer cachedSpriteRenderer;
        private string defaultSortingLayer;
        private CollectibleItemInstanceData instanceData;

        private AnimState animState = AnimState.Idle;
        private Transform targetPos;
        private Vector3 targetScale = Vector3.one;
        private float moveStep = 12f;
        private Action onArrived;

        private float clearTimer;
        private float clearDuration;
        private Vector3 clearStartScale;
        private Action<CollectibleItemView> onClearComplete;

        public CollectibleItemData ItemData => instanceData?.Item;
        public CollectibleItemInstanceData InstanceData => instanceData;
        public CollectibleItemView ParentItem => parentItem;
        public SpriteRenderer SpriteRenderer => cachedSpriteRenderer;
        public int SortingOrder => cachedSpriteRenderer != null ? cachedSpriteRenderer.sortingOrder : 0;
        public bool IsRemovedFromBoard { get; private set; }
        public AnimState CurrentAnimState => animState;

        public void AttachVisual(GameObject visual)
        {
            visualInstance = visual;
            if (visual == null)
            {
                cachedSpriteRenderer = null;
                return;
            }

            var visualTransform = visual.transform;
            visualTransform.SetParent(transform, worldPositionStays: false);
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = Vector3.one;
            
            cachedSpriteRenderer = visual.GetComponentInChildren<SpriteRenderer>();
            defaultSortingLayer = cachedSpriteRenderer != null ? cachedSpriteRenderer.sortingLayerName : "Default";
        }

        public GameObject DetachVisual()
        {
            var go = visualInstance;
            visualInstance = null;
            cachedSpriteRenderer = null;
            
            if (go != null)
            {
                go.transform.SetParent(null, worldPositionStays: false);
            }
            
            return go;
        }

        public void Initialize(CollectibleItemInstanceData data, CollectibleItemView parent)
        {
            instanceData = data;
            parentItem = parent;
            IsRemovedFromBoard = false;
            
            transform.localPosition = data.Position;
            transform.localScale = new Vector3(data.Scale.x, data.Scale.y, 1f);

            if (cachedSpriteRenderer != null)
            {
                cachedSpriteRenderer.sortingOrder = data.SortingOrder;
                cachedSpriteRenderer.sortingLayerName = string.IsNullOrEmpty(defaultSortingLayer)
                    ? "Default"
                    : defaultSortingLayer;
                var color = cachedSpriteRenderer.color;
                color.a = 1f;
                cachedSpriteRenderer.color = color;
            }

            animState = AnimState.Idle;
            targetPos = null;
            onArrived = null;
            onClearComplete = null;

            gameObject.SetActive(true);
        }

        public void MarkRemovedFromBoard()
        {
            IsRemovedFromBoard = true;
        }

        public void ClearParent()
        {
            parentItem = null;
        }

        public void ResetForPool()
        {
            instanceData = null;
            parentItem = null;
            IsRemovedFromBoard = false;
            animState = AnimState.Idle;
            targetPos = null;
            onArrived = null;
            onClearComplete = null;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Starts the flight animation to the target slot and target scale
        /// </summary>
        public void StartFlightToSlot(Transform pos, Vector3 scale, float moveStepValue, Action onArrivedCallback = null)
        {
            targetPos = pos;
            targetScale = scale;
            moveStep = Mathf.Max(0.001f, moveStepValue);
            onArrived = onArrivedCallback;
            animState = AnimState.MovingToSlot;
        }

        /// <summary>
        /// Sets a callback to be invoked when the flight animation arrives at the target slot
        /// </summary>
        public void SetOnArrivedCallback(Action callback)
        {
            if (animState == AnimState.MovingToSlot)
            {
                onArrived = callback;
                return;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Starts the clear animation over the given duration, then invokes the callback
        /// </summary>
        public void StartClear(float duration, Action<CollectibleItemView> completionCallback)
        {
            clearDuration = Mathf.Max(0.001f, duration);
            clearTimer = 0f;
            clearStartScale = transform.localScale;
            onClearComplete = completionCallback;
            animState = AnimState.Clearing;
        }

        private void Update()
        {
            switch (animState)
            {
                case AnimState.MovingToSlot: TickMoving(); break;
                case AnimState.Clearing: TickClearing(); break;
            }
        }

        private void TickMoving()
        {
            if (this.targetPos == null)
            {
                animState = AnimState.Idle;
                return;
            }
            
            // Higher moveStep value reaches the target faster.
            float t = 1f - Mathf.Exp(-moveStep * Time.deltaTime);

            Vector3 targetPos = this.targetPos.position;
            transform.position = Vector3.Lerp(transform.position, targetPos, t);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);

            Vector3 posDelta = transform.position - targetPos;
            Vector3 scaleDelta = transform.localScale - targetScale;
            
            if (posDelta.sqrMagnitude < SettleDistanceTreshold && scaleDelta.sqrMagnitude < SettleScaleTreshold)
            {
                // Settle under the slot anchor so subsequent shifts can lerp from a known parent.
                transform.SetParent(this.targetPos, worldPositionStays: false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = targetScale;
                animState = AnimState.Idle;

                var callback = onArrived;
                onArrived = null;
                callback?.Invoke();
            }
        }

        private void TickClearing()
        {
            clearTimer += Time.deltaTime;
            float t = Mathf.Clamp01(clearTimer / clearDuration);

            transform.localScale = Vector3.Lerp(clearStartScale, Vector3.zero, t);
            
            if (cachedSpriteRenderer != null)
            {
                var color = cachedSpriteRenderer.color;
                color.a = 1f - t;
                cachedSpriteRenderer.color = color;
            }

            if (t < 1f) return;

            // Restore alpha so the visual reads correctly when reused from the pool.
            if (cachedSpriteRenderer != null)
            {
                var color = cachedSpriteRenderer.color;
                color.a = 1f;
                cachedSpriteRenderer.color = color;
            }

            animState = AnimState.Idle;
            var callback = onClearComplete;
            onClearComplete = null;
            callback?.Invoke(this);
        }
    }
}
