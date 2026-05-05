using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace TripleMatch.Data
{
    [Serializable]
    public class GoalData
    {
        public CollectibleItemData Item;

        [Min(1)][ValidateInput("IsNotMultipleOf3", "Target Match Count must be multiple of 3.")]
        public int TargetMatchCount;

        private bool IsNotMultipleOf3(int value)
        {
            return value % 3 != 0;
        }
    }
}