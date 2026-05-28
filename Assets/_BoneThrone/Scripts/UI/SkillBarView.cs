using BoneThrone.Items;
using BoneThrone.Skills;
using BoneThrone.Turns;
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
        [SerializeField] private TMP_Text endTurnText;
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Color enabledTextColor = Color.white;
        [SerializeField] private Color disabledTextColor = Color.gray;
        [SerializeField] private Color cooldownTextColor = new Color(0.65f, 0.75f, 1f, 1f);

        private Button moveButton;
        private Button basicAttackButton;
        private Button slot0Button;
        private Button slot1Button;
        private Button slot2Button;
        private Button defendButton;
        private Button potionButton;
        private Button endTurnButton;

        public event System.Action MoveClicked;
        public event System.Action BasicAttackClicked;
        public event System.Action SkillSlot0Clicked;
        public event System.Action SkillSlot1Clicked;
        public event System.Action SkillSlot2Clicked;
        public event System.Action DefendClicked;
        public event System.Action PotionClicked;
        public event System.Action EndTurnClicked;

        public void Bind(
            TMP_Text move,
            TMP_Text basicAttack,
            TMP_Text slot0,
            TMP_Text slot1,
            TMP_Text slot2,
            TMP_Text defend,
            TMP_Text potion,
            TMP_Text endTurn,
            Button[] buttons)
        {
            moveText = move;
            basicAttackText = basicAttack;
            slot0Text = slot0;
            slot1Text = slot1;
            slot2Text = slot2;
            defendText = defend;
            potionText = potion;
            endTurnText = endTurn;
            actionButtons = buttons;
            moveButton = actionButtons != null && actionButtons.Length > 0 ? actionButtons[0] : null;
            basicAttackButton = actionButtons != null && actionButtons.Length > 1 ? actionButtons[1] : null;
            slot0Button = actionButtons != null && actionButtons.Length > 2 ? actionButtons[2] : null;
            slot1Button = actionButtons != null && actionButtons.Length > 3 ? actionButtons[3] : null;
            slot2Button = actionButtons != null && actionButtons.Length > 4 ? actionButtons[4] : null;
            defendButton = actionButtons != null && actionButtons.Length > 5 ? actionButtons[5] : null;
            potionButton = actionButtons != null && actionButtons.Length > 6 ? actionButtons[6] : null;
            endTurnButton = actionButtons != null && actionButtons.Length > 7 ? actionButtons[7] : null;
            ConfigureMoveButton();
            ConfigureBasicAttackButton();
            ConfigureSkillSlotButtons();
            ConfigureDefendButton();
            ConfigurePotionButton();
            ConfigureEndTurnButton();
            DisablePlaceholderButtons();
        }

        public void Refresh(Unit selectedUnit)
        {
            Refresh(selectedUnit, true);
        }

        public void Refresh(Unit selectedUnit, bool isPlayerTurn)
        {
            DisablePlaceholderButtons();
            SetMoveInteractable(isPlayerTurn && selectedUnit != null);
            SetBasicAttackInteractable(isPlayerTurn && CanSelectedUnitAct(selectedUnit));

            SetText(moveText, selectedUnit != null ? "Move\nTarget" : "Move\nSelect Unit");
            SetText(basicAttackText, selectedUnit != null ? "Basic Attack\nTarget" : "Basic Attack\nSelect Unit");
            RefreshSkillSlot(selectedUnit, 0, slot0Text, isPlayerTurn);
            RefreshSkillSlot(selectedUnit, 1, slot1Text, isPlayerTurn);
            RefreshSkillSlot(selectedUnit, 2, slot2Text, isPlayerTurn);
            RefreshDefend(selectedUnit, isPlayerTurn);
            RefreshPotion(selectedUnit, isPlayerTurn);
            SetText(endTurnText, "End\nTurn");
        }

        public void EnsureEndTurnButton()
        {
            if (endTurnButton != null)
            {
                ConfigureEndTurnButton();
                return;
            }

            if (actionButtons != null && actionButtons.Length > 7 && actionButtons[7] != null)
            {
                endTurnButton = actionButtons[7];
                ConfigureEndTurnButton();
                return;
            }

            Button createdButton = CreateRuntimeEndTurnButton();
            if (createdButton == null)
            {
                return;
            }

            Button[] expandedButtons = new Button[8];
            if (actionButtons != null)
            {
                for (int i = 0; i < actionButtons.Length && i < expandedButtons.Length; i++)
                {
                    expandedButtons[i] = actionButtons[i];
                }
            }

            expandedButtons[7] = createdButton;
            actionButtons = expandedButtons;
            endTurnButton = createdButton;
            ConfigureEndTurnButton();
            DisablePlaceholderButtons();
        }

        public void SetEndTurnInteractable(bool interactable)
        {
            if (endTurnButton == null)
            {
                EnsureEndTurnButton();
            }

            if (endTurnButton == null)
            {
                return;
            }

            endTurnButton.interactable = interactable;
            if (endTurnButton.targetGraphic != null)
            {
                endTurnButton.targetGraphic.raycastTarget = interactable;
            }
        }

        private void RefreshSkillSlot(Unit selectedUnit, int slotIndex, TMP_Text text, bool isPlayerTurn)
        {
            if (text == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                text.text = "Skill " + slotIndex + "\nNo Unit";
                SetSkillSlotVisual(slotIndex, text, false, disabledTextColor);
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null || !runtime.HasSkill(slotIndex))
            {
                text.text = "Skill " + slotIndex + "\nEmpty";
                SetSkillSlotVisual(slotIndex, text, false, disabledTextColor);
                return;
            }

            SkillData skill = runtime.GetSkill(slotIndex);
            bool unlocked = runtime.IsUnlocked(selectedUnit, slotIndex);
            int cooldown = runtime.GetCooldown(slotIndex);
            string state = !unlocked ? "Locked" : cooldown > 0 ? "Cooldown " + cooldown : "Ready";
            text.text = skill.DisplayName + "\n" + state;
            bool canUse = isPlayerTurn && unlocked && cooldown <= 0 && CanSelectedUnitAct(selectedUnit);
            Color textColor = canUse ? enabledTextColor : cooldown > 0 ? cooldownTextColor : disabledTextColor;
            SetSkillSlotVisual(slotIndex, text, canUse, textColor);
        }

        private void RefreshDefend(Unit selectedUnit, bool isPlayerTurn)
        {
            bool canAct = isPlayerTurn && CanSelectedUnitAct(selectedUnit);
            string state = selectedUnit == null ? "Unavailable" : canAct ? "Ready" : "Used";
            SetText(defendText, "Defend\n" + state);
            SetTextColor(defendText, canAct ? enabledTextColor : disabledTextColor);
            SetDefendInteractable(canAct);
        }

        private void RefreshPotion(Unit selectedUnit, bool isPlayerTurn)
        {
            bool canAct = isPlayerTurn && CanSelectedUnitAct(selectedUnit);
            int potionCount = GetPotionCount(selectedUnit);
            bool fullHp = IsFullHp(selectedUnit);
            bool canUse = canAct && potionCount > 0 && !fullHp;
            string state = selectedUnit == null
                ? "Unavailable"
                : potionCount <= 0 ? "Empty" : fullHp ? "Full HP" : "x" + potionCount;
            SetText(potionText, "Potion\n" + state);
            SetTextColor(potionText, canUse ? enabledTextColor : disabledTextColor);
            SetPotionInteractable(canUse);
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

        private void ConfigureDefendButton()
        {
            if (defendButton == null)
            {
                return;
            }

            defendButton.onClick.RemoveListener(HandleDefendClicked);
            defendButton.onClick.AddListener(HandleDefendClicked);
        }

        private void ConfigurePotionButton()
        {
            if (potionButton == null)
            {
                return;
            }

            potionButton.onClick.RemoveListener(HandlePotionClicked);
            potionButton.onClick.AddListener(HandlePotionClicked);
        }

        private void ConfigureEndTurnButton()
        {
            if (endTurnButton == null)
            {
                return;
            }

            endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
            endTurnButton.onClick.AddListener(HandleEndTurnClicked);
            SetEndTurnInteractable(true);
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

        private void HandleDefendClicked()
        {
            if (DefendClicked != null)
            {
                DefendClicked();
            }
        }

        private void HandlePotionClicked()
        {
            if (PotionClicked != null)
            {
                PotionClicked();
            }
        }

        private void HandleEndTurnClicked()
        {
            if (EndTurnClicked != null)
            {
                EndTurnClicked();
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

        private void SetDefendInteractable(bool interactable)
        {
            SetButtonInteractable(defendButton, interactable);
        }

        private void SetPotionInteractable(bool interactable)
        {
            SetButtonInteractable(potionButton, interactable);
        }

        private void SetSkillSlotVisual(int slotIndex, TMP_Text text, bool interactable, Color color)
        {
            SetSkillSlotInteractable(slotIndex, interactable);
            SetTextColor(text, color);
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
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
                    bool isSupportedAction = (i >= 0 && i <= 7);
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

        private static void SetTextColor(TMP_Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }

        private static bool CanSelectedUnitAct(Unit selectedUnit)
        {
            if (selectedUnit == null || !selectedUnit.IsAlive)
            {
                return false;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            return turnState != null && !turnState.HasActed && !turnState.HasEnded;
        }

        private static int GetPotionCount(Unit selectedUnit)
        {
            if (selectedUnit == null)
            {
                return 0;
            }

            UnitPotionState potionState = selectedUnit.GetComponent<UnitPotionState>();
            return potionState != null ? potionState.CurrentPotionCount : 1;
        }

        private static bool IsFullHp(Unit selectedUnit)
        {
            if (selectedUnit == null || selectedUnit.RuntimeState == null || selectedUnit.Stats == null)
            {
                return true;
            }

            return selectedUnit.RuntimeState.CurrentHp >= selectedUnit.Stats.GetClampedMaxHp();
        }

        private Button CreateRuntimeEndTurnButton()
        {
            GameObject buttonObject = new GameObject("EndTurn", typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.72f);
            image.raycastTarget = false;
            Button button = buttonObject.AddComponent<Button>();
            button.interactable = false;

            GameObject textObject = new GameObject("EndTurnText", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 6f);
            rect.offsetMax = new Vector2(-8f, -6f);
            TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = "End\nTurn";
            label.fontSize = 16;
            label.enableAutoSizing = false;
            label.richText = true;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.extraPadding = true;
            endTurnText = label;
            return button;
        }
    }
}
