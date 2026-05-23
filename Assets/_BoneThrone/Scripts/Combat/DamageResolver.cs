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
            int nextHp = Mathf.Max(0, target.RuntimeState.CurrentHp - clampedDamage);
            target.RuntimeState.SetCurrentHp(nextHp);

            if (target.RuntimeState.CurrentHp <= 0)
            {
                target.MarkDeadAndReleaseTile();
                return true;
            }

            return false;
        }
    }
}
