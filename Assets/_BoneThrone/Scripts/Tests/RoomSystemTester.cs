using BoneThrone.Rooms;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary Phase 9 ContextMenu helper for manually validating room entry,
    /// shadow reveal, assigned enemy activation, and cleared detection.
    /// This is not a UI, room manager, level manager, key, stair, or networking system.
    /// </summary>
    public sealed class RoomSystemTester : MonoBehaviour
    {
        [SerializeField] private RoomController roomController;
        [SerializeField] private RoomTrigger roomTrigger;
        [SerializeField] private Unit testEnteringUnit;

        [ContextMenu("Phase 9/Enter Room Test")]
        public void EnterRoomTest()
        {
            if (testEnteringUnit == null)
            {
                Debug.LogWarning("RoomSystemTester needs a test entering Unit reference.", this);
                return;
            }

            if (roomTrigger != null)
            {
                roomTrigger.TriggerEnterForTest(testEnteringUnit);
                LogRoomState();
                return;
            }

            if (!HasRoomController())
            {
                return;
            }

            roomController.EnterRoom(testEnteringUnit);
            LogRoomState();
        }

        [ContextMenu("Phase 9/Check Cleared Test")]
        public void CheckClearedTest()
        {
            if (!HasRoomController())
            {
                return;
            }

            bool cleared = roomController.CheckCleared();
            Debug.Log("RoomSystemTester CheckCleared result: " + cleared + ".", this);
            LogRoomState();
        }

        [ContextMenu("Phase 9/Mark Assigned Enemies Dead For Test")]
        public void MarkAssignedEnemiesDeadForTest()
        {
            if (!HasRoomController())
            {
                return;
            }

            RoomEnemyActivator activator = roomController.EnemyActivator;
            if (activator == null)
            {
                Debug.LogWarning("RoomSystemTester cannot mark enemies dead because RoomEnemyActivator is missing.", this);
                return;
            }

            activator.MarkAssignedEnemiesDeadForTest();
            Debug.Log("RoomSystemTester marked assigned enemies dead for test.", this);
            LogRoomState();
        }

        [ContextMenu("Phase 9/Log Room State")]
        public void LogRoomState()
        {
            if (roomController == null)
            {
                Debug.LogWarning("RoomSystemTester has no RoomController reference.", this);
                return;
            }

            Debug.Log("RoomSystemTester: RoomState=" + roomController.CurrentState + ".", this);
        }

        private bool HasRoomController()
        {
            if (roomController != null)
            {
                return true;
            }

            Debug.LogWarning("RoomSystemTester needs a RoomController reference.", this);
            return false;
        }
    }
}
