using BoneThrone.Skills;
using BoneThrone.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays the first prototype action bar without invoking gameplay actions.
    /// </summary>
    public sealed class SkillBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text basicAttackText;
        [SerializeField] private TMP_Text slot0Text;
        [SerializeField] private TMP_Text slot1Text;
        [SerializeField] private TMP_Text slot2Text;
        [SerializeField] private TMP_Text defendText;
        [SerializeField] private TMP_Text potionText;
        [SerializeField] private Button[] actionButtons;

        public void Bind(
            TMP_Text basicAttack,
            TMP_Text slot0,
            TMP_Text slot1,
            TMP_Text slot2,
            TMP_Text defend,
            TMP_Text potion,
            Button[] buttons)
        {
            basicAttackText = basicAttack;
            slot0Text = slot0;
            slot1Text = slot1;
            slot2Text = slot2;
            defendText = defend;
            potionText = potion;
            actionButtons = buttons;
            DisableButtons();
        }

        public void Refresh(Unit selectedUnit)
        {
            DisableButtons();

            SetText(basicAttackText, selectedUnit != null ? "Basic Attack\nDisplay Only" : "Basic Attack\nNo Unit");
            RefreshSlot0(selectedUnit);
            SetText(slot1Text, "Slot 1\nPlaceholder");
            SetText(slot2Text, "Slot 2\nPlaceholder");
            SetText(defendText, "Defend\nPlaceholder");
            SetText(potionText, "Potion\nPlaceholder");
        }

        private void RefreshSlot0(Unit selectedUnit)
        {
            if (slot0Text == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                slot0Text.text = "Skill 0\nNo Unit";
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null || !runtime.HasSkill(0))
            {
                slot0Text.text = "Skill 0\n--";
                return;
            }

            SkillData skill = runtime.GetSkill(0);
            bool unlocked = runtime.IsUnlocked(selectedUnit, 0);
            int cooldown = runtime.GetCooldown(0);
            string state = !unlocked ? "Locked" : cooldown > 0 ? "Cooldown " + cooldown : "Ready";
            slot0Text.text = skill.DisplayName + "\n" + state;
        }

        private void DisableButtons()
        {
            if (actionButtons == null)
            {
                return;
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                if (actionButtons[i] != null)
                {
                    actionButtons[i].interactable = false;
                }
            }
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
