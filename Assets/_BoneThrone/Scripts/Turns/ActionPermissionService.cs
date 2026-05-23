using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Answers whether a unit may consume movement or action during the current turn.
    /// By default this preserves singleplayer free selection among player units.
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
            if (!CanUsePlayerUnit(unit, turnManager, out turnState))
            {
                return false;
            }

            if (turnState.HasMoved)
            {
                Debug.LogWarning("Move denied because unit " + unit.UnitId + " has already moved this round.", unit);
                return false;
            }

            return true;
        }

        public bool CanAct(Unit unit, TurnManager turnManager)
        {
            UnitTurnState turnState;
            if (!CanUsePlayerUnit(unit, turnManager, out turnState))
            {
                return false;
            }

            if (turnState.HasActed)
            {
                Debug.LogWarning("Action denied because unit " + unit.UnitId + " has already acted this round.", unit);
                return false;
            }

            return true;
        }

        private bool CanUsePlayerUnit(Unit unit, TurnManager turnManager, out UnitTurnState turnState)
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

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                Debug.LogWarning("Action permission denied because current phase is " + turnManager.CurrentPhase + ".", unit);
                return false;
            }

            if (unit.Faction != UnitFaction.Player)
            {
                Debug.LogWarning("Action permission denied because only player units can act during PlayerTurn.", unit);
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

            if (requireCurrentRole && unit.RoleId != turnManager.CurrentRole)
            {
                Debug.LogWarning("Action permission denied because unit role " + unit.RoleId + " does not match current role " + turnManager.CurrentRole + ".", unit);
                return false;
            }

            return true;
        }
    }
}
