using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Skills
{
    /// <summary>
    /// Per-unit runtime skill slots and cooldown state.
    /// This component is intentionally separate from Unit and UnitRuntimeState for Phase 11.
    /// </summary>
    public sealed class SkillRuntime : MonoBehaviour
    {
        [SerializeField] private SkillData[] skillSlots = new SkillData[3];
        [SerializeField] private int[] currentCooldowns = new int[3];

        public int SlotCount
        {
            get { return skillSlots != null ? skillSlots.Length : 0; }
        }

        public SkillData GetSkill(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return null;
            }

            return skillSlots[slotIndex];
        }

        public bool HasSkill(int slotIndex)
        {
            return GetSkill(slotIndex) != null;
        }

        public bool IsUnlocked(Unit unit, int slotIndex)
        {
            SkillData skill = GetSkill(slotIndex);
            if (unit == null || skill == null || unit.Stats == null)
            {
                return false;
            }

            return skill.IsUnlockedForLevel(unit.Stats.Level);
        }

        public bool IsOnCooldown(int slotIndex)
        {
            return GetCooldown(slotIndex) > 0;
        }

        public int GetCooldown(int slotIndex)
        {
            EnsureCooldownArray();
            if (!IsValidCooldownSlot(slotIndex))
            {
                return 0;
            }

            return Mathf.Max(0, currentCooldowns[slotIndex]);
        }

        public void StartCooldown(int slotIndex)
        {
            SkillData skill = GetSkill(slotIndex);
            if (skill == null)
            {
                Debug.LogWarning("SkillRuntime cannot start cooldown because the skill slot is empty or invalid.", this);
                return;
            }

            EnsureCooldownArray();
            if (!IsValidCooldownSlot(slotIndex))
            {
                Debug.LogWarning("SkillRuntime cannot start cooldown because cooldown storage is not available for slot " + slotIndex + ".", this);
                return;
            }

            currentCooldowns[slotIndex] = skill.CooldownTurns;
        }

        public void TickCooldowns()
        {
            EnsureCooldownArray();
            if (currentCooldowns == null)
            {
                return;
            }

            for (int i = 0; i < currentCooldowns.Length; i++)
            {
                currentCooldowns[i] = Mathf.Max(0, currentCooldowns[i] - 1);
            }
        }

        [ContextMenu("Phase 11/Reset Skill Cooldowns")]
        public void ResetCooldownsForTest()
        {
            EnsureCooldownArray();
            if (currentCooldowns == null)
            {
                return;
            }

            for (int i = 0; i < currentCooldowns.Length; i++)
            {
                currentCooldowns[i] = 0;
            }

            Debug.Log("SkillRuntime reset all skill cooldowns for test.", this);
        }

        private bool IsValidSlot(int slotIndex)
        {
            return skillSlots != null && slotIndex >= 0 && slotIndex < skillSlots.Length;
        }

        private bool IsValidCooldownSlot(int slotIndex)
        {
            return currentCooldowns != null && slotIndex >= 0 && slotIndex < currentCooldowns.Length;
        }

        private void EnsureCooldownArray()
        {
            int slotCount = skillSlots != null ? skillSlots.Length : 0;
            if (slotCount <= 0)
            {
                currentCooldowns = new int[0];
                return;
            }

            if (currentCooldowns != null && currentCooldowns.Length == slotCount)
            {
                return;
            }

            int[] nextCooldowns = new int[slotCount];
            if (currentCooldowns != null)
            {
                int copyCount = Mathf.Min(currentCooldowns.Length, nextCooldowns.Length);
                for (int i = 0; i < copyCount; i++)
                {
                    nextCooldowns[i] = Mathf.Max(0, currentCooldowns[i]);
                }
            }

            currentCooldowns = nextCooldowns;
        }
    }
}
