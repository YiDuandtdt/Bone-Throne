using System.Collections;
using BoneThrone.Audio;
using BoneThrone.Core;
using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.UI;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal Phase 10 stairs interaction with hover material feedback and two-click confirmation.
    /// It does not implement a formal UI confirmation panel, scene loading, boss doors, or networking.
    /// </summary>
    public sealed class InteractableStairs : MonoBehaviour
    {
        private const float MinimumClickColliderWidth = 1.5f;
        private const float MinimumClickColliderHeight = 0.75f;
        private const float MinimumClickColliderDepth = 1.5f;
        private const float MinimumPracticalInteractionRange = 3f;
        private const float ProximityPollInterval = 0.15f;

        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private PromptView promptView;
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private Renderer[] feedbackRenderers;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material hoverMaterial;
        [SerializeField] private bool requireSecondClickConfirmation = true;
        [SerializeField] private bool autoEnterFinalLevelOnProximity = true;
        [SerializeField] private float interactionRange = 2.5f;

        private bool confirmationPending;
        private float nextProximityPollTime;

        public bool ConfirmationPending
        {
            get { return confirmationPending; }
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureRuntimeClickCollider();
            EnsureRuntimeProximityTrigger();
        }

        private void OnEnable()
        {
            nextProximityPollTime = 0f;
        }

        private void Update()
        {
            if (!autoEnterFinalLevelOnProximity || Time.time < nextProximityPollTime)
            {
                return;
            }

            nextProximityPollTime = Time.time + ProximityPollInterval;
            Unit interactor = FindNearestLivingPlayerInInteractionRange();
            if (interactor != null)
            {
                TryAutoEnterFinalLevel(interactor);
            }
        }

        private void OnMouseEnter()
        {
            ApplyHoverFeedback(true);
        }

        private void OnMouseExit()
        {
            ApplyHoverFeedback(false);
        }

        private void OnMouseDown()
        {
            ResolveReferences();
            Unit interactor = selectionManager != null ? selectionManager.SelectedUnit : null;
            TryRequestEnterNextLevel(interactor);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryAutoEnterFinalLevelFromTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAutoEnterFinalLevelFromTrigger(other);
        }

        public bool TryRequestEnterNextLevel(Unit interactor)
        {
            ResolveReferences();

            if (interactor != null && (interactor.Faction != UnitFaction.Player || !interactor.IsAlive))
            {
                Debug.LogWarning("Stairs interaction rejected because the interactor is not a living player unit.", interactor);
                ShowPrompt("请选择一名存活的玩家角色后再使用楼梯。", 2f);
                return false;
            }

            if (progressionService == null)
            {
                Debug.LogWarning("Stairs interaction failed because LevelProgressionService is missing.", this);
                ShowPrompt("楼梯暂时无法使用：关卡进度未绑定。", 2f);
                return false;
            }

            string reason;
            if (!progressionService.CanEnterNextLevel(out reason))
            {
                confirmationPending = false;
                Debug.LogWarning("Stairs interaction rejected: " + reason, this);
                ShowPrompt(reason, 2f);
                return false;
            }

            if (requireSecondClickConfirmation && !confirmationPending)
            {
                confirmationPending = true;
                Debug.Log("Stairs confirmation pending. Click stairs again or use ContextMenu confirm to enter next level.", this);
                ShowPrompt("楼梯：再次点击即可进入下一层。", 2.5f);
                return false;
            }

            return ConfirmEnterNextLevelForTest();
        }

        [ContextMenu("Phase 10/Confirm Enter Next Level")]
        public void ConfirmEnterNextLevelContextMenu()
        {
            ConfirmEnterNextLevelForTest();
        }

        public bool ConfirmEnterNextLevelForTest()
        {
            ResolveReferences();

            if (progressionService == null)
            {
                Debug.LogWarning("Cannot confirm stairs because LevelProgressionService is missing.", this);
                ShowPrompt("楼梯暂时无法使用：关卡进度未绑定。", 2f);
                return false;
            }

            string sceneBefore = SceneManager.GetActiveScene().name;
            if (IsFinalLevelScene())
            {
                bool finalVictory = TryShowFinalVictory();
                confirmationPending = false;
                Debug.Log("Final stairs victory result: " + finalVictory + ".", this);
                return finalVictory;
            }

            BTAudioService.PlayLoop(BTAudioCueId.StairsLoop, this);
            bool entered = progressionService.TryEnterNextLevel();
            confirmationPending = false;
            if (!entered)
            {
                BTAudioService.StopLoop(this);
                ShowPrompt("楼梯暂时无法进入：进度条件还没有满足。", 2f);
            }
            else
            {
                StartCoroutine(StopStairsLoopIfSceneDidNotChange(sceneBefore));
            }

            Debug.Log("Stairs confirm result: " + entered + ".", this);
            return entered;
        }

        private IEnumerator StopStairsLoopIfSceneDidNotChange(string sceneBefore)
        {
            yield return new WaitForSeconds(1f);
            if (SceneManager.GetActiveScene().name == sceneBefore)
            {
                BTAudioService.StopLoop(this);
            }
        }

        [ContextMenu("Phase 10/Cancel Pending Confirmation")]
        public void CancelConfirmation()
        {
            confirmationPending = false;
            Debug.Log("Stairs confirmation cancelled.", this);
            ShowPrompt("已取消进入下一层。", 1.5f);
        }

        private void ApplyHoverFeedback(bool isHovering)
        {
            if (feedbackRenderers == null || feedbackRenderers.Length == 0)
            {
                return;
            }

            Material targetMaterial = isHovering ? hoverMaterial : normalMaterial;
            if (targetMaterial == null)
            {
                return;
            }

            for (int i = 0; i < feedbackRenderers.Length; i++)
            {
                if (feedbackRenderers[i] != null)
                {
                    feedbackRenderers[i].material = targetMaterial;
                }
            }
        }

        private void ResolveReferences()
        {
            if (selectionManager == null)
            {
                selectionManager = Object.FindFirstObjectByType<SelectionManager>();
            }

            if (progressionService == null)
            {
                progressionService = Object.FindFirstObjectByType<LevelProgressionService>();
            }

            if (promptView == null)
            {
                promptView = Object.FindFirstObjectByType<PromptView>();
            }

            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }
        }

        private void ShowPrompt(string message, float duration)
        {
            ResolveReferences();

            if (promptView != null)
            {
                promptView.ShowOverride(message, duration);
            }
        }

        private void TryAutoEnterFinalLevelFromTrigger(Collider other)
        {
            if (other == null)
            {
                return;
            }

            Unit interactor = other.GetComponentInParent<Unit>();
            if (interactor != null)
            {
                TryAutoEnterFinalLevel(interactor);
            }
        }

        private bool TryAutoEnterFinalLevel(Unit interactor)
        {
            if (interactor == null || interactor.Faction != UnitFaction.Player || !interactor.IsAlive)
            {
                return false;
            }

            if (!IsFinalLevelScene())
            {
                return false;
            }

            ResolveReferences();
            if (progressionService == null)
            {
                return false;
            }

            string reason;
            if (!progressionService.CanEnterNextLevel(out reason))
            {
                return false;
            }

            confirmationPending = false;
            return ConfirmEnterNextLevelForTest();
        }

        private bool TryShowFinalVictory()
        {
            if (progressionService == null)
            {
                return false;
            }

            string reason;
            if (!progressionService.CanEnterNextLevel(out reason))
            {
                ShowPrompt(reason, 2f);
                return false;
            }

            ResolveReferences();
            if (outcomeService == null)
            {
                outcomeService = gameObject.AddComponent<GameOutcomeService>();
            }

            if (outcomeService == null)
            {
                Debug.LogWarning("Final stairs cannot show victory because GameOutcomeService is missing.", this);
                return false;
            }

            if (!outcomeService.HasOutcome)
            {
                outcomeService.SetVictory("\u961F\u4F0D\u767B\u4E0A\u7EC8\u70B9\u53F0\u9636\uFF0C\u53D6\u5F97\u4E86\u80DC\u5229\u3002");
            }

            outcomeService.ForceShowCurrentOutcomePopup();
            return true;
        }

        private bool IsFinalLevelScene()
        {
            int levelIndex = MenuProgressionState.GetLevelIndexForScene(SceneManager.GetActiveScene().name);
            return levelIndex >= 3;
        }

        private Unit FindNearestLivingPlayerInInteractionRange()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Unit nearestUnit = null;
            float interactionRangeSqr = GetEffectiveInteractionRange();
            interactionRangeSqr *= interactionRangeSqr;
            float nearestDistanceSqr = interactionRangeSqr;
            Vector3 interactionPosition = GetInteractionPosition();

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Faction != UnitFaction.Player || !unit.IsAlive || !unit.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 unitPosition = unit.transform.position;
                float deltaX = unitPosition.x - interactionPosition.x;
                float deltaZ = unitPosition.z - interactionPosition.z;
                float sqrDistance = deltaX * deltaX + deltaZ * deltaZ;
                if (sqrDistance <= nearestDistanceSqr)
                {
                    nearestDistanceSqr = sqrDistance;
                    nearestUnit = unit;
                }
            }

            return nearestUnit;
        }

        private Vector3 GetInteractionPosition()
        {
            Bounds visualBounds;
            return TryGetVisualBounds(out visualBounds) ? visualBounds.center : transform.position;
        }

        private float GetEffectiveInteractionRange()
        {
            return Mathf.Max(MinimumPracticalInteractionRange, interactionRange);
        }

        private void EnsureRuntimeClickCollider()
        {
            if (!TryGetVisualBounds(out Bounds visualBounds))
            {
                return;
            }

            BoxCollider clickCollider = GetComponent<BoxCollider>();
            if (clickCollider == null)
            {
                clickCollider = gameObject.AddComponent<BoxCollider>();
            }

            clickCollider.isTrigger = false;
            clickCollider.center = transform.InverseTransformPoint(visualBounds.center);
            clickCollider.size = GetLocalBoundsSize(visualBounds);
        }

        private void EnsureRuntimeProximityTrigger()
        {
            SphereCollider triggerCollider = GetComponent<SphereCollider>();
            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<SphereCollider>();
            }

            Bounds visualBounds;
            triggerCollider.isTrigger = true;
            triggerCollider.center = TryGetVisualBounds(out visualBounds)
                ? transform.InverseTransformPoint(visualBounds.center)
                : Vector3.zero;
            triggerCollider.radius = GetEffectiveInteractionRange();
        }

        private bool TryGetVisualBounds(out Bounds visualBounds)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            visualBounds = default;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    visualBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    visualBounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private Vector3 GetLocalBoundsSize(Bounds worldBounds)
        {
            Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;

            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 worldCorner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 localCorner = transform.InverseTransformPoint(worldCorner);
                        min = Vector3.Min(min, localCorner);
                        max = Vector3.Max(max, localCorner);
                    }
                }
            }

            Vector3 size = max - min;
            size.x = Mathf.Max(MinimumClickColliderWidth, Mathf.Abs(size.x));
            size.y = Mathf.Max(MinimumClickColliderHeight, Mathf.Abs(size.y));
            size.z = Mathf.Max(MinimumClickColliderDepth, Mathf.Abs(size.z));
            return size;
        }
    }
}
