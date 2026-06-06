using System.Collections;
using System.Collections.Generic;
using BoneThrone.AI;
using BoneThrone.Audio;
using BoneThrone.Combat;
using BoneThrone.Core;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// First-pass battle HUD coordinator. It reads existing gameplay state and never drives gameplay actions.
    /// </summary>
    public sealed class BattleHUDController : MonoBehaviour
    {
        private static readonly Color HealthFillColor = new Color(0.9f, 0.03f, 0.02f, 1f);
        private const float BossHealthLookupInterval = 1f;

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
        [SerializeField] private GameOutcomeService outcomeService;
        [SerializeField] private BattleOutcomeAutoEvaluator outcomeAutoEvaluator;
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

        [Header("Boss Health Bar")]
        [SerializeField] private GameObject bossHealthRoot;
        [SerializeField] private Image bossHealthFillImage;
        [SerializeField] private RectTransform bossHealthFillRect;
        [SerializeField] private TMP_Text bossHealthNameText;
        [SerializeField] private TMP_Text bossHealthValueText;
        [SerializeField] private string bossHealthNameContains = "Boss";
        [SerializeField] private bool showBossIntentDuringPlayerTurn = true;
        [SerializeField] private bool switchToBossBgmOnHealthIntro = true;
        [SerializeField] private bool lockActionsDuringBossHealthIntro = true;
        [SerializeField] [Min(0f)] private float bossHealthIntroFillDuration = 1.2f;

        [Header("Runtime UI")]
        [SerializeField] private bool buildRuntimeLayoutIfMissing = true;

        [Header("Boss Test Demo")]
        [SerializeField] private bool showBossTestDefeatButton = true;
        [SerializeField] private string bossTestSceneName = "boss_test";
        [SerializeField] [Min(0f)] private float bossTestOutcomeDelaySeconds = 1.2f;

        private Unit bossHealthUnit;
        private Unit bossHealthIntroUnit;
        private Coroutine bossHealthIntroRoutine;
        private bool bossHealthIntroInProgress;
        private bool restoreMovementControllerAfterBossIntro;
        private float nextBossHealthLookupTime;
        private float nextBossIntentLookupTime;
        private readonly List<Unit> activeBossLookupBuffer = new List<Unit>();
        private readonly List<Unit> bossIntentEnemyBuffer = new List<Unit>();
        private readonly List<Unit> bossIntentPlayerBuffer = new List<Unit>();
        private readonly List<Unit> demoDefeatPlayerBuffer = new List<Unit>();
        private readonly BossEnemyAIController bossEnemyAIController = new BossEnemyAIController();
        private Button bossTestDefeatButton;
        private Coroutine pendingBossTestDefeatRoutine;

        private void Awake()
        {
            NormalizeRectTransform();

            RebindMissingViews();

            if (buildRuntimeLayoutIfMissing || !HasRequiredHudViews())
            {
                EnsureRuntimeLayout();
            }

            EnsureBossHealthBar();
            ForceVisibleRuntimeViews();
            EnsureEndTurnButton();
            EnsureTurnTransitionPopupView();
            EnsureActiveUnitProvider();
            EnsureDefendSystem();
            EnsurePotionSystem();
            EnsureActionModeController();
            ConfigureActionModeController();
            EnsureBossTestDefeatButton();
            EnsureBossTestOutcomeEvaluator();
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
            if (bossTestDefeatButton != null)
            {
                bossTestDefeatButton.onClick.RemoveListener(HandleBossTestDefeatClicked);
            }

            if (pendingBossTestDefeatRoutine != null)
            {
                StopCoroutine(pendingBossTestDefeatRoutine);
                pendingBossTestDefeatRoutine = null;
            }
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
                bool canUseBattleActions = isPlayerTurn && (!lockActionsDuringBossHealthIntro || !bossHealthIntroInProgress);
                skillBarView.Refresh(selectedUnit, canUseBattleActions);
                skillBarView.SetEndTurnInteractable(canUseBattleActions);
            }

            if (promptView != null)
            {
                promptView.Refresh(selectedUnit, progressionService, stairs);
            }

            RefreshBossHealthBar();
            RefreshBossIntentPreview();
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

        private void HandleBossTestDefeatClicked()
        {
            if (!IsBossTestScene())
            {
                return;
            }

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            FillDemoDefeatPlayers();
            for (int i = 0; i < demoDefeatPlayerBuffer.Count; i++)
            {
                Unit player = demoDefeatPlayerBuffer[i];
                if (player != null && player.IsAlive)
                {
                    player.MarkDeadAndReleaseTile();
                }
            }

            if (movementHighlighter != null)
            {
                movementHighlighter.RefreshPlayerFootTiles();
            }

            if (promptView != null)
            {
                promptView.ShowOverride("Demo defeat.", 1.5f);
            }

            if (pendingBossTestDefeatRoutine != null)
            {
                StopCoroutine(pendingBossTestDefeatRoutine);
            }

            pendingBossTestDefeatRoutine = StartCoroutine(TriggerBossTestDefeatAfterDelay());
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

        private void EnsureBossTestDefeatButton()
        {
            Transform existing = FindChildByName(transform, "BossTestDefeatButton");
            if (existing != null)
            {
                bossTestDefeatButton = existing.GetComponent<Button>();
                existing.gameObject.SetActive(IsBossTestScene() && showBossTestDefeatButton);
            }

            if (!showBossTestDefeatButton || !IsBossTestScene())
            {
                return;
            }

            if (bossTestDefeatButton == null)
            {
                GameObject buttonObject = new GameObject("BossTestDefeatButton", typeof(RectTransform));
                buttonObject.transform.SetParent(transform, false);
                buttonObject.layer = gameObject.layer;

                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.sizeDelta = new Vector2(116f, 34f);
                rect.anchoredPosition = new Vector2(-18f, 118f);

                Image image = buttonObject.AddComponent<Image>();
                image.color = new Color(0.58f, 0.05f, 0.04f, 0.82f);
                image.raycastTarget = true;

                bossTestDefeatButton = buttonObject.AddComponent<Button>();
                TMP_Text label = CreateText(buttonObject.transform, "BossTestDefeatButtonText", "演示失败", 15, FontStyles.Bold);
                label.alignment = TextAlignmentOptions.Center;
            }

            bossTestDefeatButton.onClick.RemoveListener(HandleBossTestDefeatClicked);
            bossTestDefeatButton.onClick.AddListener(HandleBossTestDefeatClicked);
            bossTestDefeatButton.gameObject.SetActive(true);
            bossTestDefeatButton.transform.SetAsLastSibling();
        }

        private bool IsBossTestScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string expectedName = string.IsNullOrEmpty(bossTestSceneName) ? "boss_test" : bossTestSceneName;
            return string.Equals(sceneName, expectedName, System.StringComparison.OrdinalIgnoreCase);
        }

        private void FillDemoDefeatPlayers()
        {
            demoDefeatPlayerBuffer.Clear();

            ActiveUnitProvider provider = activeUnitProvider != null ? activeUnitProvider : Object.FindFirstObjectByType<ActiveUnitProvider>();
            if (provider != null)
            {
                provider.FillActiveAliveUnits(demoDefeatPlayerBuffer);
                for (int i = demoDefeatPlayerBuffer.Count - 1; i >= 0; i--)
                {
                    if (demoDefeatPlayerBuffer[i] == null || demoDefeatPlayerBuffer[i].Faction != UnitFaction.Player)
                    {
                        demoDefeatPlayerBuffer.RemoveAt(i);
                    }
                }
            }

            if (demoDefeatPlayerBuffer.Count == 0)
            {
                FillUnitBufferFromArray(playerUnits, UnitFaction.Player, demoDefeatPlayerBuffer);
            }

            if (demoDefeatPlayerBuffer.Count == 0)
            {
                Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < units.Length; i++)
                {
                    Unit unit = units[i];
                    if (unit != null && unit.Faction == UnitFaction.Player && unit.IsAlive)
                    {
                        demoDefeatPlayerBuffer.Add(unit);
                    }
                }
            }
        }

        private IEnumerator TriggerBossTestDefeatAfterDelay()
        {
            float delay = Mathf.Max(0f, bossTestOutcomeDelaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            pendingBossTestDefeatRoutine = null;
            ResolveOutcomeService();
            if (outcomeService == null && IsBossTestScene())
            {
                outcomeService = gameObject.AddComponent<GameOutcomeService>();
            }

            if (outcomeService != null && !outcomeService.HasOutcome)
            {
                outcomeService.SetDefeat("Demo defeat.");
            }
        }

        private void ResolveOutcomeService()
        {
            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }
        }

        private void EnsureBossTestOutcomeEvaluator()
        {
            if (!IsBossTestScene())
            {
                return;
            }

            ResolveOutcomeService();
            if (outcomeService == null)
            {
                outcomeService = gameObject.AddComponent<GameOutcomeService>();
            }

            if (outcomeAutoEvaluator == null)
            {
                outcomeAutoEvaluator = GetComponentInChildren<BattleOutcomeAutoEvaluator>(true);
            }

            if (outcomeAutoEvaluator == null)
            {
                outcomeAutoEvaluator = Object.FindFirstObjectByType<BattleOutcomeAutoEvaluator>();
            }

            if (outcomeAutoEvaluator == null)
            {
                outcomeAutoEvaluator = gameObject.AddComponent<BattleOutcomeAutoEvaluator>();
            }

            outcomeAutoEvaluator.ConfigureBossTest(
                outcomeService,
                playerUnits,
                enemyUnits,
                bossTestOutcomeDelaySeconds);
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

            RebindBossHealthViews();

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
            ForceVisible(bossHealthRoot);

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
            CreateBossHealthBar();

            turnTransitionPopupView = CreateTurnTransitionPopup();
            promptView = CreateView<PromptView>("Prompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(920f, 42f), new Vector2(0f, 20f));
            promptView.Bind(CreateText(promptView.transform, "PromptText", "Select a player unit.", 21, FontStyles.Normal));

            heroPanels = new HeroPanelView[4];
            for (int i = 0; i < heroPanels.Length; i++)
            {
                HeroPanelView panel = CreateView<HeroPanelView>("HeroPanel_" + i, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(310f, 132f), new Vector2(18f, 230f - i * 142f));
                Image healthFill = CreateHealthBar(panel.transform, "BloodBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-32f, 24f), new Vector2(0f, -18f), out RectTransform healthFillRect);
                TMP_Text heroText = CreateText(panel.transform, "HeroText", "Hero: Unbound", 18, FontStyles.Normal);
                RectTransform textRect = heroText.rectTransform;
                textRect.offsetMax = new Vector2(-8f, -38f);
                panel.Bind(heroText, healthFill, healthFillRect);
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

        private void EnsureBossHealthBar()
        {
            RebindBossHealthViews();

            if (bossHealthRoot == null && buildRuntimeLayoutIfMissing)
            {
                CreateBossHealthBar();
            }

            ConfigureBossHealthFill();
        }

        private void RebindBossHealthViews()
        {
            if (bossHealthRoot == null)
            {
                Transform root = FindChildByName(transform, "BossHealthPanel");
                if (root != null)
                {
                    bossHealthRoot = root.gameObject;
                }
            }

            if (bossHealthFillImage == null)
            {
                Transform fill = FindChildByName(transform, "BossHealthFill");
                if (fill != null)
                {
                    bossHealthFillImage = fill.GetComponent<Image>();
                }
            }

            if (bossHealthFillRect == null && bossHealthFillImage != null)
            {
                bossHealthFillRect = bossHealthFillImage.rectTransform;
            }

            if (bossHealthNameText == null)
            {
                Transform nameText = FindChildByName(transform, "BossHealthNameText");
                if (nameText != null)
                {
                    bossHealthNameText = nameText.GetComponent<TMP_Text>();
                }
            }

            if (bossHealthValueText == null)
            {
                Transform valueText = FindChildByName(transform, "BossHealthValueText");
                if (valueText != null)
                {
                    bossHealthValueText = valueText.GetComponent<TMP_Text>();
                }
            }
        }

        private void CreateBossHealthBar()
        {
            if (bossHealthRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("BossHealthPanel", typeof(RectTransform));
            rootObject.transform.SetParent(transform, false);
            rootObject.layer = gameObject.layer;
            bossHealthRoot = rootObject;

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.localScale = Vector3.one;
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.sizeDelta = new Vector2(720f, 54f);
            rootRect.anchoredPosition = new Vector2(0f, -92f);

            Image background = rootObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.03f, 0.02f, 0.86f);
            background.raycastTarget = false;

            TMP_Text nameText = CreateText(rootObject.transform, "BossHealthNameText", "Boss", 20, FontStyles.Bold);
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0.45f, 1f);
            nameRect.offsetMin = new Vector2(18f, -2f);
            nameRect.offsetMax = new Vector2(-8f, -4f);
            bossHealthNameText = nameText;

            TMP_Text valueText = CreateText(rootObject.transform, "BossHealthValueText", "-- / --", 18, FontStyles.Bold);
            valueText.alignment = TextAlignmentOptions.Right;
            RectTransform valueRect = valueText.rectTransform;
            valueRect.anchorMin = new Vector2(0.55f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = new Vector2(8f, -2f);
            valueRect.offsetMax = new Vector2(-18f, -4f);
            bossHealthValueText = valueText;

            CreateHealthBar(rootObject.transform, "BossHealthBar", new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f), new Vector2(-32f, 22f), new Vector2(0f, 8f), out bossHealthFillRect, "BossHealthFill");
            ConfigureBossHealthFill();
        }

        private Image CreateHealthBar(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position,
            out RectTransform fillRect,
            string fillName = "Fill")
        {
            GameObject barObject = new GameObject(name, typeof(RectTransform));
            barObject.transform.SetParent(parent, false);
            barObject.layer = parent.gameObject.layer;
            RectTransform barRect = barObject.GetComponent<RectTransform>();
            barRect.localScale = Vector3.one;
            barRect.anchorMin = anchorMin;
            barRect.anchorMax = anchorMax;
            barRect.pivot = pivot;
            barRect.sizeDelta = size;
            barRect.anchoredPosition = position;

            Image background = barObject.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.28f);
            background.raycastTarget = false;

            GameObject fillObject = new GameObject(fillName, typeof(RectTransform));
            fillObject.transform.SetParent(barObject.transform, false);
            fillObject.layer = barObject.layer;
            fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.localScale = Vector3.one;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = HealthFillColor;
            fillImage.raycastTarget = false;
            return fillImage;
        }

        private void ConfigureBossHealthFill()
        {
            if (bossHealthFillImage != null)
            {
                bossHealthFillImage.color = HealthFillColor;
                bossHealthFillImage.raycastTarget = false;
            }

            if (bossHealthFillRect == null)
            {
                return;
            }

            bossHealthFillRect.anchorMin = new Vector2(0f, 0f);
            bossHealthFillRect.pivot = new Vector2(0f, 0.5f);
            bossHealthFillRect.anchoredPosition = Vector2.zero;
            bossHealthFillRect.sizeDelta = Vector2.zero;
        }

        private void RefreshBossHealthBar()
        {
            if (bossHealthRoot == null)
            {
                return;
            }

            if (!ShouldExposeBossFightRuntime())
            {
                ResetBossHealthIntroState();
                bossHealthUnit = null;
                SetBossHealthVisible(false);
                return;
            }

            Unit boss = ResolveBossHealthUnit();
            if (boss == null || boss.RuntimeState == null || boss.Stats == null || !boss.IsAlive)
            {
                ResetBossHealthIntroState();
                bossHealthUnit = null;
                SetBossHealthVisible(false);
                return;
            }

            int hp = Mathf.Max(0, boss.RuntimeState.CurrentHp);
            int maxHp = Mathf.Max(1, boss.Stats.GetClampedMaxHp());

            if (bossHealthIntroUnit != boss)
            {
                StartBossHealthIntro(boss, hp, maxHp);
                return;
            }

            SetBossHealthVisible(true);
            ConfigureBossHealthFill();
            RefreshBossHealthText(boss, hp, maxHp);

            if (bossHealthIntroInProgress)
            {
                return;
            }

            if (bossHealthFillRect != null)
            {
                bossHealthFillRect.anchorMax = new Vector2(Mathf.Clamp01((float)hp / maxHp), 1f);
            }
        }

        private void StartBossHealthIntro(Unit boss, int hp, int maxHp)
        {
            if (boss == null)
            {
                return;
            }

            bossHealthIntroUnit = boss;
            bossHealthIntroInProgress = true;
            SetBossHealthVisible(true);
            ConfigureBossHealthFill();
            RefreshBossHealthText(boss, hp, maxHp);
            SetBossHealthFill(0f);

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            if (lockActionsDuringBossHealthIntro
                && movementControllerToSuspend != null
                && movementControllerToSuspend.enabled)
            {
                movementControllerToSuspend.enabled = false;
                restoreMovementControllerAfterBossIntro = true;
            }

            if (switchToBossBgmOnHealthIntro)
            {
                BTAudioService.PlayBgm(BTAudioCueId.BgmBoss);
            }

            if (bossHealthIntroRoutine != null)
            {
                StopCoroutine(bossHealthIntroRoutine);
            }

            bossHealthIntroRoutine = StartCoroutine(PlayBossHealthIntroRoutine(boss));
        }

        private IEnumerator PlayBossHealthIntroRoutine(Unit boss)
        {
            float duration = Mathf.Max(0f, bossHealthIntroFillDuration);
            float elapsed = 0f;

            while (boss != null
                && boss == bossHealthIntroUnit
                && boss.RuntimeState != null
                && boss.Stats != null
                && boss.IsAlive
                && elapsed < duration)
            {
                int hp = Mathf.Max(0, boss.RuntimeState.CurrentHp);
                int maxHp = Mathf.Max(1, boss.Stats.GetClampedMaxHp());
                RefreshBossHealthText(boss, hp, maxHp);
                float target = Mathf.Clamp01((float)hp / maxHp);
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                SetBossHealthFill(Mathf.Lerp(0f, target, t));
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (boss != null && boss == bossHealthIntroUnit && boss.RuntimeState != null && boss.Stats != null)
            {
                int hp = Mathf.Max(0, boss.RuntimeState.CurrentHp);
                int maxHp = Mathf.Max(1, boss.Stats.GetClampedMaxHp());
                RefreshBossHealthText(boss, hp, maxHp);
                SetBossHealthFill(Mathf.Clamp01((float)hp / maxHp));
            }

            bossHealthIntroInProgress = false;
            bossHealthIntroRoutine = null;
            RestoreMovementControllerAfterBossIntro();
        }

        private void RefreshBossHealthText(Unit boss, int hp, int maxHp)
        {
            if (bossHealthNameText != null)
            {
                bossHealthNameText.text = string.IsNullOrEmpty(boss.DisplayName) ? boss.gameObject.name : boss.DisplayName;
            }

            if (bossHealthValueText != null)
            {
                bossHealthValueText.text = hp + " / " + maxHp;
            }
        }

        private void SetBossHealthFill(float normalizedValue)
        {
            if (bossHealthFillRect != null)
            {
                bossHealthFillRect.anchorMax = new Vector2(Mathf.Clamp01(normalizedValue), 1f);
            }
        }

        private void ResetBossHealthIntroState()
        {
            if (bossHealthIntroRoutine != null)
            {
                StopCoroutine(bossHealthIntroRoutine);
                bossHealthIntroRoutine = null;
            }

            bossHealthIntroUnit = null;
            bossHealthIntroInProgress = false;
            RestoreMovementControllerAfterBossIntro();
        }

        private void RestoreMovementControllerAfterBossIntro()
        {
            if (restoreMovementControllerAfterBossIntro && movementControllerToSuspend != null)
            {
                movementControllerToSuspend.enabled = true;
            }

            restoreMovementControllerAfterBossIntro = false;
        }

        private void RefreshBossIntentPreview()
        {
            if (!showBossIntentDuringPlayerTurn || movementHighlighter == null)
            {
                return;
            }

            if (!ShouldExposeBossFightRuntime())
            {
                movementHighlighter.ClearBossIntentHighlights();
                return;
            }

            if (turnManager == null || turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                movementHighlighter.ClearBossIntentHighlights();
                return;
            }

            if (Time.unscaledTime < nextBossIntentLookupTime)
            {
                return;
            }

            nextBossIntentLookupTime = Time.unscaledTime + 0.18f;
            Unit boss = ResolveBossIntentUnit();
            if (boss == null)
            {
                movementHighlighter.ClearBossIntentHighlights();
                return;
            }

            BossAttackIntent intent;
            if (BossEnemyAIController.TryGetPreviewIntent(boss, out intent))
            {
                movementHighlighter.ShowBossIntent(intent.AffectedTiles);
                return;
            }

            FillBossIntentPlayers();
            if (!bossEnemyAIController.TryBuildPreviewIntent(boss, bossIntentPlayerBuffer, gridManager, out intent))
            {
                movementHighlighter.ClearBossIntentHighlights();
                return;
            }

            BossEnemyAIController.CachePreviewIntent(boss, intent);
            movementHighlighter.ShowBossIntent(intent.AffectedTiles);
        }

        private Unit ResolveBossIntentUnit()
        {
            FillBossIntentEnemies();
            return FindBossUnit(bossIntentEnemyBuffer);
        }

        private void FillBossIntentEnemies()
        {
            bossIntentEnemyBuffer.Clear();
            ActiveUnitProvider provider = ResolveActiveUnitProviderForBossIntent();
            if (provider != null)
            {
                provider.FillActiveAliveEnemies(bossIntentEnemyBuffer);
            }

            if (bossIntentEnemyBuffer.Count > 0)
            {
                return;
            }

            FillUnitBufferFromArray(enemyUnits, UnitFaction.Enemy, bossIntentEnemyBuffer);
        }

        private void FillBossIntentPlayers()
        {
            bossIntentPlayerBuffer.Clear();
            ActiveUnitProvider provider = ResolveActiveUnitProviderForBossIntent();
            if (provider != null)
            {
                provider.FillActiveAliveUnits(bossIntentPlayerBuffer);
                for (int i = bossIntentPlayerBuffer.Count - 1; i >= 0; i--)
                {
                    if (bossIntentPlayerBuffer[i] == null || bossIntentPlayerBuffer[i].Faction != UnitFaction.Player)
                    {
                        bossIntentPlayerBuffer.RemoveAt(i);
                    }
                }
            }

            if (bossIntentPlayerBuffer.Count > 0)
            {
                return;
            }

            FillUnitBufferFromArray(playerUnits, UnitFaction.Player, bossIntentPlayerBuffer);
        }

        private ActiveUnitProvider ResolveActiveUnitProviderForBossIntent()
        {
            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            return activeUnitProvider;
        }

        private static void FillUnitBufferFromArray(Unit[] units, UnitFaction faction, List<Unit> results)
        {
            if (units == null || results == null)
            {
                return;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null
                    && unit.gameObject.activeInHierarchy
                    && unit.IsAlive
                    && unit.Faction == faction)
                {
                    results.Add(unit);
                }
            }
        }

        private Unit ResolveBossHealthUnit()
        {
            if (bossHealthUnit != null && IsBossLikeUnit(bossHealthUnit))
            {
                return bossHealthUnit;
            }

            if (Time.unscaledTime < nextBossHealthLookupTime)
            {
                return null;
            }

            nextBossHealthLookupTime = Time.unscaledTime + BossHealthLookupInterval;
            bossHealthUnit = FindBossUnit(enemyUnits);

            if (bossHealthUnit == null && activeUnitProvider != null)
            {
                activeBossLookupBuffer.Clear();
                activeUnitProvider.FillActiveAliveEnemies(activeBossLookupBuffer);
                bossHealthUnit = FindBossUnit(activeBossLookupBuffer);
            }

            return bossHealthUnit;
        }

        private Unit FindBossUnit(IList<Unit> units)
        {
            if (units == null)
            {
                return null;
            }

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (IsBossLikeUnit(unit))
                {
                    return unit;
                }
            }

            return null;
        }

        private bool IsBossLikeUnit(Unit unit)
        {
            if (unit == null || unit.Faction != UnitFaction.Enemy || !unit.gameObject.activeInHierarchy || !unit.IsAlive)
            {
                return false;
            }

            string needle = string.IsNullOrEmpty(bossHealthNameContains) ? "boss" : bossHealthNameContains.ToLowerInvariant();
            string objectName = unit.gameObject.name.ToLowerInvariant();
            string displayName = string.IsNullOrEmpty(unit.DisplayName) ? string.Empty : unit.DisplayName.ToLowerInvariant();
            return objectName.Contains(needle)
                || displayName.Contains(needle)
                || objectName.Contains("golem")
                || displayName.Contains("golem");
        }

        private void SetBossHealthVisible(bool visible)
        {
            if (bossHealthRoot != null && bossHealthRoot.activeSelf != visible)
            {
                bossHealthRoot.SetActive(visible);
            }
        }

        private bool ShouldExposeBossFightRuntime()
        {
            BossGateProgressionState progressionState = Object.FindFirstObjectByType<BossGateProgressionState>();
            return progressionState != null && progressionState.ShouldExposeBossFightRuntime();
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindChildByName(root.GetChild(i), childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
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
