using BoneThrone.Skills;
using BoneThrone.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays the first prototype action bar. Move, Basic Attack, and skill slots emit UI intent events.
    /// </summary>
    public sealed class SkillBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text moveText;
        [SerializeField] private TMP_Text basicAttackText;
        [SerializeField] private TMP_Text slot0Text;
        [SerializeField] private TMP_Text slot1Text;
        [SerializeField] private TMP_Text slot2Text;
        [SerializeField] private TMP_Text defendText;
        [SerializeField] private TMP_Text potionText;
        [SerializeField] private Button[] actionButtons;

        private Button moveButton;
        private Button basicAttackButton;
        private Button slot0Button;
        private Button slot1Button;
        private Button slot2Button;

        public event System.Action MoveClicked;
        public event System.Action BasicAttackClicked;
        public event System.Action SkillSlot0Clicked;
        public event System.Action SkillSlot1Clicked;
        public event System.Action SkillSlot2Clicked;

        public void Bind(
            TMP_Text move,
            TMP_Text basicAttack,
            TMP_Text slot0,
            TMP_Text slot1,
            TMP_Text slot2,
            TMP_Text defend,
            TMP_Text potion,
            Button[] buttons)
        {
            moveText = move;
            basicAttackText = basicAttack;
            slot0Text = slot0;
            slot1Text = slot1;
            slot2Text = slot2;
            defendText = defend;
            potionText = potion;
            actionButtons = buttons;
            moveButton = actionButtons != null && actionButtons.Length > 0 ? actionButtons[0] : null;
            basicAttackButton = actionButtons != null && actionButtons.Length > 1 ? actionButtons[1] : null;
            slot0Button = actionButtons != null && actionButtons.Length > 2 ? actionButtons[2] : null;
            slot1Button = actionButtons != null && actionButtons.Length > 3 ? actionButtons[3] : null;
            slot2Button = actionButtons != null && actionButtons.Length > 4 ? actionButtons[4] : null;
            ConfigureMoveButton();
            ConfigureBasicAttackButton();
            ConfigureSkillSlotButtons();
            DisablePlaceholderButtons();
        }

        public void Refresh(Unit selectedUnit)
        {
            DisablePlaceholderButtons();
            SetMoveInteractable(true);
            SetBasicAttackInteractable(true);
            SetSkillSlotInteractable(0, true);
            SetSkillSlotInteractable(1, true);
            SetSkillSlotInteractable(2, true);

            SetText(moveText, selectedUnit != null ? "Move\nTarget" : "Move\nSelect Unit");
            SetText(basicAttackText, selectedUnit != null ? "Basic Attack\nTarget" : "Basic Attack\nSelect Unit");
            RefreshSkillSlot(selectedUnit, 0, slot0Text);
            RefreshSkillSlot(selectedUnit, 1, slot1Text);
            RefreshSkillSlot(selectedUnit, 2, slot2Text);
            SetText(defendText, "Defend\nPlaceholder");
            SetText(potionText, "Potion\nPlaceholder");
        }

        private void RefreshSkillSlot(Unit selectedUnit, int slotIndex, TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                text.text = "Skill " + slotIndex + "\nNo Unit";
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null || !runtime.HasSkill(slotIndex))
            {
                text.text = "Skill " + slotIndex + "\nEmpty";
                return;
            }

            SkillData skill = runtime.GetSkill(slotIndex);
            bool unlocked = runtime.IsUnlocked(selectedUnit, slotIndex);
            int cooldown = runtime.GetCooldown(slotIndex);
            string state = !unlocked ? "Locked" : cooldown > 0 ? "Cooldown " + cooldown : "Ready";
            text.text = skill.DisplayName + "\n" + state;
        }

        private void ConfigureMoveButton()
        {
            if (moveButton == null)
            {
                return;
            }

            moveButton.onClick.RemoveListener(HandleMoveClicked);
            moveButton.onClick.AddListener(HandleMoveClicked);
            SetMoveInteractable(true);
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

        private void ConfigureSkillSlotButtons()
        {
            ConfigureSkillSlotButton(slot0Button, HandleSkillSlot0Clicked);
            ConfigureSkillSlotButton(slot1Button, HandleSkillSlot1Clicked);
            ConfigureSkillSlotButton(slot2Button, HandleSkillSlot2Clicked);
            SetSkillSlotInteractable(0, true);
            SetSkillSlotInteractable(1, true);
            SetSkillSlotInteractable(2, true);
        }

        private void ConfigureSkillSlotButton(Button button, UnityEngine.Events.UnityAction handler)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(handler);
            button.onClick.AddListener(handler);
        }

        private void HandleMoveClicked()
        {
            if (MoveClicked != null)
            {
                MoveClicked();
            }
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

        private void HandleSkillSlot1Clicked()
        {
            if (SkillSlot1Clicked != null)
            {
                SkillSlot1Clicked();
            }
        }

        private void HandleSkillSlot2Clicked()
        {
            if (SkillSlot2Clicked != null)
            {
                SkillSlot2Clicked();
            }
        }

        private void SetMoveInteractable(bool interactable)
        {
            if (moveButton == null)
            {
                return;
            }

            moveButton.interactable = interactable;
            if (moveButton.targetGraphic != null)
            {
                moveButton.targetGraphic.raycastTarget = interactable;
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

        private void SetSkillSlotInteractable(int slotIndex, bool interactable)
        {
            Button button = GetSkillSlotButton(slotIndex);
            if (button == null)
            {
                return;
            }

            button.interactable = interactable;
            if (button.targetGraphic != null)
            {
                button.targetGraphic.raycastTarget = interactable;
            }
        }

        private Button GetSkillSlotButton(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                    return slot0Button;
                case 1:
                    return slot1Button;
                case 2:
                    return slot2Button;
                default:
                    return null;
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
                    bool isSupportedAction = i >= 0 && i <= 4;
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
