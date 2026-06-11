using UnityEngine;
using UnityEngine.EventSystems;

namespace BoneThrone.CameraControls
{
    /// <summary>
    /// Lightweight camera controls for the GridTest scene only.
    /// Handles middle-mouse panning, right-mouse rotation, mouse-wheel zoom, and mobile touch gestures without touching gameplay systems.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class GridTestCameraController : MonoBehaviour
    {
        private const float MinPerspectiveFieldOfView = 35f;
        private const float MaxPerspectiveFieldOfView = 75f;

        private enum MobileSingleTouchDragMode
        {
            Rotate = 0,
            Pan = 1
        }

        [SerializeField] private bool controlsEnabled = true;
        [SerializeField] private bool blockWhenPointerOverUI = true;
        [SerializeField] private float panSpeed = 0.02f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private bool singleTouchDragEnabled = true;
        [SerializeField] private MobileSingleTouchDragMode singleTouchDragMode = MobileSingleTouchDragMode.Rotate;
        [SerializeField] private float touchDragStartThresholdPixels = 18f;
        [SerializeField] private bool requireMobileCameraJoystickForSingleTouchDrag = true;
        [SerializeField] private Vector2 mobileCameraJoystickViewportCenter = new Vector2(0.17f, 0.16f);
        [SerializeField] private float mobileCameraJoystickRadiusViewportHeight = 0.085f;
        [SerializeField] private bool pinchZoomEnabled = true;
        [SerializeField] private float pinchZoomSensitivity = 0.02f;
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
        private Vector2 touchDragStartPosition;
        private Vector2 lastTouchPosition;
        private int activeTouchFingerId = -1;
        private bool isDragging;
        private bool isRightMouseHeld;
        private bool isRotating;
        private bool isSingleTouchHeld;
        private bool isSingleTouchDragging;
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

            if (HandlePinchZoom())
            {
                isDragging = false;
                ResetRightMouseRotation();
                ResetSingleTouchDrag();
                return;
            }

            if (HandleSingleTouchDrag())
            {
                isDragging = false;
                ResetRightMouseRotation();
                return;
            }

            HandleMouseWheelZoom();

            if (IsPointerOverBlockingUI())
            {
                isDragging = false;
                ResetRightMouseRotation();
                ResetSingleTouchDrag();
                return;
            }

            HandleMiddleMousePan();
            HandleRightMouseRotation();
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
            Vector2 delta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            ApplyPanDelta(delta);
        }

        private void ApplyPanDelta(Vector2 delta)
        {
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
            ApplyRotationDelta(frameDelta);
        }

        private void ApplyRotationDelta(Vector2 frameDelta)
        {
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

        private bool HandleSingleTouchDrag()
        {
            if (!singleTouchDragEnabled)
            {
                ResetSingleTouchDrag();
                return false;
            }

            if (Input.touchCount != 1)
            {
                ResetSingleTouchDrag();
                return false;
            }

            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (!IsScreenPositionInMobileCameraJoystick(touch.position))
                {
                    ResetSingleTouchDrag();
                    return false;
                }

                if (IsTouchOverBlockingUI(touch))
                {
                    ResetSingleTouchDrag();
                    return true;
                }

                activeTouchFingerId = touch.fingerId;
                isSingleTouchHeld = true;
                isSingleTouchDragging = false;
                touchDragStartPosition = touch.position;
                lastTouchPosition = touch.position;
                return true;
            }

            if (!isSingleTouchHeld || touch.fingerId != activeTouchFingerId)
            {
                return false;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                ResetSingleTouchDrag();
                return true;
            }

            if (IsTouchOverBlockingUI(touch))
            {
                ResetSingleTouchDrag();
                return true;
            }

            Vector2 totalDelta = touch.position - touchDragStartPosition;
            if (!isSingleTouchDragging && totalDelta.sqrMagnitude < touchDragStartThresholdPixels * touchDragStartThresholdPixels)
            {
                lastTouchPosition = touch.position;
                return true;
            }

            isSingleTouchDragging = true;

            Vector2 frameDelta = touch.position - lastTouchPosition;
            lastTouchPosition = touch.position;
            if (singleTouchDragMode == MobileSingleTouchDragMode.Pan)
            {
                ApplyPanDelta(frameDelta);
                return true;
            }

            ApplyRotationDelta(frameDelta);
            return true;
        }

