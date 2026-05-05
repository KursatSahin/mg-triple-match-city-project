using System;
using TripleMatch.Core.Attributes;
using UnityEngine;

namespace TripleMatch.Data
{
    [Serializable]
    public class GoalData
    {
        public CollectibleItemData Item;
        
        [Tooltip("Target Match Count must be multiple of 3")]
        [ValidateField(nameof(ValidateMultipleOf3))]
        public int TargetMatchCount;

        private int ValidateMultipleOf3(int value)
        {
            if (value < 3)
            {
                return 3;
            }
            else
            {
                if (value % 3 != 0)
                {
                    return value - (value % 3);
                }
                
                return value;
            }
        }
    }
}
