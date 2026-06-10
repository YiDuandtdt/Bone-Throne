using System.Collections.Generic;
using BoneThrone.Units;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Structured feedback result for skill UI/log output. It does not drive gameplay state.
    /// </summary>
    public sealed class SkillEffectResult
    {
        private readonly List<SkillDamageLogEntry> damageEntries = new List<SkillDamageLogEntry>();

        public string Summary { get; set; }

        public IReadOnlyList<SkillDamageLogEntry> DamageEntries
        {
            get { return damageEntries; }
        }

        public bool AnyTargetDied { get; private set; }

        public bool PrimaryTargetDied { get; private set; }

        public void AddDamage(Unit target, int damage, int remainingHp, bool targetDied, bool isPrimaryTarget)
        {
            damageEntries.Add(new SkillDamageLogEntry(target, damage, remainingHp, targetDied, isPrimaryTarget));
            if (targetDied)
            {
                AnyTargetDied = true;
                if (isPrimaryTarget)
                {
                    PrimaryTargetDied = true;
                }
            }
        }
    }

    public readonly struct SkillDamageLogEntry
    {
        public SkillDamageLogEntry(Unit target, int damage, int remainingHp, bool targetDied, bool isPrimaryTarget)
        {
            Target = target;
            Damage = damage;
            RemainingHp = remainingHp;
            TargetDied = targetDied;
            IsPrimaryTarget = isPrimaryTarget;
        }

        public Unit Target { get; }
        public int Damage { get; }
        public int RemainingHp { get; }
        public bool TargetDied { get; }
        public bool IsPrimaryTarget { get; }
    }
}
