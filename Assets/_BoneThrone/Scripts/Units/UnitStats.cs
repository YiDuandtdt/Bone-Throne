using System;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Inspectable base stats for a unit. Combat and movement systems consume these later.
    /// </summary>
    [Serializable]
    public sealed class UnitStats
    {
        [SerializeField] private int maxHp = 10;
        [SerializeField] private int moveRange = 4;
        [SerializeField] private int attackModifier;
        [SerializeField] private int defense = 10;
        [SerializeField] private int baseDamage = 1;

        public int MaxHp
        {
            get { return maxHp; }
        }

        public int MoveRange
        {
            get { return moveRange; }
        }

        public int AttackModifier
        {
            get { return attackModifier; }
        }

        public int Defense
        {
            get { return defense; }
        }

        public int BaseDamage
        {
            get { return baseDamage; }
        }

        public int GetClampedMaxHp()
        {
            return Mathf.Max(1, maxHp);
        }
    }
}
