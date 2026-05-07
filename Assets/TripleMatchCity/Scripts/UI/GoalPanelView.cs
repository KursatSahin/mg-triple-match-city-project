using System.Collections.Generic;
using TripleMatch.Data;
using TripleMatch.Level;
using UnityEngine;

namespace TripleMatch.UI
{
    /// <summary>
    /// Goal panel view container at the topbar of the game scene
    /// </summary>
    public class GoalPanelView : MonoBehaviour
    {
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GoalSlotView slotPrefab;

        private readonly Dictionary<CollectibleItemData, GoalSlotView> _slotsByItem = new();

        public void Build(IReadOnlyList<LevelGoalEntity> goals)
        {
            ClearSlots();

            if (slotsContainer == null || slotPrefab == null || goals == null) return;

            for (int i = 0; i < goals.Count; i++)
            {
                LevelGoalEntity goal = goals[i];
                
                if (goal == null || goal.Item == null) continue;

                var slot = Instantiate(slotPrefab, slotsContainer);
                slot.Bind(goal);
                
                _slotsByItem[goal.Item] = slot;
            }
        }

        public void UpdateGoal(CollectibleItemData type, int remaining)
        {
            if (type == null) return;
            
            if (_slotsByItem.TryGetValue(type, out var slot))
            {
                slot.SetRemaining(remaining);
            }
        }

        private void ClearSlots()
        {
            foreach (var pair in _slotsByItem)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }
            
            _slotsByItem.Clear();
        }
    }
}
