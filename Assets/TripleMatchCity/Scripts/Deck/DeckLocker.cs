using UnityEngine;

namespace TripleMatch.Deck
{
    /// <summary>
    /// Keeps the deck container visually locked to its original screen position even if camera zooms.
    /// Deck should be placed on a child of the camera
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class DeckLocker : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        private float _originalOrthoSize;
        private Vector3 _originalLocalPosition;
        private Vector3 _originalLocalScale;
        private bool _captured;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Start()
        {
            if (worldCamera == null) return;

            _originalOrthoSize = worldCamera.orthographicSize;
            _originalLocalPosition = transform.localPosition;
            _originalLocalScale = transform.localScale;
            _captured = _originalOrthoSize > 0f;
        }

        private void LateUpdate()
        {
            if (!_captured || worldCamera == null) return;

            float ratio = worldCamera.orthographicSize / _originalOrthoSize;

            transform.localPosition = new Vector3(
                _originalLocalPosition.x * ratio,
                _originalLocalPosition.y * ratio,
                _originalLocalPosition.z);

            transform.localScale = _originalLocalScale * ratio;
        }
    }
}
