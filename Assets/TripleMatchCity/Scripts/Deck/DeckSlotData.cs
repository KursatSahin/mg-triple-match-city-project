using TripleMatch.Board;
using TripleMatch.Data;

namespace TripleMatch.Deck
{
    /// <summary>
    /// Represents data fort deck slot
    /// Holds the item placed in the slot if its exist.
    /// </summary>
    public class DeckSlotData
    {
        public CollectibleItemView Item;

        public bool IsEmpty => Item == null;

        public CollectibleItemData Type => Item != null ? Item.ItemData : null;
    }
}
