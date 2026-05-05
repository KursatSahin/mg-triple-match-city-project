using NaughtyAttributes;
using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Collectible Item Data")]
    public class CollectibleItemData : ScriptableObject
    {
        [Tooltip("If you want to edit this field, please change inspector to Debug Mode")]
        [ReadOnly] 
        public string DisplayName;
        public GameObject VisualPrefab;
    }
}