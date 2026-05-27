using UnityEngine;
using UnityEngine.EventSystems;

namespace BoneThrone.CameraControls
{
    /// <summary>
    /// Lightweight camera controls for the GridTest scene only.
    /// Handles middle-mouse panning, right-mouse rotation, and mouse-wheel zoom without touching gameplay systems.
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
        [SerializeField] private bool rotationEnabled = true;
        [SerializeField] private float rotationSpeed = 0.2f;
        [SerializeField] private bool invertRotation;
        [SerializeField] private float rotationStartThresholdPixels = 3f;
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private bool reuseZoomPivotForRotation = true;
        [SerializeField] private float fallbackPivotDistance = 18f;
        [SerializeField] private bool verticalRotationEnabled = true;
        [SerializeField] private float verticalRotationSpeed = 0.15f;
        [SerializeField] private bool invertVerticalRotation;
        [SerializeField] private float minPitch = 35f;
        [SerializeField] private float maxPitch = 75f;

        private Camera targetCamera;
        private Vector3 lastMousePosition;
        private Vector3 rightDragStartMousePosition;
        private Vector3 lastRightMousePosition;
        private bool isDragging;
        private bool isRightMouseHeld;
        private bool isRotating;
        private float currentPitch;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                currentPitch = ClampPitch(targetCamera.transform.eulerAngles.x);
                ApplyPitchYaw(targetCamera.transform.eulerAngles.y);
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
                ResetRightMouseRotation();
                return;
            }

            HandleMiddleMousePan();
            HandleRightMouseRotation();
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

        private void HandleRightMouseRotation()
        {
            if (!rotationEnabled)
            {
                ResetRightMouseRotation();
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                isRightMouseHeld = true;
                isRotating = false;
                rightDragStartMousePosition = Input.mousePosition;
                lastRightMousePosition = rightDragStartMousePosition;
                return;
            }

            if (Input.GetMouseButtonUp(1))
            {
                ResetRightMouseRotation();
                return;
            }

            if (!isRightMouseHeld || !Input.GetMouseButton(1))
            {
                return;
            }

            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 totalDelta = currentMousePosition - rightDragStartMousePosition;
            if (!isRotating && Mathf.Abs(totalDelta.x) < rotationStartThresholdPixels)
            {
                lastRightMousePosition = currentMousePosition;
                return;
            }

            isRotating = true;

            Vector3 frameDelta = currentMousePosition - lastRightMousePosition;
            lastRightMousePosition = currentMousePosition;
            if (Mathf.Approximately(frameDelta.x, 0f) && (!verticalRotationEnabled || Mathf.Approximately(frameDelta.y, 0f)))
            {
                return;
            }

            float direction = invertRotation ? -1f : 1f;
            float yawDelta = frameDelta.x * rotationSpeed * direction;

            if (verticalRotationEnabled)
            {
                float verticalDirection = invertVerticalRotation ? -1f : 1f;
                currentPitch = ClampPitch(currentPitch - frameDelta.y * verticalRotationSpeed * verticalDirection);
            }

            RotateAroundPivot(yawDelta);
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

        private void RotateAroundPivot(float yawDelta)
        {
            Transform cameraTransform = targetCamera.transform;
            Vector3 pivotPosition = ResolveRotationPivot();

            cameraTransform.RotateAround(pivotPosition, Vector3.up, yawDelta);

            Vector3 eulerAngles = cameraTransform.eulerAngles;
            ApplyPitchYaw(eulerAngles.y);
        }

        private Vector3 ResolveRotationPivot()
        {
            if (rotationPivot != null)
            {
                return rotationPivot.position;
            }

            if (reuseZoomPivotForRotation && zoomPivot != null)
            {
                return zoomPivot.position;
            }

            Transform cameraTransform = targetCamera.transform;
            Vector3 pivotPosition = cameraTransform.position + cameraTransform.forward * fallbackPivotDistance;
            pivotPosition.y = 0f;
            return pivotPosition;
        }

        private void ResetRightMouseRotation()
        {
            isRightMouseHeld = false;
            isRotating = false;
        }

        private float ClampPitch(float pitch)
        {
            return Mathf.Clamp(NormalizePitch(pitch), minPitch, maxPitch);
        }

        private static float NormalizePitch(float pitch)
        {
            if (pitch > 180f)
            {
                return pitch - 360f;
            }

            return pitch;
        }

        private void ApplyPitchYaw(float yaw)
        {
            targetCamera.transform.rotation = Quaternion.Euler(currentPitch, yaw, 0f);
        }

        private bool ShouldBlockForUI()
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
