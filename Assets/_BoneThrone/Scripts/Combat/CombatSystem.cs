using System;
using System.Collections;
using BoneThrone.Audio;
using BoneThrone.Core;
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
        private static bool AnimationDebug { get { return false; } }

        [SerializeField] private D20Roller d20Roller;
        [SerializeField] private AttackRangeService attackRangeService;
        [SerializeField] private DamageResolver damageResolver;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [Header("Presentation Timing")]
        [SerializeField] [Min(0f)] private float basicAttackImpactDelay = 0.18f;
        [SerializeField] [Min(0f)] private float meleeBasicAttackImpactDelay = 0.65f;
        [SerializeField] [Min(0f)] private float axeBasicAttackSfxDelay = 0.8f;

        public event Action<Unit, Unit> BasicAttackResolved;

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
            int range = attackRangeService.GetBasicAttackRange(attacker);
            if (!attackRangeService.IsInBasicAttackRange(attacker, target))
            {
                reason = "目标超出普通攻击范围。";
                return false;
            }

            reason = "普通攻击目标有效。";
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
            int range = attackRangeService.GetBasicAttackRange(attacker);
            if (!attackRangeService.IsInBasicAttackRange(attacker, target))
            {
                LogRejected(
                    "target is out of basic attack range. Distance="
                    + distance + " Range=" + range + ".",
                    attacker);
                return false;
            }

            UnitAnimationController attackerAnimation = attacker.GetComponent<UnitAnimationController>();
            LogAnimationDebug(attacker, attackerAnimation, "PlayBasicAttack");
            if (attackerAnimation != null)
            {
                attackerAnimation.FaceTowards(target.transform.position);
                attackerAnimation.PlayBasicAttack(GetBasicAttackAnimationSpeed(attacker), GetBasicAttackAnimationRestoreDelay(attacker));
            }

            PlayBasicAttackSfx(attacker);

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
            MarkAttackerActed(attacker);
            StartCoroutine(ResolveBasicAttackImpact(attacker, target, hit));
            return true;
        }

        private void PlayBasicAttackSfx(Unit attacker)
        {
            BTAudioCueId cue = BTAudioService.GetBasicAttackCue(attacker);
            float delay = GetBasicAttackSfxDelay(attacker, cue);
            if (delay <= 0f)
            {
                BTAudioService.PlaySfx(cue);
                return;
            }

            StartCoroutine(PlaySfxAfterDelay(cue, delay));
        }

        private IEnumerator PlaySfxAfterDelay(BTAudioCueId cue, float delay)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            BTAudioService.PlaySfx(cue);
        }

        private IEnumerator ResolveBasicAttackImpact(Unit attacker, Unit target, bool hit)
        {
            float delay = GetBasicAttackImpactDelay(attacker);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (!hit)
            {
                if (combatLog != null && attacker != null && target != null)
                {
                    combatLog.LogMiss(attacker, target);
                }

                NotifyBasicAttackResolved(attacker, target);
                TryAutoEndAttackerTurn(attacker);
                yield break;
            }

            if (target == null || !target.IsAlive)
            {
                NotifyBasicAttackResolved(attacker, target);
                TryAutoEndAttackerTurn(attacker);
                yield break;
            }

            RangerHitPresentationConfig rangerPresentation = attacker != null ? attacker.GetComponent<RangerHitPresentationConfig>() : null;
            if (rangerPresentation != null)
            {
                float arrowFlightDelay = rangerPresentation.TryPlayBasicAttackArrow(attacker, target, turnManager);
                if (arrowFlightDelay > 0f)
                {
                    yield return new WaitForSeconds(arrowFlightDelay);
                }
            }

            if (target == null || !target.IsAlive)
            {
                NotifyBasicAttackResolved(attacker, target);
                TryAutoEndAttackerTurn(attacker);
                yield break;
            }

            int damage = attacker != null && attacker.Stats != null ? attacker.Stats.BaseDamage : 0;
            bool targetDied = damageResolver.ApplyDamage(target, damage);
            MageHitPresentationConfig magePresentation = attacker != null ? attacker.GetComponent<MageHitPresentationConfig>() : null;
            if (magePresentation != null)
            {
                magePresentation.TryPlayBasicAttackImpactEffect(attacker, target);
            }

            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;

            if (combatLog != null && attacker != null)
            {
                combatLog.LogHit(attacker, target, Mathf.Max(0, damage), remainingHp);
                if (targetDied)
                {
                    combatLog.LogDeath(target);
                }
            }

            NotifyBasicAttackResolved(attacker, target);
            TryAutoEndAttackerTurn(attacker);
        }

        private void NotifyBasicAttackResolved(Unit attacker, Unit target)
        {
            if (BasicAttackResolved != null)
            {
                BasicAttackResolved(attacker, target);
            }
        }

        private float GetBasicAttackImpactDelay(Unit attacker)
        {
            if (IsMeleePresentation(attacker))
            {
                return meleeBasicAttackImpactDelay > 0f ? meleeBasicAttackImpactDelay : 0.65f;
            }

            return basicAttackImpactDelay > 0f ? basicAttackImpactDelay : 0.18f;
        }

        private float GetBasicAttackAnimationSpeed(Unit attacker)
        {
            return attacker != null && attacker.Faction == UnitFaction.Enemy ? 0.75f : 1f;
        }

        private float GetBasicAttackAnimationRestoreDelay(Unit attacker)
        {
            if (attacker == null || attacker.Faction != UnitFaction.Enemy)
            {
                return 0f;
            }

            return Mathf.Max(0.9f, GetBasicAttackImpactDelay(attacker) + 0.35f);
        }

        private float GetBasicAttackSfxDelay(Unit attacker, BTAudioCueId cue)
        {
            if (cue == BTAudioCueId.AxeChop && attacker != null && attacker.RoleId == RoleId.Barbarian)
            {
                return Mathf.Max(0f, axeBasicAttackSfxDelay);
            }

            return 0f;
        }

        private static bool IsMeleePresentation(Unit attacker)
        {
            if (attacker == null)
            {
                return true;
            }

            if (attacker.GetComponent<RangerHitPresentationConfig>() != null
                || attacker.GetComponent<MageHitPresentationConfig>() != null)
            {
                return false;
            }

            return attacker.RoleId == RoleId.Fighter
                || attacker.RoleId == RoleId.Barbarian
                || attacker.RoleId == RoleId.Enemy
                || attacker.RoleId == RoleId.None;
        }

        private bool CanBasicParticipantsAttack(Unit attacker, Unit target, out string reason)
        {
            if (attacker == null)
            {
                reason = "攻击者不存在。";
                return false;
            }

            if (target == null)
            {
                reason = "目标不存在。";
                return false;
            }

            if (attacker == target)
            {
                reason = "攻击者不能以自己为目标。";
                return false;
            }

            if (!attacker.IsAlive)
            {
                reason = "攻击者已倒下。";
                return false;
            }

            if (!target.IsAlive)
            {
                reason = "目标已倒下。";
                return false;
            }

            if (attacker.Faction == UnitFaction.None || target.Faction == UnitFaction.None)
            {
                reason = "攻击者或目标阵营缺失。";
                return false;
            }

            if (attacker.Faction == target.Faction)
            {
                reason = "普通攻击只能选择敌对阵营目标。";
                return false;
            }

            if (attacker.CurrentTile == null)
            {
                reason = "攻击者没有所在格。";
                return false;
            }

            if (target.CurrentTile == null)
            {
                reason = "目标没有所在格。";
                return false;
            }

            reason = "普通攻击基础校验通过。";
            return true;
        }

        private bool CanPassTurnGate(Unit attacker, out string reason)
        {
            bool hasTurnManager = turnManager != null;
            bool hasActionPermissionService = actionPermissionService != null;

            if (hasTurnManager != hasActionPermissionService)
            {
                reason = "回合行动校验配置不完整。";
                return false;
            }

            if (!hasTurnManager)
            {
                reason = "未配置回合行动校验。";
                return true;
            }

            UnitTurnState turnState = attacker != null ? attacker.GetComponent<UnitTurnState>() : null;
            if (turnState == null)
            {
                reason = "攻击者缺少回合状态。";
                return false;
            }

            if (!IsFactionAllowedForCurrentPhase(attacker, turnManager))
            {
                reason = "当前回合不能由该阵营行动。";
                return false;
            }

            if (turnState.HasActed)
            {
                reason = "攻击者本回合已经行动过。";
                return false;
            }

            if (attacker.Faction == UnitFaction.Player
                && actionPermissionService.RequireCurrentRole
                && attacker.RoleId != turnManager.CurrentRole)
            {
                reason = "当前角色与回合角色不匹配。";
                return false;
            }

            bool canAct = actionPermissionService.CanAct(attacker, turnManager);
            reason = canAct ? "回合行动校验通过。" : "攻击者当前不能行动。";
            return canAct;
        }

        private bool CanUseCombatServices(out string reason)
        {
            if (d20Roller == null)
            {
                reason = "D20 掷骰器未绑定。";
                return false;
            }

            if (attackRangeService == null)
            {
                reason = "攻击范围系统未绑定。";
                return false;
            }

            if (damageResolver == null)
            {
                reason = "伤害结算系统未绑定。";
                return false;
            }

            reason = "战斗系统引用完整。";
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
                TryAutoEndAttackerTurn(attacker);
                return false;
            }

            return actionPermissionService.CanAct(attacker, turnManager);
        }

        private static bool IsFactionAllowedForCurrentPhase(Unit attacker, TurnManager turnManager)
        {
            if (attacker == null || turnManager == null)
            {
                return false;
            }

            if (turnManager.CurrentPhase == TurnPhase.PlayerTurn)
            {
                return attacker.Faction == UnitFaction.Player;
            }

            if (turnManager.CurrentPhase == TurnPhase.EnemyTurn)
            {
                return attacker.Faction == UnitFaction.Enemy;
            }

            return false;
        }

        private void TryAutoEndAttackerTurn(Unit attacker)
        {
            if (turnManager != null)
            {
                turnManager.TryAutoEndPlayerUnitTurnIfNoAvailableActions(attacker);
            }
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

        private void LogRejected(string reason, UnityEngine.Object context)
        {
            BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
            if (combatLog != null)
            {
                combatLog.LogRejected(reason, context);
                return;
            }

            Debug.LogWarning("Basic attack rejected: " + reason, context);
        }

        private void LogAnimationDebug(Unit unit, UnitAnimationController animationController, string method)
        {
            if (!AnimationDebug)
            {
                return;
            }

            Debug.Log(
                "AnimationDebug CombatSystem: unit="
                + (unit != null ? unit.name : "null")
                + " UnitId="
                + (unit != null ? unit.UnitId.ToString() : "n/a")
                + " controller="
                + (animationController != null ? animationController.name : "null")
                + " method="
                + method
                + ".",
                unit);
        }
    }
}
