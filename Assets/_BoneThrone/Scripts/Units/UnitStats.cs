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
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 3;
        [SerializeField] private int maxHpPerLevel = 2;
        [SerializeField] private int maxHp = 10;
        [SerializeField] private int moveRange = 4;
        [SerializeField] private int basicAttackRange = 1;
        [SerializeField] private int attackModifier;
        [SerializeField] private int defense = 10;
        [SerializeField] private int baseDamage = 1;

        public int Level
        {
            get { return Mathf.Max(1, level); }
        }

        public int MaxLevel
        {
            get { return Mathf.Max(1, maxLevel); }
        }

        public int MaxHp
        {
            get { return maxHp; }
        }

        public int MoveRange
        {
            get { return moveRange; }
        }

        public int BasicAttackRange
        {
            get { return Mathf.Max(1, basicAttackRange); }
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

        public bool CanLevelUp()
        {
            return Level < MaxLevel;
        }

        public bool TryLevelUp()
        {
            if (!CanLevelUp())
            {
                return false;
            }

            level = Mathf.Min(MaxLevel, Level + 1);
            maxHp = Mathf.Max(1, maxHp + Mathf.Max(0, maxHpPerLevel));
            return true;
        }
    }
}
