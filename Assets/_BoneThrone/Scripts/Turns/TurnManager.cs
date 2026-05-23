using BoneThrone.Core;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Minimal local turn coordinator for player/enemy phases and fixed-order reservation.
    /// EnemyTurn is a placeholder and does not run AI.
    /// </summary>
    public sealed class TurnManager : MonoBehaviour
    {
        [SerializeField] private TurnOrderService turnOrderService;
        [SerializeField] private Unit[] playerUnits;
        [SerializeField] private TurnPhase currentPhase = TurnPhase.None;
        [SerializeField] private RoleId currentRole = RoleId.None;
        [SerializeField] private int currentTurnIndex = -1;

        public TurnPhase CurrentPhase
        {
            get { return currentPhase; }
        }

        public RoleId CurrentRole
        {
            get { return currentRole; }
        }

        public int CurrentTurnIndex
        {
            get { return currentTurnIndex; }
        }

        public void StartPlayerRound()
        {
            ResetPlayerUnitTurnStates();
            currentPhase = TurnPhase.PlayerTurn;
            currentTurnIndex = 0;
            currentRole = GetRoleAtCurrentIndex();
            Debug.Log("Player round started. Current role reservation: " + currentRole + ".", this);
        }

        public void AdvanceTurn()
        {
            if (turnOrderService == null)
            {
                Debug.LogWarning("TurnManager cannot advance because TurnOrderService is missing.", this);
                return;
            }

            if (currentTurnIndex < 0)
            {
                StartPlayerRound();
                return;
            }

            bool wasEnemyTurn = currentPhase == TurnPhase.EnemyTurn;
            currentTurnIndex++;
            currentRole = GetRoleAtCurrentIndex();
            currentPhase = turnOrderService.IsEnemyRole(currentRole) ? TurnPhase.EnemyTurn : TurnPhase.PlayerTurn;

            if (wasEnemyTurn && currentPhase == TurnPhase.PlayerTurn)
            {
                ResetPlayerUnitTurnStates();
            }

            Debug.Log("Advanced turn. Phase=" + currentPhase + " Role=" + currentRole + ".", this);
        }

        public void EndCurrentActorTurn()
        {
            AdvanceTurn();
        }

        public void ResetPlayerUnitTurnStates()
        {
            if (playerUnits == null)
            {
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null)
                {
                    continue;
                }

                UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
                if (turnState == null)
                {
                    Debug.LogWarning("Player unit " + unit.UnitId + " has no UnitTurnState to reset.", unit);
                    continue;
                }

                turnState.ResetForNewRound();
            }
        }

        private RoleId GetRoleAtCurrentIndex()
        {
            if (turnOrderService == null)
            {
                return RoleId.None;
            }

            return turnOrderService.GetRoleAt(currentTurnIndex);
        }
    }
}
