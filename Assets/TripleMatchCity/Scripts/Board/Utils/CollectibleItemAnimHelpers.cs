using System.Collections.Generic;
using TripleMatch.Board;

namespace TripleMatch.Deck
{

    /// <summary>
    /// Includes packaged data for shifting item
    /// </summary>
    public struct ShiftItemData
    {
        public CollectibleItemView Item;
        public int TargetSlotIndex;

        public ShiftItemData(CollectibleItemView item, int targetSlotIndex)
        {
            Item = item;
            TargetSlotIndex = targetSlotIndex;
        }
    }
    
    /// <summary>
    /// Includes packaged data for inserting item
    /// </summary>
    public class InsertItemData
    {
        public bool IsValid;
        public CollectibleItemView ItemView;
        public int TargetSlotIndex;
        public List<ShiftItemData> ShiftRightMoves;

        public static InsertItemData Empty => new InsertItemData { IsValid = false, ShiftRightMoves = new List<ShiftItemData>() };
    }
    
    /// <summary>
    /// Includes packaged data for resolving match
    /// </summary>
    public class MatchItemData
    {
        public bool HasMatch;
        public List<CollectibleItemView> Cleared;
        public List<ShiftItemData> ShiftLeftMoves;

        public static MatchItemData Empty => new MatchItemData
        {
            HasMatch = false,
            Cleared = new List<CollectibleItemView>(),
            ShiftLeftMoves = new List<ShiftItemData>()
        };
    }
}
