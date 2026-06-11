using System.Collections;
using System.Collections.Generic;
using BoneThrone.Audio;
using BoneThrone.Interactables;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BoneThrone.UI
{
    /// <summary>
    /// Runtime-only mouse feedback bridge for global click SFX and custom cursor states.
    /// </summary>
    public sealed class BTMouseFeedbackController : MonoBehaviour
    {
        private const string RuntimeObjectName = "BTMouseFeedbackController_Runtime";
        private const string NormalCursorResourcePath = "BoneThroneCursor/鼠标图标样式";
        private const string InteractiveCursorResourcePath = "BoneThroneCursor/遇到能点击的物体时候的鼠标样式";
        private const string NormalCursorAssetPath = "Assets/_BoneThrone/Art/2D/通用/鼠标/鼠标图标样式.png";
        private const string InteractiveCursorAssetPath = "Assets/_BoneThrone/Art/2D/通用/鼠标/遇到能点击的物体时候的鼠标样式.png";
        private const float WorldRayDistance = 500f;

        private static BTMouseFeedbackController instance;

        [Header("Cursor Textures")]
        [Tooltip("Optional normal cursor texture. If empty, Editor Play Mode loads the project cursor asset; builds can also use a Resources/BoneThroneCursor texture.")]
        [SerializeField] private Texture2D normalCursorTexture;
        [Tooltip("Optional hover cursor texture for clickable targets. Locked-but-clickable UI should use this too.")]
        [SerializeField] private Texture2D interactiveCursorTexture;
        [SerializeField] private Vector2 normalCursorHotspot;
        [SerializeField] private Vector2 interactiveCursorHotspot;

        [Header("World Hover")]
        [SerializeField] private LayerMask worldHoverLayerMask = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>(16);
        private PointerEventData pointerEventData;
        private EventSystem pointerEventSystem;
        private bool isUsingInteractiveCursor;
        private bool hasAppliedAnyCursor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        private static BTMouseFeedbackController EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            BTMouseFeedbackController existing = Object.FindFirstObjectByType<BTMouseFeedbackController>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            GameObject host = new GameObject(RuntimeObjectName);
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BTMouseFeedbackController>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCursorTexturesIfNeeded();
            ApplyCursor(false);
        }

        private void OnDisable()
        {
            if (hasAppliedAnyCursor)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                hasAppliedAnyCursor = false;
            }
        }

        private void Update()
        {
            UpdateCursorState();

            Vector2 pointerPosition;
            if (BTPrimaryPointerInput.TryGetPrimaryDown(out pointerPosition))
            {
                if (IsPointerOverInteractiveUi(pointerPosition))
                {
                    BTAudioService.PlayImmediateUiPressFeedback();
                }
            }

            if (BTPrimaryPointerInput.TryGetPrimaryClick(out pointerPosition)
                && !IsPointerOverInteractiveUi(pointerPosition))
            {
                StartCoroutine(PlayMouseClickFallbackAfterClickHandlers(Time.frameCount));
            }

            if (BTPrimaryPointerInput.TryGetPrimaryUpOrCanceled(out _))
            {
                StartCoroutine(ClearImmediateUiPressFeedbackSuppressionAtEndOfFrame());
            }
        }

        private IEnumerator PlayMouseClickFallbackAfterClickHandlers(int clickFrame)
        {
            yield return new WaitForEndOfFrame();

            if (BTAudioService.WasExplicitUiClickFeedbackPlayedSinceFrame(clickFrame))
            {
                yield break;
            }

            yield return null;

            if (!BTAudioService.WasExplicitUiClickFeedbackPlayedSinceFrame(clickFrame))
            {
                BTAudioService.PlaySfx(BTAudioCueId.MouseClick);
            }
        }

        private IEnumerator ClearImmediateUiPressFeedbackSuppressionAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            BTAudioService.ClearPendingImmediateUiPressFeedbackSuppression();
        }

        private void UpdateCursorState()
        {
            bool shouldUseInteractiveCursor = IsPointerOverInteractiveTarget();
            if (shouldUseInteractiveCursor == isUsingInteractiveCursor && hasAppliedAnyCursor)
            {
                return;
            }

            ApplyCursor(shouldUseInteractiveCursor);
        }

        private void ApplyCursor(bool useInteractiveCursor)
        {
            LoadCursorTexturesIfNeeded();
            Texture2D texture = useInteractiveCursor ? interactiveCursorTexture : normalCursorTexture;
            Vector2 hotspot = useInteractiveCursor ? interactiveCursorHotspot : normalCursorHotspot;

            Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
            isUsingInteractiveCursor = useInteractiveCursor;
            hasAppliedAnyCursor = true;
        }

        private bool IsPointerOverInteractiveTarget()
        {
            return IsPointerOverInteractiveUi() || IsPointerOverInteractiveWorldObject();
        }

        private bool IsPointerOverInteractiveUi()
        {
            return IsPointerOverInteractiveUi(Input.mousePosition);
        }

        private bool IsPointerOverInteractiveUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            if (pointerEventData == null || pointerEventSystem != eventSystem)
            {
                pointerEventData = new PointerEventData(eventSystem);
                pointerEventSystem = eventSystem;
            }

            pointerEventData.Reset();
            pointerEventData.position = screenPosition;
            uiRaycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResults);

            for (int i = 0; i < uiRaycastResults.Count; i++)
            {
                GameObject target = uiRaycastResults[i].gameObject;
                if (target == null)
                {
                    continue;
                }

                Selectable selectable = target.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.IsActive())
                {
                    return true;
                }

                if (ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPointerOverInteractiveWorldObject()
        {
            Camera cameraToUse = Camera.main;
            if (cameraToUse == null)
            {
                return false;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, WorldRayDistance, worldHoverLayerMask, triggerInteraction);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            System.Array.Sort(hits, CompareRaycastHitsByDistance);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hitCollider = hits[i].collider;
                if (hitCollider != null && HasInteractiveWorldComponent(hitCollider))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasInteractiveWorldComponent(Collider hitCollider)
        {
            return hitCollider.GetComponentInParent<Unit>() != null
                || hitCollider.GetComponentInParent<KeyItem>() != null
                || hitCollider.GetComponentInParent<BossKeyItem>() != null
                || hitCollider.GetComponentInParent<BossDoor>() != null
                || hitCollider.GetComponentInParent<InteractableStairs>() != null
                || hitCollider.GetComponentInParent<HealthPotionPickup>() != null
                || hitCollider.GetComponentInParent<SupplyPoint>() != null;
        }

        private static int CompareRaycastHitsByDistance(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }

        private void LoadCursorTexturesIfNeeded()
        {
            if (normalCursorTexture == null)
            {
                normalCursorTexture = Resources.Load<Texture2D>(NormalCursorResourcePath);
            }

            if (interactiveCursorTexture == null)
            {
                interactiveCursorTexture = Resources.Load<Texture2D>(InteractiveCursorResourcePath);
            }

#if UNITY_EDITOR
            if (normalCursorTexture == null)
            {
                normalCursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalCursorAssetPath);
            }

            if (interactiveCursorTexture == null)
            {
                interactiveCursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(InteractiveCursorAssetPath);
            }
#endif
        }
    }

    internal static class BTPrimaryPointerInput
    {
        private const float DefaultTouchTapMaxMovementPixels = 18f;

        private static readonly List<RaycastResult> UiRaycastResults = new List<RaycastResult>(16);
        private static PointerEventData pointerEventData;
        private static EventSystem pointerEventSystem;
        private static int trackedTouchFingerId = -1;
        private static Vector2 trackedTouchStartPosition;
        private static bool trackedTouchStartedOverUi;
        private static bool trackedTouchExceededTapMovement;
        private static int lastPrimaryClickFrame = -1;
        private static Vector2 lastPrimaryClickPosition;
        private static int lastPrimaryDragReleaseFrame = -1;
        private static Vector2 lastPrimaryDragReleasePosition;

        public static bool TryGetPrimaryDown(out Vector2 screenPosition)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    TrackTouchStart(touch);
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        public static bool TryGetPrimaryClick(out Vector2 screenPosition)
        {
            UpdateTrackedTouchClick();
            if (lastPrimaryClickFrame == Time.frameCount)
            {
                screenPosition = lastPrimaryClickPosition;
                return true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                lastPrimaryClickFrame = Time.frameCount;
                lastPrimaryClickPosition = Input.mousePosition;
                screenPosition = lastPrimaryClickPosition;
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        public static bool TryGetPrimaryDragRelease(out Vector2 screenPosition)
        {
            UpdateTrackedTouchClick();
            if (lastPrimaryDragReleaseFrame == Time.frameCount)
            {
                screenPosition = lastPrimaryDragReleasePosition;
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        public static bool TryGetPrimaryUpOrCanceled(out Vector2 screenPosition)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        private static void UpdateTrackedTouchClick()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    TrackTouchStart(touch);
                    continue;
                }

                if (touch.fingerId != trackedTouchFingerId)
                {
                    continue;
                }

                UpdateTrackedTouchMovement(touch.position);
                if (touch.phase == TouchPhase.Ended)
                {
                    if (!trackedTouchStartedOverUi && !trackedTouchExceededTapMovement)
                    {
                        lastPrimaryClickFrame = Time.frameCount;
                        lastPrimaryClickPosition = touch.position;
                    }
                    else if (!trackedTouchStartedOverUi && trackedTouchExceededTapMovement)
                    {
                        lastPrimaryDragReleaseFrame = Time.frameCount;
                        lastPrimaryDragReleasePosition = touch.position;
                    }

                    ClearTrackedTouch();
                    return;
                }

                if (touch.phase == TouchPhase.Canceled)
                {
                    ClearTrackedTouch();
                    return;
                }
            }
        }

        private static void TrackTouchStart(Touch touch)
        {
            trackedTouchFingerId = touch.fingerId;
            trackedTouchStartPosition = touch.position;
            trackedTouchStartedOverUi = IsPointerOverUi(touch.position);
            trackedTouchExceededTapMovement = false;
        }

        private static void UpdateTrackedTouchMovement(Vector2 screenPosition)
        {
            if (trackedTouchFingerId < 0 || trackedTouchExceededTapMovement)
            {
                return;
            }

            float threshold = ResolveTouchTapMaxMovementPixels();
            trackedTouchExceededTapMovement = (screenPosition - trackedTouchStartPosition).sqrMagnitude > threshold * threshold;
        }

        private static float ResolveTouchTapMaxMovementPixels()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return DefaultTouchTapMaxMovementPixels;
            }

            return Mathf.Max(DefaultTouchTapMaxMovementPixels, eventSystem.pixelDragThreshold * 2f);
        }

        private static void ClearTrackedTouch()
        {
            trackedTouchFingerId = -1;
            trackedTouchStartPosition = Vector2.zero;
            trackedTouchStartedOverUi = false;
            trackedTouchExceededTapMovement = false;
        }

        public static bool IsPointerOverUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            if (pointerEventData == null || pointerEventSystem != eventSystem)
            {
                pointerEventData = new PointerEventData(eventSystem);
                pointerEventSystem = eventSystem;
            }

            pointerEventData.Reset();
            pointerEventData.position = screenPosition;
            UiRaycastResults.Clear();
            eventSystem.RaycastAll(pointerEventData, UiRaycastResults);
            return UiRaycastResults.Count > 0;
        }
    }
}
