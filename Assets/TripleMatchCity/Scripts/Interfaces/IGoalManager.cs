using System.Collections.Generic;

namespace TripleMatch.Level
{
    public interface IGoalManager
    {
        IReadOnlyList<LevelGoalEntity> Goals { get; }
        bool AreAllGoalsComplete { get; }
    }
}
