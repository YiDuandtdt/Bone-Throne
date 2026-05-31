using BoneThrone.Skills;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary Phase 11 ContextMenu helper for validating skill unlock, range, target checks,
    /// guaranteed damage, action consumption, and cooldowns without production UI.
    /// </summary>
    public sealed class Phase11SkillTester : MonoBehaviour
    {
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private Unit caster;
        [SerializeField] private Unit target;
        [SerializeField] private int slotIndex;

        [ContextMenu("Phase 11/Use Skill Test")]
        public void UseSkillTest()
        {
            if (!HasSkillSystem())
            {
                return;
            }

            bool result = skillSystem.TryUseSkill(caster, target, slotIndex);
            Debug.Log("Phase11SkillTester UseSkillTest result: " + result + ".", this);
            LogSkillState();
        }

        [ContextMenu("Phase 11/Tick Caster Cooldowns")]
        public void TickCasterCooldownsTest()
        {
            if (!HasSkillSystem())
            {
                return;
            }

            skillSystem.TickCooldownsForUnit(caster);
            LogSkillState();
        }

        [ContextMenu("Phase 11/Reset Caster Cooldowns")]
        public void ResetCasterCooldownsTest()
        {
            SkillRuntime runtime = GetCasterRuntime();
            if (runtime == null)
            {
                return;
            }

            runtime.ResetCooldownsForTest();
            LogSkillState();
        }

        [ContextMenu("Phase 11/Log Skill State")]
        public void LogSkillState()
        {
            if (caster == null)
            {
                Debug.LogWarning("Phase11SkillTester cannot log skill state because caster is missing.", this);
                return;
            }

            SkillRuntime runtime = GetCasterRuntime();
            if (runtime == null)
            {
                return;
            }

            int level = caster.Stats != null ? caster.Stats.Level : 0;
            string message = "Phase11SkillTester: caster="
                + caster.UnitId
                + " level="
                + level
                + " slotCount="
                + runtime.SlotCount
                + ".";

            for (int i = 0; i < runtime.SlotCount; i++)
            {
                SkillData skill = runtime.GetSkill(i);
                string skillName = skill != null ? skill.DisplayName : "None";
                string unlockLevel = skill != null ? skill.UnlockLevel.ToString() : "-";
                message += " Slot"
                    + i
                    + "[skill="
                    + skillName
                    + ", unlockLevel="
                    + unlockLevel
                    + ", unlocked="
                    + runtime.IsUnlocked(caster, i)
                    + ", cooldown="
                    + runtime.GetCooldown(i)
                    + "]";
            }

            Debug.Log(message, this);
        }

        private bool HasSkillSystem()
        {
            if (skillSystem != null)
            {
                return true;
            }

            Debug.LogWarning("Phase11SkillTester needs a SkillSystem reference.", this);
            return false;
        }

        private SkillRuntime GetCasterRuntime()
        {
            if (caster == null)
            {
                Debug.LogWarning("Phase11SkillTester needs a caster Unit reference.", this);
                return null;
            }

            SkillRuntime runtime = caster.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                Debug.LogWarning("Phase11SkillTester needs the caster Unit to have a SkillRuntime component.", caster);
                return null;
            }

            return runtime;
        }
    }
}
