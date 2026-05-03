using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Collectable Item Data")]
    public class CollectableItemData : ScriptableObject
    {
        public Sprite Sprite;
        public string DisplayName;
    }
}
