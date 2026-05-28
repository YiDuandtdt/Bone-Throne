using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Applies direct HP damage and delegates death/tile release to Unit.
    /// It does not implement damage types, armor, buffs, skills, or alternate death state.
    /// </summary>
    public sealed class DamageResolver : MonoBehaviour
    {
        [SerializeField] private CombatLog combatLog;

        public bool ApplyDamage(Unit target, int damage)
        {
            if (target == null)
            {
                Debug.LogWarning("Damage ignored because target Unit is missing.", this);
                return false;
            }

            if (target.RuntimeState == null)
            {
                Debug.LogWarning("Damage ignored because target Unit has no runtime state.", target);
                return false;
            }

            int clampedDamage = Mathf.Max(0, damage);
            int amplifiedDamage = clampedDamage;
            UnitDamageAmplifyState amplifyState = target.GetComponent<UnitDamageAmplifyState>();
            if (amplifyState != null && amplifyState.HasAmplify)
            {
                int bonusDamage;
                if (amplifyState.TryConsumeAmplify(out bonusDamage))
                {
                    amplifiedDamage = Mathf.Max(0, clampedDamage + bonusDamage);
                    ResolveCombatLog();
                    if (combatLog != null)
                    {
                        combatLog.LogDamageAmplified(target, clampedDamage, bonusDamage, amplifiedDamage);
                    }
                }
            }

            int finalDamage = amplifiedDamage;
            UnitDefenseState defenseState = target.GetComponent<UnitDefenseState>();
            if (defenseState != null && defenseState.IsDefending)
            {
                int reducedAmount;
                if (defenseState.TryConsumeReduction(amplifiedDamage, out finalDamage, out reducedAmount))
                {
                    ResolveCombatLog();
                    if (combatLog != null)
                    {
                        combatLog.LogDamageReduced(target, amplifiedDamage, finalDamage, reducedAmount);
                    }
                }
            }

            int nextHp = Mathf.Max(0, target.RuntimeState.CurrentHp - finalDamage);
            target.RuntimeState.SetCurrentHp(nextHp);

            if (target.RuntimeState.CurrentHp <= 0)
            {
                target.MarkDeadAndReleaseTile();
                return true;
            }

            return false;
        }

        private void ResolveCombatLog()
        {
            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }
    }
}
