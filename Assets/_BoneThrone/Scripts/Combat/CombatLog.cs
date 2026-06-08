using System;
using System.Collections.Generic;
using BoneThrone.Core;
using BoneThrone.Grid;
using BoneThrone.Skills;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Combat
{
    /// <summary>
    /// Collects recent combat feedback for the HUD and mirrors it to the Console for debugging.
    /// </summary>
    public sealed class CombatLog : MonoBehaviour
    {
        public enum EntryType
        {
            Rejected = 0,
            AttackAttempt = 1,
            Hit = 2,
            Miss = 3,
            Death = 4,
            SkillUse = 5,
            SkillEffect = 6,
            SkillRejected = 7,
            SkillCooldown = 8,
            Defend = 9,
            DamageReduced = 10,
            Potion = 11,
            Heal = 12,
            PotionRejected = 13,
            StunApplied = 14,
            StunConsumed = 15,
            BleedApplied = 16,
            BleedTick = 17,
            DamageAmplifyApplied = 18,
            DamageAmplified = 19,
            Knockback = 20,
            KnockbackBlocked = 21
        }

        public sealed class Entry
        {
            public Entry(EntryType type, string message)
            {
                Type = type;
                Message = message;
            }

            public EntryType Type { get; private set; }
            public string Message { get; private set; }
        }

        private const int MaxRecentEntries = 12;

        private readonly List<Entry> recentEntries = new List<Entry>();

        public event Action<Entry> EntryAdded;

        public IReadOnlyList<Entry> RecentEntries
        {
            get { return recentEntries; }
        }

        public void LogRejected(string reason, UnityEngine.Object context)
        {
            string message = "行动失败：" + FormatReason(reason);
            AddEntry(EntryType.Rejected, message);
            Debug.LogWarning(message, context);
        }

        public void LogAttackAttempt(Unit attacker, Unit target, int roll, int attackModifier, int defense)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(attacker)
                + " 准备攻击 "
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "。命中检定为 "
                + roll
                + " + "
                + attackModifier
                + "，目标防御为 "
                + defense
                + "。";
            Debug.Log(message, attacker);
        }

        public void LogBasicAttackRoll(Unit attacker, int roll, int attackModifier)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(attacker)
                + " 的普通攻击掷出了 "
                + roll
                + "，修正值为 "
                + attackModifier
                + "。";
            Debug.Log(message, attacker);
        }

        public void LogHit(Unit attacker, Unit target, int damage, int remainingHp)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(attacker)
                + " 命中了 "
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "，造成 "
                + Mathf.Max(0, damage)
                + " 点伤害。剩余生命 "
                + Mathf.Max(0, remainingHp)
                + "。";
            AddEntry(EntryType.Hit, message);
            Debug.Log(message, target);
        }

        public void LogMiss(Unit attacker, Unit target)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(attacker)
                + " 攻击 "
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "，但未能命中。";
            AddEntry(EntryType.Miss, message);
            Debug.Log(message, attacker);
        }

        public void LogDeath(Unit target)
        {
            string message = "<b>" + BoneThroneTextUtility.ColorizeUnitName(target) + " 倒下了。</b>";
            AddEntry(EntryType.Death, message);
            Debug.Log(message, target);
        }

        public void LogSkillRejected(string reason, UnityEngine.Object context)
        {
            string message = "技能失败：" + FormatReason(reason);
            AddEntry(EntryType.SkillRejected, message);
            Debug.LogWarning(message, context);
        }

        public void LogSkillUse(Unit caster, Unit target, SkillData skill)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(caster)
                + " 使用了 "
                + BoneThroneTextUtility.ColorizeSkillName(caster, skill)
                + "，目标是 "
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "。";
            AddEntry(EntryType.SkillUse, message);
            Debug.Log(message, caster);
        }

        public void LogSkillEffect(Unit caster, Unit target, SkillData skill, string effectSummary, int remainingHp)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(caster)
                + " 的 "
                + BoneThroneTextUtility.ColorizeSkillName(caster, skill)
                + " 命中了 "
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "。"
                + FormatEffectSummary(effectSummary)
                + "剩余生命 "
                + Mathf.Max(0, remainingHp)
                + "。";
            AddEntry(EntryType.SkillEffect, message);
            Debug.Log(message, target);
        }

        public void LogSkillDamage(Unit caster, Unit target, SkillData skill, int damage, int remainingHp, bool isPrimaryTarget)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(caster)
                + " 使用 "
                + BoneThroneTextUtility.ColorizeSkillName(caster, skill)
                + (isPrimaryTarget ? " 命中 " : " 的溅射命中 ")
                + BoneThroneTextUtility.ColorizeUnitName(target)
                + "，造成 "
                + Mathf.Max(0, damage)
                + " 点伤害。剩余生命 "
                + Mathf.Max(0, remainingHp)
                + "。";
            AddEntry(EntryType.SkillEffect, message);
            Debug.Log(message, target);
        }

        public void LogSkillCooldown(Unit caster, SkillData skill, int cooldown)
        {
            string message = BoneThroneTextUtility.ColorizeSkillName(caster, skill)
                + " 进入冷却，"
                + Mathf.Max(0, cooldown)
                + " 回合后可再次使用。";
            Debug.Log(message, caster);
        }

        public void LogDefend(Unit unit, int reduction)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(unit)
                + " 进入防御姿态，下次受到的伤害减少 "
                + Mathf.Max(0, reduction)
                + " 点。";
            AddEntry(EntryType.Defend, message);
            Debug.Log(message, unit);
        }

        public void LogDefendRejected(string reason, UnityEngine.Object context)
        {
            string message = "防御失败：" + FormatReason(reason);
            AddEntry(EntryType.Rejected, message);
            Debug.LogWarning(message, context);
        }

        public void LogDamageReduced(Unit target, int originalDamage, int finalDamage, int reducedAmount)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target)
                + " 的防御生效，伤害由 "
                + Mathf.Max(0, originalDamage)
                + " 降至 "
                + Mathf.Max(0, finalDamage)
                + "，共减免 "
                + Mathf.Max(0, reducedAmount)
                + " 点。";
            AddEntry(EntryType.DamageReduced, message);
            Debug.Log(message, target);
        }

        public void LogPotionUsed(Unit unit, int healAmount, int currentHp, int maxHp, int remainingPotions)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(unit)
                + " 使用药水，恢复 "
                + Mathf.Max(0, healAmount)
                + " 点生命。当前生命 "
                + Mathf.Max(0, currentHp)
                + " / "
                + Mathf.Max(1, maxHp)
                + "，剩余药水 "
                + Mathf.Max(0, remainingPotions)
                + "。";
            AddEntry(EntryType.Potion, message);
            AddEntry(EntryType.Heal, message);
            Debug.Log(message, unit);
        }

        public void LogPotionRejected(string reason, UnityEngine.Object context)
        {
            string message = "药水使用失败：" + FormatReason(reason);
            AddEntry(EntryType.PotionRejected, message);
            Debug.LogWarning(message, context);
        }

        public void LogStunApplied(Unit target)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target) + " 被眩晕，下一次移动和行动将被跳过。";
            AddEntry(EntryType.StunApplied, message);
            Debug.Log(message, target);
        }

        public void LogStunConsumed(Unit target)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target) + " 因眩晕跳过了本次移动和行动。";
            AddEntry(EntryType.StunConsumed, message);
            Debug.Log(message, target);
        }

        public void LogBleedApplied(Unit target, int stacks)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target)
                + " 获得了 "
                + Mathf.Max(0, stacks)
                + " 层流血。";
            AddEntry(EntryType.BleedApplied, message);
            Debug.Log(message, target);
        }

        public void LogBleedTick(Unit target, int damage, int remainingHp, int remainingStacks)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target)
                + " 受到 "
                + Mathf.Max(0, damage)
                + " 点流血伤害，剩余生命 "
                + Mathf.Max(0, remainingHp)
                + "，流血剩余 "
                + Mathf.Max(0, remainingStacks)
                + " 层。";
            AddEntry(EntryType.BleedTick, message);
            Debug.Log(message, target);
        }

        public void LogDamageAmplifyApplied(Unit target, int bonus)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target)
                + " 获得增伤标记，下次受到的伤害增加 "
                + Mathf.Max(0, bonus)
                + " 点。";
            AddEntry(EntryType.DamageAmplifyApplied, message);
            Debug.Log(message, target);
        }

        public void LogDamageAmplified(Unit target, int baseDamage, int bonusDamage, int amplifiedDamage)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target)
                + " 触发增伤，伤害由 "
                + Mathf.Max(0, baseDamage)
                + " 提升至 "
                + Mathf.Max(0, amplifiedDamage)
                + "。";
            AddEntry(EntryType.DamageAmplified, message);
            Debug.Log(message, target);
        }

        public void LogKnockback(Unit target, GridPosition destination)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target) + " 被击退到了 " + destination + "。";
            AddEntry(EntryType.Knockback, message);
            Debug.Log(message, target);
        }

        public void LogKnockbackBlocked(Unit target, string reason)
        {
            string message = BoneThroneTextUtility.ColorizeUnitName(target) + " 未能被击退：" + FormatReason(reason);
            AddEntry(EntryType.KnockbackBlocked, message);
            Debug.Log(message, target);
        }

        private void AddEntry(EntryType type, string message)
        {
            Entry entry = new Entry(type, message);
            recentEntries.Add(entry);
            while (recentEntries.Count > MaxRecentEntries)
            {
                recentEntries.RemoveAt(0);
            }

            if (EntryAdded != null)
            {
                EntryAdded(entry);
            }
        }

        private static string FormatReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "原因未知。";
            }

            switch (BoneThroneTextUtility.NormalizeKey(reason))
            {
                case "thepartydoesnothavethesharedkey":
                    return "队伍还没有获得共用钥匙。";
                case "casterismissing":
                case "attackerismissing":
                    return "行动者不存在。";
                case "targetismissing":
                    return "目标不存在。";
                case "casterisdead":
                case "attackerisdead":
                    return "行动者已经倒下。";
                case "targetisdead":
                    return "目标已经倒下。";
                case "skilltargetingserviceismissing":
                    return "技能目标系统未绑定。";
                case "damageresolverismissing":
                    return "伤害结算系统未绑定。";
                case "targetisoutofbasicattackrange":
                    return "目标超出普通攻击范围。";
                case "targetisoutofskillrange":
                    return "目标超出技能范围。";
                case "attackercannottargetitself":
                    return "不能以自己为目标。";
                case "basicattacktargetmustbeanopposingfaction":
                    return "普通攻击只能选择敌对阵营目标。";
                default:
                    return EnsureChinesePunctuation(reason);
            }
        }

        private static string FormatEffectSummary(string effectSummary)
        {
            if (string.IsNullOrWhiteSpace(effectSummary))
            {
                return string.Empty;
            }

            string normalized = EnsureChinesePunctuation(effectSummary);
            return normalized.EndsWith("。", StringComparison.Ordinal) ? normalized : normalized + "。";
        }

        private static string EnsureChinesePunctuation(string message)
        {
            if (message.EndsWith("。", StringComparison.Ordinal)
                || message.EndsWith("！", StringComparison.Ordinal)
                || message.EndsWith("？", StringComparison.Ordinal)
                || message.EndsWith(".", StringComparison.Ordinal)
                || message.EndsWith("!", StringComparison.Ordinal)
                || message.EndsWith("?", StringComparison.Ordinal))
            {
                return message;
            }

            return message + "。";
        }
    }
}
