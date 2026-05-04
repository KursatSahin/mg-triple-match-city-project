using System;
using NaughtyAttributes;
using UnityEngine;

namespace TripleMatch.Data
{
    [Serializable]
    public class GoalData
    {
        public CollectibleItemData Item;

        [Min(1)][ValidateInput("IsMultipleOf3", "Target Match Count must be at least 1.")]
        public int TargetMatchCount;

        private bool IsMultipleOf3(int value)
        {
            return value % 3 == 0;
        }
    }
}