        private void HandleMouseWheelZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            ApplyZoom(scroll);
        }

        private bool HandlePinchZoom()
        {
            if (!pinchZoomEnabled || Input.touchCount < 2)
            {
                return false;
            }

            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);
            if (IsTouchOverBlockingUI(firstTouch) || IsTouchOverBlockingUI(secondTouch))
            {
                return true;
            }

            Vector2 firstPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
            Vector2 secondPreviousPosition = secondTouch.position - secondTouch.deltaPosition;
            float previousDistance = Vector2.Distance(firstPreviousPosition, secondPreviousPosition);
            float currentDistance = Vector2.Distance(firstTouch.position, secondTouch.position);
            float pinchDelta = currentDistance - previousDistance;

            if (Mathf.Approximately(pinchDelta, 0f))
            {
                return true;
            }

            ApplyZoom(pinchDelta * pinchZoomSensitivity);
            return true;
        }

        private void ApplyZoom(float zoomDelta)
        {
            if (targetCamera.orthographic)
            {
                float size = targetCamera.orthographicSize - zoomDelta * zoomSpeed;
                targetCamera.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
                return;
            }

            if (useForwardDollyForPerspective)
            {
                DollyPerspectiveCamera(zoomDelta);
                return;
            }

            float fieldOfView = targetCamera.fieldOfView - zoomDelta * zoomSpeed;
            targetCamera.fieldOfView = Mathf.Clamp(fieldOfView, MinPerspectiveFieldOfView, MaxPerspectiveFieldOfView);
        }

        private void DollyPerspectiveCamera(float scroll)
        {
            Transform cameraTransform = targetCamera.transform;
            Vector3 pivotPosition = ResolveZoomPivot();
            Vector3 candidatePosition = cameraTransform.position + cameraTransform.forward * (scroll * zoomSpeed);
            float candidateDistance = Vector3.Distance(candidatePosition, pivotPosition);

            if (candidateDistance < perspectiveMinDistance || candidateDistance > perspectiveMaxDistance)
            {
                return;
            }

            cameraTransform.position = candidatePosition;
        }

        private Vector3 ResolveZoomPivot()
        {
            if (zoomPivot != null)
            {
                return zoomPivot.position;
            }

            Transform cameraTransform = targetCamera.transform;
            return cameraTransform.position + cameraTransform.forward * fallbackPivotDistance;
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

        private void ResetSingleTouchDrag()
        {
            activeTouchFingerId = -1;
            isSingleTouchHeld = false;
            isSingleTouchDragging = false;
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

        private bool IsPointerOverBlockingUI()
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }

        private bool IsTouchOverBlockingUI(Touch touch)
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }

        private bool IsScreenPositionInMobileCameraJoystick(Vector2 screenPosition)
        {
            if (!requireMobileCameraJoystickForSingleTouchDrag)
            {
                return true;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            Vector2 clampedViewportCenter = new Vector2(
                Mathf.Clamp01(mobileCameraJoystickViewportCenter.x),
                Mathf.Clamp01(mobileCameraJoystickViewportCenter.y));
            Vector2 joystickCenter = new Vector2(
                safeArea.xMin + safeArea.width * clampedViewportCenter.x,
                safeArea.yMin + safeArea.height * clampedViewportCenter.y);
            float joystickRadius = Mathf.Max(1f, safeArea.height * Mathf.Max(0.01f, mobileCameraJoystickRadiusViewportHeight));

            return (screenPosition - joystickCenter).sqrMagnitude <= joystickRadius * joystickRadius;
        }
    }
}
