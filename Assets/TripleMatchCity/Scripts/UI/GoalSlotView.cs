using TMPro;
using TripleMatch.Data;
using TripleMatch.Level;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TripleMatch.UI
{
    /// <summary>
    /// View of GaolSlot ui item instance
    /// </summary>
    public class GoalSlotView : MonoBehaviour
    {
        [FormerlySerializedAs("icon")] [SerializeField] private Image _icon;
        [FormerlySerializedAs("countText")] [SerializeField] private TextMeshProUGUI _countText;
        [FormerlySerializedAs("completeBadge")] [SerializeField] private GameObject _completeBadge;

        public CollectibleItemData Item { get; private set; }

        public void Bind(LevelGoalEntity goalEntity)
        {
            Item = goalEntity != null ? goalEntity.Item : null;

            if (_icon != null) _icon.sprite = GetIconSprite(Item);

            SetRemaining(goalEntity != null ? goalEntity.Remaining : 0);
        }

        public void SetRemaining(int remaining)
        {
            if (_countText != null) _countText.text = remaining.ToString();
            if (_completeBadge != null) _completeBadge.SetActive(remaining <= 0);
        }

        private static Sprite GetIconSprite(CollectibleItemData item)
        {
            if (item == null || item.VisualPrefab == null) return null;

            var sr = item.VisualPrefab.GetComponentInChildren<SpriteRenderer>(includeInactive: true);
            return sr != null ? sr.sprite : null;
        }
    }
}
