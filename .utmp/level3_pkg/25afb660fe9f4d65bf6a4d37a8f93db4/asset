using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Minimal one-tile knockback helper for skill effects.
    /// </summary>
    public static class SkillKnockbackUtility
    {
        public static bool TryKnockbackOneTile(Unit caster, Unit target, CombatLog combatLog)
        {
            if (caster == null || target == null || caster.CurrentTile == null || target.CurrentTile == null)
            {
                LogBlocked(combatLog, target, "missing caster, target, or tile.");
                return false;
            }

            GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
            if (gridManager == null)
            {
                LogBlocked(combatLog, target, "GridManager missing.");
                return false;
            }

            GridPosition casterPosition = caster.CurrentTile.Position;
            GridPosition targetPosition = target.CurrentTile.Position;
            int dx = targetPosition.X - casterPosition.X;
            int dy = targetPosition.Y - casterPosition.Y;
            int stepX = 0;
            int stepY = 0;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                stepX = dx == 0 ? 0 : dx > 0 ? 1 : -1;
            }
            else
            {
                stepY = dy == 0 ? 0 : dy > 0 ? 1 : -1;
            }

            if (stepX == 0 && stepY == 0)
            {
                LogBlocked(combatLog, target, "no push direction.");
                return false;
            }

            GridPosition destination = new GridPosition(targetPosition.X + stepX, targetPosition.Y + stepY);
            Tile destinationTile;
            if (!gridManager.TryGetTile(destination, out destinationTile) || destinationTile == null || !destinationTile.CanEnter())
            {
                LogBlocked(combatLog, target, "destination blocked at " + destination + ".");
                return false;
            }

            UnitMover unitMover = Object.FindFirstObjectByType<UnitMover>();
            if (unitMover == null)
            {
                LogBlocked(combatLog, target, "UnitMover missing.");
                return false;
            }

            List<GridPosition> path = new List<GridPosition>(2)
            {
                targetPosition,
                destination
            };

            if (!unitMover.TryMove(target, gridManager, path))
            {
                LogBlocked(combatLog, target, "UnitMover rejected movement to " + destination + ".");
                return false;
            }

            if (combatLog != null)
            {
                combatLog.LogKnockback(target, destination);
            }

            return true;
        }

        private static void LogBlocked(CombatLog combatLog, Unit target, string reason)
        {
            if (combatLog != null)
            {
                combatLog.LogKnockbackBlocked(target, reason);
            }
        }
    }
}
