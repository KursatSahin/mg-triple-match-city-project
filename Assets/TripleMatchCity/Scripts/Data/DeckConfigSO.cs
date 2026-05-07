using UnityEngine;
using UnityEngine.Serialization;

namespace TripleMatch.Data
{
    [CreateAssetMenu(menuName = "TripleMatch/Config/Deck Config")]
    public class DeckConfigSO : ScriptableObject
    {
        public int SlotCount = 7;

        [Header("Slot Layout")]
        [Tooltip("Target world-unit size of an item once it sits in a deck slot. The sprite is scaled uniformly so its largest dimension matches this value.")]
        public float SlotSize = 1f;

        [FormerlySerializedAs("MoveSmoothness")]
        [Header("Reactive Animation")]
        [Tooltip("Higher values make items reach their target slot faster. Used for inserts, shifts, and compaction (exponential smoothing factor).")]
        public float MoveStepValue = 12f;

        [Tooltip("Duration of the match-clear scale + alpha fade before the item is despawned.")]
        public float MatchClearDuration = 0.15f;
    }
}
