using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Collectible Item Data")]
    public class CollectibleItemData : ScriptableObject
    {
        public Sprite Sprite;
        public string DisplayName;
    }
}