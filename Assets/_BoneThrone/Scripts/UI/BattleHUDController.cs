using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Interactables;
using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.Turns;
using BoneThrone.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// First-pass battle HUD coordinator. It reads existing gameplay state and never drives gameplay actions.
    /// </summary>
    public sealed class BattleHUDController : MonoBehaviour
    {
        [Header("Gameplay References")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private LevelProgressionService progressionService;
        [SerializeField] private InteractableStairs stairs;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private Unit[] playerUnits = new Unit[4];

        [Header("Views")]
        [SerializeField] private TurnBannerView turnBannerView;
        [SerializeField] private TMP_Text selectedUnitText;
        [SerializeField] private HeroPanelView[] heroPanels = new HeroPanelView[4];
        [SerializeField] private SkillBarView skillBarView;
        [SerializeField] private CombatFeedbackView combatFeedbackView;
        [SerializeField] private PromptView promptView;

        [Header("Runtime UI")]
        [SerializeField] private bool buildRuntimeLayoutIfMissing = true;

        private void Awake()
        {
            NormalizeRectTransform();

            if (buildRuntimeLayoutIfMissing)
            {
                EnsureRuntimeLayout();
            }
        }

        private void OnEnable()
        {
            SubscribeCombatLog();
        }

        private void Start()
        {
            if (combatFeedbackView != null)
            {
                combatFeedbackView.SeedFrom(combatLog);
            }
        }

        private void OnDisable()
        {
            if (combatLog != null)
            {
                combatLog.EntryAdded -= HandleCombatEntryAdded;
            }
        }

        private void Update()
        {
            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;

            if (turnBannerView != null)
            {
                turnBannerView.Refresh(turnManager);
            }

            RefreshSelectedUnit(selectedUnit);
            RefreshHeroPanels();

            if (skillBarView != null)
            {
                skillBarView.Refresh(selectedUnit);
            }

            if (promptView != null)
            {
                promptView.Refresh(selectedUnit, progressionService, stairs);
            }
        }

        private void SubscribeCombatLog()
        {
            if (combatLog == null)
            {
                if (combatFeedbackView != null)
                {
                    combatFeedbackView.ShowUnbound();
                }

                return;
            }

            combatLog.EntryAdded -= HandleCombatEntryAdded;
            combatLog.EntryAdded += HandleCombatEntryAdded;
        }

        private void HandleCombatEntryAdded(CombatLog.Entry entry)
        {
            if (combatFeedbackView != null)
            {
                combatFeedbackView.AddEntry(entry);
            }
        }

        private void RefreshSelectedUnit(Unit selectedUnit)
        {
            if (selectedUnitText == null)
            {
                return;
            }

            if (selectedUnit == null)
            {
                selectedUnitText.text = "Selected Unit: None";
                return;
            }

            string displayName = string.IsNullOrEmpty(selectedUnit.DisplayName)
                ? selectedUnit.RoleId.ToString()
                : selectedUnit.DisplayName;
            selectedUnitText.text = "Selected Unit: " + displayName + " (#" + selectedUnit.UnitId + ")";
        }

        private void RefreshHeroPanels()
        {
            if (heroPanels == null)
            {
                return;
            }

            for (int i = 0; i < heroPanels.Length; i++)
            {
                if (heroPanels[i] == null)
                {
                    continue;
                }

                Unit unit = GetPlayerUnit(i);
                heroPanels[i].Refresh(unit);
            }
        }

        private Unit GetPlayerUnit(int index)
        {
            if (playerUnits != null && index >= 0 && index < playerUnits.Length && playerUnits[index] != null)
            {
                return playerUnits[index];
            }

            Unit[] progressionUnits = progressionService != null ? progressionService.PlayerUnits : null;
            if (progressionUnits != null && index >= 0 && index < progressionUnits.Length)
            {
                return progressionUnits[index];
            }

            return null;
        }

        private void NormalizeRectTransform()
        {
            RectTransform root = GetComponent<RectTransform>();
            if (root == null)
            {
                return;
            }

            root.localScale = Vector3.one;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }

        private void EnsureRuntimeLayout()
        {
            if (turnBannerView != null
                && selectedUnitText != null
                && skillBarView != null
                && combatFeedbackView != null
                && promptView != null
                && HasHeroPanels())
            {
                return;
            }

            RectTransform root = GetComponent<RectTransform>();
            if (root == null)
            {
                root = gameObject.AddComponent<RectTransform>();
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            FontStyles headerStyle = FontStyles.Bold;
            turnBannerView = CreateView<TurnBannerView>("TurnBanner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 52f), new Vector2(0f, -22f));
            turnBannerView.Bind(CreateText(turnBannerView.transform, "TurnText", "Turn: Unbound", 28, headerStyle));

            selectedUnitText = CreateAnchoredText("SelectedUnitText", "Selected Unit: None", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(460f, 42f), new Vector2(18f, -18f), 22, FontStyles.Normal);

            promptView = CreateView<PromptView>("Prompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(920f, 42f), new Vector2(0f, 20f));
            promptView.Bind(CreateText(promptView.transform, "PromptText", "Select a player unit.", 21, FontStyles.Normal));

            heroPanels = new HeroPanelView[4];
            for (int i = 0; i < heroPanels.Length; i++)
            {
                HeroPanelView panel = CreateView<HeroPanelView>("HeroPanel_" + i, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(310f, 132f), new Vector2(18f, 230f - i * 142f));
                panel.Bind(CreateText(panel.transform, "HeroText", "Hero: Unbound", 18, FontStyles.Normal));
                heroPanels[i] = panel;
            }

            skillBarView = CreateSkillBar();

            combatFeedbackView = CreateView<CombatFeedbackView>("CombatFeedback", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(520f, 210f), new Vector2(-18f, 0f));
            combatFeedbackView.Bind(CreateText(combatFeedbackView.transform, "CombatLogText", "Combat Log: No combat yet", 18, FontStyles.Normal));
        }

        private SkillBarView CreateSkillBar()
        {
            SkillBarView bar = CreateView<SkillBarView>("SkillBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(920f, 88f), new Vector2(0f, 76f));
            HorizontalLayoutGroup layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            List<Button> buttons = new List<Button>();
            TMP_Text basic = CreateActionButton(bar.transform, "BasicAttack", "Basic Attack", buttons);
            TMP_Text slot0 = CreateActionButton(bar.transform, "SkillSlot0", "Skill 0", buttons);
            TMP_Text slot1 = CreateActionButton(bar.transform, "SkillSlot1", "Slot 1", buttons);
            TMP_Text slot2 = CreateActionButton(bar.transform, "SkillSlot2", "Slot 2", buttons);
            TMP_Text defend = CreateActionButton(bar.transform, "Defend", "Defend", buttons);
            TMP_Text potion = CreateActionButton(bar.transform, "Potion", "Potion", buttons);
            bar.Bind(basic, slot0, slot1, slot2, defend, potion, buttons.ToArray());
            return bar;
        }

        private TMP_Text CreateActionButton(Transform parent, string name, string text, List<Button> buttons)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.72f);
            image.raycastTarget = false;
            Button button = buttonObject.AddComponent<Button>();
            button.interactable = false;
            buttons.Add(button);
            TMP_Text label = CreateText(buttonObject.transform, name + "Text", text, 16, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private T CreateView<T>(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
            where T : Component
        {
            GameObject viewObject = new GameObject(name, typeof(RectTransform));
            viewObject.transform.SetParent(transform, false);
            RectTransform rect = viewObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image background = viewObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.48f);
            background.raycastTarget = false;
            return viewObject.AddComponent<T>();
        }

        private TMP_Text CreateAnchoredText(string name, string value, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position, int fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            ConfigureText(text, value, fontSize, style);
            return text;
        }

        private TMP_Text CreateText(Transform parent, string name, string value, int fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 6f);
            rect.offsetMax = new Vector2(-8f, -6f);
            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            ConfigureText(text, value, fontSize, style);
            return text;
        }

        private static void ConfigureText(TMP_Text text, string value, int fontSize, FontStyles style)
        {
            text.text = value;
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.extraPadding = true;
        }

        private bool HasHeroPanels()
        {
            if (heroPanels == null || heroPanels.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < heroPanels.Length; i++)
            {
                if (heroPanels[i] == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
