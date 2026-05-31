using BoneThrone.Core;
using BoneThrone.AI;
using BoneThrone.Combat;
using BoneThrone.Skills;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Minimal local turn coordinator for player/enemy phases and fixed-order reservation.
    /// EnemyTurn orchestration is delegated to EnemyTurnRunner.
    /// </summary>
    public sealed class TurnManager : MonoBehaviour
    {
        [SerializeField] private TurnOrderService turnOrderService;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private EnemyTurnRunner enemyTurnRunner;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private Unit[] playerUnits;
        [SerializeField] private TurnPhase currentPhase = TurnPhase.None;
        [SerializeField] private RoleId currentRole = RoleId.None;
        [SerializeField] private int currentTurnIndex = -1;

        public TurnPhase CurrentPhase
        {
            get { return currentPhase; }
        }

        public RoleId CurrentRole
        {
            get { return currentRole; }
        }

        public int CurrentTurnIndex
        {
            get { return currentTurnIndex; }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void StartPlayerRound()
        {
            if (!HasAnyAlivePlayerUnit())
            {
                currentPhase = TurnPhase.None;
                currentRole = RoleId.None;
                currentTurnIndex = -1;
                Debug.LogWarning("TurnManager stopped player round start because no alive player units are available.", this);
                return;
            }

            ResetPlayerUnitTurnStates();
            DisablePlayerRoleRequirementForFreeOrder();
            TickCooldownsForAlivePlayers();
            TickBleedForAlivePlayers();
            currentPhase = TurnPhase.PlayerTurn;
            currentRole = RoleId.None;
            currentTurnIndex = -1;
            Debug.Log("Player round started. Select any alive player unit that has not ended.", this);
        }

        public void AdvanceTurn()
        {
            EndCurrentActorTurn();
        }

        public void EndCurrentActorTurn()
        {
            if (turnOrderService == null)
            {
                Debug.LogWarning("TurnManager cannot advance because TurnOrderService is missing.", this);
                return;
            }

            if (currentTurnIndex < 0)
            {
                Debug.LogWarning("TurnManager cannot end a fixed actor turn because free player order is active.", this);
                return;
            }

            currentTurnIndex++;
            currentRole = GetRoleAtCurrentIndex();

            if (turnOrderService.IsEnemyRole(currentRole))
            {
                BeginEnemyTurn();
                return;
            }

            BeginActorTurn(currentRole);
        }

        public void BeginActorTurn(RoleId role)
        {
            if (turnOrderService == null)
            {
                Debug.LogWarning("TurnManager cannot begin actor turn because TurnOrderService is missing.", this);
                return;
            }

            if (turnOrderService.IsEnemyRole(role))
            {
                BeginEnemyTurn();
                return;
            }

            currentPhase = TurnPhase.PlayerTurn;
            currentRole = role;

            Unit actor = GetCurrentActorUnit();
            if (actor == null || !actor.IsAlive)
            {
                SkipUnavailableActor(role, actor);
                return;
            }

            TickCooldownsForActor(actor);
            Debug.Log("Actor turn started. Phase=" + currentPhase + " Role=" + currentRole + " Unit=" + actor.UnitId + ".", actor);
        }

        public void BeginEnemyTurn()
        {
            ResolveReferences();

            currentPhase = TurnPhase.EnemyTurn;
            currentRole = RoleId.Enemy;
            currentTurnIndex = turnOrderService != null ? turnOrderService.GetIndexOf(RoleId.Enemy) : currentTurnIndex;

            if (enemyTurnRunner == null)
            {
                Debug.LogWarning("TurnManager cannot run EnemyTurn because EnemyTurnRunner is missing.", this);
                EndEnemyTurn();
                return;
            }

            Debug.Log("Enemy turn started.", this);
            enemyTurnRunner.RunEnemyTurn(this);
        }

        public void EndEnemyTurn()
        {
            Debug.Log("Enemy turn ended. Returning to PlayerTurn.", this);
            StartPlayerRound();
        }

        public void ResetPlayerUnitTurnStates()
        {
            if (playerUnits == null)
            {
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null)
                {
                    continue;
                }

                UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
                if (turnState == null)
                {
                    Debug.LogWarning("Player unit " + unit.UnitId + " has no UnitTurnState to reset.", unit);
                    continue;
                }

                turnState.ResetForNewRound();
                UnitDefenseState defenseState = unit.GetComponent<UnitDefenseState>();
                if (defenseState != null && defenseState.IsDefending)
                {
                    defenseState.ClearDefending();
                }
            }
        }

        public Unit GetCurrentActorUnit()
        {
            return FindPlayerUnitForRole(currentRole);
        }

        public bool TryEndPlayerUnitTurn(Unit unit)
        {
            if (currentPhase != TurnPhase.PlayerTurn)
            {
                Debug.LogWarning("TurnManager cannot end a player unit turn because current phase is " + currentPhase + ".", this);
                return false;
            }

            if (unit == null)
            {
                Debug.LogWarning("TurnManager cannot end turn because Unit is missing.", this);
                return false;
            }

            if (unit.Faction != UnitFaction.Player)
            {
                Debug.LogWarning("TurnManager cannot end turn because unit " + unit.UnitId + " is not a player unit.", unit);
                return false;
            }

            if (!unit.IsAlive)
            {
                Debug.LogWarning("TurnManager cannot end turn because unit " + unit.UnitId + " is dead.", unit);
                return false;
            }

            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            if (turnState == null)
            {
                Debug.LogWarning("TurnManager cannot end turn because unit " + unit.UnitId + " has no UnitTurnState.", unit);
                return false;
            }

            if (turnState.HasEnded)
            {
                Debug.LogWarning("TurnManager cannot end turn because unit " + unit.UnitId + " has already ended.", unit);
                return false;
            }

            turnState.MarkEnded();
            currentRole = RoleId.None;
            currentTurnIndex = -1;
            Debug.Log("Player unit " + unit.UnitId + " ended its turn.", unit);

            if (AreAllAlivePlayersEnded())
            {
                BeginEnemyTurn();
            }
            else
            {
                currentPhase = TurnPhase.PlayerTurn;
            }

            return true;
        }

        public bool AreAllAlivePlayersEnded()
        {
            bool foundAlivePlayer = false;
            if (playerUnits == null)
            {
                return false;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null || !unit.IsAlive)
                {
                    continue;
                }

                foundAlivePlayer = true;
                UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
                if (turnState == null || !turnState.HasEnded)
                {
                    return false;
                }
            }

            return foundAlivePlayer;
        }

        private RoleId GetRoleAtCurrentIndex()
        {
            if (turnOrderService == null)
            {
                return RoleId.None;
            }

            return turnOrderService.GetRoleAt(currentTurnIndex);
        }

        private Unit FindPlayerUnitForRole(RoleId role)
        {
            if (playerUnits == null)
            {
                return null;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit != null && unit.RoleId == role)
                {
                    return unit;
                }
            }

            return null;
        }

        private void TickCooldownsForActor(Unit actor)
        {
            if (actor == null || !actor.IsAlive)
            {
                return;
            }

            if (skillSystem == null)
            {
                Debug.LogWarning("TurnManager skipped cooldown tick because SkillSystem is missing.", this);
                return;
            }

            SkillRuntime runtime = actor.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                return;
            }

            skillSystem.TickCooldownsForUnit(actor);
        }

        private void TickCooldownsForAlivePlayers()
        {
            if (skillSystem == null)
            {
                Debug.LogWarning("TurnManager skipped player round cooldown ticks because SkillSystem is missing.", this);
                return;
            }

            if (playerUnits == null)
            {
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null || !unit.IsAlive)
                {
                    continue;
                }

                SkillRuntime runtime = unit.GetComponent<SkillRuntime>();
                if (runtime == null)
                {
                    continue;
                }

                skillSystem.TickCooldownsForUnit(unit);
            }
        }

        private void TickBleedForAlivePlayers()
        {
            ResolveCombatReferences();
            if (damageResolver == null || playerUnits == null)
            {
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit == null || !unit.IsAlive)
                {
                    continue;
                }

                UnitBleedState bleedState = unit.GetComponent<UnitBleedState>();
                if (bleedState == null || !bleedState.HasBleed)
                {
                    continue;
                }

                int bleedDamage;
                if (!bleedState.TryConsumeTick(out bleedDamage))
                {
                    continue;
                }

                bool died = damageResolver.ApplyDamage(unit, bleedDamage);
                int remainingHp = unit.RuntimeState != null ? unit.RuntimeState.CurrentHp : 0;
                if (combatLog != null)
                {
                    combatLog.LogBleedTick(unit, bleedDamage, remainingHp, bleedState.RemainingTurns);
                    if (died)
                    {
                        combatLog.LogDeath(unit);
                    }
                }
            }
        }

        private void SkipUnavailableActor(RoleId role, Unit actor)
        {
            int maxSkips = turnOrderService != null ? turnOrderService.Count : 5;
            currentTurnIndex++;
            if (currentTurnIndex > maxSkips)
            {
                currentPhase = TurnPhase.None;
                currentRole = RoleId.None;
                currentTurnIndex = -1;
                Debug.LogWarning("TurnManager stopped advancing because no available actor could begin a turn.", this);
                return;
            }

            string reason = actor == null ? "missing" : "dead";
            Debug.LogWarning("TurnManager skipped " + role + " because the actor is " + reason + ".", this);
            EndCurrentActorTurn();
        }

        private bool HasAnyAlivePlayerUnit()
        {
            if (playerUnits == null)
            {
                return false;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                Unit unit = playerUnits[i];
                if (unit != null && unit.IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (turnOrderService == null)
            {
                turnOrderService = Object.FindFirstObjectByType<TurnOrderService>();
            }

            if (skillSystem == null)
            {
                skillSystem = Object.FindFirstObjectByType<SkillSystem>();
            }

            if (enemyTurnRunner == null)
            {
                enemyTurnRunner = Object.FindFirstObjectByType<EnemyTurnRunner>();
            }

            if (enemyTurnRunner == null)
            {
                enemyTurnRunner = gameObject.AddComponent<EnemyTurnRunner>();
            }

            if (actionPermissionService == null)
            {
                actionPermissionService = Object.FindFirstObjectByType<ActionPermissionService>();
            }

            ResolveCombatReferences();
        }

        private void ResolveCombatReferences()
        {
            if (damageResolver == null)
            {
                damageResolver = Object.FindFirstObjectByType<DamageResolver>();
            }

            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }

        private void DisablePlayerRoleRequirementForFreeOrder()
        {
            if (actionPermissionService == null)
            {
                actionPermissionService = Object.FindFirstObjectByType<ActionPermissionService>();
            }

            if (actionPermissionService != null)
            {
                actionPermissionService.RequireCurrentRole = false;
            }
        }
    }
}
