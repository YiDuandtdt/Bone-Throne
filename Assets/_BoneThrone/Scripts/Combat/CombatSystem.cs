using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Minimal Phase 7 basic attack resolver: range, D20 hit check, base damage, death, and MarkActed.
    /// It contains no extra gameplay systems, production UI, command routing, or networking logic.
    /// </summary>
    public sealed class CombatSystem : MonoBehaviour
    {
        [SerializeField] private D20Roller d20Roller;
        [SerializeField] private AttackRangeService attackRangeService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;

        public bool CanBasicAttack(Unit attacker, Unit target, out string reason)
        {
            if (!CanBasicParticipantsAttack(attacker, target, out reason))
            {
                return false;
            }

            if (!CanPassTurnGate(attacker, out reason))
            {
                return false;
            }

            if (!CanUseCombatServices(out reason))
            {
                return false;
            }

            int distance = attackRangeService.GetManhattanDistance(attacker, target);
            if (!attackRangeService.IsInBasicAttackRange(attacker, target))
            {
                reason = "Target is out of basic attack range. Distance="
                    + distance
                    + " Range="
                    + attackRangeService.BasicAttackRange
                    + ".";
                return false;
            }

            reason = "Basic attack target is valid.";
            return true;
        }

        public bool TryBasicAttack(Unit attacker, Unit target)
        {
            if (!ValidateBasicParticipants(attacker, target))
            {
                return false;
            }

            if (!ValidateTurnGate(attacker))
            {
                return false;
            }

            if (!HasCombatServices())
            {
                return false;
            }

            int distance = attackRangeService.GetManhattanDistance(attacker, target);
            if (!attackRangeService.IsInBasicAttackRange(attacker, target))
            {
                LogRejected(
                    "target is out of basic attack range. Distance="
                    + distance + " Range=" + attackRangeService.BasicAttackRange + ".",
                    attacker);
                return false;
            }

            UnitAnimationController attackerAnimation = attacker.GetComponent<UnitAnimationController>();
            attackerAnimation?.PlayBasicAttack();

            int roll = d20Roller.RollD20();
            int attackModifier = attacker.Stats != null ? attacker.Stats.AttackModifier : 0;
            int defense = target.Stats != null ? target.Stats.Defense : 0;
            int attackTotal = roll + attackModifier;

            if (combatLog != null)
            {
                combatLog.LogAttackAttempt(attacker, target, roll, attackModifier, defense);
                combatLog.LogBasicAttackRoll(attacker, roll, attackModifier);
            }

            bool hit = attackTotal >= defense;
            if (hit)
            {
                int damage = attacker.Stats != null ? attacker.Stats.BaseDamage : 0;
                bool targetDied = damageResolver.ApplyDamage(target, damage);
                int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;

                if (combatLog != null)
                {
                    combatLog.LogHit(attacker, target, Mathf.Max(0, damage), remainingHp);
                    if (targetDied)
                    {
                        combatLog.LogDeath(target);
                    }
                }
            }
            else if (combatLog != null)
            {
                combatLog.LogMiss(attacker, target);
            }

            MarkAttackerActed(attacker);
            return true;
        }

        private bool CanBasicParticipantsAttack(Unit attacker, Unit target, out string reason)
        {
            if (attacker == null)
            {
                reason = "Attacker is missing.";
                return false;
            }

            if (target == null)
            {
                reason = "Target is missing.";
                return false;
            }

            if (attacker == target)
            {
                reason = "Attacker cannot target itself.";
                return false;
            }

            if (!attacker.IsAlive)
            {
                reason = "Attacker is dead.";
                return false;
            }

            if (!target.IsAlive)
            {
                reason = "Target is dead.";
                return false;
            }

            if (attacker.Faction == UnitFaction.None || target.Faction == UnitFaction.None)
            {
                reason = "Attacker or target faction is missing.";
                return false;
            }

            if (attacker.Faction == target.Faction)
            {
                reason = "Basic attack target must be an opposing faction.";
                return false;
            }

            if (attacker.CurrentTile == null)
            {
                reason = "Attacker has no current tile.";
                return false;
            }

            if (target.CurrentTile == null)
            {
                reason = "Target has no current tile.";
                return false;
            }

            reason = "Basic attack participants are valid.";
            return true;
        }

        private bool CanPassTurnGate(Unit attacker, out string reason)
        {
            bool hasTurnManager = turnManager != null;
            bool hasActionPermissionService = actionPermissionService != null;

            if (hasTurnManager != hasActionPermissionService)
            {
                reason = "Turn gating is partially configured. Bind both TurnManager and ActionPermissionService, or leave both empty.";
                return false;
            }

            if (!hasTurnManager)
            {
                reason = "Turn gating is not configured.";
                return true;
            }

            UnitTurnState turnState = attacker != null ? attacker.GetComponent<UnitTurnState>() : null;
            if (turnState == null)
            {
                reason = "Attacker has no UnitTurnState.";
                return false;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                reason = "Current phase is " + turnManager.CurrentPhase + ".";
                return false;
            }

            if (attacker.Faction != UnitFaction.Player)
            {
                reason = "Only player units can act during PlayerTurn.";
                return false;
            }

            if (turnState.HasActed)
            {
                reason = "Attacker has already acted.";
                return false;
            }

            if (actionPermissionService.RequireCurrentRole && attacker.RoleId != turnManager.CurrentRole)
            {
                reason = "Attacker role " + attacker.RoleId + " does not match current role " + turnManager.CurrentRole + ".";
                return false;
            }

            bool canAct = actionPermissionService.CanAct(attacker, turnManager);
            reason = canAct ? "Turn gate passed." : "Attacker cannot act.";
            return canAct;
        }

        private bool CanUseCombatServices(out string reason)
        {
            if (d20Roller == null)
            {
                reason = "D20Roller is missing.";
                return false;
            }

            if (attackRangeService == null)
            {
                reason = "AttackRangeService is missing.";
                return false;
            }

            if (damageResolver == null)
            {
                reason = "DamageResolver is missing.";
                return false;
            }

            reason = "Combat services are bound.";
            return true;
        }

        private bool ValidateBasicParticipants(Unit attacker, Unit target)
        {
            if (attacker == null)
            {
                LogRejected("attacker is missing.", this);
                return false;
            }

            if (target == null)
            {
                LogRejected("target is missing.", attacker);
                return false;
            }

            if (attacker == target)
            {
                LogRejected("attacker cannot target itself.", attacker);
                return false;
            }

            if (!attacker.IsAlive)
            {
                LogRejected("attacker is dead.", attacker);
                return false;
            }

            if (!target.IsAlive)
            {
                LogRejected("target is dead.", target);
                return false;
            }

            if (attacker.CurrentTile == null)
            {
                LogRejected("attacker has no current tile.", attacker);
                return false;
            }

            if (target.CurrentTile == null)
            {
                LogRejected("target has no current tile.", target);
                return false;
            }

            return true;
        }

        private bool ValidateTurnGate(Unit attacker)
        {
            bool hasTurnManager = turnManager != null;
            bool hasActionPermissionService = actionPermissionService != null;

            if (hasTurnManager != hasActionPermissionService)
            {
                LogRejected("turn gating is partially configured. Bind both TurnManager and ActionPermissionService, or leave both empty.", attacker);
                return false;
            }

            if (!hasTurnManager)
            {
                return true;
            }

            if (actionPermissionService.TryConsumeStunForAction(attacker, turnManager))
            {
                return false;
            }

            return actionPermissionService.CanAct(attacker, turnManager);
        }

        private bool HasCombatServices()
        {
            if (d20Roller == null)
            {
                LogRejected("D20Roller is missing.", this);
                return false;
            }

            if (attackRangeService == null)
            {
                LogRejected("AttackRangeService is missing.", this);
                return false;
            }

            if (damageResolver == null)
            {
                LogRejected("DamageResolver is missing.", this);
                return false;
            }

            return true;
        }

        private static void MarkAttackerActed(Unit attacker)
        {
            UnitTurnState turnState = attacker != null ? attacker.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkActed();
            }
        }

        private void LogRejected(string reason, Object context)
        {
            if (combatLog != null)
            {
                combatLog.LogRejected(reason, context);
                return;
            }

            Debug.LogWarning("Basic attack rejected: " + reason, context);
        }
    }
}
