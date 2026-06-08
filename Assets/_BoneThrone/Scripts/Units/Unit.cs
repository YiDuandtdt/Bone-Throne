using BoneThrone.Audio;
using BoneThrone.Core;
using BoneThrone.Grid;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Minimal unit component for identity, basic state, and tile occupancy.
    /// It does not move, attack, take turns, submit commands, or use networking APIs.
    /// </summary>
    public sealed class Unit : MonoBehaviour
    {
        private static bool AnimationDebug { get { return false; } }

        [SerializeField] private int unitId = 1;
        [SerializeField] private string displayName;
        [SerializeField] private RoleId roleId = RoleId.None;
        [SerializeField] private UnitFaction faction = UnitFaction.None;
        [SerializeField] private UnitStats stats = new UnitStats();
        [SerializeField] private UnitRuntimeState runtimeState = new UnitRuntimeState();
        [SerializeField] private Tile currentTile;

        public int UnitId
        {
            get { return unitId; }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public RoleId RoleId
        {
            get { return roleId; }
        }

        public UnitFaction Faction
        {
            get { return faction; }
        }

        public UnitStats Stats
        {
            get { return stats; }
        }

        public UnitRuntimeState RuntimeState
        {
            get { return runtimeState; }
        }

        public Tile CurrentTile
        {
            get { return currentTile; }
        }

        public bool IsAlive
        {
            get { return runtimeState != null && runtimeState.IsAlive; }
        }

        private void Awake()
        {
            InitializeRuntime();
        }

        public void InitializeRuntime()
        {
            if (runtimeState == null)
            {
                runtimeState = new UnitRuntimeState();
            }

            runtimeState.Initialize(stats);
        }

        public bool TryPlaceAt(GridManager gridManager, GridPosition position)
        {
            if (gridManager == null)
            {
                Debug.LogWarning("Unit placement failed because GridManager is missing.", this);
                return false;
            }

            Tile targetTile;
            if (!gridManager.TryGetTile(position, out targetTile))
            {
                Debug.LogWarning("Unit placement failed because no tile exists at " + position + ".", this);
                return false;
            }

            return TryPlaceOnTile(targetTile);
        }

        public bool TryPlaceOnTile(Tile tile)
        {
            if (unitId <= 0)
            {
                Debug.LogWarning("Unit placement failed because UnitId must be a positive integer.", this);
                return false;
            }

            if (tile == null)
            {
                Debug.LogWarning("Unit placement failed because target tile is missing.", this);
                return false;
            }

            if (!tile.CanEnter())
            {
                Debug.LogWarning("Unit placement failed because target tile cannot be entered.", tile);
                return false;
            }

            if (currentTile != null && currentTile != tile)
            {
                ReleaseTile();
            }

            tile.SetOccupant(unitId);
            currentTile = tile;
            return true;
        }

        public void ReleaseTile()
        {
            if (currentTile == null)
            {
                return;
            }

            currentTile.ClearOccupant();
            currentTile = null;
        }

        public void MarkDeadAndReleaseTile(bool playDeathSfx = true)
        {
            if (runtimeState == null)
            {
                runtimeState = new UnitRuntimeState();
            }

            bool wasDead = runtimeState.IsDead;
            runtimeState.MarkDead();
            if (playDeathSfx && !wasDead)
            {
                BTAudioService.PlayDeathSfx(this);
            }

            UnitAnimationController animationController = GetComponent<UnitAnimationController>();
            LogAnimationDebug(animationController, "SetDead(true)");
            animationController?.SetDead(true);
            ReleaseTile();
        }

        private void LogAnimationDebug(UnitAnimationController animationController, string method)
        {
            if (!AnimationDebug)
            {
                return;
            }

            Debug.Log(
                "AnimationDebug Unit: unit="
                + name
                + " UnitId="
                + unitId
                + " controller="
                + (animationController != null ? animationController.name : "null")
                + " method="
                + method
                + ".",
                this);
        }
    }
}
