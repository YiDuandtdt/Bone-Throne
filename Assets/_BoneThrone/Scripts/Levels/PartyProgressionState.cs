using System.Collections.Generic;
using BoneThrone.Items;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Small in-memory party state used while moving between local single-player level scenes.
    /// It is intentionally not a save system and does not persist after the app exits.
    /// </summary>
    public static class PartyProgressionState
    {
        private static readonly Dictionary<int, UnitSnapshot> UnitSnapshotsById = new Dictionary<int, UnitSnapshot>();

        public static bool HasState
        {
            get { return UnitSnapshotsById.Count > 0; }
        }

        public static int Capture(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return 0;
            }

            int capturedCount = 0;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.UnitId <= 0 || unit.Faction != UnitFaction.Player)
                {
                    continue;
                }

                UnitSnapshot snapshot = new UnitSnapshot();
                snapshot.UnitId = unit.UnitId;
                snapshot.Level = unit.Stats != null ? unit.Stats.Level : 1;
                snapshot.MaxHp = unit.Stats != null ? unit.Stats.GetClampedMaxHp() : 1;
                snapshot.CurrentHp = unit.RuntimeState != null ? unit.RuntimeState.CurrentHp : snapshot.MaxHp;
                snapshot.IsDead = unit.RuntimeState != null && unit.RuntimeState.IsDead;

                UnitPotionState potionState = unit.GetComponent<UnitPotionState>();
                snapshot.PotionCount = potionState != null ? potionState.CurrentPotionCount : -1;

                UnitSnapshotsById[unit.UnitId] = snapshot;
                capturedCount++;
            }

            return capturedCount;
        }

        public static int Apply(Unit[] units)
        {
            if (units == null || units.Length == 0 || UnitSnapshotsById.Count == 0)
            {
                return 0;
            }

            int appliedCount = 0;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.UnitId <= 0 || unit.Faction != UnitFaction.Player)
                {
                    continue;
                }

                UnitSnapshot snapshot;
                if (!UnitSnapshotsById.TryGetValue(unit.UnitId, out snapshot))
                {
                    continue;
                }

                if (unit.Stats != null)
                {
                    unit.Stats.ApplyProgressionState(snapshot.Level, snapshot.MaxHp);
                }

                if (unit.RuntimeState != null)
                {
                    if (snapshot.IsDead)
                    {
                        unit.MarkDeadAndReleaseTile();
                    }
                    else
                    {
                        unit.RuntimeState.SetCurrentHp(Mathf.Min(snapshot.CurrentHp, unit.Stats != null ? unit.Stats.GetClampedMaxHp() : snapshot.MaxHp));
                    }
                }

                UnitPotionState potionState = unit.GetComponent<UnitPotionState>();
                if (potionState != null && snapshot.PotionCount >= 0)
                {
                    potionState.SetPotionCount(snapshot.PotionCount);
                }

                appliedCount++;
            }

            return appliedCount;
        }

        public static void Clear()
        {
            UnitSnapshotsById.Clear();
        }

        private struct UnitSnapshot
        {
            public int UnitId;
            public int Level;
            public int MaxHp;
            public int CurrentHp;
            public bool IsDead;
            public int PotionCount;
        }
    }
}
