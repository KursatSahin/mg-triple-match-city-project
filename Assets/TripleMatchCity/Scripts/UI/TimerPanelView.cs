using TMPro;
using UnityEngine;

namespace TripleMatch.UI
{
    /// <summary>
    /// Topbar timer mm:ss.
    /// </summary>
    public class TimerPanelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        public void SetTime(float seconds)
        {
            if (timerText == null) return;
            if (seconds < 0f) seconds = 0f;

            int total = Mathf.CeilToInt(seconds);
            int minutes = total / 60;
            int secs = total % 60;
            timerText.text = $"{minutes:00}:{secs:00}";
        }
    }
}
