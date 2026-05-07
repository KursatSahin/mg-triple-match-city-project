using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TripleMatch.UI
{
    /// <summary>
    /// Main menu view component.
    /// Holds references to the menu's interactive widgets and text fields.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI nextLevelText;

        public Button PlayButton => playButton;
        public Button ClearButton => clearButton;
        public TextMeshProUGUI TitleText => titleText;
        public TextMeshProUGUI NextLevelText => nextLevelText;
    }
}
