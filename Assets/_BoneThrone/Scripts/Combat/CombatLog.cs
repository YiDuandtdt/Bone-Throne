using System;
using System.Collections.Generic;
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
            Death = 4
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
            AddEntry(EntryType.Rejected, message);
            Debug.LogWarning(message, context);
        }

        public void LogAttackAttempt(Unit attacker, Unit target, int roll, int attackModifier, int defense)
        {
            string message = "Unit " + attacker.UnitId + " attacks Unit " + target.UnitId
                + ". D20=" + roll
                + " AttackModifier=" + attackModifier
                + " Total=" + (roll + attackModifier)
                + " TargetDefense=" + defense + ".";
            AddEntry(EntryType.AttackAttempt, message);
            Debug.Log(message, attacker);
        }

        public void LogHit(Unit attacker, Unit target, int damage, int remainingHp)
        {
            string message = "Basic attack hit. Unit " + attacker.UnitId
                + " dealt " + damage
                + " damage to Unit " + target.UnitId
                + ". TargetHP=" + remainingHp + ".";
            AddEntry(EntryType.Hit, message);
            Debug.Log(message, target);
        }

        public void LogMiss(Unit attacker, Unit target)
        {
            string message = "Basic attack missed. Unit " + attacker.UnitId
                + " dealt no damage to Unit " + target.UnitId + ".";
            AddEntry(EntryType.Miss, message);
            Debug.Log(message, attacker);
        }

        public void LogDeath(Unit target)
        {
            string message = "Unit " + target.UnitId + " died and released its tile.";
            AddEntry(EntryType.Death, message);
            Debug.Log(message, target);
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
    }
}
