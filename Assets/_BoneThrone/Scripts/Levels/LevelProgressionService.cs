using BoneThrone.Audio;
using BoneThrone.Rooms;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Levels
{
    /// <summary>
    /// Minimal Phase 10 progression state for one shared key, stairs validation, floor switching, and party level-up.
    /// Scene loading is optional through an explicitly assigned SceneLevelTransition.
    /// It does not implement inventory, rewards, boss keys, UI panels, or networking.
    /// </summary>
    public sealed class LevelProgressionService : MonoBehaviour
    {
        [SerializeField] private bool hasSharedKey;
        [SerializeField] private Unit[] playerUnits;
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

        public int CurrentLevelIndex
        {
            get { return levelManager != null ? levelManager.CurrentLevelIndex : 0; }
        }

        private void Start()
        {
            int appliedCount = PartyProgressionState.Apply(playerUnits);
            if (appliedCount > 0)
            {
                Debug.Log("LevelProgressionService applied saved party state to " + appliedCount + " player units.", this);
            }
        }

        public void CollectSharedKey(Object source)
        {
            if (hasSharedKey)
            {
                Debug.Log("Shared key collection ignored because the party already has the key.", source != null ? source : this);
                return;
            }

            hasSharedKey = true;
            Debug.Log("Shared key collected. Party key state is now true.", source != null ? source : this);
        }

        public bool CanEnterNextLevel(out string reason)
        {
            if (!hasSharedKey)
            {
                reason = "The party does not have the shared key.";
                return false;
            }

            if (!AreRequiredRoomsCleared(out reason))
            {
                return false;
            }

            if (transitionInProgress)
            {
                reason = "A level transition is already in progress.";
                return false;
            }

            reason = "Progression conditions satisfied.";
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
                Debug.Log("Entered next level placeholder. Shared key state reset to false.", this);
            }

            transitionInProgress = false;
            return switched;
        }

        public int LevelUpLivingPlayerUnitsOnce()
        {
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

        [ContextMenu("Phase 10/Reset Progression For Test")]
        public void ResetProgressionForTest()
        {
            hasSharedKey = false;
            transitionInProgress = false;
            PartyProgressionState.Clear();
            Debug.Log("Phase 10 progression reset for test. Shared key=false and saved party state cleared.", this);
        }

        private bool AreRequiredRoomsCleared(out string reason)
        {
            if (requiredClearedRooms == null || requiredClearedRooms.Length == 0)
            {
                reason = "No required rooms configured; treating room condition as satisfied.";
                return true;
            }

            for (int i = 0; i < requiredClearedRooms.Length; i++)
            {
                RoomController room = requiredClearedRooms[i];
                if (room == null)
                {
                    reason = "Required room reference at index " + i + " is missing.";
                    return false;
                }

                if (!room.CheckCleared())
                {
                    reason = "Required room at index " + i + " is not cleared. CurrentState=" + room.CurrentState + ".";
                    return false;
                }
            }

            reason = "All required rooms are cleared.";
            return true;
        }
    }
}
