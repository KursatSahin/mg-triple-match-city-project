using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TripleMatch.Deck
{
    /// <summary>
    /// Deck view component.
    /// </summary>
    public class DeckView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private List<Transform> slots = new();

        public Transform Container => container;
        public IReadOnlyList<Transform> Slots => slots;
        public int SlotCount => slots != null ? slots.Count : 0;

        public Transform GetSlotPos(int index)
        {
            if (slots == null) return null;
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }
    }
}
