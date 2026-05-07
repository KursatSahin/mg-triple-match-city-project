using TripleMatch.Data;

namespace TripleMatch.Level
{
    public class LevelGoalEntity
    {
        public CollectibleItemData Item { get; }
        public int Target { get; }
        public int Remaining { get; private set; }

        public bool IsComplete => Remaining <= 0;

        public LevelGoalEntity(CollectibleItemData item, int target)
        {
            Item = item;
            Target = target;
            Remaining = target;
        }

        public void Decrement(int amount)
        {
            if (amount <= 0) return;
            Remaining -= amount;
            if (Remaining < 0) Remaining = 0;
        }
    }
}
