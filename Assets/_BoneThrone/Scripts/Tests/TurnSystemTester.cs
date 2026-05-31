using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary ContextMenu helper for manually verifying Phase 6 turn flags and fixed order.
    /// This is not a UI, AI, combat, room, networking, or production input system.
    /// </summary>
    public sealed class TurnSystemTester : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Unit testUnit;

        [ContextMenu("Phase 6/Start Player Round")]
        public void StartPlayerRoundTest()
        {
            if (!HasTurnManager())
            {
                return;
            }

            turnManager.StartPlayerRound();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/Advance Turn")]
        public void AdvanceTurnTest()
        {
            if (!HasTurnManager())
            {
                return;
            }

            turnManager.AdvanceTurn();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/End Current Actor Turn")]
        public void EndCurrentActorTurnTest()
        {
            if (!HasTurnManager())
            {
                return;
            }

            turnManager.EndCurrentActorTurn();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/Mark Test Unit Moved")]
        public void MarkMovedTest()
        {
            UnitTurnState turnState = GetTestUnitTurnState();
            if (turnState == null)
            {
                return;
            }

            turnState.MarkMoved();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/Mark Test Unit Acted")]
        public void MarkActedTest()
        {
            UnitTurnState turnState = GetTestUnitTurnState();
            if (turnState == null)
            {
                return;
            }

            turnState.MarkActed();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/Reset Test Unit Turn State")]
        public void ResetTestUnitTurnState()
        {
            UnitTurnState turnState = GetTestUnitTurnState();
            if (turnState == null)
            {
                return;
            }

            turnState.ResetForNewRound();
            LogTurnStateTest();
        }

        [ContextMenu("Phase 6/Log Turn State")]
        public void LogTurnStateTest()
        {
            string managerText = turnManager != null
                ? "Phase=" + turnManager.CurrentPhase + " Role=" + turnManager.CurrentRole + " Index=" + turnManager.CurrentTurnIndex
                : "TurnManager missing";

            string unitText = "TestUnit missing";
            if (testUnit != null)
            {
                UnitTurnState turnState = testUnit.GetComponent<UnitTurnState>();
                unitText = turnState != null
                    ? "Unit=" + testUnit.UnitId + " Role=" + testUnit.RoleId + " HasMoved=" + turnState.HasMoved + " HasActed=" + turnState.HasActed
                    : "Unit=" + testUnit.UnitId + " has no UnitTurnState";
            }

            Debug.Log("TurnSystemTester: " + managerText + " | " + unitText + ".", this);
        }

        private bool HasTurnManager()
        {
            if (turnManager != null)
            {
                return true;
            }

            Debug.LogWarning("TurnSystemTester needs a TurnManager reference.", this);
            return false;
        }

        private UnitTurnState GetTestUnitTurnState()
        {
            if (testUnit == null)
            {
                Debug.LogWarning("TurnSystemTester needs a test Unit reference.", this);
                return null;
            }

            UnitTurnState turnState = testUnit.GetComponent<UnitTurnState>();
            if (turnState == null)
            {
                Debug.LogWarning("Test unit " + testUnit.UnitId + " needs a UnitTurnState component.", testUnit);
                return null;
            }

            return turnState;
        }
    }
}
