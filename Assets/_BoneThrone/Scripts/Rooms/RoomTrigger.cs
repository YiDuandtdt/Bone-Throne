using BoneThrone.Interactables;
using BoneThrone.Levels;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Rooms
{
    /// <summary>
    /// Temporary Phase 9 trigger for entering one room.
    /// This does not implement UI, networking, or formal interaction prompts.
    /// </summary>
    public sealed class RoomTrigger : MonoBehaviour
    {
        [SerializeField] private RoomController roomController;
        [SerializeField] private bool requireOpenedBossDoorEntryForBossRooms = true;
        [SerializeField] [Min(0.1f)] private float openedBossDoorEntryRadius = 4f;

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return;
            }

            Unit enteringUnit = other.GetComponentInParent<Unit>();
            TryEnterRoom(enteringUnit);
        }

        public void TriggerEnterForTest(Unit unit)
        {
            TryEnterRoom(unit);
        }

        private void TryEnterRoom(Unit unit)
        {
            if (roomController == null)
            {
                Debug.LogWarning("RoomTrigger cannot enter room because RoomController is missing.", this);
                return;
            }

            if (unit == null)
            {
                Debug.LogWarning("RoomTrigger ignored entry because no Unit was found.", this);
                return;
            }

            if (unit.Faction != UnitFaction.Player || !unit.IsAlive)
            {
                return;
            }

            if (requireOpenedBossDoorEntryForBossRooms && !CanEnterThroughBossDoor(unit))
            {
                Debug.Log("RoomTrigger ignored boss-room entry because the unit did not enter from an opened BossDoor.", this);
                return;
            }

            roomController.EnterRoom(unit);
        }

        private bool CanEnterThroughBossDoor(Unit unit)
        {
            BossGateProgressionState progressionState = BossGateProgressionState.GetOrCreateSceneState();
            if (progressionState == null || !progressionState.IsBossRoom(roomController))
            {
                return true;
            }

            BossDoor[] doors = Object.FindObjectsByType<BossDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            float radiusSqr = openedBossDoorEntryRadius * openedBossDoorEntryRadius;
            for (int i = 0; i < doors.Length; i++)
            {
                BossDoor door = doors[i];
                if (door == null || !door.IsOpened)
                {
                    continue;
                }

                Vector3 delta = unit.transform.position - door.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
