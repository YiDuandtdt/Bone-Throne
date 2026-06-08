using BoneThrone.Audio;
using BoneThrone.Core;
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
    /// Displays the battle action bar and emits UI intent events for movement, attacks, skills, defense, items, and ending the turn.
    /// </summary>
    public sealed class SkillBarView : MonoBehaviour
    {
        [System.Serializable]
        private sealed class RoleSkillIconSet
        {
            [SerializeField] private RoleId roleId = RoleId.None;
            [SerializeField] private Sprite slot0Sprite;
            [SerializeField] private Sprite slot1Sprite;
            [SerializeField] private Sprite slot2Sprite;

            public RoleId RoleId
            {
                get { return roleId; }
            }

            public Sprite GetSprite(int slotIndex)
            {
                switch (slotIndex)
                {
                    case 0:
                        return slot0Sprite;
                    case 1:
                        return slot1Sprite;
                    case 2:
                        return slot2Sprite;
                    default:
                        return null;
                }
            }
        }

        [SerializeField] private TMP_Text moveText;
        [SerializeField] private TMP_Text basicAttackText;
        [SerializeField] private TMP_Text slot0Text;
        [SerializeField] private TMP_Text slot1Text;
        [SerializeField] private TMP_Text slot2Text;
        [SerializeField] private TMP_Text defendText;
        [SerializeField] private TMP_Text potionText;
        [SerializeField] private TMP_Text endTurnText;
        [SerializeField] private Button[] actionButtons;

        [Header("Replaceable Icon Sprites")]
        [SerializeField] private bool hideIconText = true;
        [SerializeField] private RoleId previewRoleWhenNoSelection = RoleId.Fighter;
        [SerializeField] private Sprite moveSprite;
        [SerializeField] private Sprite basicAttackSprite;
        [SerializeField] private Sprite defendSprite;
        [SerializeField] private Sprite potionSprite;
        [SerializeField] private Sprite endTurnSprite;
        [SerializeField] private RoleSkillIconSet[] roleSkillIconSets;

        [Header("Icon State Colors")]
        [SerializeField] private Color enabledIconColor = Color.white;
        [SerializeField] private Color disabledIconColor = new Color(1f, 1f, 1f, 0.42f);
        [SerializeField] private Color cooldownIconColor = new Color(0.65f, 0.75f, 1f, 0.72f);

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
        private readonly bool[] invalidSkillSlotClickEnabled = new bool[3];

        public event System.Action MoveClicked;
        public event System.Action BasicAttackClicked;
        public event System.Action SkillSlot0Clicked;
        public event System.Action SkillSlot1Clicked;
        public event System.Action SkillSlot2Clicked;
        public event System.Action DefendClicked;
        public event System.Action PotionClicked;
        public event System.Action EndTurnClicked;

        private void Awake()
        {
            InitializeButtonsFromSerializedState();
        }

        private void OnEnable()
        {
            InitializeButtonsFromSerializedState();
        }

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
            InitializeButtonsFromSerializedState();
        }

        public void Refresh(Unit selectedUnit)
        {
            Refresh(selectedUnit, true);
        }

        public void Refresh(Unit selectedUnit, bool isPlayerTurn)
        {
            DisablePlaceholderButtons();
            ApplyIconTextVisibility();
            ApplyActionIconSprites(selectedUnit);
            SetMoveInteractable(isPlayerTurn && CanSelectedUnitMove(selectedUnit));
            SetBasicAttackInteractable(isPlayerTurn && CanSelectedUnitAct(selectedUnit));

            SetText(moveText, selectedUnit != null ? "移动\n选择目标" : "移动\n选择角色");
            SetText(basicAttackText, selectedUnit != null ? "普通攻击\n选择目标" : "普通攻击\n选择角色");
            RefreshSkillSlot(selectedUnit, 0, slot0Text, isPlayerTurn);
            RefreshSkillSlot(selectedUnit, 1, slot1Text, isPlayerTurn);
            RefreshSkillSlot(selectedUnit, 2, slot2Text, isPlayerTurn);
            RefreshDefend(selectedUnit, isPlayerTurn);
            RefreshPotion(selectedUnit, isPlayerTurn);
            SetText(endTurnText, "结束\n回合");
        }

        public void EnsureEndTurnButton()
        {
            InitializeButtonsFromSerializedState();

            if (endTurnButton != null)
            {
                ConfigureEndTurnButton();
                return;
            }

            if (actionButtons != null && actionButtons.Length > 7 && actionButtons[7] != null)
            {
                endTurnButton = actionButtons[7];
                ConfigureEndTurnButton();
                DisablePlaceholderButtons();
            }
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

            ApplyButtonIconColor(endTurnButton, interactable ? enabledIconColor : disabledIconColor);
        }

        private void RefreshSkillSlot(Unit selectedUnit, int slotIndex, TMP_Text text, bool isPlayerTurn)
        {
            if (selectedUnit == null)
            {
                SetText(text, "技能" + (slotIndex + 1) + "\n未选择角色");
                SetInvalidSkillSlotClickEnabled(slotIndex, false);
                SetSkillSlotVisual(slotIndex, text, false, disabledTextColor, disabledIconColor);
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null || !runtime.HasSkill(slotIndex))
            {
                SetText(text, "技能" + (slotIndex + 1) + "\n未装备");
                SetInvalidSkillSlotClickEnabled(slotIndex, false);
                SetSkillSlotVisual(slotIndex, text, false, disabledTextColor, disabledIconColor);
                return;
            }

            SkillData skill = runtime.GetSkill(slotIndex);
            bool unlocked = runtime.IsUnlocked(selectedUnit, slotIndex);
            int cooldown = runtime.GetCooldown(slotIndex);
            string state = !unlocked ? "未解锁" : cooldown > 0 ? "冷却 " + cooldown : "可用";
            SetText(text, GetSkillDisplayName(skill) + "\n" + state);
            bool canUse = isPlayerTurn && unlocked && cooldown <= 0 && CanSelectedUnitAct(selectedUnit);
            bool canClickForInvalidFeedback = !unlocked;
            SetInvalidSkillSlotClickEnabled(slotIndex, canClickForInvalidFeedback);
            Color textColor = canUse ? enabledTextColor : cooldown > 0 ? cooldownTextColor : disabledTextColor;
            Color iconColor = canUse ? enabledIconColor : cooldown > 0 ? cooldownIconColor : disabledIconColor;
            SetSkillSlotVisual(slotIndex, text, canUse || canClickForInvalidFeedback, textColor, iconColor);
        }

        private void RefreshDefend(Unit selectedUnit, bool isPlayerTurn)
        {
            bool canAct = isPlayerTurn && CanSelectedUnitAct(selectedUnit);
            string state = selectedUnit == null ? "不可用" : canAct ? "可用" : "已使用";
            SetText(defendText, "防御\n" + state);
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
                ? "不可用"
                : potionCount <= 0 ? "已用尽" : fullHp ? "生命已满" : "剩余 " + potionCount;
            SetText(potionText, "药水\n" + state);
            SetTextColor(potionText, canUse ? enabledTextColor : disabledTextColor);
            SetPotionInteractable(canUse);
        }

        private void InitializeButtonsFromSerializedState()
        {
            CacheActionButtons();
            ApplyIconTextVisibility();
            ApplyActionIconSprites(null);
            ConfigureMoveButton();
            ConfigureBasicAttackButton();
            ConfigureSkillSlotButtons();
            ConfigureDefendButton();
            ConfigurePotionButton();
            ConfigureEndTurnButton();
            DisablePlaceholderButtons();
        }

        private void CacheActionButtons()
        {
            if (actionButtons == null || actionButtons.Length == 0)
            {
                actionButtons = GetComponentsInChildren<Button>(true);
            }

            moveButton = GetActionButton(0);
            basicAttackButton = GetActionButton(1);
            slot0Button = GetActionButton(2);
            slot1Button = GetActionButton(3);
            slot2Button = GetActionButton(4);
            defendButton = GetActionButton(5);
            potionButton = GetActionButton(6);
            endTurnButton = GetActionButton(7);
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
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            MoveClicked?.Invoke();
        }

        private void HandleBasicAttackClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            BasicAttackClicked?.Invoke();
        }

        private void HandleSkillSlot0Clicked()
        {
            PlaySkillSlotClickSfx(0);
            SkillSlot0Clicked?.Invoke();
        }

        private void HandleSkillSlot1Clicked()
        {
            PlaySkillSlotClickSfx(1);
            SkillSlot1Clicked?.Invoke();
        }

        private void HandleSkillSlot2Clicked()
        {
            PlaySkillSlotClickSfx(2);
            SkillSlot2Clicked?.Invoke();
        }

        private void HandleDefendClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            DefendClicked?.Invoke();
        }

        private void HandlePotionClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            PotionClicked?.Invoke();
        }

        private void HandleEndTurnClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            EndTurnClicked?.Invoke();
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

            ApplyButtonIconColor(moveButton, interactable ? enabledIconColor : disabledIconColor);
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

            ApplyButtonIconColor(basicAttackButton, interactable ? enabledIconColor : disabledIconColor);
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

            ApplyButtonIconColor(button, interactable ? enabledIconColor : disabledIconColor);
        }

        private void SetInvalidSkillSlotClickEnabled(int slotIndex, bool enabled)
        {
            if (slotIndex >= 0 && slotIndex < invalidSkillSlotClickEnabled.Length)
            {
                invalidSkillSlotClickEnabled[slotIndex] = enabled;
            }
        }

        private bool IsInvalidSkillSlotClickEnabled(int slotIndex)
        {
            return slotIndex >= 0
                && slotIndex < invalidSkillSlotClickEnabled.Length
                && invalidSkillSlotClickEnabled[slotIndex];
        }

        private void PlaySkillSlotClickSfx(int slotIndex)
        {
            BTAudioService.PlaySfx(IsInvalidSkillSlotClickEnabled(slotIndex)
                ? BTAudioCueId.InvalidAction
                : BTAudioCueId.ButtonClick);
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

        private Button GetActionButton(int index)
        {
            return actionButtons != null && index >= 0 && index < actionButtons.Length
                ? actionButtons[index]
                : null;
        }

        private void SetDefendInteractable(bool interactable)
        {
            SetButtonInteractable(defendButton, interactable);
        }

        private void SetPotionInteractable(bool interactable)
        {
            SetButtonInteractable(potionButton, interactable);
        }

        private void SetSkillSlotVisual(int slotIndex, TMP_Text text, bool interactable, Color textColor, Color iconColor)
        {
            SetSkillSlotInteractable(slotIndex, interactable);
            SetTextColor(text, textColor);
            ApplyButtonIconColor(GetSkillSlotButton(slotIndex), iconColor);
        }

        private void SetButtonInteractable(Button button, bool interactable)
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

            ApplyButtonIconColor(button, interactable ? enabledIconColor : disabledIconColor);
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
                    bool isSupportedAction = i >= 0 && i <= 7;
                    actionButtons[i].interactable = isSupportedAction;
                    if (actionButtons[i].targetGraphic != null)
                    {
                        actionButtons[i].targetGraphic.raycastTarget = isSupportedAction;
                    }
                }
            }
        }

        private void ApplyActionIconSprites(Unit selectedUnit)
        {
            SetButtonSprite(moveButton, moveSprite, false);
            SetButtonSprite(basicAttackButton, basicAttackSprite, false);
            SetButtonSprite(defendButton, defendSprite, false);
            SetButtonSprite(potionButton, potionSprite, false);
            SetButtonSprite(endTurnButton, endTurnSprite, false);
            SetButtonSprite(slot0Button, GetSkillSprite(selectedUnit, 0), true);
            SetButtonSprite(slot1Button, GetSkillSprite(selectedUnit, 1), true);
            SetButtonSprite(slot2Button, GetSkillSprite(selectedUnit, 2), true);
        }

        private void ApplyIconTextVisibility()
        {
            bool visible = !hideIconText;
            SetTextVisible(moveText, visible);
            SetTextVisible(basicAttackText, visible);
            SetTextVisible(slot0Text, visible);
            SetTextVisible(slot1Text, visible);
            SetTextVisible(slot2Text, visible);
            SetTextVisible(defendText, visible);
            SetTextVisible(potionText, visible);
            SetTextVisible(endTurnText, visible);
        }

        private Sprite GetSkillSprite(Unit selectedUnit, int slotIndex)
        {
            if (roleSkillIconSets == null)
            {
                return null;
            }

            RoleId roleId = selectedUnit != null ? selectedUnit.RoleId : previewRoleWhenNoSelection;
            if (roleId == RoleId.None)
            {
                return null;
            }

            for (int i = 0; i < roleSkillIconSets.Length; i++)
            {
                RoleSkillIconSet iconSet = roleSkillIconSets[i];
                if (iconSet != null && iconSet.RoleId == roleId)
                {
                    return iconSet.GetSprite(slotIndex);
                }
            }

            return null;
        }

        private static string GetSkillDisplayName(SkillData skill)
        {
            if (skill == null)
            {
                return "技能";
            }

            string key = NormalizeSkillKey(skill.DisplayName);
            switch (key)
            {
                case "fightershieldbash":
                    return "战士·盾击";
                case "fighterguardstrike":
                    return "战士·守备突刺";
                case "fightercrushingchallenge":
                    return "战士·重压嘲讽";
                case "rangerprecisionshot":
                    return "游侠·精准射击";
                case "rangerquickshot":
                    return "游侠·速射";
                case "rangerpiercingarrow":
                    return "游侠·穿透箭矢";
                case "magefireball":
                    return "法师·火球术";
                case "magefrostbolt":
                    return "法师·冰霜箭";
                case "magearcaneburst":
                    return "法师·奥术爆发";
                case "barbarianheavycleave":
                    return "野蛮人·重击斩";
                case "barbarianragestrike":
                    return "野蛮人·狂怒打击";
                case "barbarianbloodfuryslash":
                    return "野蛮人·血怒斩击";
                default:
                    return string.IsNullOrWhiteSpace(skill.DisplayName) ? "技能" : skill.DisplayName;
            }
        }

        private static string NormalizeSkillKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            char[] buffer = new char[value.Length];
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count] = char.ToLowerInvariant(c);
                    count++;
                }
            }

            return new string(buffer, 0, count);
        }

        private static void SetButtonSprite(Button button, Sprite sprite, bool clearWhenMissing)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.targetGraphic as Image;
            if (image == null)
            {
                return;
            }

            if (sprite == null)
            {
                if (clearWhenMissing)
                {
                    image.sprite = null;
                }

                return;
            }

            image.sprite = sprite;
        }

        private static void ApplyButtonIconColor(Button button, Color color)
        {
            if (button != null && button.targetGraphic != null)
            {
                button.targetGraphic.color = color;
            }
        }

        private static void SetTextVisible(TMP_Text text, bool visible)
        {
            if (text != null && text.gameObject.activeSelf != visible)
            {
                text.gameObject.SetActive(visible);
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

        private static bool CanSelectedUnitMove(Unit selectedUnit)
        {
            if (selectedUnit == null || !selectedUnit.IsAlive)
            {
                return false;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            return turnState != null
                && !turnState.HasMoved
                && !turnState.HasActed
                && !turnState.HasEnded;
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

    }
}
