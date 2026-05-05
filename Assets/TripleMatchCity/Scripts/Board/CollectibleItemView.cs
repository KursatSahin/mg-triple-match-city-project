using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    public class CollectibleItemView : MonoBehaviour
    {
        GameObject visualInstance;
        SpriteRenderer cachedSpriteRenderer;

        CollectibleItemInstanceData instanceData;
        CollectibleItemView parentItem;

        public CollectibleItemData ItemData => instanceData?.Item;
        public CollectibleItemInstanceData InstanceData => instanceData;
        public CollectibleItemView ParentItem => parentItem;
        public int SortingOrder => cachedSpriteRenderer != null ? cachedSpriteRenderer.sortingOrder : 0;
        public bool IsRemovedFromBoard { get; private set; }

        public void Initialize(CollectibleItemInstanceData data, CollectibleItemView parent)
        {
            instanceData = data;
            parentItem = parent;
            IsRemovedFromBoard = false;

            EnsureVisualInstantiated(data.Item);

            var t = transform;
            t.localPosition = data.Position;
            t.localScale = new Vector3(
                data.IsMirrored ? -data.Scale.x : data.Scale.x,
                data.Scale.y,
                1f);

            if (cachedSpriteRenderer != null)
                cachedSpriteRenderer.sortingOrder = data.SortingOrder;

            gameObject.SetActive(true);
        }

        public void MarkRemovedFromBoard()
        {
            IsRemovedFromBoard = true;
        }

        public void ResetForPool()
        {
            instanceData = null;
            parentItem = null;
            IsRemovedFromBoard = false;
            gameObject.SetActive(false);
        }

        void EnsureVisualInstantiated(CollectibleItemData itemData)
        {
            if (visualInstance != null || itemData == null || itemData.VisualPrefab == null)
                return;

            visualInstance = Instantiate(itemData.VisualPrefab, transform);
            var visualTransform = visualInstance.transform;
            visualTransform.localPosition = Vector3.zero;
            visualTransform.localRotation = Quaternion.identity;
            visualTransform.localScale = Vector3.one;
            cachedSpriteRenderer = visualInstance.GetComponentInChildren<SpriteRenderer>();
        }
    }
}
