using System;
using NaughtyAttributes;
using TripleMatch.Core.Attributes;
using UnityEngine;

namespace TripleMatch.Data
{
    [Serializable]
    public class CollectibleItemInstanceData
    {
        public CollectibleItemData Item;
        public Vector2 Position;

        [ValidateField(nameof(ValidateScale))]
        public Vector2 Scale = Vector2.one;
        
        [AllowNesting] [ReadOnly]
        public bool IsMirrored = false;
        public int SortingOrder;
        public int CollectibleParentIndex = -1;
        public bool IsCollectible = true;

        private Vector2 ValidateScale(Vector2 value)
        {
            IsMirrored = value.x < 0f ? true : false;
            
            return value;
        }
    }
}
