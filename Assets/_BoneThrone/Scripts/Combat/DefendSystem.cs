using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Executes the local Defend action. It consumes action but never ends the turn.
    /// </summary>
    public sealed class DefendSystem : MonoBehaviour
    {
        private const int DefaultReduction = 2;

        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private int defendReduction = DefaultReduction;

        public bool TryDefend(Unit unit)
        {
            ResolveReferences();

            if (unit == null)
            {
                LogRejected("unit is missing.", this);
                return false;
            }

            if (!unit.IsAlive)
            {
                LogRejected("unit is dead.", unit);
                return false;
            }

            if (turnManager == null || actionPermissionService == null)
            {
                LogRejected("turn gate is missing.", unit);
                return false;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                LogRejected("current phase is " + turnManager.CurrentPhase + ".", unit);
                return false;
            }

            if (actionPermissionService.TryConsumeStunForAction(unit, turnManager))
            {
                return false;
            }

            if (!actionPermissionService.CanAct(unit, turnManager))
            {
                LogRejected("unit cannot act.", unit);
                return false;
            }

            UnitDefenseState defenseState = unit.GetComponent<UnitDefenseState>();
            if (defenseState == null)
            {
                defenseState = unit.gameObject.AddComponent<UnitDefenseState>();
            }

            defenseState.SetDefending(defendReduction);
            MarkActed(unit);
            if (combatLog != null)
            {
                combatLog.LogDefend(unit, defendReduction);
            }

            Debug.Log("DefendSystem: unit " + unit.UnitId + " is defending with reduction " + defendReduction + ".", unit);
            return true;
        }

        private void ResolveReferences()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindFirstObjectByType<TurnManager>();
            }

            if (actionPermissionService == null)
            {
                actionPermissionService = Object.FindFirstObjectByType<ActionPermissionService>();
            }

            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }

        private static void MarkActed(Unit unit)
        {
            UnitTurnState turnState = unit != null ? unit.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkActed();
            }
        }

        private void LogRejected(string reason, Object context)
        {
            if (combatLog != null)
            {
                combatLog.LogDefendRejected(reason, context);
                return;
            }

            Debug.LogWarning("Defend rejected: " + reason, context);
        }
    }
}
