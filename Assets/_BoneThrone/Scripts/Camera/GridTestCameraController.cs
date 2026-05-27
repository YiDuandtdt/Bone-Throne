using UnityEngine;
using UnityEngine.EventSystems;

namespace BoneThrone.CameraControls
{
    /// <summary>
    /// Lightweight camera controls for the GridTest scene only.
    /// Handles middle-mouse panning and mouse-wheel zoom without touching gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GridTestCameraController : MonoBehaviour
    {
        private const float MinPerspectiveFieldOfView = 35f;
        private const float MaxPerspectiveFieldOfView = 75f;

        [SerializeField] private bool controlsEnabled = true;
        [SerializeField] private bool blockWhenPointerOverUI = true;
        [SerializeField] private float panSpeed = 0.02f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 14f;
        [SerializeField] private float perspectiveMinDistance = 8f;
        [SerializeField] private float perspectiveMaxDistance = 35f;
        [SerializeField] private bool invertDrag;
        [SerializeField] private bool useForwardDollyForPerspective = true;
        [SerializeField] private Transform zoomPivot;

        private Camera targetCamera;
        private Vector3 lastMousePosition;
        private bool isDragging;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (!controlsEnabled || targetCamera == null)
            {
                return;
            }

            if (ShouldBlockForUI())
            {
                isDragging = false;
                return;
            }

            HandleMiddleMousePan();
            HandleMouseWheelZoom();
        }

        private void HandleMiddleMousePan()
        {
            if (Input.GetMouseButtonDown(2))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
                return;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
                return;
            }

            if (!isDragging || !Input.GetMouseButton(2))
            {
                return;
            }

            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 delta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            Vector3 right = targetCamera.transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = targetCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (right.sqrMagnitude <= Mathf.Epsilon || forward.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float direction = invertDrag ? 1f : -1f;
            Vector3 panDelta = (right * delta.x + forward * delta.y) * (direction * panSpeed);
            Transform cameraTransform = targetCamera.transform;
            Vector3 newPosition = cameraTransform.position + panDelta;
            newPosition.y = cameraTransform.position.y;
            cameraTransform.position = newPosition;
        }

        private void HandleMouseWheelZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            if (targetCamera.orthographic)
            {
                float size = targetCamera.orthographicSize - scroll * zoomSpeed;
                targetCamera.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
                return;
            }

            if (useForwardDollyForPerspective)
            {
                DollyPerspectiveCamera(scroll);
                return;
            }

            float fieldOfView = targetCamera.fieldOfView - scroll * zoomSpeed;
            targetCamera.fieldOfView = Mathf.Clamp(fieldOfView, MinPerspectiveFieldOfView, MaxPerspectiveFieldOfView);
        }

        private void DollyPerspectiveCamera(float scroll)
        {
            Transform cameraTransform = targetCamera.transform;
            Vector3 pivotPosition = zoomPivot != null ? zoomPivot.position : Vector3.zero;
            Vector3 candidatePosition = cameraTransform.position + cameraTransform.forward * (scroll * zoomSpeed);
            float candidateDistance = Vector3.Distance(candidatePosition, pivotPosition);

            if (candidateDistance < perspectiveMinDistance || candidateDistance > perspectiveMaxDistance)
            {
                return;
            }

            cameraTransform.position = candidatePosition;
        }

        private bool ShouldBlockForUI()
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
