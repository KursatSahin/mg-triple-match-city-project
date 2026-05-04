using System;
using UnityEngine;

namespace TripleMatch.Data
{
    [Serializable]
    public class CollectibleItemInstanceData
    {
        public CollectibleItemData Item;
        public Vector2 Position;
        public Vector2 Scale = Vector2.one;
        public bool IsMirrored;
        public int SortingOrder;
        public int CollectibleParentIndex = -1;
    }
}