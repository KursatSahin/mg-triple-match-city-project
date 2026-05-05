using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Collectible Item Data")]
    public class CollectibleItemData : ScriptableObject
    {
        public string DisplayName;
        public GameObject VisualPrefab;
    }
}   