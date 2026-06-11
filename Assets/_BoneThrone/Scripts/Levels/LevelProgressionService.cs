using BoneThrone.Audio;
using BoneThrone.Core;
using BoneThrone.Rooms;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Minimal Phase 10 progression state for one floor pass key, stairs validation, floor switching, and party level-up.
    /// Scene loading is optional through an explicitly assigned SceneLevelTransition.
    /// It does not implement inventory, rewards, boss keys, UI panels, or networking.
    /// </summary>
    public sealed class LevelProgressionService : MonoBehaviour
    {
        [SerializeField] private bool hasSharedKey;
        [SerializeField] private bool skipSharedKeyRequirement;
        [SerializeField] private Unit[] playerUnits;
        [SerializeField] private bool autoFindPlayerUnitsIfMissing = true;
        [SerializeField] private RoomController[] requiredBossDefeatedRooms;
        [SerializeField] private RoomController[] requiredClearedRooms;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SceneLevelTransition sceneTransition;

        private bool transitionInProgress;

        public bool HasSharedKey
        {
            get { return hasSharedKey; }
        }

        public Unit[] PlayerUnits
        {
            get { return playerUnits; }
        }

        public bool RequiresSharedKey
        {
            get { return !skipSharedKeyRequirement; }
        }

        public int CurrentLevelIndex
        {
            get { return levelManager != null ? levelManager.CurrentLevelIndex : 0; }
        }

        private void Start()
        {
            ResolvePlayerUnitsIfNeeded();

            int appliedCount = PartyProgressionState.Apply(playerUnits);
            if (appliedCount > 0)
            {
                Debug.Log("LevelProgressionService applied saved party state to " + appliedCount + " player units.", this);
                return;
            }

            if (!PartyProgressionState.HasState)
            {
                int sceneDefaultLevel = MenuProgressionState.GetLevelIndexForScene(SceneManager.GetActiveScene().name);
                int defaultAppliedCount = ApplySceneDefaultPartyLevel(sceneDefaultLevel);
                if (defaultAppliedCount > 0)
                {
                    Debug.Log("LevelProgressionService applied scene default party level " + sceneDefaultLevel + " to " + defaultAppliedCount + " player units.", this);
                }
            }
        }

        public void CollectSharedKey(Object source)
        {
            if (hasSharedKey)
            {
                Debug.Log("Pass key collection ignored because the party already has the key.", source != null ? source : this);
                return;
            }

            hasSharedKey = true;
            Debug.Log("Pass key collected. Party key state is now true.", source != null ? source : this);
        }

        public bool CanEnterNextLevel(out string reason)
        {
            if (!skipSharedKeyRequirement && !hasSharedKey)
            {
                reason = "队伍还没有获得通行钥匙。";
                return false;
            }

            if (!AreRequiredBossDefeatedRoomsSatisfied(out reason))
            {
                return false;
            }

            if (!AreRequiredRoomsCleared(out reason))
            {
                return false;
            }

            if (transitionInProgress)
            {
                reason = "正在切换关卡，请稍等。";
                return false;
            }

            reason = "进度条件已满足。";
            return true;
        }

        public bool TryEnterNextLevel()
        {
            string reason;
            if (!CanEnterNextLevel(out reason))
            {
                Debug.LogWarning("Cannot enter next level: " + reason, this);
                return false;
            }

            if (sceneTransition != null && sceneTransition.HasNextScene)
            {
                if (!sceneTransition.CanLoadNextScene(out reason))
                {
                    Debug.LogWarning("Cannot enter next level: " + reason, sceneTransition);
                    return false;
                }

                transitionInProgress = true;
                LevelUpLivingPlayerUnitsOnce();
                hasSharedKey = false;
                int capturedCount = PartyProgressionState.Capture(playerUnits);
                Debug.Log("LevelProgressionService captured party state for " + capturedCount + " player units before scene transition.", this);
                bool loaded = sceneTransition.TryLoadNextScene();
                transitionInProgress = false;
                return loaded;
            }

            if (levelManager == null)
            {
                Debug.LogWarning("Cannot enter next level because LevelManager is missing.", this);
                return false;
            }

            transitionInProgress = true;
            bool switched = levelManager.SwitchToNextLevelPlaceholder(playerUnits);
            if (switched)
            {
                LevelUpLivingPlayerUnitsOnce();
                hasSharedKey = false;
                Debug.Log("Entered next level placeholder. Pass key state reset to false.", this);
            }

            transitionInProgress = false;
            return switched;
        }

        public int LevelUpLivingPlayerUnitsOnce()
        {
            ResolvePlayerUnitsIfNeeded();

            if (playerUnits == null || playerUnits.Length == 0)
            {
                Debug.LogWarning("LevelProgressionService cannot level up party because no player units are configured.", this);
                return 0;
            }

            int leveledCount = 0;
            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null)
                {
                    continue;
                }

                if (unit.Faction != UnitFaction.Player || !unit.IsAlive)
                {
                    continue;
                }

                if (unit.Stats == null)
                {
                    Debug.LogWarning("LevelProgressionService skipped unit " + unit.UnitId + " because UnitStats is missing.", unit);
                    continue;
                }

                bool leveled = unit.Stats.TryLevelUp();
                if (!leveled)
                {
                    Debug.Log("Unit " + unit.UnitId + " is already at max level " + unit.Stats.MaxLevel + ".", unit);
                    continue;
                }

                if (unit.RuntimeState != null)
                {
                    unit.RuntimeState.SetCurrentHp(unit.Stats.GetClampedMaxHp());
                }

                leveledCount++;
                BTInteractionVfxService.PlayLevelUp(unit.transform.position);
                Debug.Log("Unit " + unit.UnitId + " leveled up to " + unit.Stats.Level + " and HP was refilled.", unit);
            }

            if (leveledCount > 0)
            {
                BTAudioService.PlaySfx(BTAudioCueId.LevelUp);
            }

            Debug.Log("LevelProgressionService leveled up " + leveledCount + " living player units.", this);
            return leveledCount;
        }

        private int ApplySceneDefaultPartyLevel(int targetLevel)
        {
            int clampedTargetLevel = Mathf.Clamp(targetLevel, 1, 3);
            if (clampedTargetLevel <= 1)
            {
                return 0;
            }

            ResolvePlayerUnitsIfNeeded();
            if (playerUnits == null || playerUnits.Length == 0)
            {
                return 0;
            }

            int appliedCount = 0;
            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null || unit.Faction != UnitFaction.Player || !unit.IsAlive || unit.Stats == null)
                {
                    continue;
                }

                bool changed = false;
                while (unit.Stats.Level < clampedTargetLevel && unit.Stats.CanLevelUp())
                {
                    changed |= unit.Stats.TryLevelUp();
                }

                if (unit.RuntimeState != null)
                {
                    int maxHp = unit.Stats.GetClampedMaxHp();
                    if (unit.RuntimeState.CurrentHp != maxHp)
                    {
                        unit.RuntimeState.SetCurrentHp(maxHp);
                        changed = true;
                    }
                }

                if (changed)
                {
                    appliedCount++;
                }
            }

            return appliedCount;
        }

        [ContextMenu("Phase 10/Reset Progression For Test")]
        public void ResetProgressionForTest()
        {
            hasSharedKey = false;
            transitionInProgress = false;
            PartyProgressionState.Clear();
            Debug.Log("Phase 10 progression reset for test. Pass key=false and saved party state cleared.", this);
        }

        private bool AreRequiredBossDefeatedRoomsSatisfied(out string reason)
        {
            if (requiredBossDefeatedRooms == null || requiredBossDefeatedRooms.Length == 0)
            {
                reason = "没有配置必须击败的首领房间。";
                return true;
            }

            for (int i = 0; i < requiredBossDefeatedRooms.Length; i++)
            {
                RoomController room = requiredBossDefeatedRooms[i];
                if (room == null)
                {
                    reason = "第 " + (i + 1) + " 个首领房间引用丢失。";
                    return false;
                }

                if (!room.CheckBossLikeEnemiesDefeated())
                {
                    reason = "首领房间尚未完成：必须先击败首领。";
                    return false;
                }
            }

            reason = "首领房间已完成。";
            return true;
        }

        private bool AreRequiredRoomsCleared(out string reason)
        {
            if (requiredClearedRooms == null || requiredClearedRooms.Length == 0)
            {
                reason = "没有配置必清房间，视为已满足。";
                return true;
            }

            int validRoomCount = 0;
            for (int i = 0; i < requiredClearedRooms.Length; i++)
            {
                RoomController room = requiredClearedRooms[i];
                if (room == null)
                {
                    Debug.LogWarning("LevelProgressionService ignored a missing required room reference at index " + i + ".", this);
                    continue;
                }

                validRoomCount++;
                if (!room.CheckCleared())
                {
                    reason = "第 " + i + " 个必清房间尚未清理完成。当前状态：" + room.CurrentState + "。";
                    return false;
                }
            }

            if (validRoomCount == 0)
            {
                reason = "没有有效的必清房间配置，视为已满足。";
                return true;
            }

            reason = "所有必清房间已清理完成。";
            return true;
        }

        private void ResolvePlayerUnitsIfNeeded()
        {
            if (!autoFindPlayerUnitsIfMissing || HasAnyConfiguredUnit(playerUnits))
            {
                return;
            }

            Unit[] sceneUnits = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int playerCount = 0;
            for (int i = 0; i < sceneUnits.Length; i++)
            {
                Unit unit = sceneUnits[i];
                if (unit != null && unit.Faction == UnitFaction.Player)
                {
                    playerCount++;
                }
            }

            if (playerCount == 0)
            {
                return;
            }

            playerUnits = new Unit[playerCount];
            int writeIndex = 0;
            for (int i = 0; i < sceneUnits.Length; i++)
            {
                Unit unit = sceneUnits[i];
                if (unit != null && unit.Faction == UnitFaction.Player)
                {
                    playerUnits[writeIndex] = unit;
                    writeIndex++;
                }
            }

            System.Array.Sort(playerUnits, CompareUnitsForProgression);
            Debug.Log("LevelProgressionService auto-assigned " + playerUnits.Length + " player units.", this);
        }

        private static bool HasAnyConfiguredUnit(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareUnitsForProgression(Unit left, Unit right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int unitIdCompare = left.UnitId.CompareTo(right.UnitId);
            if (unitIdCompare != 0)
            {
                return unitIdCompare;
            }

            return string.CompareOrdinal(left.name, right.name);
        }
    }
}
