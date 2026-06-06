using BoneThrone.Combat;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Captures the delayed-resolution outcome of one skill impact.
    /// </summary>
    public sealed class SkillImpactResolution
    {
        public bool TargetDied { get; set; }

        public SkillEffectResult EffectResult { get; set; }
    }
}
