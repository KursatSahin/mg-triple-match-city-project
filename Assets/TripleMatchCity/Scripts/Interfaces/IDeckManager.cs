using System.Collections.Generic;
using TripleMatch.Board;

namespace TripleMatch.Deck
{
    public interface IDeckManager
    {
        IReadOnlyList<DeckSlotData> Slots { get; }
        int SlotCount { get; }
        bool IsFull { get; }
        InsertItemData InsertData(CollectibleItemView itemView);
        MatchItemData ResolveMatchData();
        void AnimateInsert(InsertItemData insertItemData);
        void AnimateMatch(MatchItemData matchItemData);
    }
}