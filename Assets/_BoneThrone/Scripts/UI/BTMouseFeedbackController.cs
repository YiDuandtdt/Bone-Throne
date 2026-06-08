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

            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(PlayMouseClickFallbackAfterClickHandlers(Time.frameCount, IsPointerOverInteractiveUi()));
            }
        }

        private IEnumerator PlayMouseClickFallbackAfterClickHandlers(int clickFrame, bool startedOverInteractiveUi)
        {
            yield return new WaitForEndOfFrame();

            if (startedOverInteractiveUi || BTAudioService.WasExplicitUiClickFeedbackPlayedSinceFrame(clickFrame))
            {
                yield break;
            }

            yield return null;

            if (!BTAudioService.WasExplicitUiClickFeedbackPlayedSinceFrame(clickFrame))
            {
                BTAudioService.PlaySfx(BTAudioCueId.MouseClick);
            }
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
            pointerEventData.position = Input.mousePosition;
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
}
