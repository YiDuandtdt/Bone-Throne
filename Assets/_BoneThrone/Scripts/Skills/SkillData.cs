using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Minimal Phase 11 unit target categories for guaranteed-hit skills.
    /// </summary>
    public enum SkillTargetType
    {
        Enemy = 0,
        Ally = 1,
        Self = 2,
        AnyUnit = 3
    }

    public enum SkillRangeShape
    {
        Automatic = 0,
        Manhattan = 1,
        Chebyshev = 2,
        ManhattanWithCloseDiagonals = 3,
        ManhattanWithCardinalTips = 4
    }

    /// <summary>
    /// Minimal inspectable skill definition for Phase 11.
    /// It describes unlock level, range, cooldown, target rule, and guaranteed damage only.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "Bone Throne/Skills/Skill Data")]
    public sealed class SkillData : ScriptableObject
    {
        [SerializeField] private int skillId = 1;
        [SerializeField] private string displayName = "Test Skill";
        [SerializeField] private int unlockLevel = 1;
        [SerializeField] private int range = 1;
        [SerializeField]
        [Tooltip("Automatic chooses a skill-specific, area-based target pattern from the skill identity. Override only when this asset needs a custom pattern.")]
        private SkillRangeShape rangeShape = SkillRangeShape.Automatic;
        [SerializeField] private int cooldownTurns = 1;
        [SerializeField] private int guaranteedDamage = 1;
        [SerializeField] private SkillTargetType targetType = SkillTargetType.Enemy;

        public int SkillId
        {
            get { return Mathf.Max(0, skillId); }
        }

        public string DisplayName
        {
            get { return displayName; }
        }

        public int UnlockLevel
        {
            get { return Mathf.Clamp(unlockLevel, 1, 3); }
        }

        public int Range
        {
            get { return Mathf.Max(0, range); }
        }

        public SkillRangeShape RangeShape
        {
            get { return rangeShape; }
        }

        public int CooldownTurns
        {
            get { return Mathf.Max(0, cooldownTurns); }
        }

        public int GuaranteedDamage
        {
            get { return Mathf.Max(0, guaranteedDamage); }
        }

        public SkillTargetType TargetType
        {
            get { return targetType; }
        }

        public bool IsUnlockedForLevel(int unitLevel)
        {
            return Mathf.Max(1, unitLevel) >= UnlockLevel;
        }
    }
}
