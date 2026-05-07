using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TripleMatch.UI
{
    public class EndGamePopupView : MonoBehaviour
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

        private Action _onReturnHome;

        private void Awake()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
            
            if (fadeBackground != null)
            {
                fadeBackground.SetActive(false);
            }
            
            if (returnHomeButton != null)
            {
                returnHomeButton.onClick.AddListener(HandleReturnHomeClicked);
            }
            
            ResetStars();
        }

        private void OnDestroy()
        {
            if (returnHomeButton != null)
            {
                returnHomeButton.onClick.RemoveListener(HandleReturnHomeClicked);
            }
        }

        public void Show(string titleText, int starCount, Action onReturnHome)
        {
            _onReturnHome = onReturnHome;

            if (popupTitleText != null)
            {
                popupTitleText.text = titleText;
            }
            
            if (fadeBackground != null)
            {
                fadeBackground.SetActive(true);
            }
            
            if (root != null)
            {
                root.SetActive(true);
            }

            AnimateStars(starCount);
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
            
            if (fadeBackground != null)
            {
                fadeBackground.SetActive(false);
            }
        }

        private void ResetStars()
        {
            if (starFills == null) return;
            
            for (int i = 0; i < starFills.Length; i++)
            {
                if (starFills[i] != null)
                {
                    starFills[i].localScale = Vector3.zero;
                }
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
            var cb = _onReturnHome;
            _onReturnHome = null;
            cb?.Invoke();
        }
    }
}