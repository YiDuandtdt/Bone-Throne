using System.Collections.Generic;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Read-only scene unit collector for GridTest stabilization.
    /// It does not mutate unit state, placement, activation, turns, damage, or skills.
    /// </summary>
    public sealed class ActiveUnitProvider : MonoBehaviour
    {
        public Unit[] GetActiveAliveEnemies()
        {
            List<Unit> results = new List<Unit>();
            FillActiveAliveEnemies(results);
            return results.ToArray();
        }

        public Unit[] GetActiveAliveUnits()
        {
            List<Unit> results = new List<Unit>();
            FillActiveAliveUnits(results);
            return results.ToArray();
        }

        public void FillActiveAliveEnemies(List<Unit> results)
        {
            if (results == null)
            {
                Debug.LogWarning("ActiveUnitProvider cannot fill a null enemy result list.", this);
                return;
            }

            FillActiveAliveUnits(results);
            for (int i = results.Count - 1; i >= 0; i--)
            {
                if (results[i].Faction != UnitFaction.Enemy)
                {
                    results.RemoveAt(i);
                }
            }
        }

        public void FillActiveAliveUnits(List<Unit> results)
        {
            if (results == null)
            {
                Debug.LogWarning("ActiveUnitProvider cannot fill a null unit result list.", this);
                return;
            }

            results.Clear();
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (IsActiveAliveUnit(unit))
                {
                    results.Add(unit);
                }
            }
        }

        private static bool IsActiveAliveUnit(Unit unit)
        {
            return unit != null
                && unit.gameObject.activeInHierarchy
                && unit.IsAlive;
        }
    }
}
