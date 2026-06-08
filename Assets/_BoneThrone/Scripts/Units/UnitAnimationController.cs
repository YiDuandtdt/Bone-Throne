using System.Collections;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Presentation-only bridge between gameplay systems and the visual Animator.
    /// It never drives gameplay, turn flow, damage, cooldown, or tile state.
    /// </summary>
    public sealed class UnitAnimationController : MonoBehaviour
    {
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int BasicAttackHash = Animator.StringToHash("BasicAttack");
        private static readonly int SkillHash = Animator.StringToHash("Skill");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DefendHash = Animator.StringToHash("Defend");
        private static readonly int UsePotionHash = Animator.StringToHash("UsePotion");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int IsDefendingHash = Animator.StringToHash("IsDefending");
        private static readonly int IdleStateHash = Animator.StringToHash("Idle");
        private static readonly int MoveStateHash = Animator.StringToHash("Move");
        private static readonly int BasicAttackStateHash = Animator.StringToHash("BasicAttack");
        private static readonly int SkillCastStateHash = Animator.StringToHash("SkillCast");
        private static readonly int HitStateHash = Animator.StringToHash("Hit");
        private static readonly int DefendStateHash = Animator.StringToHash("Defend");
        private static readonly int DefendHoldStateHash = Animator.StringToHash("DefendHold");
        private static readonly int UsePotionStateHash = Animator.StringToHash("UsePotion");
        private static readonly int DeadStateHash = Animator.StringToHash("Dead");

        [SerializeField] private Animator animator;
        [SerializeField] private bool warnWhenAnimatorMissing = true;
        [SerializeField] private bool debugLogging;
        [SerializeField] private bool playStatesDirectly;
        [SerializeField] private float directCrossFadeDuration = 0.03f;
        [SerializeField] private float faceTurnDuration = 0.12f;

        private bool hasAnimator;
        private bool warnedMissingAnimator;
        private bool warnedMissingMoveSpeed;
        private bool warnedMissingBasicAttack;
        private bool warnedMissingSkill;
        private bool warnedMissingHit;
        private bool warnedMissingDefend;
        private bool warnedMissingUsePotion;
        private bool warnedMissingIsDead;
        private bool warnedMissingIsDefending;
        private bool presentationIsDead;
        private Coroutine faceRoutine;
        private Coroutine temporaryAnimatorSpeedRoutine;
        private float animatorSpeedBeforeTemporaryOverride = 1f;

        private void OnDisable()
        {
            RestoreTemporaryAnimatorSpeed();
        }

        private void Awake()
        {
            ResolveAnimator();
            LogAnimatorSnapshot("Awake");
        }

        private void Start()
        {
            LogAnimatorSnapshot("Start");
        }

        private void OnValidate()
        {
            if (animator == null)
            {
                ResolveAnimator();
            }
        }

        public void SetMoveSpeed(float speed)
        {
            LogMethodStart("SetMoveSpeed", "MoveSpeed");
            if (presentationIsDead)
            {
                LogDebug("SetMoveSpeed ignored because presentation is dead.");
                return;
            }

            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter("MoveSpeed", AnimatorControllerParameterType.Float, ref warnedMissingMoveSpeed))
            {
                return;
            }

            animator.SetFloat(MoveSpeedHash, Mathf.Max(0f, speed));
            LogDebug("Set MoveSpeed=" + Mathf.Max(0f, speed) + ".");

            if (ShouldPlayStatesDirectly())
            {
                PlayState(speed > 0.1f ? MoveStateHash : IdleStateHash, speed > 0.1f ? "Move" : "Idle");
            }
        }

        public void PlayBasicAttack()
        {
            SetTrigger("BasicAttack", BasicAttackHash, BasicAttackStateHash, "BasicAttack", ref warnedMissingBasicAttack);
        }

        public void PlayBasicAttack(float speedMultiplier, float restoreAfterSeconds)
        {
            SetTrigger("BasicAttack", BasicAttackHash, BasicAttackStateHash, "BasicAttack", ref warnedMissingBasicAttack, speedMultiplier, restoreAfterSeconds);
        }

        public void PlaySkill()
        {
            SetTrigger("Skill", SkillHash, SkillCastStateHash, "SkillCast", ref warnedMissingSkill);
        }

        public void PlaySkill(float speedMultiplier, float restoreAfterSeconds)
        {
            SetTrigger("Skill", SkillHash, SkillCastStateHash, "SkillCast", ref warnedMissingSkill, speedMultiplier, restoreAfterSeconds);
        }

        public void PlayHit()
        {
            SetTrigger("Hit", HitHash, HitStateHash, "Hit", ref warnedMissingHit);
        }

        public void PlayDefend()
        {
            SetTrigger("Defend", DefendHash, DefendStateHash, "Defend", ref warnedMissingDefend);
        }

        public void PlayUsePotion()
        {
            SetTrigger("UsePotion", UsePotionHash, UsePotionStateHash, "UsePotion", ref warnedMissingUsePotion);
        }

        public void SetDead(bool isDead)
        {
            LogMethodStart("SetDead", "IsDead");
            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter("IsDead", AnimatorControllerParameterType.Bool, ref warnedMissingIsDead))
            {
                return;
            }

            presentationIsDead = isDead;
            SetDefendingParameter(false);
            animator.SetBool(IsDeadHash, isDead);
            LogDebug("Set IsDead=" + isDead + ".");

            if (ShouldPlayStatesDirectly())
            {
                PlayState(isDead ? DeadStateHash : IdleStateHash, isDead ? "Dead" : "Idle");
            }
        }

        public void SetDefending(bool isDefending)
        {
            LogMethodStart("SetDefending", "IsDefending");
            if (!CanUseAnimator())
            {
                return;
            }

            SetDefendingParameter(isDefending);
            if (presentationIsDead)
            {
                LogDebug("SetDefending visual state ignored because presentation is dead.");
                return;
            }

            if (!isDefending && ShouldPlayStatesDirectly())
            {
                PlayState(IdleStateHash, "Idle");
            }
        }

        public void FaceTowards(Vector3 worldPosition)
        {
            FaceTowardsDirection(worldPosition - transform.position);
        }

        public void FaceTowardsDirection(Vector3 worldDelta)
        {
            worldDelta.y = 0f;
            if (worldDelta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (faceRoutine != null)
            {
                StopCoroutine(faceRoutine);
            }

            faceRoutine = StartCoroutine(FaceRoutine(GetCardinalDirection(worldDelta)));
        }

        [ContextMenu("BT Test/BasicAttack")]
        private void DebugPlayBasicAttack()
        {
            PlayBasicAttack();
        }

        [ContextMenu("BT Test/Skill")]
        private void DebugPlaySkill()
        {
            PlaySkill();
        }

        [ContextMenu("BT Test/Hit")]
        private void DebugPlayHit()
        {
            PlayHit();
        }

        [ContextMenu("BT Test/Defend")]
        private void DebugPlayDefend()
        {
            PlayDefend();
        }

        [ContextMenu("BT Test/Defending True")]
        private void DebugSetDefendingTrue()
        {
            SetDefending(true);
        }

        [ContextMenu("BT Test/Defending False")]
        private void DebugSetDefendingFalse()
        {
            SetDefending(false);
        }

        [ContextMenu("BT Test/UsePotion")]
        private void DebugPlayUsePotion()
        {
            PlayUsePotion();
        }

        [ContextMenu("BT Test/Dead True")]
        private void DebugSetDeadTrue()
        {
            SetDead(true);
        }

        [ContextMenu("BT Test/Dead False")]
        private void DebugSetDeadFalse()
        {
            SetDead(false);
        }

        [ContextMenu("BT Test/MoveSpeed 1")]
        private void DebugSetMoveSpeedOne()
        {
            SetMoveSpeed(1f);
        }

        [ContextMenu("BT Test/MoveSpeed 0")]
        private void DebugSetMoveSpeedZero()
        {
            SetMoveSpeed(0f);
        }

        private void SetTrigger(string parameterName, int parameterHash, int stateHash, string stateName, ref bool warnedMissingParameter)
        {
            SetTrigger(parameterName, parameterHash, stateHash, stateName, ref warnedMissingParameter, 1f, 0f);
        }

        private void SetTrigger(
            string parameterName,
            int parameterHash,
            int stateHash,
            string stateName,
            ref bool warnedMissingParameter,
            float speedMultiplier,
            float restoreAfterSeconds)
        {
            LogMethodStart("SetTrigger", parameterName);
            if (presentationIsDead)
            {
                LogDebug("Trigger '" + parameterName + "' ignored because presentation is dead.");
                return;
            }

            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter(parameterName, AnimatorControllerParameterType.Trigger, ref warnedMissingParameter))
            {
                return;
            }

            animator.SetTrigger(parameterHash);
            ApplyTemporaryAnimatorSpeed(speedMultiplier, restoreAfterSeconds);
            LogDebug("Set trigger " + parameterName + ".");

            if (ShouldPlayStatesDirectly())
            {
                PlayState(stateHash, stateName);
            }
        }

        private void ApplyTemporaryAnimatorSpeed(float speedMultiplier, float restoreAfterSeconds)
        {
            if (animator == null || restoreAfterSeconds <= 0f || Mathf.Approximately(speedMultiplier, 1f))
            {
                return;
            }

            RestoreTemporaryAnimatorSpeed();
            animatorSpeedBeforeTemporaryOverride = animator.speed;
            animator.speed = Mathf.Clamp(speedMultiplier, 0.25f, 2f);
            temporaryAnimatorSpeedRoutine = StartCoroutine(RestoreAnimatorSpeedAfterDelay(restoreAfterSeconds));
        }

        private IEnumerator RestoreAnimatorSpeedAfterDelay(float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            temporaryAnimatorSpeedRoutine = null;
            RestoreAnimatorSpeedValue();
        }

        private void RestoreTemporaryAnimatorSpeed()
        {
            if (temporaryAnimatorSpeedRoutine != null)
            {
                StopCoroutine(temporaryAnimatorSpeedRoutine);
                temporaryAnimatorSpeedRoutine = null;
            }

            RestoreAnimatorSpeedValue();
        }

        private void RestoreAnimatorSpeedValue()
        {
            if (animator != null)
            {
                animator.speed = Mathf.Max(0.01f, animatorSpeedBeforeTemporaryOverride);
            }
        }

        private bool ShouldPlayStatesDirectly()
        {
            return debugLogging && playStatesDirectly;
        }

        private void SetDefendingParameter(bool isDefending)
        {
            if (animator == null)
            {
                return;
            }

            if (!HasParameter("IsDefending", AnimatorControllerParameterType.Bool, ref warnedMissingIsDefending))
            {
                return;
            }

            animator.SetBool(IsDefendingHash, isDefending);
            LogDebug("Set IsDefending=" + isDefending + ".");
        }

        private bool CanUseAnimator()
        {
            if (!hasAnimator || animator == null)
            {
                ResolveAnimator();
            }

            if (hasAnimator && animator != null)
            {
                return true;
            }

            if (warnWhenAnimatorMissing && !warnedMissingAnimator)
            {
                warnedMissingAnimator = true;
                Debug.LogWarning("UnitAnimationController could not find an Animator on this unit or its children.", this);
            }

            return false;
        }

        private void ResolveAnimator()
        {
            if (animator == null)
            {
                Animator[] animators = GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i] != null && animators[i].gameObject != gameObject)
                    {
                        animator = animators[i];
                        break;
                    }
                }

                if (animator == null && animators.Length > 0)
                {
                    animator = animators[0];
                }
            }

            hasAnimator = animator != null;
            if (hasAnimator)
            {
                string controllerName = animator.runtimeAnimatorController != null
                    ? animator.runtimeAnimatorController.name
                    : "none";
                LogDebug("Resolved Animator '" + animator.name + "' with controller '" + controllerName + "'.");
            }
        }

        private bool HasParameter(string parameterName, AnimatorControllerParameterType expectedType, ref bool warnedMissingParameter)
        {
            if (animator == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter != null && parameter.name == parameterName && parameter.type == expectedType)
                {
                    return true;
                }
            }

            if (!warnedMissingParameter)
            {
                warnedMissingParameter = true;
                Debug.LogWarning("Animator is missing parameter '" + parameterName + "' of type " + expectedType + ".", animator);
            }

            return false;
        }

        private void PlayState(int stateHash, string stateName)
        {
            if (animator == null)
            {
                LogDebug("CrossFade skipped for state '" + stateName + "' because Animator is null.");
                return;
            }

            if (!animator.HasState(0, stateHash))
            {
                LogDebug("Animator has no layer 0 state '" + stateName + "' hash=" + stateHash + ".");
                return;
            }

            animator.CrossFadeInFixedTime(stateHash, Mathf.Max(0f, directCrossFadeDuration), 0, 0f);
            LogDebug("CrossFade state '" + stateName + "' hash=" + stateHash + " duration=" + Mathf.Max(0f, directCrossFadeDuration) + ".");
            StartCoroutine(LogStateNextFrame(stateName));
        }

        private IEnumerator LogStateNextFrame(string requestedStateName)
        {
            if (!debugLogging)
            {
                yield break;
            }

            yield return null;

            if (animator == null || animator.layerCount <= 0)
            {
                LogDebug("Next-frame state check after '" + requestedStateName + "' skipped because Animator/layer is missing.");
                yield break;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            LogDebug(
                "Next-frame state after '"
                + requestedStateName
                + "': fullPathHash="
                + state.fullPathHash
                + " shortNameHash="
                + state.shortNameHash
                + " normalizedTime="
                + state.normalizedTime
                + ".");
        }

        private IEnumerator FaceRoutine(Vector3 cardinalDirection)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(cardinalDirection, Vector3.up);
            float duration = Mathf.Max(0f, faceTurnDuration);
            if (duration <= 0f)
            {
                transform.rotation = targetRotation;
                faceRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
            faceRoutine = null;
        }

        private static Vector3 GetCardinalDirection(Vector3 worldDelta)
        {
            if (Mathf.Abs(worldDelta.x) >= Mathf.Abs(worldDelta.z))
            {
                return worldDelta.x >= 0f ? Vector3.right : Vector3.left;
            }

            return worldDelta.z >= 0f ? Vector3.forward : Vector3.back;
        }

        private void LogMethodStart(string methodName, string parameterName)
        {
            if (!debugLogging)
            {
                return;
            }

            string controllerName = animator != null && animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.name
                : "none";
            LogDebug(
                methodName
                + " called on '"
                + gameObject.name
                + "'. animator="
                + (animator != null ? animator.gameObject.name : "null")
                + " controller="
                + controllerName
                + " parameter="
                + parameterName
                + ".");
        }

        private void LogAnimatorSnapshot(string phase)
        {
            if (!debugLogging)
            {
                return;
            }

            string animatorObjectName = animator != null ? animator.gameObject.name : "none";
            string controllerName = animator != null && animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.name
                : "none";
            string avatarName = animator != null && animator.avatar != null ? animator.avatar.name : "none";
            int layerCount = animator != null ? animator.layerCount : 0;
            string stateInfo = "unavailable";
            if (animator != null && animator.layerCount > 0)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                stateInfo = "fullPathHash=" + state.fullPathHash + " shortNameHash=" + state.shortNameHash;
            }

            LogDebug(
                phase
                + " snapshot: unit='"
                + gameObject.name
                + "' foundAnimator="
                + (animator != null)
                + " animatorObject='"
                + animatorObjectName
                + "' controller='"
                + controllerName
                + "' avatar='"
                + avatarName
                + "' layerCount="
                + layerCount
                + " layer0="
                + stateInfo
                + ".");

            LogParameterSnapshot();
        }

        private void LogParameterSnapshot()
        {
            if (!debugLogging || animator == null)
            {
                return;
            }

            LogDebug(
                "Parameters: MoveSpeed="
                + HasParameterForLog("MoveSpeed", AnimatorControllerParameterType.Float)
                + " BasicAttack="
                + HasParameterForLog("BasicAttack", AnimatorControllerParameterType.Trigger)
                + " Skill="
                + HasParameterForLog("Skill", AnimatorControllerParameterType.Trigger)
                + " Hit="
                + HasParameterForLog("Hit", AnimatorControllerParameterType.Trigger)
                + " Defend="
                + HasParameterForLog("Defend", AnimatorControllerParameterType.Trigger)
                + " UsePotion="
                + HasParameterForLog("UsePotion", AnimatorControllerParameterType.Trigger)
                + " IsDead="
                + HasParameterForLog("IsDead", AnimatorControllerParameterType.Bool)
                + " IsDefending="
                + HasParameterForLog("IsDefending", AnimatorControllerParameterType.Bool)
                + ".");
        }

        private bool HasParameterForLog(string parameterName, AnimatorControllerParameterType expectedType)
        {
            if (animator == null)
            {
                return false;
            }

            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter != null && parameter.name == parameterName && parameter.type == expectedType)
                {
                    return true;
                }
            }

            return false;
        }

        private void LogDebug(string message)
        {
            if (debugLogging)
            {
                Debug.Log("UnitAnimationController: " + message, this);
            }
        }
    }
}
