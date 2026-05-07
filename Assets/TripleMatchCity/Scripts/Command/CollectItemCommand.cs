using TripleMatch.Board;
using TripleMatch.Deck;

namespace TripleMatch.Command
{
    /// <summary>
    /// Picks a collectible item from the board and inserts it into the deck.
    /// </summary>
    public class CollectItemCommand : ICommand
    {
        private readonly IBoardManager _board;
        private readonly IDeckManager _deck;
        private readonly CollectibleItemView _item;

        public CollectItemCommand(IBoardManager board, IDeckManager deck, CollectibleItemView item)
        {
            _board = board;
            _deck = deck;
            _item = item;
        }

        public void Execute()
        {
            _board.DetachItem(_item);

            var insertPlan = _deck.InsertData(_item);
            if (!insertPlan.IsValid) return;

            _deck.AnimateInsert(insertPlan);

            var matchPlan = _deck.ResolveMatchData();
            _deck.AnimateMatch(matchPlan);
        }
    }
}
