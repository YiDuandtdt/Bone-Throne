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

            roomController.EnterRoom(unit);
        }
    }
}
