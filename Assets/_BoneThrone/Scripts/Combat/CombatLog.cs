using System;
using System.Collections.Generic;
using BoneThrone.Skills;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Temporary combat feedback sink for Phase 7 manual testing.
    /// This intentionally logs to Console only and does not implement HUD/UI.
    /// </summary>
    public sealed class CombatLog : MonoBehaviour
    {
        public enum EntryType
        {
            Rejected = 0,
            AttackAttempt = 1,
            Hit = 2,
            Miss = 3,
            Death = 4,
            SkillUse = 5,
            SkillEffect = 6,
            SkillRejected = 7,
            SkillCooldown = 8
        }

        public sealed class Entry
        {
            public Entry(EntryType type, string message)
            {
                Type = type;
                Message = message;
            }

            public EntryType Type { get; private set; }
            public string Message { get; private set; }
        }

        private const int MaxRecentEntries = 12;

        private readonly List<Entry> recentEntries = new List<Entry>();

        public event Action<Entry> EntryAdded;

        public IReadOnlyList<Entry> RecentEntries
        {
            get { return recentEntries; }
        }

        public void LogRejected(string reason, UnityEngine.Object context)
        {
            string message = "Basic attack rejected: " + reason;
            Debug.LogWarning(message, context);
        }

        public void LogAttackAttempt(Unit attacker, Unit target, int roll, int attackModifier, int defense)
        {
            string message = "Unit " + attacker.UnitId + " attacks Unit " + target.UnitId
                + ". D20=" + roll
                + " AttackModifier=" + attackModifier
                + " Total=" + (roll + attackModifier)
                + " TargetDefense=" + defense + ".";
            Debug.Log(message, attacker);
        }

        public void LogHit(Unit attacker, Unit target, int damage, int remainingHp)
        {
            string message = GetDisplayName(attacker)
                + " attacked "
                + GetDisplayName(target)
                + ", dealt "
                + Mathf.Max(0, damage)
                + " damage.";
            AddEntry(EntryType.Hit, message);
            Debug.Log(message, target);
        }

        public void LogMiss(Unit attacker, Unit target)
        {
            string message = "Basic attack missed. Unit " + attacker.UnitId
                + " dealt no damage to Unit " + target.UnitId + ".";
            Debug.Log(message, attacker);
        }

        public void LogDeath(Unit target)
        {
            string message = "<b>" + GetDisplayName(target) + " died.</b>";
            AddEntry(EntryType.Death, message);
            Debug.Log(message, target);
        }

        public void LogSkillRejected(string reason, UnityEngine.Object context)
        {
            string message = "Skill rejected: " + reason;
            Debug.LogWarning(message, context);
        }

        public void LogSkillUse(Unit caster, Unit target, SkillData skill)
        {
            string skillName = skill != null ? skill.DisplayName : "Unknown Skill";
            string message = "Unit " + GetUnitId(caster)
                + " used " + skillName
                + " on Unit " + GetUnitId(target) + ".";
            Debug.Log(message, caster);
        }

        public void LogSkillEffect(Unit caster, Unit target, SkillData skill, string effectSummary, int remainingHp)
        {
            string skillName = skill != null ? skill.DisplayName : "Unknown Skill";
            string message = GetDisplayName(caster)
                + " used "
                + skillName
                + " on "
                + GetDisplayName(target)
                + ". "
                + effectSummary
                + " TargetHP="
                + Mathf.Max(0, remainingHp)
                + ".";
            AddEntry(EntryType.SkillEffect, message);
            Debug.Log(message, target);
        }

        public void LogSkillDamage(Unit caster, Unit target, SkillData skill, int damage, int remainingHp, bool isPrimaryTarget)
        {
            string skillName = skill != null ? skill.DisplayName : "Unknown Skill";
            string damageLabel = isPrimaryTarget ? " damage." : " splash damage.";
            string message = GetDisplayName(caster)
                + " used "
                + skillName
                + " on "
                + GetDisplayName(target)
                + ", dealt "
                + Mathf.Max(0, damage)
                + damageLabel;
            AddEntry(EntryType.SkillEffect, message);
            Debug.Log(message + " TargetHP=" + Mathf.Max(0, remainingHp) + ".", target);
        }

        public void LogSkillCooldown(Unit caster, SkillData skill, int cooldown)
        {
            string skillName = skill != null ? skill.DisplayName : "Unknown Skill";
            string message = skillName
                + " cooldown for Unit " + GetUnitId(caster)
                + ": " + Mathf.Max(0, cooldown) + ".";
            Debug.Log(message, caster);
        }

        private void AddEntry(EntryType type, string message)
        {
            Entry entry = new Entry(type, message);
            recentEntries.Add(entry);
            while (recentEntries.Count > MaxRecentEntries)
            {
                recentEntries.RemoveAt(0);
            }

            if (EntryAdded != null)
            {
                EntryAdded(entry);
            }
        }

        private static int GetUnitId(Unit unit)
        {
            return unit != null ? unit.UnitId : 0;
        }

        private static string GetDisplayName(Unit unit)
        {
            if (unit == null)
            {
                return "Unit 0";
            }

            if (!string.IsNullOrEmpty(unit.DisplayName))
            {
                return unit.DisplayName;
            }

            if (unit.RoleId != BoneThrone.Core.RoleId.None)
            {
                return unit.RoleId.ToString();
            }

            return "Unit " + unit.UnitId;
        }
    }
}
