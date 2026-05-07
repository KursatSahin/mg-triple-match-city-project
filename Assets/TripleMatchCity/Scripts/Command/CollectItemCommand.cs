using TripleMatch.Board;
using TripleMatch.Core;
using TripleMatch.Deck;

namespace TripleMatch.Command
{
    /// <summary>
    /// Picks a collectible item from the board and inserts it into the deck
    /// If after the insert and the match resolution the deck is still full, raise DeckFullEvent
    /// </summary>
    public class CollectItemCommand : ICommand
    {
        private readonly IBoardManager _board;
        private readonly IDeckManager _deck;
        private readonly IEventBus _eventBus;
        private readonly CollectibleItemView _item;

        public CollectItemCommand(IBoardManager board, IDeckManager deck, IEventBus eventBus, CollectibleItemView item)
        {
            _board = board;
            _deck = deck;
            _eventBus = eventBus;
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

            if (_deck.IsFull)
            {
                _eventBus.Raise(new DeckFullEvent());
            }
        }
    }
}
