using BoneThrone.Combat;
using BoneThrone.Core;
using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Phase 12 role skill effect entry point. Unknown skills keep the Phase 11 damage behavior.
    /// </summary>
    public sealed class SkillEffectExecutor : MonoBehaviour
    {
        [SerializeField] private Unit[] knownUnits;
        [SerializeField] private ActiveUnitProvider activeUnitProvider;
        [SerializeField] private CombatLog combatLog;

        public bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out string resultLog)
        {
            SkillEffectResult result;
            bool targetDied = TryExecute(caster, target, skill, damageResolver, out result);
            resultLog = result != null ? result.Summary : "Skill effect did not run.";
            return targetDied;
        }

        public bool TryExecute(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, out SkillEffectResult result)
        {
            result = new SkillEffectResult { Summary = "Skill effect did not run." };

            if (caster == null || target == null || skill == null || damageResolver == null)
            {
                result.Summary = "SkillEffectExecutor missing caster, target, SkillData, or DamageResolver.";
                Debug.LogWarning(result.Summary, this);
                return false;
            }

            if (TryExecutePhase1419Skill(caster, target, skill, damageResolver, result))
            {
                return result.PrimaryTargetDied;
            }

            switch (caster.RoleId)
            {
                case RoleId.Fighter:
                    if (FighterSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Ranger:
                    if (RangerSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Mage:
                    if (MageSkillEffects.TryExecute(caster, target, skill, damageResolver, GetKnownUnitsForSkillEffects(), this, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;

                case RoleId.Barbarian:
                    if (BarbarianSkillEffects.TryExecute(caster, target, skill, damageResolver, result))
                    {
                        return result.PrimaryTargetDied;
                    }

                    break;
            }

            if (TryExecutePhase1413Skill(caster, target, skill, damageResolver, result))
            {
                return result.PrimaryTargetDied;
            }

            ApplyFallbackDamage(target, skill, damageResolver, result);
            result.Summary = "Phase 11 fallback guaranteed damage " + skill.GuaranteedDamage + ".";
            return result.PrimaryTargetDied;
        }

        private bool TryExecutePhase1419Skill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "fighter_shield_bash"))
            {
                ApplySingleTargetDamage(target, 3, damageResolver, result);
                SkillKnockbackUtility.TryKnockbackOneTile(caster, target, ResolveCombatLog());
                result.Summary = "Shield Bash dealt 3 damage and attempted knockback.";
                return true;
            }

            if (SkillNameMatches(skill, "fighter_guard_strike"))
            {
                ApplySingleTargetDamage(target, 5, damageResolver, result);
                ApplyDamageAmplify(target, 1);
                result.Summary = "Guard Strike dealt 5 damage and applied Damage Amplify +1.";
                return true;
            }

            if (SkillNameMatches(skill, "fighter_crushing_challenge"))
            {
                ApplySingleTargetDamage(target, 5, damageResolver, result);
                ApplyStun(target);
                result.Summary = "Crushing Challenge dealt 5 damage and applied Stun.";
                return true;
            }

            if (SkillNameMatches(skill, "ranger_precision_shot"))
            {
                ApplySingleTargetDamage(target, 5, damageResolver, result);
                result.Summary = "Precision Shot dealt 5 damage.";
                return true;
            }

            if (SkillNameMatches(skill, "ranger_quick_shot"))
            {
                ApplySingleTargetDamage(target, 3, damageResolver, result);
                ApplyBleed(target, 2);
                result.Summary = "Quick Shot dealt 3 damage and applied 2 Bleed stack(s).";
                return true;
            }

            if (SkillNameMatches(skill, "ranger_piercing_arrow"))
            {
                ApplySingleTargetDamage(target, 6, damageResolver, result);
                int secondaryHits = ApplyPiercingSecondaryHit(caster, target, 3, damageResolver, result);
                result.Summary = "Piercing Arrow dealt 6 primary damage and secondary hits " + secondaryHits + ".";
                return true;
            }

            if (SkillNameMatches(skill, "mage_fireball"))
            {
                ApplySingleTargetDamage(target, 3, damageResolver, result);
                int splashHits = ApplyAdjacentSplash(caster, target, damageResolver, GetKnownUnitsForSkillEffects(), 1, result);
                result.Summary = "Fireball dealt 3 primary damage and splash hits " + splashHits + ".";
                return true;
            }

            if (SkillNameMatches(skill, "mage_frost_bolt"))
            {
                ApplySingleTargetDamage(target, 2, damageResolver, result);
                ApplyStun(target);
                result.Summary = "Frost Bolt dealt 2 damage and applied Stun.";
                return true;
            }

            if (SkillNameMatches(skill, "mage_arcane_burst"))
            {
                ApplySingleTargetDamage(target, 5, damageResolver, result);
                int splashHits = ApplyAdjacentSplash(caster, target, damageResolver, GetKnownUnitsForSkillEffects(), 2, result);
                ApplyDamageAmplify(target, 2);
                result.Summary = "Arcane Burst dealt 5 primary damage, splash hits " + splashHits + ", and applied Damage Amplify +2.";
                return true;
            }

            if (SkillNameMatches(skill, "barbarian_heavy_cleave"))
            {
                int damage = GetHeavyCleaveDamage(caster);
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Heavy Cleave dealt " + damage + " damage.";
                return true;
            }

            if (SkillNameMatches(skill, "barbarian_rage_strike"))
            {
                bool bloodied = IsAtOrBelowHalfHp(caster);
                int damage = bloodied ? 8 : 4;
                int bleedStacks = bloodied ? 6 : 3;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                ApplyBleed(target, bleedStacks);
                result.Summary = "Rage Strike dealt " + damage + " damage and applied " + bleedStacks + " Bleed stack(s).";
                return true;
            }

            if (SkillNameMatches(skill, "barbarian_blood_fury_slash"))
            {
                int damage = IsAtOrBelowHalfHp(caster) ? 6 : 4;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                ApplyBleed(target, 2);
                result.Summary = "Blood Fury Slash dealt " + damage + " damage and applied 2 Bleed stack(s).";
                return true;
            }

            return false;
        }

        private bool TryExecutePhase1413Skill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            switch (caster.RoleId)
            {
                case RoleId.Fighter:
                    return TryExecutePhase1413FighterSkill(target, skill, damageResolver, result);
                case RoleId.Ranger:
                    return TryExecutePhase1413RangerSkill(target, skill, damageResolver, result);
                case RoleId.Mage:
                    return TryExecutePhase1413MageSkill(caster, target, skill, damageResolver, result);
                case RoleId.Barbarian:
                    return TryExecutePhase1413BarbarianSkill(caster, target, skill, damageResolver, result);
                default:
                    return false;
            }
        }

        private static bool TryExecutePhase1413FighterSkill(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "fighter_guard_strike"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Fighter Guard Strike dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "fighter_crushing_challenge"))
            {
                int damage = skill.GuaranteedDamage + 2;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Fighter Crushing Challenge dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private static bool TryExecutePhase1413RangerSkill(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "ranger_quick_shot"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Ranger Quick Shot dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "ranger_piercing_arrow"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Ranger Piercing Arrow dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private bool TryExecutePhase1413MageSkill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "mage_frost_bolt"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Mage Frost Bolt dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "mage_arcane_burst"))
            {
                int damage = skill.GuaranteedDamage;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                int splashHits = ApplyArcaneBurstSplash(caster, target, damageResolver, GetKnownUnitsForSkillEffects(), result);
                result.Summary = "Mage Arcane Burst dealt guaranteed damage " + damage + " and splash hits " + splashHits + ".";
                return true;
            }

            return false;
        }

        private static bool TryExecutePhase1413BarbarianSkill(Unit caster, Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            if (SkillNameMatches(skill, "barbarian_rage_strike"))
            {
                int damage = skill.GuaranteedDamage + 1;
                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Barbarian Rage Strike dealt guaranteed damage " + damage + ".";
                return true;
            }

            if (SkillNameMatches(skill, "barbarian_blood_fury_slash"))
            {
                int damage = skill.GuaranteedDamage + 2;
                if (IsAtOrBelowHalfHp(caster))
                {
                    damage++;
                }

                ApplySingleTargetDamage(target, damage, damageResolver, result);
                result.Summary = "Barbarian Blood Fury Slash dealt guaranteed damage " + damage + ".";
                return true;
            }

            return false;
        }

        private Unit[] GetKnownUnitsForSkillEffects()
        {
            ActiveUnitProvider provider = ResolveActiveUnitProvider();
            Unit[] activeUnits = provider != null ? provider.GetActiveAliveUnits() : null;
            if (activeUnits != null && activeUnits.Length > 0)
            {
                return activeUnits;
            }

            return knownUnits;
        }

        private ActiveUnitProvider ResolveActiveUnitProvider()
        {
            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            return activeUnitProvider;
        }

        private CombatLog ResolveCombatLog()
        {
            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }

            return combatLog;
        }

        internal static bool SkillNameMatches(SkillData skill, string expectedName)
        {
            if (skill == null)
            {
                return false;
            }

            return NormalizeSkillName(skill.DisplayName) == NormalizeSkillName(expectedName);
        }

        internal static bool SkillNameMatchesAny(SkillData skill, string firstName, string secondName)
        {
            return SkillNameMatches(skill, firstName) || SkillNameMatches(skill, secondName);
        }

        internal static int GetManhattanDistance(Unit first, Unit second)
        {
            if (first == null || second == null || first.CurrentTile == null || second.CurrentTile == null)
            {
                return int.MaxValue;
            }

            int dx = Mathf.Abs(first.CurrentTile.Position.X - second.CurrentTile.Position.X);
            int dy = Mathf.Abs(first.CurrentTile.Position.Y - second.CurrentTile.Position.Y);
            return dx + dy;
        }

        private static void ApplySingleTargetDamage(Unit target, int damage, DamageResolver damageResolver, SkillEffectResult result)
        {
            bool targetDied = damageResolver.ApplyDamage(target, damage);
            int remainingHp = target.RuntimeState != null ? target.RuntimeState.CurrentHp : 0;
            result.AddDamage(target, damage, remainingHp, targetDied, true);
        }

        private static int ApplyArcaneBurstSplash(Unit caster, Unit target, DamageResolver damageResolver, Unit[] knownUnits, SkillEffectResult result)
        {
            if (knownUnits == null || knownUnits.Length == 0)
            {
                return 0;
            }

            int splashHits = 0;
            for (int i = 0; i < knownUnits.Length; i++)
            {
                Unit candidate = knownUnits[i];
                if (!IsValidArcaneBurstSplashTarget(caster, target, candidate))
                {
                    continue;
                }

                if (GetManhattanDistance(target, candidate) == 1)
                {
                    bool targetDied = damageResolver.ApplyDamage(candidate, 1);
                    int remainingHp = candidate.RuntimeState != null ? candidate.RuntimeState.CurrentHp : 0;
                    result.AddDamage(candidate, 1, remainingHp, targetDied, false);
                    splashHits++;
                }
            }

            return splashHits;
        }

        private static int ApplyAdjacentSplash(Unit caster, Unit target, DamageResolver damageResolver, Unit[] knownUnits, int damage, SkillEffectResult result)
        {
            if (knownUnits == null || knownUnits.Length == 0)
            {
                return 0;
            }

            int splashHits = 0;
            for (int i = 0; i < knownUnits.Length; i++)
            {
                Unit candidate = knownUnits[i];
                if (!IsValidArcaneBurstSplashTarget(caster, target, candidate))
                {
                    continue;
                }

                if (GetManhattanDistance(target, candidate) == 1)
                {
                    bool targetDied = damageResolver.ApplyDamage(candidate, damage);
                    int remainingHp = candidate.RuntimeState != null ? candidate.RuntimeState.CurrentHp : 0;
                    result.AddDamage(candidate, damage, remainingHp, targetDied, false);
                    splashHits++;
                }
            }

            return splashHits;
        }

        private int ApplyPiercingSecondaryHit(Unit caster, Unit target, int damage, DamageResolver damageResolver, SkillEffectResult result)
        {
            Unit secondaryTarget = FindPiercingSecondaryTarget(caster, target);
            if (secondaryTarget == null)
            {
                return 0;
            }

            bool targetDied = damageResolver.ApplyDamage(secondaryTarget, damage);
            int remainingHp = secondaryTarget.RuntimeState != null ? secondaryTarget.RuntimeState.CurrentHp : 0;
            result.AddDamage(secondaryTarget, damage, remainingHp, targetDied, false);
            return 1;
        }

        private Unit FindPiercingSecondaryTarget(Unit caster, Unit target)
        {
            if (caster == null || target == null || caster.CurrentTile == null || target.CurrentTile == null)
            {
                return null;
            }

            GridPosition casterPosition = caster.CurrentTile.Position;
            GridPosition targetPosition = target.CurrentTile.Position;
            int dx = targetPosition.X - casterPosition.X;
            int dy = targetPosition.Y - casterPosition.Y;
            int stepX = 0;
            int stepY = 0;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                stepX = dx == 0 ? 0 : dx > 0 ? 1 : -1;
            }
            else
            {
                stepY = dy == 0 ? 0 : dy > 0 ? 1 : -1;
            }

            if (stepX == 0 && stepY == 0)
            {
                return null;
            }

            GridPosition secondaryPosition = new GridPosition(targetPosition.X + stepX, targetPosition.Y + stepY);
            Unit[] units = GetKnownUnitsForSkillEffects();
            if (units == null)
            {
                return null;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit candidate = units[i];
                if (candidate != null
                    && candidate != target
                    && candidate.gameObject.activeInHierarchy
                    && candidate.IsAlive
                    && candidate.Faction != caster.Faction
                    && candidate.CurrentTile != null
                    && candidate.CurrentTile.Position == secondaryPosition)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ApplyStun(Unit target)
        {
            if (target == null)
            {
                return;
            }

            UnitStunState stunState = target.GetComponent<UnitStunState>();
            if (stunState == null)
            {
                stunState = target.gameObject.AddComponent<UnitStunState>();
            }

            stunState.ApplyStun();
            CombatLog log = ResolveCombatLog();
            if (log != null)
            {
                log.LogStunApplied(target);
            }
        }

        private void ApplyBleed(Unit target, int stacks)
        {
            if (target == null)
            {
                return;
            }

            UnitBleedState bleedState = target.GetComponent<UnitBleedState>();
            if (bleedState == null)
            {
                bleedState = target.gameObject.AddComponent<UnitBleedState>();
            }

            bleedState.ApplyBleed(stacks);
            CombatLog log = ResolveCombatLog();
            if (log != null)
            {
                log.LogBleedApplied(target, stacks);
            }
        }

        private void ApplyDamageAmplify(Unit target, int bonus)
        {
            if (target == null)
            {
                return;
            }

            UnitDamageAmplifyState amplifyState = target.GetComponent<UnitDamageAmplifyState>();
            if (amplifyState == null)
            {
                amplifyState = target.gameObject.AddComponent<UnitDamageAmplifyState>();
            }

            amplifyState.ApplyAmplify(bonus);
            CombatLog log = ResolveCombatLog();
            if (log != null)
            {
                log.LogDamageAmplifyApplied(target, bonus);
            }
        }

        private static bool IsValidArcaneBurstSplashTarget(Unit caster, Unit primaryTarget, Unit candidate)
        {
            return caster != null
                && primaryTarget != null
                && candidate != null
                && candidate != primaryTarget
                && candidate.gameObject.activeInHierarchy
                && candidate.IsAlive
                && candidate.Faction != caster.Faction
                && candidate.CurrentTile != null;
        }

        private static bool IsAtOrBelowHalfHp(Unit caster)
        {
            if (caster == null || caster.Stats == null || caster.RuntimeState == null)
            {
                return false;
            }

            int maxHp = caster.Stats.GetClampedMaxHp();
            return caster.RuntimeState.CurrentHp * 2 <= maxHp;
        }

        private static int GetHeavyCleaveDamage(Unit caster)
        {
            if (caster == null || caster.Stats == null || caster.RuntimeState == null)
            {
                return 5;
            }

            int maxHp = caster.Stats.GetClampedMaxHp();
            int lostHp = Mathf.Max(0, maxHp - caster.RuntimeState.CurrentHp);
            int bonus = Mathf.FloorToInt(lostHp * 0.1f);
            return Mathf.Max(5, 5 + bonus);
        }

        private static void ApplyFallbackDamage(Unit target, SkillData skill, DamageResolver damageResolver, SkillEffectResult result)
        {
            ApplySingleTargetDamage(target, skill.GuaranteedDamage, damageResolver, result);
        }

        private static string NormalizeSkillName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string lower = value.ToLowerInvariant();
            char[] buffer = new char[lower.Length];
            int count = 0;

            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    buffer[count] = c;
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }
    }
}
