using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using TripleMatch.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace TripleMatch.UI
{
    /// <summary>
    /// End-of-game popup screen. Activated by IUIService.Open via OnOpenAsync, deactivated via
    /// OnCloseAsync. The return-home button raises MainMenuRequestedEvent on the injected event
    /// bus; SceneFlowService handles the scene transition.
    /// </summary>
    public class EndGamePopupView : MonoBehaviour, IScreen<EndGameScreenArgs>
    {
        [Header("Root & Title")]
        [SerializeField] private GameObject root;
        [Tooltip("Semi-transparent blocker behind the popup. Toggled together with the popup.")]
        [SerializeField] private GameObject fadeBackground;
        [SerializeField] private TextMeshProUGUI popupTitleText;

        [Header("Stars")]
        [Tooltip("Star fills (the foreground). Outlines are always shown by their parent objects.")]
        [SerializeField] private Transform[] starFills;
        [SerializeField] private float starBounceUpDuration = 0.18f;
        [SerializeField] private float starSettleDuration = 0.12f;
        [SerializeField] private float starOvershoot = 1.2f;
        [SerializeField] private float starStaggerDelay = 0.1f;

        [Header("Button")]
        [SerializeField] private Button returnHomeButton;

        private IEventBus _eventBus;

        public bool BlockInput => true;

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (fadeBackground != null) fadeBackground.SetActive(false);
            if (returnHomeButton != null) returnHomeButton.onClick.AddListener(HandleReturnHomeClicked);

            ResetStars();
        }

        private void OnDestroy()
        {
            if (returnHomeButton != null) returnHomeButton.onClick.RemoveListener(HandleReturnHomeClicked);
        }

        public UniTask OnOpenAsync(EndGameScreenArgs args)
        {
            if (args == null) return UniTask.CompletedTask;

            if (popupTitleText != null) popupTitleText.text = args.Title;
            if (fadeBackground != null) fadeBackground.SetActive(true);
            if (root != null) root.SetActive(true);

            AnimateStars(args.StarCount);

            return UniTask.CompletedTask;
        }

        public UniTask OnCloseAsync()
        {
            if (root != null) root.SetActive(false);
            if (fadeBackground != null) fadeBackground.SetActive(false);

            return UniTask.CompletedTask;
        }

        private void ResetStars()
        {
            if (starFills == null) return;

            for (int i = 0; i < starFills.Length; i++)
            {
                if (starFills[i] != null) starFills[i].localScale = Vector3.zero;
            }
        }

        private void AnimateStars(int count)
        {
            ResetStars();

            if (starFills == null || starFills.Length == 0) return;

            int reveal = Mathf.Clamp(count, 0, starFills.Length);

            for (int i = 0; i < reveal; i++)
            {
                Transform target = starFills[i];
                if (target == null) continue;

                float delay = i * starStaggerDelay;

                Sequence.Create()
                    .ChainDelay(delay)
                    .Chain(Tween.Scale(target, Vector3.one * starOvershoot, starBounceUpDuration, Ease.OutQuad))
                    .Chain(Tween.Scale(target, Vector3.one, starSettleDuration, Ease.InOutQuad));
            }
        }

        private void HandleReturnHomeClicked()
        {
            _eventBus?.Raise(new MainMenuRequestedEvent());
        }
    }
}
