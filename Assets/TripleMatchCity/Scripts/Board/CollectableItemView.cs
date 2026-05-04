using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Board
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CollectibleItemView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] BoxCollider2D boxCollider;

        CollectibleItemInstanceData instanceData;
        CollectibleItemView parentItem;

        public CollectibleItemData ItemData => instanceData?.Item;
        public CollectibleItemInstanceData InstanceData => instanceData;
        public CollectibleItemView ParentItem => parentItem;
        public int SortingOrder => spriteRenderer.sortingOrder;
        public bool IsRemovedFromBoard { get; private set; }

        public void Initialize(CollectibleItemInstanceData data, CollectibleItemView parent)
        {
            instanceData = data;
            parentItem = parent;
            IsRemovedFromBoard = false;

            spriteRenderer.sprite = data.Item != null ? data.Item.Sprite : null;
            spriteRenderer.sortingOrder = data.SortingOrder;

            var t = transform;
            t.localPosition = data.Position;
            t.localScale = new Vector3(
                data.IsMirrored ? -data.Scale.x : data.Scale.x,
                data.Scale.y,
                1f);

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
            spriteRenderer.sprite = null;
            gameObject.SetActive(false);
        }

        void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }
}