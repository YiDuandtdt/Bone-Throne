using BoneThrone.Rooms;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Small local state holder for future user-placed boss gate pieces.
    /// It does not own boss fights, scene loading, save/load, or level progression.
    /// </summary>
    public sealed class BossGateProgressionState : MonoBehaviour
    {
        [SerializeField] private bool hasBossKey;
        [SerializeField] private bool isBossDoorOpened;
        [SerializeField] private bool isBossFightStarted;
        [SerializeField] private bool hasUsedSupplyPoint;
        [Header("Auto Boss Gate Rules")]
        [SerializeField] private bool autoRequireAllNonBossRoomsCleared = true;
        [SerializeField] private bool requireNoActiveNonBossEnemies = true;
        [SerializeField] private RoomController[] requiredClearedRooms;
        [SerializeField] private string bossNameContains = "boss";
        [SerializeField] private string golemNameContains = "golem";
        [Header("Shared Key Compatibility")]
        [SerializeField] private bool acceptSharedKeyForBossDoor = true;
        [SerializeField] private LevelProgressionService levelProgressionService;
        [SerializeField] private bool debugLogging;

        public bool HasBossKey
        {
            get { return hasBossKey || HasSharedKeyFallback(); }
        }

        public bool IsBossDoorOpened
        {
            get { return isBossDoorOpened; }
        }

        public bool IsBossFightStarted
        {
            get { return isBossFightStarted; }
        }

        public bool HasUsedSupplyPoint
        {
            get { return hasUsedSupplyPoint; }
        }

        public static BossGateProgressionState GetOrCreateSceneState()
        {
            BossGateProgressionState state = Object.FindFirstObjectByType<BossGateProgressionState>();
            if (state != null)
            {
                return state;
            }

            GameObject host = new GameObject("BossGateProgressionState_Auto");
            return host.AddComponent<BossGateProgressionState>();
        }

        public bool CanCollectBossKey()
        {
            return !hasBossKey && AreBossGateRequirementsMet();
        }

        public bool CanOpenBossDoor()
        {
            return HasAnyBossDoorKey() && !isBossDoorOpened && AreBossGateRequirementsMet();
        }

        public bool AreBossGateRequirementsMet()
        {
            if (requireNoActiveNonBossEnemies && HasActiveLivingNonBossEnemy())
            {
                return false;
            }

            RoomController[] rooms = ResolveRequiredRooms();
            if (rooms == null || rooms.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < rooms.Length; i++)
            {
                RoomController room = rooms[i];
                if (room == null)
                {
                    continue;
                }

                if (!IsRoomClearedForBossGate(room))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsBossRoom(RoomController room)
        {
            return IsBossRoomInternal(room);
        }

        public bool ShouldExposeBossFightRuntime()
        {
            return isBossDoorOpened && isBossFightStarted;
        }

        public bool CollectBossKey()
        {
            if (hasBossKey)
            {
                Log("Boss key collection ignored because the party already has it.");
                return false;
            }

            if (!AreBossGateRequirementsMet())
            {
                Log("Boss key collection rejected because non-boss rooms are not cleared.");
                return false;
            }

            hasBossKey = true;
            Log("Boss key collected.");
            return true;
        }

        public bool OpenBossDoor()
        {
            if (!HasAnyBossDoorKey())
            {
                Log("Boss door open rejected because the boss key is missing.");
                return false;
            }

            if (!AreBossGateRequirementsMet())
            {
                Log("Boss door open rejected because non-boss rooms are not cleared.");
                return false;
            }

            if (isBossDoorOpened)
            {
                Log("Boss door open ignored because it is already opened.");
                return false;
            }

            isBossDoorOpened = true;
            Log("Boss door opened.");
            return true;
        }

        public bool MarkBossFightStarted(RoomController room)
        {
            if (!IsBossRoomInternal(room))
            {
                Log("Boss fight start ignored because the entered room is not a boss room.");
                return false;
            }

            if (!isBossDoorOpened)
            {
                Log("Boss fight start rejected because the boss door is not opened.");
                return false;
            }

            if (isBossFightStarted)
            {
                return false;
            }

            isBossFightStarted = true;
            Log("Boss fight started.");
            return true;
        }

        public bool MarkSupplyPointUsed()
        {
            if (hasUsedSupplyPoint)
            {
                Log("Supply point use ignored because it was already used.");
                return false;
            }

            hasUsedSupplyPoint = true;
            Log("Supply point marked used.");
            return true;
        }

        public void ResetState()
        {
            hasBossKey = false;
            isBossDoorOpened = false;
            isBossFightStarted = false;
            hasUsedSupplyPoint = false;
            Log("Boss gate progression state reset.");
        }

        private RoomController[] ResolveRequiredRooms()
        {
            if (HasAnyRoom(requiredClearedRooms))
            {
                return requiredClearedRooms;
            }

            if (!autoRequireAllNonBossRoomsCleared)
            {
                return null;
            }

            RoomController[] allRooms = Object.FindObjectsByType<RoomController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (allRooms == null || allRooms.Length == 0)
            {
                return null;
            }

            int requiredCount = 0;
            for (int i = 0; i < allRooms.Length; i++)
            {
                if (allRooms[i] != null && !IsBossRoomInternal(allRooms[i]))
                {
                    requiredCount++;
                }
            }

            if (requiredCount == 0)
            {
                return null;
            }

            RoomController[] rooms = new RoomController[requiredCount];
            int writeIndex = 0;
            for (int i = 0; i < allRooms.Length; i++)
            {
                RoomController room = allRooms[i];
                if (room == null || IsBossRoomInternal(room))
                {
                    continue;
                }

                rooms[writeIndex] = room;
                writeIndex++;
            }

            return rooms;
        }

        private bool HasAnyBossDoorKey()
        {
            return hasBossKey || HasSharedKeyFallback();
        }

        private bool HasSharedKeyFallback()
        {
            if (!acceptSharedKeyForBossDoor)
            {
                return false;
            }

            ResolveLevelProgressionService();
            return levelProgressionService != null && levelProgressionService.HasSharedKey;
        }

        private void ResolveLevelProgressionService()
        {
            if (levelProgressionService == null)
            {
                levelProgressionService = Object.FindFirstObjectByType<LevelProgressionService>();
            }
        }

        private bool IsRoomClearedForBossGate(RoomController room)
        {
            RoomEnemyActivator activator = room.EnemyActivator;
            if (activator == null || !activator.HasConfiguredEnemies)
            {
                return true;
            }

            if (room.CurrentState == RoomState.Cleared)
            {
                return true;
            }

            if (room.CurrentState == RoomState.CombatActive || room.CurrentState == RoomState.Entered)
            {
                return room.CheckCleared();
            }

            return false;
        }

        private bool HasActiveLivingNonBossEnemy()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (units == null || units.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null
                    || !unit.gameObject.activeInHierarchy
                    || unit.Faction != UnitFaction.Enemy
                    || !unit.IsAlive)
                {
                    continue;
                }

                if (!ContainsBossNeedle(unit.gameObject.name) && !ContainsBossNeedle(unit.DisplayName))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBossRoomInternal(RoomController room)
        {
            if (room == null)
            {
                return false;
            }

            if (ContainsBossNeedle(room.gameObject.name))
            {
                return true;
            }

            RoomEnemyActivator activator = room.EnemyActivator;
            if (activator == null)
            {
                return false;
            }

            return HasBossLikeUnit(activator.AssignedEnemies) || HasBossLikeUnit(activator.EnemyPrefabs);
        }

        private bool HasBossLikeUnit(Unit[] units)
        {
            if (units == null)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                if (ContainsBossNeedle(unit.gameObject.name) || ContainsBossNeedle(unit.DisplayName))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsBossNeedle(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string normalized = value.ToLowerInvariant();
            string bossNeedle = string.IsNullOrEmpty(bossNameContains) ? "boss" : bossNameContains.ToLowerInvariant();
            string golemNeedle = string.IsNullOrEmpty(golemNameContains) ? "golem" : golemNameContains.ToLowerInvariant();
            return normalized.Contains(bossNeedle) || normalized.Contains(golemNeedle);
        }

        private static bool HasAnyRoom(RoomController[] rooms)
        {
            if (rooms == null)
            {
                return false;
            }

            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossGateProgressionState: " + message, this);
            }
        }
    }
}
