using UnityEngine;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Config/Deck Config")]
    public class DeckConfigSO : ScriptableObject
    {
        public int SlotCount = 7;
    }
}
