using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Rooms
{
    /// <summary>
    /// Owns the minimal Phase 9 state for one room only.
    /// It does not manage levels, rewards, UI, keys, stairs, or networking.
    /// </summary>
    public sealed class RoomController : MonoBehaviour
    {
        [SerializeField] private RoomState currentState = RoomState.Unentered;
        [SerializeField] private RoomShadowController shadowController;
        [SerializeField] private RoomEnemyActivator enemyActivator;

        public RoomState CurrentState
        {
            get { return currentState; }
        }

        public RoomEnemyActivator EnemyActivator
        {
            get { return enemyActivator; }
        }

        private void Start()
        {
            ApplyInitialPresentation();
        }

        public void EnterRoom(Unit enteringUnit)
        {
            if (!CanEnterRoom(enteringUnit))
            {
                return;
            }

            currentState = RoomState.Entered;
            RevealRoom();

            int aliveEnemyCount = ActivateEnemies();
            currentState = aliveEnemyCount > 0 ? RoomState.CombatActive : RoomState.Cleared;

            Debug.Log("Room entered. State=" + currentState + " AliveEnemies=" + aliveEnemyCount + ".", this);
        }

        public void RevealRoom()
        {
            if (shadowController == null)
            {
                Debug.LogWarning("RoomController cannot reveal room because RoomShadowController is missing.", this);
                return;
            }

            shadowController.HideShadow();
        }

        public int ActivateEnemies()
        {
            if (enemyActivator == null)
            {
                Debug.LogWarning("RoomController has no RoomEnemyActivator. Room will be treated as cleared if no enemies are available.", this);
                return 0;
            }

            return enemyActivator.ActivateEnemies();
        }

        public bool CheckCleared()
        {
            if (currentState == RoomState.Cleared)
            {
                return true;
            }

            if (enemyActivator == null)
            {
                currentState = RoomState.Cleared;
                Debug.Log("Room cleared because no RoomEnemyActivator is assigned.", this);
                return true;
            }

            if (!enemyActivator.AreAllAssignedEnemiesDead())
            {
                return false;
            }

            currentState = RoomState.Cleared;
            Debug.Log("Room cleared because all assigned enemies are dead.", this);
            return true;
        }

        public void ForceSetClearedForTest()
        {
            currentState = RoomState.Cleared;
            Debug.Log("Room state forced to Cleared for test.", this);
        }

        private void ApplyInitialPresentation()
        {
            if (currentState == RoomState.Unentered)
            {
                if (shadowController != null)
                {
                    shadowController.ShowShadow();
                }

                return;
            }

            if (shadowController != null)
            {
                shadowController.HideShadow();
            }
        }

        private bool CanEnterRoom(Unit enteringUnit)
        {
            if (currentState != RoomState.Unentered)
            {
                Debug.Log("Room entry ignored because current state is " + currentState + ".", this);
                return false;
            }

            if (enteringUnit == null)
            {
                Debug.LogWarning("Room entry ignored because entering Unit is missing.", this);
                return false;
            }

            if (enteringUnit.Faction != UnitFaction.Player)
            {
                Debug.LogWarning("Room entry ignored because Unit " + enteringUnit.UnitId + " is not a player unit.", enteringUnit);
                return false;
            }

            if (!enteringUnit.IsAlive)
            {
                Debug.LogWarning("Room entry ignored because Unit " + enteringUnit.UnitId + " is dead.", enteringUnit);
                return false;
            }

            return true;
        }
    }
}
