using System;
using System.Collections.Generic;
using TripleMatch.Core;
using TripleMatch.Data;
using UnityEngine;
using VContainer.Unity;

namespace TripleMatch.Level
{
    /// <summary>
    /// Manages goals in many ways
    /// Reads goals from level, track status of each goal, notify observers for goal updates
    /// </summary>
    public class GoalManager : IGoalManager, IStartable, IDisposable
    {
        private readonly ILevelManager _levelManager;
        private readonly List<LevelGoalEntity> _goals = new();

        private EventBinding<MatchCompletedEvent> _matchBinding;
        private bool _completionRaised;

        public IReadOnlyList<LevelGoalEntity> Goals => _goals;
        public bool AreAllGoalsComplete => CheckAllComplete();

        public GoalManager(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void Start()
        {
            BuildGoalsFromCurrentLevel();

            _matchBinding = new EventBinding<MatchCompletedEvent>(OnMatchCompleted);
            EventBus<MatchCompletedEvent>.Register(_matchBinding);
        }

        public void Dispose()
        {
            if (_matchBinding != null)
            {
                EventBus<MatchCompletedEvent>.Deregister(_matchBinding);
                _matchBinding = null;
            }
        }

        private void BuildGoalsFromCurrentLevel()
        {
            _goals.Clear();
            _completionRaised = false;

            var level = _levelManager?.CurrentLevel;
            if (level == null || level.Goals == null) return;

            for (int i = 0; i < level.Goals.Count; i++)
            {
                GoalData gd = level.Goals[i];
                
                if (gd == null || gd.Item == null) continue;
                
                _goals.Add(new LevelGoalEntity(gd.Item, gd.TargetMatchCount));
            }
        }

        private void OnMatchCompleted(MatchCompletedEvent matchCompletedEvent)
        {
            if (matchCompletedEvent.ItemData == null) return;

            for (int i = 0; i < _goals.Count; i++)
            {
                var goal = _goals[i];
                if (goal.Item != matchCompletedEvent.ItemData) continue;

                goal.Decrement(matchCompletedEvent.Count);

                EventBus<GoalUpdatedEvent>.Raise(new GoalUpdatedEvent
                {
                    ItemData = goal.Item,
                    Current = goal.Remaining
                });
            }

            if (!_completionRaised && CheckAllComplete())
            {
                _completionRaised = true;
                EventBus<GoalCompletedEvent>.Raise(new GoalCompletedEvent());
            }
        }

        private bool CheckAllComplete()
        {
            if (_goals.Count == 0) return false;
            
            for (int i = 0; i < _goals.Count; i++)
            {
                if (!_goals[i].IsComplete) return false;
            }
            
            return true;
        }
    }
}
