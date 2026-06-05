using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Interactables;
using BoneThrone.Items;
using BoneThrone.Levels;
using BoneThrone.Movement;
using BoneThrone.Skills;
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
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private DefendSystem defendSystem;
        [SerializeField] private PotionSystem potionSystem;
        [SerializeField] private Unit[] playerUnits = new Unit[4];
        [SerializeField] private Unit[] enemyUnits;
        [SerializeField] private ActiveUnitProvider activeUnitProvider;

        [Header("Action Mode References")]
        [SerializeField] private UIActionModeController actionModeController;
        [SerializeField] private Camera actionInputCamera;
        [SerializeField] private LayerMask actionTargetLayerMask = ~0;
        [SerializeField] private PlayerMovementController movementControllerToSuspend;
        [SerializeField] private MovementDebugHighlighter movementHighlighter;

        [Header("Views")]
        [SerializeField] private TurnBannerView turnBannerView;
        [SerializeField] private HeroPanelView[] heroPanels = new HeroPanelView[4];
        [SerializeField] private SkillBarView skillBarView;
        [SerializeField] private CombatFeedbackView combatFeedbackView;
        [SerializeField] private PromptView promptView;
        [SerializeField] private TurnTransitionPopupView turnTransitionPopupView;
        [SerializeField] private TurnTransitionPopupView turnTransitionPopupPrefab;

        [Header("Runtime UI")]
        [SerializeField] private bool buildRuntimeLayoutIfMissing = true;

        private void Awake()
        {
            NormalizeRectTransform();

            RebindMissingViews();

            if (buildRuntimeLayoutIfMissing || !HasRequiredHudViews())
            {
                EnsureRuntimeLayout();
            }

            ForceVisibleRuntimeViews();
            EnsureEndTurnButton();
            EnsureTurnTransitionPopupView();
            EnsureActiveUnitProvider();
            EnsureDefendSystem();
            EnsurePotionSystem();
            EnsureActionModeController();
            ConfigureActionModeController();
        }

        private void OnEnable()
        {
            SubscribeCombatLog();
            SubscribeSkillBar();
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

            UnsubscribeSkillBar();
        }

        private void Update()
        {
            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;

            if (turnBannerView != null)
            {
                turnBannerView.Refresh(turnManager, selectedUnit);
            }

            RefreshHeroPanels(selectedUnit);

            if (skillBarView != null)
            {
                bool isPlayerTurn = turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn;
                skillBarView.Refresh(selectedUnit, isPlayerTurn);
                skillBarView.SetEndTurnInteractable(turnManager != null && turnManager.CurrentPhase == TurnPhase.PlayerTurn);
            }

            if (promptView != null)
            {
                promptView.Refresh(selectedUnit, progressionService, stairs);
            }
        }

        private void SubscribeSkillBar()
        {
            if (skillBarView == null)
            {
                return;
            }

            skillBarView.MoveClicked -= HandleMoveClicked;
            skillBarView.MoveClicked += HandleMoveClicked;
            skillBarView.BasicAttackClicked -= HandleBasicAttackClicked;
            skillBarView.BasicAttackClicked += HandleBasicAttackClicked;
            skillBarView.SkillSlot0Clicked -= HandleSkillSlot0Clicked;
            skillBarView.SkillSlot0Clicked += HandleSkillSlot0Clicked;
            skillBarView.SkillSlot1Clicked -= HandleSkillSlot1Clicked;
            skillBarView.SkillSlot1Clicked += HandleSkillSlot1Clicked;
            skillBarView.SkillSlot2Clicked -= HandleSkillSlot2Clicked;
            skillBarView.SkillSlot2Clicked += HandleSkillSlot2Clicked;
            skillBarView.DefendClicked -= HandleDefendClicked;
            skillBarView.DefendClicked += HandleDefendClicked;
            skillBarView.PotionClicked -= HandlePotionClicked;
            skillBarView.PotionClicked += HandlePotionClicked;
            skillBarView.EndTurnClicked -= HandleEndTurnClicked;
            skillBarView.EndTurnClicked += HandleEndTurnClicked;
        }

        private void UnsubscribeSkillBar()
        {
            if (skillBarView != null)
            {
                skillBarView.MoveClicked -= HandleMoveClicked;
                skillBarView.BasicAttackClicked -= HandleBasicAttackClicked;
                skillBarView.SkillSlot0Clicked -= HandleSkillSlot0Clicked;
                skillBarView.SkillSlot1Clicked -= HandleSkillSlot1Clicked;
                skillBarView.SkillSlot2Clicked -= HandleSkillSlot2Clicked;
                skillBarView.DefendClicked -= HandleDefendClicked;
                skillBarView.PotionClicked -= HandlePotionClicked;
                skillBarView.EndTurnClicked -= HandleEndTurnClicked;
            }
        }

        private void HandleMoveClicked()
        {
            if (actionModeController == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Move unavailable: action mode unbound.", 1.5f);
                }

                return;
            }

            actionModeController.HandleMoveButtonClicked();
        }

        private void HandleBasicAttackClicked()
        {
            if (actionModeController == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Basic attack unavailable: action mode unbound.", 1.5f);
                }

                return;
            }

            actionModeController.HandleBasicAttackButtonClicked();
        }

        private void HandleSkillSlot0Clicked()
        {
            HandleSkillSlotClicked(0);
        }

        private void HandleSkillSlot1Clicked()
        {
            HandleSkillSlotClicked(1);
        }

        private void HandleSkillSlot2Clicked()
        {
            HandleSkillSlotClicked(2);
        }

        private void HandleSkillSlotClicked(int slotIndex)
        {
            if (actionModeController == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Skill unavailable: action mode unbound.", 1.5f);
                }

                return;
            }

            actionModeController.HandleSkillSlotButtonClicked(slotIndex);
        }

        private void HandleDefendClicked()
        {
            Unit selectedUnit = GetSelectedUnitForSelfAction("Defend");
            if (selectedUnit == null)
            {
                return;
            }

            EnsureDefendSystem();
            if (defendSystem == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Defend unavailable: DefendSystem missing.", 1.5f);
                }

                return;
            }

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            defendSystem.TryDefend(selectedUnit);
        }

        private void HandlePotionClicked()
        {
            Unit selectedUnit = GetSelectedUnitForSelfAction("Potion");
            if (selectedUnit == null)
            {
                return;
            }

            EnsurePotionSystem();
            if (potionSystem == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Potion unavailable: PotionSystem missing.", 1.5f);
                }

                return;
            }

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            potionSystem.TryUsePotion(selectedUnit);
        }

        private void HandleEndTurnClicked()
        {
            if (turnManager == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("End turn unavailable: TurnManager unbound.", 1.5f);
                }

                return;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("End turn unavailable during EnemyTurn.", 1.5f);
                }

                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Select a player unit first.", 1.5f);
                }

                return;
            }

            if (selectedUnit.Faction != UnitFaction.Player)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit is not a player unit.", 1.5f);
                }

                return;
            }

            if (!selectedUnit.IsAlive)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit is dead.", 1.5f);
                }

                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit has no turn state.", 1.5f);
                }

                return;
            }

            if (turnState.HasEnded)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit has already ended.", 1.5f);
                }

                return;
            }

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            bool ended = turnManager.TryEndPlayerUnitTurn(selectedUnit);
            if (ended)
            {
                if (selectionManager != null)
                {
                    selectionManager.ClearSelection();
                }

                if (movementHighlighter != null)
                {
                    movementHighlighter.Clear();
                }
            }
        }

        private void EnsureActionModeController()
        {
            if (actionModeController == null)
            {
                actionModeController = GetComponent<UIActionModeController>();
            }

            if (actionModeController == null)
            {
                actionModeController = gameObject.AddComponent<UIActionModeController>();
            }
        }

        private void EnsureDefendSystem()
        {
            if (defendSystem == null)
            {
                defendSystem = Object.FindFirstObjectByType<DefendSystem>();
            }

            if (defendSystem == null)
            {
                defendSystem = gameObject.AddComponent<DefendSystem>();
            }
        }

        private void EnsurePotionSystem()
        {
            if (potionSystem == null)
            {
                potionSystem = Object.FindFirstObjectByType<PotionSystem>();
            }

            if (potionSystem == null)
            {
                potionSystem = gameObject.AddComponent<PotionSystem>();
            }
        }

        private void EnsureActiveUnitProvider()
        {
            if (activeUnitProvider != null)
            {
                return;
            }

            activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            if (activeUnitProvider == null)
            {
                activeUnitProvider = gameObject.AddComponent<ActiveUnitProvider>();
            }
        }

        private void ConfigureActionModeController()
        {
            if (actionModeController == null)
            {
                return;
            }

            actionModeController.Configure(
                selectionManager,
                gridManager,
                combatSystem,
                skillSystem,
                enemyUnits,
                promptView,
                actionInputCamera,
                actionTargetLayerMask,
                movementControllerToSuspend,
                movementHighlighter,
                activeUnitProvider);
        }

        private void EnsureEndTurnButton()
        {
            if (skillBarView != null)
            {
                skillBarView.EnsureEndTurnButton();
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

        private Unit GetSelectedUnitForSelfAction(string actionName)
        {
            if (turnManager == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride(actionName + " unavailable: TurnManager unbound.", 1.5f);
                }

                return null;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride(actionName + " unavailable during EnemyTurn.", 1.5f);
                }

                return null;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Select a player unit first.", 1.5f);
                }

                return null;
            }

            if (selectedUnit.Faction != UnitFaction.Player)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit is not a player unit.", 1.5f);
                }

                return null;
            }

            if (!selectedUnit.IsAlive)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("Selected unit is dead.", 1.5f);
                }

                return null;
            }

            return selectedUnit;
        }

        private void RefreshHeroPanels(Unit selectedUnit)
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
                heroPanels[i].Refresh(unit, selectedUnit);
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

        private void RebindMissingViews()
        {
            if (turnBannerView == null)
            {
                turnBannerView = GetComponentInChildren<TurnBannerView>(true);
            }

            if (skillBarView == null)
            {
                skillBarView = GetComponentInChildren<SkillBarView>(true);
            }

            if (combatFeedbackView == null)
            {
                combatFeedbackView = GetComponentInChildren<CombatFeedbackView>(true);
            }

            if (promptView == null)
            {
                promptView = GetComponentInChildren<PromptView>(true);
            }

            if (turnTransitionPopupView == null)
            {
                turnTransitionPopupView = GetComponentInChildren<TurnTransitionPopupView>(true);
            }

            if (!HasHeroPanels())
            {
                HeroPanelView[] discoveredPanels = GetComponentsInChildren<HeroPanelView>(true);
                if (discoveredPanels != null && discoveredPanels.Length > 0)
                {
                    heroPanels = discoveredPanels;
                }
            }
        }

        private bool HasRequiredHudViews()
        {
            return turnBannerView != null
                && skillBarView != null
                && combatFeedbackView != null
                && promptView != null
                && HasHeroPanels();
        }

        private void ForceVisibleRuntimeViews()
        {
            ForceVisible(gameObject);
            ForceVisible(turnBannerView != null ? turnBannerView.gameObject : null);
            ForceVisible(skillBarView != null ? skillBarView.gameObject : null);
            ForceVisible(combatFeedbackView != null ? combatFeedbackView.gameObject : null);
            ForceVisible(promptView != null ? promptView.gameObject : null);
            ForceVisible(turnTransitionPopupView != null ? turnTransitionPopupView.gameObject : null);

            if (heroPanels == null)
            {
                return;
            }

            for (int i = 0; i < heroPanels.Length; i++)
            {
                if (heroPanels[i] != null)
                {
                    ForceVisible(heroPanels[i].gameObject);
                }
            }
        }

        private static void ForceVisible(GameObject viewObject)
        {
            if (viewObject == null)
            {
                return;
            }

            if (!viewObject.activeSelf)
            {
                viewObject.SetActive(true);
            }

            RectTransform rect = viewObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.localScale = Vector3.one;
        }

        private void EnsureTurnTransitionPopupView()
        {
            if (turnTransitionPopupView == null)
            {
                turnTransitionPopupView = GetComponentInChildren<TurnTransitionPopupView>(true);
            }

            if (turnTransitionPopupView == null && turnTransitionPopupPrefab != null)
            {
                turnTransitionPopupView = Instantiate(turnTransitionPopupPrefab, transform);
                RectTransform popupRect = turnTransitionPopupView.GetComponent<RectTransform>();
                if (popupRect != null)
                {
                    popupRect.localScale = Vector3.one;
                }

                turnTransitionPopupView.transform.SetAsLastSibling();
            }

            if (turnTransitionPopupView == null && buildRuntimeLayoutIfMissing)
            {
                turnTransitionPopupView = CreateTurnTransitionPopup();
            }
        }

        private void EnsureRuntimeLayout()
        {
            if (turnBannerView != null
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

            turnTransitionPopupView = CreateTurnTransitionPopup();
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

            combatFeedbackView = CreateView<CombatFeedbackView>("CombatFeedback", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(580f, 380f), new Vector2(-18f, 0f));
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
            TMP_Text move = CreateActionButton(bar.transform, "Move", "Move", buttons);
            TMP_Text basic = CreateActionButton(bar.transform, "BasicAttack", "Basic Attack", buttons);
            TMP_Text slot0 = CreateActionButton(bar.transform, "SkillSlot0", "Skill 0", buttons);
            TMP_Text slot1 = CreateActionButton(bar.transform, "SkillSlot1", "Slot 1", buttons);
            TMP_Text slot2 = CreateActionButton(bar.transform, "SkillSlot2", "Slot 2", buttons);
            TMP_Text defend = CreateActionButton(bar.transform, "Defend", "Defend", buttons);
            TMP_Text potion = CreateActionButton(bar.transform, "Potion", "Potion", buttons);
            TMP_Text endTurn = CreateActionButton(bar.transform, "EndTurn", "End\nTurn", buttons);
            bar.Bind(move, basic, slot0, slot1, slot2, defend, potion, endTurn, buttons.ToArray());
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

        private TurnTransitionPopupView CreateTurnTransitionPopup()
        {
            TurnTransitionPopupView popup = CreateView<TurnTransitionPopupView>(
                "TurnTransitionPopup",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(640f, 96f),
                new Vector2(0f, 120f));

            CanvasGroup canvasGroup = popup.gameObject.AddComponent<CanvasGroup>();
            TMP_Text text = CreateText(popup.transform, "PopupText", "敌方回合", 34, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            popup.Bind(text, canvasGroup, popup.GetComponent<RectTransform>());
            popup.HideImmediate();
            return popup;
        }

        private static void ConfigureText(TMP_Text text, string value, int fontSize, FontStyles style)
        {
            text.text = value;
            text.fontSize = fontSize;
            text.enableAutoSizing = false;
            text.richText = true;
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
