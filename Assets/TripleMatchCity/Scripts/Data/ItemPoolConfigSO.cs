using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Config/Item Pool Config")]
    public class ItemPoolConfigSO : ScriptableObject
    {
        public int DefaultCapacity = 16;
        public int MaxPoolSize = 256;
        public int DefaultMaxVisualPoolCount = 16;
    }
}