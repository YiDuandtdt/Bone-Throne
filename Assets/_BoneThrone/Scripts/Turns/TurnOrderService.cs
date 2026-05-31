using BoneThrone.Core;
using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Provides the fixed role order reserved for future LAN multiplayer validation.
    /// It contains no ownership, client id, RPC, NetworkVariable, or NetworkBehaviour logic.
    /// </summary>
    public sealed class TurnOrderService : MonoBehaviour
    {
        private static readonly RoleId[] FixedRoleOrder =
        {
            RoleId.Fighter,
            RoleId.Ranger,
            RoleId.Mage,
            RoleId.Barbarian,
            RoleId.Enemy
        };

        public int Count
        {
            get { return FixedRoleOrder.Length; }
        }

        public RoleId GetRoleAt(int index)
        {
            if (FixedRoleOrder.Length == 0)
            {
                return RoleId.None;
            }

            int wrappedIndex = WrapIndex(index);
            return FixedRoleOrder[wrappedIndex];
        }

        public RoleId GetNextRole(RoleId currentRole)
        {
            int index = GetIndexOf(currentRole);
            if (index < 0)
            {
                return GetRoleAt(0);
            }

            return GetRoleAt(index + 1);
        }

        public int GetIndexOf(RoleId role)
        {
            for (int i = 0; i < FixedRoleOrder.Length; i++)
            {
                if (FixedRoleOrder[i] == role)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool IsPlayerRole(RoleId role)
        {
            return role == RoleId.Fighter
                || role == RoleId.Ranger
                || role == RoleId.Mage
                || role == RoleId.Barbarian;
        }

        public bool IsEnemyRole(RoleId role)
        {
            return role == RoleId.Enemy;
        }

        private static int WrapIndex(int index)
        {
            int count = FixedRoleOrder.Length;
            int wrapped = index % count;
            if (wrapped < 0)
            {
                wrapped += count;
            }

            return wrapped;
        }
    }
}
