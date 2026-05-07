using System.Collections.Generic;
using TripleMatch.Board;

namespace TripleMatch.Deck
{
    public interface IDeckManager
    {
        IReadOnlyList<DeckSlotData> Slots { get; }
        int SlotCount { get; }
        bool IsFull { get; }
        bool TryInsert(CollectibleItemView item);
    }
}