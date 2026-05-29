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

        [SerializeField] private Animator animator;
        [SerializeField] private bool warnWhenAnimatorMissing = true;

        private bool hasAnimator;
        private bool warnedMissingAnimator;
        private bool warnedMissingMoveSpeed;
        private bool warnedMissingBasicAttack;
        private bool warnedMissingSkill;
        private bool warnedMissingHit;
        private bool warnedMissingDefend;
        private bool warnedMissingUsePotion;
        private bool warnedMissingIsDead;

        private void Awake()
        {
            ResolveAnimator();
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
            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter("MoveSpeed", AnimatorControllerParameterType.Float, ref warnedMissingMoveSpeed))
            {
                return;
            }

            animator.SetFloat(MoveSpeedHash, Mathf.Max(0f, speed));
        }

        public void PlayBasicAttack()
        {
            SetTrigger("BasicAttack", BasicAttackHash, ref warnedMissingBasicAttack);
        }

        public void PlaySkill()
        {
            SetTrigger("Skill", SkillHash, ref warnedMissingSkill);
        }

        public void PlayHit()
        {
            SetTrigger("Hit", HitHash, ref warnedMissingHit);
        }

        public void PlayDefend()
        {
            SetTrigger("Defend", DefendHash, ref warnedMissingDefend);
        }

        public void PlayUsePotion()
        {
            SetTrigger("UsePotion", UsePotionHash, ref warnedMissingUsePotion);
        }

        public void SetDead(bool isDead)
        {
            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter("IsDead", AnimatorControllerParameterType.Bool, ref warnedMissingIsDead))
            {
                return;
            }

            animator.SetBool(IsDeadHash, isDead);
        }

        private void SetTrigger(string parameterName, int parameterHash, ref bool warnedMissingParameter)
        {
            if (!CanUseAnimator())
            {
                return;
            }

            if (!HasParameter(parameterName, AnimatorControllerParameterType.Trigger, ref warnedMissingParameter))
            {
                return;
            }

            animator.SetTrigger(parameterHash);
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
    }
}
