using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Minimal non-stacking bleed state. Stacks tick for their current value, then decay by one.
    /// </summary>
    public sealed class UnitBleedState : MonoBehaviour
    {
        [SerializeField] private int stacks;

        public bool HasBleed
        {
            get { return stacks > 0; }
        }

        public int Stacks
        {
            get { return Mathf.Max(0, stacks); }
        }

        public int RemainingTurns
        {
            get { return Stacks; }
        }

        public int TickDamage
        {
            get { return Stacks; }
        }

        public void ApplyBleed(int newStacks)
        {
            stacks = Mathf.Max(Stacks, Mathf.Max(0, newStacks));
        }

        public void ApplyBleed(int durationTurns, int damagePerTick)
        {
            ApplyBleed(durationTurns);
        }

        public bool TryConsumeTick(out int damage)
        {
            damage = 0;
            if (stacks <= 0)
            {
                return false;
            }

            damage = Stacks;
            stacks = Mathf.Max(0, stacks - 1);
            return damage > 0;
        }
    }
}
