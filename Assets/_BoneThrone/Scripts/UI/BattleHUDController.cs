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
        private const float BossHealthLookupInterval = 1f;
        private const string MobileCameraJoystickObjectName = "MobileCameraJoystickHint";

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
        [SerializeField] [Min(0f)] private float battleOutcomeDelaySeconds = 1f;
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

        [Header("Boss Health Bar")]
        [SerializeField] private GameObject bossHealthRoot;
        [SerializeField] private Image bossHealthFillImage;
        [SerializeField] private RectTransform bossHealthFillRect;
        [SerializeField] private Image bossHealthMissingImage;
        [SerializeField] private RectTransform bossHealthMissingRect;
        [SerializeField] private TMP_Text bossHealthNameText;
        [SerializeField] private TMP_Text bossHealthValueText;
        [SerializeField] private string bossHealthNameContains = "Boss";
        [SerializeField] private bool showBossIntentDuringPlayerTurn = true;
        [SerializeField] private bool switchToBossBgmOnHealthIntro = true;
        [SerializeField] private bool lockActionsDuringBossHealthIntro = true;
        [SerializeField] [Min(0f)] private float bossHealthIntroFillDuration = 1.2f;

        [Header("In-Game Settings")]
        [SerializeField] private Button gameplaySettingsButton;
        [SerializeField] private GameObject gameplaySettingsOverlayRoot;
        [SerializeField] private SettingsPageView gameplaySettingsPageView;
        [SerializeField] private Slider gameplayBgmSlider;
        [SerializeField] private Slider gameplaySfxSlider;
        [SerializeField] private Button gameplayReturnButton;
        [SerializeField] private Button gameplayQuitButton;
        [SerializeField] private string mainMenuSceneName = "StartMenu";

        [Header("Mobile HUD")]
        [SerializeField] private bool mobileHudSafeLayoutEnabled = true;
        [SerializeField] private bool showMobileCameraJoystick = true;
        [SerializeField] private Vector2 mobileCameraJoystickViewportCenter = new Vector2(0.17f, 0.16f);
        [SerializeField] [Range(0.04f, 0.16f)] private float mobileCameraJoystickRadiusViewportHeight = 0.085f;
        [SerializeField] private Color mobileCameraJoystickColor = new Color(1f, 1f, 1f, 0.32f);

        [Header("Boss Test Demo")]
        [SerializeField] private bool showBossTestVictoryButton = true;
        [SerializeField] private bool showBossTestDefeatButton = true;
        [SerializeField] private string bossTestSceneName = "boss_test";
        [SerializeField] private string bossTestVictoryReason = "演示胜利。";
        [SerializeField] private string bossTestDefeatReason = "演示失败。";
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
        private readonly List<Unit> playerLookupBuffer = new List<Unit>();
        private readonly BossEnemyAIController bossEnemyAIController = new BossEnemyAIController();
        private Button bossTestVictoryButton;
        private Button bossTestDefeatButton;
        private RectTransform bossTestOutcomeButtonRow;
        private bool gameplaySettingsSilentlyUpdating;
        private bool gameplaySettingsVisible;
        private bool restoreMovementControllerAfterGameplaySettings;
        private Coroutine pendingBossTestDefeatRoutine;
        private RectTransform mobileCameraJoystickRect;
        private MobileCameraJoystickGraphic mobileCameraJoystickGraphic;

        private void Awake()
        {
            RebindMissingViews();
            ResolveMissingGameplayReferences();
            EnsureMobileHudSafeLayout();
            EnsureMobileCameraJoystickHint();
            RefreshMobileCameraJoystickHint();

            EnsureBossHealthBar();
            EnsureEndTurnButton();
            EnsureTurnTransitionPopupView();
            EnsureActiveUnitProvider();
            EnsurePlayerUnitBindings();
            EnsureBattleOutcomeAutoEvaluator();
            EnsureDefendSystem();
            EnsurePotionSystem();
            EnsureActionModeController();
            ConfigureActionModeController();
            EnsureGameplaySettingsPageView();
            ConfigureGameplaySettingsPageView();
            RefreshGameplaySettingsPanelVisibility();
            EnsureBossTestOutcomeButtons();
            EnsureBossTestOutcomeEvaluator();
        }

        private void OnEnable()
        {
            SubscribeCombatLog();
            SubscribeSkillBar();
            EnsureMobileHudSafeLayout();
            EnsureMobileCameraJoystickHint();
            RefreshMobileCameraJoystickHint();
            EnsureGameplaySettingsPageView();
            ConfigureGameplaySettingsPageView();
            BindGameplaySettingsUi();
        }

        private void Start()
        {
            MenuProgressionState.RegisterVisitedScene(SceneManager.GetActiveScene().name);
            ApplyGameplayAudioSliderValues();
            EnsurePlayerUnitBindings();
            RefreshHeroPanels(selectionManager != null ? selectionManager.SelectedUnit : null);

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
            UnbindGameplaySettingsUi();
            if (bossTestVictoryButton != null)
            {
                bossTestVictoryButton.onClick.RemoveListener(HandleBossTestVictoryClicked);
            }

            if (bossTestDefeatButton != null)
            {
                bossTestDefeatButton.onClick.RemoveListener(HandleBossTestDefeatClicked);
            }

            RestoreMovementControllerAfterGameplaySettings();
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
            RefreshMobileCameraJoystickHint();
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
                    promptView.ShowOverride("移动暂不可用：行动模式未绑定。", 1.5f);
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
                    promptView.ShowOverride("普通攻击暂不可用：行动模式未绑定。", 1.5f);
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
                    promptView.ShowOverride("技能暂不可用：行动模式未绑定。", 1.5f);
                }

                return;
            }

            actionModeController.HandleSkillSlotButtonClicked(slotIndex);
        }

        private void HandleDefendClicked()
        {
            Unit selectedUnit = GetSelectedUnitForSelfAction("防御");
            if (selectedUnit == null)
            {
                return;
            }

            EnsureDefendSystem();
            if (defendSystem == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("防御暂不可用：防御系统未绑定。", 1.5f);
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
            Unit selectedUnit = GetSelectedUnitForSelfAction("药水");
            if (selectedUnit == null)
            {
                return;
            }

            EnsurePotionSystem();
            if (potionSystem == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("药水暂不可用：药水系统未绑定。", 1.5f);
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
                    promptView.ShowOverride("结束回合暂不可用：回合系统未绑定。", 1.5f);
                }

                return;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("敌方回合中不能结束玩家回合。", 1.5f);
                }

                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("请先选择一名玩家角色。", 1.5f);
                }

                return;
            }

            if (selectedUnit.Faction != UnitFaction.Player)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前选择的不是玩家角色。", 1.5f);
                }

                return;
            }

            if (!selectedUnit.IsAlive)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前角色已经倒下。", 1.5f);
                }

                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前角色缺少回合状态。", 1.5f);
                }

                return;
            }

            if (turnState.HasEnded)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前角色本回合已经结束。", 1.5f);
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
            if (TryTriggerBossTestOutcome(GameOutcome.Defeat, bossTestDefeatReason))
            {
                return;
            }

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
                promptView.ShowOverride("演示失败。", 1.5f);
            }

            if (pendingBossTestDefeatRoutine != null)
            {
                StopCoroutine(pendingBossTestDefeatRoutine);
            }

            pendingBossTestDefeatRoutine = StartCoroutine(TriggerBossTestDefeatAfterDelay());
        }

        private void HandleBossTestVictoryClicked()
        {
            TryTriggerBossTestOutcome(GameOutcome.Victory, bossTestVictoryReason);
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

        private void ResolveMissingGameplayReferences()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindFirstObjectByType<TurnManager>();
            }

            if (selectionManager == null)
            {
                selectionManager = Object.FindFirstObjectByType<SelectionManager>();
            }

            if (progressionService == null)
            {
                progressionService = Object.FindFirstObjectByType<LevelProgressionService>();
            }

            if (stairs == null)
            {
                stairs = Object.FindFirstObjectByType<InteractableStairs>();
            }

            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }

            if (gridManager == null)
            {
                gridManager = Object.FindFirstObjectByType<GridManager>();
            }

            if (combatSystem == null)
            {
                combatSystem = Object.FindFirstObjectByType<CombatSystem>();
            }

            if (skillSystem == null)
            {
                skillSystem = Object.FindFirstObjectByType<SkillSystem>();
            }

            if (movementControllerToSuspend == null)
            {
                movementControllerToSuspend = Object.FindFirstObjectByType<PlayerMovementController>();
            }

            if (movementHighlighter == null)
            {
                movementHighlighter = Object.FindFirstObjectByType<MovementDebugHighlighter>();
            }

            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }

            if (outcomeAutoEvaluator == null)
            {
                outcomeAutoEvaluator = Object.FindFirstObjectByType<BattleOutcomeAutoEvaluator>();
            }

            if (actionInputCamera == null)
            {
                actionInputCamera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            }

            if (!HasAnyUnit(enemyUnits))
            {
                enemyUnits = FindSceneUnitsByFaction(UnitFaction.Enemy, FindObjectsInactive.Include);
            }
        }

        private void BindGameplaySettingsUi()
        {
            if (gameplaySettingsButton != null)
            {
                gameplaySettingsButton.onClick.RemoveListener(HandleGameplaySettingsOpenClicked);
                gameplaySettingsButton.onClick.AddListener(HandleGameplaySettingsOpenClicked);
            }

            Button resolvedReturnButton = GetGameplayReturnButton();
            if (resolvedReturnButton != null)
            {
                resolvedReturnButton.onClick.RemoveListener(HandleGameplaySettingsReturnClicked);
                resolvedReturnButton.onClick.AddListener(HandleGameplaySettingsReturnClicked);
            }

            Button resolvedQuitButton = GetGameplayQuitButton();
            if (resolvedQuitButton != null)
            {
                resolvedQuitButton.onClick.RemoveListener(HandleGameplaySettingsQuitClicked);
                resolvedQuitButton.onClick.AddListener(HandleGameplaySettingsQuitClicked);
            }

            Slider resolvedBgmSlider = GetGameplayBgmSlider();
            if (resolvedBgmSlider != null)
            {
                resolvedBgmSlider.onValueChanged.RemoveListener(HandleGameplayBgmSliderChanged);
                resolvedBgmSlider.onValueChanged.AddListener(HandleGameplayBgmSliderChanged);
            }

            Slider resolvedSfxSlider = GetGameplaySfxSlider();
            if (resolvedSfxSlider != null)
            {
                resolvedSfxSlider.onValueChanged.RemoveListener(HandleGameplaySfxSliderChanged);
                resolvedSfxSlider.onValueChanged.AddListener(HandleGameplaySfxSliderChanged);
            }
        }

        private void UnbindGameplaySettingsUi()
        {
            if (gameplaySettingsButton != null)
            {
                gameplaySettingsButton.onClick.RemoveListener(HandleGameplaySettingsOpenClicked);
            }

            Button resolvedReturnButton = GetGameplayReturnButton();
            if (resolvedReturnButton != null)
            {
                resolvedReturnButton.onClick.RemoveListener(HandleGameplaySettingsReturnClicked);
            }

            Button resolvedQuitButton = GetGameplayQuitButton();
            if (resolvedQuitButton != null)
            {
                resolvedQuitButton.onClick.RemoveListener(HandleGameplaySettingsQuitClicked);
            }

            Slider resolvedBgmSlider = GetGameplayBgmSlider();
            if (resolvedBgmSlider != null)
            {
                resolvedBgmSlider.onValueChanged.RemoveListener(HandleGameplayBgmSliderChanged);
            }

            Slider resolvedSfxSlider = GetGameplaySfxSlider();
            if (resolvedSfxSlider != null)
            {
                resolvedSfxSlider.onValueChanged.RemoveListener(HandleGameplaySfxSliderChanged);
            }
        }

        private void ApplyGameplayAudioSliderValues()
        {
            gameplaySettingsSilentlyUpdating = true;

            if (gameplaySettingsPageView != null)
            {
                gameplaySettingsPageView.SetAudioValuesWithoutNotify(
                    BTAudioService.GetBgmVolume(),
                    BTAudioService.GetSfxVolume());
            }
            else
            {
                Slider resolvedBgmSlider = GetGameplayBgmSlider();
                if (resolvedBgmSlider != null)
                {
                    resolvedBgmSlider.SetValueWithoutNotify(BTAudioService.GetBgmVolume());
                }

                Slider resolvedSfxSlider = GetGameplaySfxSlider();
                if (resolvedSfxSlider != null)
                {
                    resolvedSfxSlider.SetValueWithoutNotify(BTAudioService.GetSfxVolume());
                }
            }

            gameplaySettingsSilentlyUpdating = false;
        }

        private void HandleGameplaySettingsOpenClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            ApplyGameplayAudioSliderValues();
            gameplaySettingsVisible = true;
            RefreshGameplaySettingsPanelVisibility();

            if (movementControllerToSuspend != null && movementControllerToSuspend.enabled)
            {
                movementControllerToSuspend.enabled = false;
                restoreMovementControllerAfterGameplaySettings = true;
            }
        }

        private void HandleGameplaySettingsReturnClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);
            gameplaySettingsVisible = false;
            RefreshGameplaySettingsPanelVisibility();
            RestoreMovementControllerAfterGameplaySettings();
        }

        private void HandleGameplaySettingsQuitClicked()
        {
            BTAudioService.PlaySfx(BTAudioCueId.ButtonClick);

            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogWarning("BattleHUDController cannot return to the main menu because no scene name is configured.", this);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogWarning("BattleHUDController cannot return to main menu scene '" + mainMenuSceneName + "' because it is not in Build Settings.", this);
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        }

        private void HandleGameplayBgmSliderChanged(float value)
        {
            if (gameplaySettingsSilentlyUpdating)
            {
                return;
            }

            BTAudioService.SetBgmVolume(SettingsPageView.SnapAudioValue(value));
        }

        private void HandleGameplaySfxSliderChanged(float value)
        {
            if (gameplaySettingsSilentlyUpdating)
            {
                return;
            }

            BTAudioService.SetSfxVolume(SettingsPageView.SnapAudioValue(value));
        }

        private void RefreshGameplaySettingsPanelVisibility()
        {
            if (gameplaySettingsOverlayRoot != null)
            {
                gameplaySettingsOverlayRoot.SetActive(gameplaySettingsVisible);
            }

            RefreshMobileCameraJoystickHint();
            RefreshBossHealthLayering();
        }

        private void EnsureGameplaySettingsPageView()
        {
            if (gameplaySettingsPageView == null && gameplaySettingsOverlayRoot != null)
            {
                gameplaySettingsPageView = gameplaySettingsOverlayRoot.GetComponentInChildren<SettingsPageView>(true);
            }
        }

        private void ConfigureGameplaySettingsPageView()
        {
            if (gameplaySettingsPageView != null)
            {
                gameplaySettingsPageView.SetButtonLabels("返回游戏", "返回主页");
            }
        }

        private Slider GetGameplayBgmSlider()
        {
            return gameplaySettingsPageView != null && gameplaySettingsPageView.BgmSlider != null
                ? gameplaySettingsPageView.BgmSlider
                : gameplayBgmSlider;
        }

        private Slider GetGameplaySfxSlider()
        {
            return gameplaySettingsPageView != null && gameplaySettingsPageView.SfxSlider != null
                ? gameplaySettingsPageView.SfxSlider
                : gameplaySfxSlider;
        }

        private Button GetGameplayReturnButton()
        {
            return gameplaySettingsPageView != null && gameplaySettingsPageView.PrimaryButton != null
                ? gameplaySettingsPageView.PrimaryButton
                : gameplayReturnButton;
        }

        private Button GetGameplayQuitButton()
        {
            return gameplaySettingsPageView != null && gameplaySettingsPageView.SecondaryButton != null
                ? gameplaySettingsPageView.SecondaryButton
                : gameplayQuitButton;
        }

        private void RestoreMovementControllerAfterGameplaySettings()
        {
            if (restoreMovementControllerAfterGameplaySettings && movementControllerToSuspend != null)
            {
                movementControllerToSuspend.enabled = true;
            }

            restoreMovementControllerAfterGameplaySettings = false;
        }

        private void EnsureBossTestOutcomeButtons()
        {
            bool isBossTest = IsBossTestScene();
            Transform row = FindChildByName(transform, "BossTestOutcomeButtonRow");
            if (row != null)
            {
                bossTestOutcomeButtonRow = row as RectTransform;
            }

            if (!isBossTest)
            {
                if (bossTestOutcomeButtonRow != null)
                {
                    bossTestOutcomeButtonRow.gameObject.SetActive(false);
                }

                return;
            }

            if (bossTestOutcomeButtonRow == null)
            {
                bossTestOutcomeButtonRow = CreateBossTestOutcomeButtonRow();
            }

            if (bossTestOutcomeButtonRow == null)
            {
                return;
            }

            bossTestVictoryButton = EnsureBossTestOutcomeButton(
                bossTestOutcomeButtonRow,
                "BossTestVictoryButton",
                "直接胜利",
                new Color(0.2f, 0.45f, 0.24f, 0.96f));

            bossTestDefeatButton = EnsureBossTestOutcomeButton(
                bossTestOutcomeButtonRow,
                "BossTestDefeatButton",
                "直接失败",
                new Color(0.52f, 0.12f, 0.12f, 0.96f));

            if (bossTestVictoryButton != null)
            {
                bossTestVictoryButton.onClick.RemoveListener(HandleBossTestVictoryClicked);
                bossTestVictoryButton.onClick.AddListener(HandleBossTestVictoryClicked);
                bossTestVictoryButton.gameObject.SetActive(showBossTestVictoryButton);
            }

            if (bossTestDefeatButton != null)
            {
                bossTestDefeatButton.onClick.RemoveListener(HandleBossTestDefeatClicked);
                bossTestDefeatButton.onClick.AddListener(HandleBossTestDefeatClicked);
                bossTestDefeatButton.gameObject.SetActive(showBossTestDefeatButton);
            }

            bossTestOutcomeButtonRow.gameObject.SetActive(showBossTestVictoryButton || showBossTestDefeatButton);
        }

        private RectTransform CreateBossTestOutcomeButtonRow()
        {
            GameObject rowObject = new GameObject(
                "BossTestOutcomeButtonRow",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));
            rowObject.layer = gameObject.layer;
            rowObject.transform.SetParent(transform, false);

            RectTransform rectTransform = rowObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-24f, -24f);
            rectTransform.sizeDelta = new Vector2(452f, 56f);

            HorizontalLayoutGroup layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return rectTransform;
        }

        private Button EnsureBossTestOutcomeButton(
            RectTransform parent,
            string buttonName,
            string labelText,
            Color backgroundColor)
        {
            if (parent == null)
            {
                return null;
            }

            Transform existing = parent.Find(buttonName);
            if (existing != null)
            {
                Button existingButton = existing.GetComponent<Button>();
                ApplyBossTestOutcomeButtonVisuals(existingButton, labelText, backgroundColor);
                return existingButton;
            }

            GameObject buttonObject = new GameObject(
                buttonName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.layer = gameObject.layer;
            buttonObject.transform.SetParent(parent, false);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 220f;
            layout.preferredHeight = 56f;
            layout.minWidth = 180f;

            GameObject labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.layer = gameObject.layer;
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(14f, 8f);
            labelRect.offsetMax = new Vector2(-14f, -8f);

            Button button = buttonObject.GetComponent<Button>();
            ApplyBossTestOutcomeButtonVisuals(button, labelText, backgroundColor);
            return button;
        }

        private void ApplyBossTestOutcomeButtonVisuals(Button button, string labelText, Color backgroundColor)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = backgroundColor;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                TMP_Text templateText = GetComponentInChildren<TMP_Text>(true);
                if (templateText != null && templateText.font != null)
                {
                    label.font = templateText.font;
                    label.fontSharedMaterial = templateText.fontSharedMaterial;
                }

                label.text = labelText;
                label.fontSize = 24f;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
                label.raycastTarget = false;
            }
        }

        private bool IsBossTestScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string expectedName = string.IsNullOrEmpty(bossTestSceneName) ? "boss_test" : bossTestSceneName;
            return string.Equals(sceneName, expectedName, System.StringComparison.OrdinalIgnoreCase);
        }

        private bool TryTriggerBossTestOutcome(GameOutcome outcome, string reason)
        {
            if (!IsBossTestScene())
            {
                return false;
            }

            if (actionModeController != null)
            {
                actionModeController.CancelTargetingForExternalAction();
            }

            ResolveOutcomeService();
            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }

            if (outcomeService == null)
            {
                outcomeService = gameObject.AddComponent<GameOutcomeService>();
            }

            if (outcomeService == null)
            {
                return false;
            }

            if (outcomeService.HasOutcome)
            {
                outcomeService.ClearOutcome();
            }

            if (promptView != null)
            {
                promptView.ShowOverride(reason, 1.5f);
            }

            if (outcome == GameOutcome.Victory)
            {
                if (outcomeService.SetVictory(reason))
                {
                    outcomeService.ForceShowCurrentOutcomePopup();
                }

                return true;
            }

            if (outcomeService.SetDefeat(reason))
            {
                outcomeService.ForceShowCurrentOutcomePopup();
            }

            return true;
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
                outcomeService.SetDefeat("演示失败。");
            }
        }

        private void ResolveOutcomeService()
        {
            if (outcomeService == null)
            {
                outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            }
        }

        private void EnsureBattleOutcomeAutoEvaluator()
        {
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

            EnsurePlayerUnitBindings();
            outcomeAutoEvaluator.ConfigureDefeatOnly(
                outcomeService,
                playerUnits,
                battleOutcomeDelaySeconds);
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
            ResolveCombatLog();

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

        private void ResolveCombatLog()
        {
            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }

        private Unit GetSelectedUnitForSelfAction(string actionName)
        {
            if (turnManager == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride(actionName + "暂不可用：回合系统未绑定。", 1.5f);
                }

                return null;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("敌方回合中不能使用" + actionName + "。", 1.5f);
                }

                return null;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("请先选择一名玩家角色。", 1.5f);
                }

                return null;
            }

            if (selectedUnit.Faction != UnitFaction.Player)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前选择的不是玩家角色。", 1.5f);
                }

                return null;
            }

            if (!selectedUnit.IsAlive)
            {
                if (promptView != null)
                {
                    promptView.ShowOverride("当前角色已经倒下。", 1.5f);
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

            EnsurePlayerUnitBindings();

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

        private void EnsurePlayerUnitBindings()
        {
            if (!HasMissingPlayerUnitSlot())
            {
                return;
            }

            if (playerUnits == null || playerUnits.Length < 4)
            {
                Unit[] resizedUnits = new Unit[4];
                if (playerUnits != null)
                {
                    int copyCount = Mathf.Min(playerUnits.Length, resizedUnits.Length);
                    for (int i = 0; i < copyCount; i++)
                    {
                        resizedUnits[i] = playerUnits[i];
                    }
                }

                playerUnits = resizedUnits;
            }

            Unit[] progressionUnits = progressionService != null ? progressionService.PlayerUnits : null;
            FillMissingPlayerSlotsFromArray(progressionUnits);

            if (!HasMissingPlayerUnitSlot())
            {
                return;
            }

            playerLookupBuffer.Clear();
            if (activeUnitProvider != null)
            {
                activeUnitProvider.FillActiveAliveUnits(playerLookupBuffer);
                for (int i = playerLookupBuffer.Count - 1; i >= 0; i--)
                {
                    Unit unit = playerLookupBuffer[i];
                    if (unit == null || unit.Faction != UnitFaction.Player)
                    {
                        playerLookupBuffer.RemoveAt(i);
                    }
                }
            }

            if (playerLookupBuffer.Count == 0)
            {
                Unit[] sceneUnits = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < sceneUnits.Length; i++)
                {
                    Unit unit = sceneUnits[i];
                    if (unit != null && unit.Faction == UnitFaction.Player)
                    {
                        playerLookupBuffer.Add(unit);
                    }
                }
            }

            FillMissingPlayerSlotsFromList(playerLookupBuffer);
        }

        private bool HasMissingPlayerUnitSlot()
        {
            if (playerUnits == null || playerUnits.Length < 4)
            {
                return true;
            }

            for (int i = 0; i < playerUnits.Length && i < 4; i++)
            {
                if (playerUnits[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void FillMissingPlayerSlotsFromArray(Unit[] units)
        {
            if (units == null)
            {
                return;
            }

            for (int i = 0; i < units.Length; i++)
            {
                AssignPlayerUnitToPreferredSlot(units[i]);
            }
        }

        private void FillMissingPlayerSlotsFromList(List<Unit> units)
        {
            if (units == null)
            {
                return;
            }

            for (int i = 0; i < units.Count; i++)
            {
                AssignPlayerUnitToPreferredSlot(units[i]);
            }
        }

        private void AssignPlayerUnitToPreferredSlot(Unit unit)
        {
            if (unit == null || unit.Faction != UnitFaction.Player || playerUnits == null)
            {
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] == unit)
                {
                    return;
                }
            }

            int preferredIndex = GetPreferredPlayerSlot(unit.RoleId);
            if (preferredIndex >= 0 && preferredIndex < playerUnits.Length && playerUnits[preferredIndex] == null)
            {
                playerUnits[preferredIndex] = unit;
                return;
            }

            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] == null)
                {
                    playerUnits[i] = unit;
                    return;
                }
            }
        }

        private static int GetPreferredPlayerSlot(RoleId roleId)
        {
            switch (roleId)
            {
                case RoleId.Fighter:
                    return 0;
                case RoleId.Ranger:
                    return 1;
                case RoleId.Mage:
                    return 2;
                case RoleId.Barbarian:
                    return 3;
                default:
                    return -1;
            }
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

        private void EnsureMobileHudSafeLayout()
        {
            if (!mobileHudSafeLayoutEnabled)
            {
                return;
            }

            RectTransform sharedFrame = FindChildByName(transform, "HeroPanelsSharedFrameImage") as RectTransform;
            if (sharedFrame != null)
            {
                Image frameImage = sharedFrame.GetComponent<Image>();
                if (frameImage != null)
                {
                    frameImage.raycastTarget = false;
                }
            }

            DisableLooseHeroBackdropRaycasts();
        }

        private void DisableLooseHeroBackdropRaycasts()
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null || image.transform == null || !image.name.StartsWith("BattleCustomImagePlaceholder"))
                {
                    continue;
                }

                image.raycastTarget = false;
            }
        }

        private void EnsureMobileCameraJoystickHint()
        {
            Transform existing = FindChildByName(transform, MobileCameraJoystickObjectName);
            if (existing != null)
            {
                mobileCameraJoystickRect = existing as RectTransform;
                mobileCameraJoystickGraphic = existing.GetComponent<MobileCameraJoystickGraphic>();
            }

            if (mobileCameraJoystickRect != null && mobileCameraJoystickGraphic != null)
            {
                return;
            }

            GameObject joystickObject = new GameObject(
                MobileCameraJoystickObjectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(MobileCameraJoystickGraphic));
            joystickObject.layer = gameObject.layer;
            joystickObject.transform.SetParent(transform, false);

            mobileCameraJoystickRect = joystickObject.GetComponent<RectTransform>();
            mobileCameraJoystickGraphic = joystickObject.GetComponent<MobileCameraJoystickGraphic>();
            mobileCameraJoystickGraphic.raycastTarget = false;
            mobileCameraJoystickGraphic.color = mobileCameraJoystickColor;
        }

        private void RefreshMobileCameraJoystickHint()
        {
            if (mobileCameraJoystickRect == null || mobileCameraJoystickGraphic == null)
            {
                return;
            }

            bool visible = ShouldShowMobileCameraJoystick();
            if (mobileCameraJoystickRect.gameObject.activeSelf != visible)
            {
                mobileCameraJoystickRect.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            RectTransform rootRect = transform as RectTransform;
            Rect canvasRect = rootRect != null ? rootRect.rect : new Rect(0f, 0f, 1920f, 1080f);
            float width = Mathf.Max(1f, canvasRect.width);
            float height = Mathf.Max(1f, canvasRect.height);
            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f || Screen.width <= 0 || Screen.height <= 0)
            {
                safeArea = new Rect(0f, 0f, Screen.width > 0 ? Screen.width : 1920f, Screen.height > 0 ? Screen.height : 1080f);
            }

            float scaleX = width / Mathf.Max(1f, Screen.width);
            float scaleY = height / Mathf.Max(1f, Screen.height);
            Vector2 clampedViewportCenter = new Vector2(
                Mathf.Clamp01(mobileCameraJoystickViewportCenter.x),
                Mathf.Clamp01(mobileCameraJoystickViewportCenter.y));
            Vector2 center = new Vector2(
                (safeArea.xMin + safeArea.width * clampedViewportCenter.x) * scaleX,
                (safeArea.yMin + safeArea.height * clampedViewportCenter.y) * scaleY);
            float diameter = Mathf.Max(64f, safeArea.height * scaleY * Mathf.Max(0.01f, mobileCameraJoystickRadiusViewportHeight) * 2f);

            Vector2 joystickSize = new Vector2(diameter, diameter);
            if (mobileCameraJoystickRect.anchorMin != Vector2.zero)
            {
                mobileCameraJoystickRect.anchorMin = Vector2.zero;
            }

            if (mobileCameraJoystickRect.anchorMax != Vector2.zero)
            {
                mobileCameraJoystickRect.anchorMax = Vector2.zero;
            }

            if (mobileCameraJoystickRect.pivot != new Vector2(0.5f, 0.5f))
            {
                mobileCameraJoystickRect.pivot = new Vector2(0.5f, 0.5f);
            }

            if ((mobileCameraJoystickRect.anchoredPosition - center).sqrMagnitude > 0.01f)
            {
                mobileCameraJoystickRect.anchoredPosition = center;
            }

            if ((mobileCameraJoystickRect.sizeDelta - joystickSize).sqrMagnitude > 0.01f)
            {
                mobileCameraJoystickRect.sizeDelta = joystickSize;
            }

            if (mobileCameraJoystickRect.localScale != Vector3.one)
            {
                mobileCameraJoystickRect.localScale = Vector3.one;
            }

            if (mobileCameraJoystickGraphic.color != mobileCameraJoystickColor)
            {
                mobileCameraJoystickGraphic.color = mobileCameraJoystickColor;
            }
        }

        private bool ShouldShowMobileCameraJoystick()
        {
            return showMobileCameraJoystick
                && IsMobileCameraJoystickPlatform()
                && !IsGameplaySettingsPanelVisible();
        }

        private static bool IsMobileCameraJoystickPlatform()
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return Application.isMobilePlatform;
#endif
        }

        private bool IsGameplaySettingsPanelVisible()
        {
            if (gameplaySettingsVisible)
            {
                return true;
            }

            if (gameplaySettingsOverlayRoot != null && gameplaySettingsOverlayRoot.activeInHierarchy)
            {
                return true;
            }

            return gameplaySettingsPageView != null && gameplaySettingsPageView.gameObject.activeInHierarchy;
        }

        private void EnsureTurnTransitionPopupView()
        {
            if (turnTransitionPopupView == null)
            {
                turnTransitionPopupView = GetComponentInChildren<TurnTransitionPopupView>(true);
            }
        }

        private void EnsureBossHealthBar()
        {
            RebindBossHealthViews();
            if (bossHealthRoot == null)
            {
                CreateRuntimeBossHealthBar();
            }

            ConfigureBossHealthFill();
            RefreshBossHealthLayering();
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

            if (bossHealthMissingImage == null)
            {
                Transform missingFill = FindChildByName(transform, "BossHealthMissingFill");
                if (missingFill == null)
                {
                    missingFill = FindChildByName(transform, "BossHealthBackground");
                }

                if (missingFill != null)
                {
                    bossHealthMissingImage = missingFill.GetComponent<Image>();
                }
            }

            if (bossHealthMissingRect == null && bossHealthMissingImage != null)
            {
                bossHealthMissingRect = bossHealthMissingImage.rectTransform;
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

        private void ConfigureBossHealthFill()
        {
            if (bossHealthFillImage != null)
            {
                bossHealthFillImage.raycastTarget = false;
            }

            if (bossHealthMissingImage != null)
            {
                bossHealthMissingImage.raycastTarget = false;
                bossHealthMissingImage.type = Image.Type.Simple;
            }
        }

        private void CreateRuntimeBossHealthBar()
        {
            GameObject rootObject = new GameObject(
                "BossHealthPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            rootObject.layer = gameObject.layer;
            rootObject.transform.SetParent(transform, false);

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -28f);
            rootRect.sizeDelta = new Vector2(760f, 72f);

            Image background = rootObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.03f, 0.02f, 0.88f);
            background.raycastTarget = false;

            bossHealthRoot = rootObject;
            bossHealthNameText = CreateBossHealthText(rootObject.transform, "BossHealthNameText", "Boss", TextAlignmentOptions.Left);
            bossHealthValueText = CreateBossHealthText(rootObject.transform, "BossHealthValueText", "-- / --", TextAlignmentOptions.Right);
            CreateBossHealthFill(rootObject.transform);
            rootObject.SetActive(false);
            RefreshBossHealthLayering();
        }

        private TMP_Text CreateBossHealthText(Transform parent, string objectName, string text, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = alignment == TextAlignmentOptions.Right ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 0.5f);
            rect.anchorMax = alignment == TextAlignmentOptions.Right ? Vector2.one : new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(18f, -2f);
            rect.offsetMax = new Vector2(-18f, -6f);

            TMP_Text label = textObject.GetComponent<TMP_Text>();
            TMP_Text template = GetComponentInChildren<TMP_Text>(true);
            if (template != null && template.font != null)
            {
                label.font = template.font;
                label.fontSharedMaterial = template.fontSharedMaterial;
            }

            label.text = text;
            label.fontSize = 22f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.richText = true;
            return label;
        }

        private void CreateBossHealthFill(Transform parent)
        {
            GameObject barObject = new GameObject("BossHealthBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barObject.layer = gameObject.layer;
            barObject.transform.SetParent(parent, false);

            RectTransform barRect = barObject.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0.5f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.offsetMin = new Vector2(22f, 12f);
            barRect.offsetMax = new Vector2(-22f, -8f);

            Image barBackground = barObject.GetComponent<Image>();
            barBackground.color = new Color(0.1f, 0.02f, 0.02f, 1f);
            barBackground.raycastTarget = false;

            bossHealthMissingImage = CreateBossHealthFillImage(barObject.transform, "BossHealthMissingFill", new Color(0.22f, 0.02f, 0.02f, 1f), out bossHealthMissingRect);
            bossHealthFillImage = CreateBossHealthFillImage(barObject.transform, "BossHealthFill", new Color(0.86f, 0.03f, 0.02f, 1f), out bossHealthFillRect);
            SetBossHealthFill(1f);
        }

        private Image CreateBossHealthFillImage(Transform parent, string objectName, Color color, out RectTransform rect)
        {
            GameObject fillObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObject.layer = gameObject.layer;
            fillObject.transform.SetParent(parent, false);

            rect = fillObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 4f);
            rect.offsetMax = new Vector2(-4f, -4f);

            Image image = fillObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            image.type = Image.Type.Simple;
            return image;
        }

        private void RefreshBossHealthBar()
        {
            if (bossHealthRoot == null)
            {
                CreateRuntimeBossHealthBar();
            }

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

            SetBossHealthFill(Mathf.Clamp01((float)hp / maxHp));
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
                bossHealthNameText.text = BoneThroneTextUtility.GetUnitDisplayName(boss);
            }

            if (bossHealthValueText != null)
            {
                bossHealthValueText.text = hp + " / " + maxHp;
            }
        }

        private void SetBossHealthFill(float normalizedValue)
        {
            float clampedValue = Mathf.Clamp01(normalizedValue);

            if (bossHealthFillRect != null)
            {
                bossHealthFillRect.anchorMax = new Vector2(clampedValue, bossHealthFillRect.anchorMax.y);
            }

            if (bossHealthMissingRect != null)
            {
                bossHealthMissingRect.anchorMin = new Vector2(clampedValue, bossHealthMissingRect.anchorMin.y);
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

        private static bool HasAnyUnit(Unit[] units)
        {
            if (units == null || units.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static Unit[] FindSceneUnitsByFaction(UnitFaction faction, FindObjectsInactive inactiveMode)
        {
            Unit[] sceneUnits = Object.FindObjectsByType<Unit>(inactiveMode, FindObjectsSortMode.InstanceID);
            if (sceneUnits == null || sceneUnits.Length == 0)
            {
                return new Unit[0];
            }

            int count = 0;
            for (int i = 0; i < sceneUnits.Length; i++)
            {
                Unit unit = sceneUnits[i];
                if (unit != null && unit.Faction == faction)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return new Unit[0];
            }

            Unit[] results = new Unit[count];
            int writeIndex = 0;
            for (int i = 0; i < sceneUnits.Length; i++)
            {
                Unit unit = sceneUnits[i];
                if (unit != null && unit.Faction == faction)
                {
                    results[writeIndex] = unit;
                    writeIndex++;
                }
            }

            return results;
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

            if (bossHealthUnit == null)
            {
                activeBossLookupBuffer.Clear();
                Unit[] sceneUnits = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < sceneUnits.Length; i++)
                {
                    Unit unit = sceneUnits[i];
                    if (unit != null && unit.Faction == UnitFaction.Enemy && unit.gameObject.activeInHierarchy && unit.IsAlive)
                    {
                        activeBossLookupBuffer.Add(unit);
                    }
                }

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
            if (bossHealthRoot == null)
            {
                return;
            }

            bool effectiveVisible = visible && !IsGameplaySettingsPanelVisible();
            if (bossHealthRoot.activeSelf != effectiveVisible)
            {
                bossHealthRoot.SetActive(effectiveVisible);
            }
        }

        private void RefreshBossHealthLayering()
        {
            if (bossHealthRoot == null || gameplaySettingsOverlayRoot == null)
            {
                return;
            }

            Transform bossTransform = bossHealthRoot.transform;
            Transform settingsTransform = gameplaySettingsOverlayRoot.transform;
            if (bossTransform.parent != settingsTransform.parent)
            {
                return;
            }

            int settingsIndex = settingsTransform.GetSiblingIndex();
            int bossIndex = bossTransform.GetSiblingIndex();
            if (bossIndex > settingsIndex)
            {
                bossTransform.SetSiblingIndex(settingsIndex);
            }
        }

        private bool ShouldExposeBossFightRuntime()
        {
            if (BossEncounterIntroController.HasRevealedActiveBossEncounter())
            {
                return true;
            }

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

    internal sealed class MobileCameraJoystickGraphic : MaskableGraphic
    {
        private const int SegmentCount = 56;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (radius <= 0f)
            {
                return;
            }

            Vector2 center = rect.center;
            Color32 ringColor = color;
            AddRing(vertexHelper, center, radius, radius * 0.78f, ringColor);

            Color knobColor = color;
            knobColor.a *= 0.75f;
            AddFilledCircle(vertexHelper, center, radius * 0.14f, knobColor);
        }

        private static void AddRing(VertexHelper vertexHelper, Vector2 center, float outerRadius, float innerRadius, Color32 vertexColor)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                float angle0 = Mathf.PI * 2f * i / SegmentCount;
                float angle1 = Mathf.PI * 2f * (i + 1) / SegmentCount;
                Vector2 outer0 = center + new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * outerRadius;
                Vector2 outer1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * outerRadius;
                Vector2 inner0 = center + new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * innerRadius;
                Vector2 inner1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;

                int startIndex = vertexHelper.currentVertCount;
                AddVertex(vertexHelper, outer0, vertexColor);
                AddVertex(vertexHelper, outer1, vertexColor);
                AddVertex(vertexHelper, inner1, vertexColor);
                AddVertex(vertexHelper, inner0, vertexColor);
                vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
            }
        }

        private static void AddFilledCircle(VertexHelper vertexHelper, Vector2 center, float radius, Color vertexColor)
        {
            int centerIndex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, center, vertexColor);

            for (int i = 0; i <= SegmentCount; i++)
            {
                float angle = Mathf.PI * 2f * i / SegmentCount;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                AddVertex(vertexHelper, point, vertexColor);
            }

            for (int i = 1; i <= SegmentCount; i++)
            {
                vertexHelper.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color32 vertexColor)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = vertexColor;
            vertexHelper.AddVert(vertex);
        }
    }
}
