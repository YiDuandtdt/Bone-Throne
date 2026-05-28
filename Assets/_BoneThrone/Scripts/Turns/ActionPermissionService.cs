using BoneThrone.Units;
using BoneThrone.Combat;
using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Answers whether a unit may consume movement or action during the current turn.
    /// Player units are gated by PlayerTurn, and enemy units are gated by EnemyTurn.
    /// </summary>
    public sealed class ActionPermissionService : MonoBehaviour
    {
        [SerializeField] private bool requireCurrentRole;

        public bool RequireCurrentRole
        {
            get { return requireCurrentRole; }
            set { requireCurrentRole = value; }
        }

        public bool CanMove(Unit unit, TurnManager turnManager)
        {
            UnitTurnState turnState;
            if (!CanUseUnit(unit, turnManager, out turnState))
            {
                return false;
            }

            if (turnState.HasMoved)
            {
                Debug.LogWarning("Move denied because unit " + unit.UnitId + " has already moved this round.", unit);
                return false;
            }

            UnitStunState stunState = unit.GetComponent<UnitStunState>();
            if (stunState != null && stunState.IsStunned)
            {
                Debug.LogWarning("Move denied because unit " + unit.UnitId + " is stunned.", unit);
                return false;
            }

            return true;
        }

        public bool CanAct(Unit unit, TurnManager turnManager)
        {
            UnitTurnState turnState;
            if (!CanUseUnit(unit, turnManager, out turnState))
            {
                return false;
            }

            if (turnState.HasActed)
            {
                Debug.LogWarning("Action denied because unit " + unit.UnitId + " has already acted this round.", unit);
                return false;
            }

            UnitStunState stunState = unit.GetComponent<UnitStunState>();
            if (stunState != null && stunState.IsStunned)
            {
                Debug.LogWarning("Action denied because unit " + unit.UnitId + " is stunned.", unit);
                return false;
            }

            return true;
        }

        public bool TryConsumeStunForAction(Unit unit, TurnManager turnManager)
        {
            UnitTurnState turnState;
            if (!CanUseUnit(unit, turnManager, out turnState))
            {
                return false;
            }

            if (turnState.HasActed)
            {
                return false;
            }

            UnitStunState stunState = unit.GetComponent<UnitStunState>();
            if (stunState == null || !stunState.TryConsumeStun())
            {
                return false;
            }

            turnState.MarkMoved();
            turnState.MarkActed();
            CombatLog combatLog = Object.FindFirstObjectByType<CombatLog>();
            if (combatLog != null)
            {
                combatLog.LogStunConsumed(unit);
            }

            Debug.LogWarning("Turn opportunity blocked because unit " + unit.UnitId + " is stunned.", unit);
            return true;
        }

        private bool CanUseUnit(Unit unit, TurnManager turnManager, out UnitTurnState turnState)
        {
            turnState = null;

            if (unit == null)
            {
                Debug.LogWarning("Action permission denied because Unit is missing.", this);
                return false;
            }

            if (turnManager == null)
            {
                Debug.LogWarning("Action permission denied because TurnManager is missing.", unit);
                return false;
            }

            if (!IsFactionAllowedForCurrentPhase(unit, turnManager))
            {
                Debug.LogWarning(
                    "Action permission denied because unit faction "
                    + unit.Faction
                    + " cannot act during "
                    + turnManager.CurrentPhase
                    + ".",
                    unit);
                return false;
            }

            if (!unit.IsAlive)
            {
                Debug.LogWarning("Action permission denied because dead units cannot act.", unit);
                return false;
            }

            turnState = unit.GetComponent<UnitTurnState>();
            if (turnState == null)
            {
                Debug.LogWarning("Action permission denied because unit " + unit.UnitId + " has no UnitTurnState.", unit);
                return false;
            }

            if (turnState.HasEnded)
            {
                Debug.LogWarning("Action permission denied because unit " + unit.UnitId + " has already ended this turn.", unit);
                return false;
            }

            if (requireCurrentRole
                && unit.Faction == UnitFaction.Player
                && turnManager.CurrentRole != BoneThrone.Core.RoleId.None
                && unit.RoleId != turnManager.CurrentRole)
            {
                Debug.LogWarning("Action permission denied because unit role " + unit.RoleId + " does not match current role " + turnManager.CurrentRole + ".", unit);
                return false;
            }

            return true;
        }

        private static bool IsFactionAllowedForCurrentPhase(Unit unit, TurnManager turnManager)
        {
            if (unit == null || turnManager == null)
            {
                return false;
            }

            if (turnManager.CurrentPhase == TurnPhase.PlayerTurn)
            {
                return unit.Faction == UnitFaction.Player;
            }

            if (turnManager.CurrentPhase == TurnPhase.EnemyTurn)
            {
                return unit.Faction == UnitFaction.Enemy;
            }

            return false;
        }
    }
}
