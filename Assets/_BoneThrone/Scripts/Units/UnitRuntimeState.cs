using System;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Minimal runtime unit state for HP and death only.
    /// </summary>
    [Serializable]
    public sealed class UnitRuntimeState
    {
        [SerializeField] private int currentHp;
        [SerializeField] private bool isDead;

        public int CurrentHp
        {
            get { return currentHp; }
        }

        public bool IsDead
        {
            get { return isDead; }
        }

        public bool IsAlive
        {
            get { return !isDead && currentHp > 0; }
        }

        public void Initialize(UnitStats stats)
        {
            int maxHp = stats != null ? stats.GetClampedMaxHp() : 1;
            currentHp = maxHp;
            isDead = false;
        }

        public void SetCurrentHp(int value)
        {
            currentHp = Mathf.Max(0, value);
            isDead = currentHp <= 0;
        }

        public void MarkDead()
        {
            currentHp = 0;
            isDead = true;
        }
    }
}
