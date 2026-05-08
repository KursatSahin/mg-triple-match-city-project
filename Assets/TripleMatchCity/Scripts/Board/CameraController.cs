using UnityEngine;

namespace TripleMatch.Board
{
    /// <summary>
    /// Pan + pinch-zoom camera controller. Bounds are set according to world rect of background image
    /// the camera frustum is clamped to stay inside those bounds.
    /// Single touch drags pan, two touches pinch-zoom.
    /// In editor, mouse drag pans and scroll wheel zooms.
    /// </summary>
    public class CameraController : MonoBehaviour, ICameraController
    {
        [SerializeField] private Camera worldCamera;

        [Header("Sensitivity")]
        [SerializeField] private float panSensitivity = 1f;
        [SerializeField] private float pinchZoomSensitivity = 0.01f;
        [SerializeField] private float scrollZoomSensitivity = 5f;

        [Header("Zoom Range (auto-derived from bounds)")]
        [Tooltip("Max ortho size = full bounds fit. Min ortho size = max / this factor (more zoomed in).")]
        [SerializeField] private float autoZoomFactor = 3f;

        private Bounds _bounds;
        private float _minOrthoSize;
        private float _maxOrthoSize;
        private bool _hasBounds;

        private Vector3 _lastMousePos;
        private bool _mouseDragging;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = GetComponent<Camera>();
            if (worldCamera == null) worldCamera = Camera.main;
        }

        public void SetBounds(Vector3 worldCenter, Vector2 worldSize)
        {
            _bounds = new Bounds(worldCenter, new Vector3(worldSize.x, worldSize.y, 0f));
            ComputeZoomLimits();
            FitAndCenter();
            _hasBounds = true;
        }

        private void ComputeZoomLimits()
        {
            if (worldCamera == null) return;

            float aspect = worldCamera.aspect;
            float fitVertical = _bounds.size.y / 2f;
            float fitHorizontal = _bounds.size.x / (2f * aspect);

            // Camera frustum must stay inside bounds → ortho size <= min(fitVertical, fitHorizontal).
            // The smaller of the two is the limiting axis.
            _maxOrthoSize = Mathf.Min(fitVertical, fitHorizontal);
            if (_maxOrthoSize <= 0f) _maxOrthoSize = 1f;

            float factor = Mathf.Max(1f, autoZoomFactor);
            _minOrthoSize = _maxOrthoSize / factor;
        }

        private void FitAndCenter()
        {
            if (worldCamera == null) return;

            worldCamera.orthographicSize = _maxOrthoSize;

            Vector3 pos = worldCamera.transform.position;
            pos.x = _bounds.center.x;
            pos.y = _bounds.center.y;
            worldCamera.transform.position = pos;
        }

        private void Update()
        {
            if (!_hasBounds || worldCamera == null) return;

            int touchCount = Input.touchCount;

            if (touchCount >= 2)
            {
                HandlePinch();
            }
            else if (touchCount == 1)
            {
                HandleTouchDrag(Input.GetTouch(0));
            }
            else
            {
                HandleEditorMouse();
            }

            ApplyBoundsClamp();
        }

        private void HandlePinch()
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Skip the frame either finger lands; deltaPosition is unreliable on Began.
            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began) return;

            Vector2 prev0 = t0.position - t0.deltaPosition;
            Vector2 prev1 = t1.position - t1.deltaPosition;

            float prevDistance = Vector2.Distance(prev0, prev1);
            float currDistance = Vector2.Distance(t0.position, t1.position);
            if (prevDistance < 0.001f) return;

            float distanceDelta = currDistance - prevDistance;

            // Fingers moving apart -> zoom in (smaller orthographicSize).
            float newSize = worldCamera.orthographicSize - distanceDelta * pinchZoomSensitivity;
            worldCamera.orthographicSize = Mathf.Clamp(newSize, _minOrthoSize, _maxOrthoSize);
        }

        private void HandleTouchDrag(Touch touch)
        {
            if (touch.phase != TouchPhase.Moved) return;
            if (touch.deltaPosition.sqrMagnitude < 0.0001f) return;

            PanByScreenDelta(touch.deltaPosition);
        }

        private void HandleEditorMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastMousePos = Input.mousePosition;
                _mouseDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _mouseDragging = false;
            }
            else if (_mouseDragging && Input.GetMouseButton(0))
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector2 delta = currentMousePos - _lastMousePos;
                _lastMousePos = currentMousePos;

                if (delta.sqrMagnitude > 0f) PanByScreenDelta(delta);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                float newSize = worldCamera.orthographicSize - scroll * scrollZoomSensitivity;
                worldCamera.orthographicSize = Mathf.Clamp(newSize, _minOrthoSize, _maxOrthoSize);
            }
        }

        private void PanByScreenDelta(Vector2 screenDelta)
        {
            // World units per screen pixel along the vertical axis.
            float worldUnitsPerPixel = worldCamera.orthographicSize * 2f / Mathf.Max(1, Screen.height);
            Vector2 worldDelta = -screenDelta * worldUnitsPerPixel * panSensitivity;
            worldCamera.transform.position += new Vector3(worldDelta.x, worldDelta.y, 0f);
        }

        private void ApplyBoundsClamp()
        {
            float vertExtent = worldCamera.orthographicSize;
            float horzExtent = vertExtent * worldCamera.aspect;

            Vector3 pos = worldCamera.transform.position;
            Vector3 boundsMin = _bounds.min;
            Vector3 boundsMax = _bounds.max;

            float minX = boundsMin.x + horzExtent;
            float maxX = boundsMax.x - horzExtent;
            float minY = boundsMin.y + vertExtent;
            float maxY = boundsMax.y - vertExtent;

            // If frustum is wider/taller than bounds on an axis, center the camera on that axis
            // (otherwise Mathf.Clamp with min > max returns max and skews the view).
            pos.x = minX > maxX ? _bounds.center.x : Mathf.Clamp(pos.x, minX, maxX);
            pos.y = minY > maxY ? _bounds.center.y : Mathf.Clamp(pos.y, minY, maxY);

            worldCamera.transform.position = pos;
        }
    }
}
