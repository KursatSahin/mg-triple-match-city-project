using System;
using UnityEngine;

namespace TripleMatch.Data
{
    [Serializable]
    public class CollectableItemInstanceData
    {
        public CollectableItemData Item;
        public Vector2 Position;
        public Vector2 Scale = Vector2.one;
        public bool IsMirrored;
        public int SortingOrder;
        public int CollectableParentIndex = -1;
    }
}
