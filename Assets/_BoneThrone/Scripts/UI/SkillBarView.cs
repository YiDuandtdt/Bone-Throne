using BoneThrone.Skills;
using BoneThrone.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays the first prototype action bar. Basic Attack and Skill Slot 0 emit UI intent events.
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

        private Button basicAttackButton;
        private Button slot0Button;

        public event System.Action BasicAttackClicked;
        public event System.Action SkillSlot0Clicked;

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
            basicAttackButton = actionButtons != null && actionButtons.Length > 0 ? actionButtons[0] : null;
            slot0Button = actionButtons != null && actionButtons.Length > 1 ? actionButtons[1] : null;
            ConfigureBasicAttackButton();
            ConfigureSlot0Button();
            DisablePlaceholderButtons();
        }

        public void Refresh(Unit selectedUnit)
        {
            DisablePlaceholderButtons();
            SetBasicAttackInteractable(true);
            SetSlot0Interactable(true);

            SetText(basicAttackText, selectedUnit != null ? "Basic Attack\nTarget" : "Basic Attack\nSelect Unit");
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

        private void ConfigureBasicAttackButton()
        {
            if (basicAttackButton == null)
            {
                return;
            }

            basicAttackButton.onClick.RemoveListener(HandleBasicAttackClicked);
            basicAttackButton.onClick.AddListener(HandleBasicAttackClicked);
            SetBasicAttackInteractable(true);
        }

        private void ConfigureSlot0Button()
        {
            if (slot0Button == null)
            {
                return;
            }

            slot0Button.onClick.RemoveListener(HandleSkillSlot0Clicked);
            slot0Button.onClick.AddListener(HandleSkillSlot0Clicked);
            SetSlot0Interactable(true);
        }

        private void HandleBasicAttackClicked()
        {
            if (BasicAttackClicked != null)
            {
                BasicAttackClicked();
            }
        }

        private void HandleSkillSlot0Clicked()
        {
            if (SkillSlot0Clicked != null)
            {
                SkillSlot0Clicked();
            }
        }

        private void SetBasicAttackInteractable(bool interactable)
        {
            if (basicAttackButton == null)
            {
                return;
            }

            basicAttackButton.interactable = interactable;
            if (basicAttackButton.targetGraphic != null)
            {
                basicAttackButton.targetGraphic.raycastTarget = interactable;
            }
        }

        private void SetSlot0Interactable(bool interactable)
        {
            if (slot0Button == null)
            {
                return;
            }

            slot0Button.interactable = interactable;
            if (slot0Button.targetGraphic != null)
            {
                slot0Button.targetGraphic.raycastTarget = interactable;
            }
        }

        private void DisablePlaceholderButtons()
        {
            if (actionButtons == null)
            {
                return;
            }

            for (int i = 0; i < actionButtons.Length; i++)
            {
                if (actionButtons[i] != null)
                {
                    bool isSupportedAction = i == 0 || i == 1;
                    actionButtons[i].interactable = isSupportedAction;
                    if (actionButtons[i].targetGraphic != null)
                    {
                        actionButtons[i].targetGraphic.raycastTarget = isSupportedAction;
                    }
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
