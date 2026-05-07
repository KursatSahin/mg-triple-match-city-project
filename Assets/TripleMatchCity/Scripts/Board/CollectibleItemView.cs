using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public class CollectibleItemView : MonoBehaviour
    {
        [SerializeField] private CollectibleItemView parentItem;
        
        private GameObject visualInstance;
        private SpriteRenderer cachedSpriteRenderer;
        private CollectibleItemInstanceData instanceData;

        public CollectibleItemData ItemData => instanceData?.Item;
        public CollectibleItemInstanceData InstanceData => instanceData;
        public CollectibleItemView ParentItem => parentItem;
        public int SortingOrder => cachedSpriteRenderer != null ? cachedSpriteRenderer.sortingOrder : 0;
        public bool IsRemovedFromBoard { get; private set; }

        public void AttachVisual(GameObject visual)
        {
            visualInstance = visual;
            if (visual == null)
            {
                cachedSpriteRenderer = null;
                return;
            }

            var t = visual.transform;
            t.SetParent(transform, worldPositionStays: false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            cachedSpriteRenderer = visual.GetComponentInChildren<SpriteRenderer>();
        }

        public GameObject DetachVisual()
        {
            var v = visualInstance;
            visualInstance = null;
            cachedSpriteRenderer = null;
            if (v != null) v.transform.SetParent(null, worldPositionStays: false);
            return v;
        }

        public void Initialize(CollectibleItemInstanceData data, CollectibleItemView parent)
        {
            instanceData = data;
            parentItem = parent;
            IsRemovedFromBoard = false;

            var t = transform;
            t.localPosition = data.Position;
            t.localScale = new Vector3(data.Scale.x, data.Scale.y, 1f);

            if (cachedSpriteRenderer != null)
                cachedSpriteRenderer.sortingOrder = data.SortingOrder;

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
            gameObject.SetActive(false);
        }
    }
}
