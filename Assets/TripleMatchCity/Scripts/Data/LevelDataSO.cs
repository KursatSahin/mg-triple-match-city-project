using System.Collections.Generic;
using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        public int LevelIndex;
        public float TimeLimitSeconds = 120f;
        public BackgroundData Background = new();
        public List<GoalData> Goals = new();
        public List<CollectableItemInstanceData> Items = new();
    }
}
