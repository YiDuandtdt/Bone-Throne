using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Skills;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoneThrone.UI
{
    /// <summary>
    /// Lightweight UI bridge for battle action targeting. It stores UI intent and calls existing gameplay APIs.
    /// </summary>
    public sealed class UIActionModeController : MonoBehaviour
    {
        private enum ActionMode
        {
            None,
            MoveTargeting,
            BasicAttackTargeting,
            SkillTargeting
        }

        [SerializeField] private SelectionManager selectionManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private Unit[] enemyUnits;
        [SerializeField] private PromptView promptView;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask targetLayerMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;
        [SerializeField] private PlayerMovementController movementControllerToSuspend;
        [SerializeField] private MovementDebugHighlighter highlighter;
        [SerializeField] private ActiveUnitProvider activeUnitProvider;

        private ActionMode currentMode = ActionMode.None;
        private int pendingSkillSlotIndex = -1;
        private readonly HashSet<GridPosition> currentMoveTargets = new HashSet<GridPosition>();
        private bool movementControllerWasEnabled;
        private bool movementInputSuspended;
        private Unit lastHighlightedUnit;
        private Tile lastHighlightedTile;

        public bool IsTargeting
        {
            get { return currentMode != ActionMode.None; }
        }

        public void Configure(
            SelectionManager selection,
            GridManager grid,
            CombatSystem combat,
            SkillSystem skills,
            Unit[] enemies,
            PromptView prompt,
            Camera camera,
            LayerMask layerMask,
            PlayerMovementController movementController,
            MovementDebugHighlighter tileHighlighter,
            ActiveUnitProvider unitProvider = null)
        {
            selectionManager = selection;
            gridManager = grid;
            combatSystem = combat;
            skillSystem = skills;
            enemyUnits = enemies;
            promptView = prompt;
            inputCamera = camera;
            targetLayerMask = layerMask;
            movementControllerToSuspend = movementController;
            highlighter = tileHighlighter;
            activeUnitProvider = unitProvider;
        }

        private void OnDisable()
        {
            ExitTargetingMode();
        }

        private void Update()
        {
            RefreshSelectedHighlight();

            if (currentMode != ActionMode.None && (selectionManager == null || selectionManager.SelectedUnit == null))
            {
                ExitTargetingMode();
                ShowPrompt("Select a unit first.", 1.5f);
                return;
            }

            if (currentMode == ActionMode.None)
            {
                return;
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelTargeting();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            HandleTargetClick();
        }

        public void HandleMoveButtonClicked()
        {
            if (currentMode == ActionMode.MoveTargeting)
            {
                CancelTargeting();
                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("Select a unit first.");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("Selected unit is dead.");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasMoved)
            {
                ShowPrompt("Selected unit has already moved.");
                return;
            }

            if (movementControllerToSuspend == null)
            {
                ShowPrompt("Move unavailable: movement controller unbound.");
                return;
            }

            if (gridManager == null)
            {
                ShowPrompt("Move unavailable: GridManager unbound.");
                return;
            }

            EnterMoveTargeting();
        }

        public void HandleBasicAttackButtonClicked()
        {
            if (currentMode == ActionMode.BasicAttackTargeting)
            {
                CancelTargeting();
                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("Select a unit first.");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("Selected unit is dead.");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasActed)
            {
                ShowPrompt("Selected unit has already acted.");
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("Basic attack unavailable: CombatSystem unbound.");
                return;
            }

            EnterBasicAttackTargeting();
        }

        public void HandleSkillSlot0ButtonClicked()
        {
            HandleSkillSlotButtonClicked(0);
        }

        public void HandleSkillSlotButtonClicked(int slotIndex)
        {
            if (currentMode == ActionMode.SkillTargeting && pendingSkillSlotIndex == slotIndex)
            {
                CancelTargeting();
                return;
            }

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("Select a unit first.");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("Selected unit is dead.");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasActed)
            {
                ShowPrompt("Selected unit has already acted.");
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                ShowPrompt("No SkillRuntime on selected unit.");
                return;
            }

            if (!runtime.HasSkill(slotIndex))
            {
                ShowPrompt("No skill in slot " + slotIndex + ".");
                return;
            }

            if (!runtime.IsUnlocked(selectedUnit, slotIndex))
            {
                ShowPrompt("Skill locked.");
                return;
            }

            if (runtime.IsOnCooldown(slotIndex))
            {
                ShowPrompt("Skill on cooldown.");
                return;
            }

            if (skillSystem == null)
            {
                ShowPrompt("Skill unavailable: SkillSystem unbound.");
                return;
            }

            EnterSkillTargeting(slotIndex);
        }

        private void EnterMoveTargeting()
        {
            ExitTargetingMode();
            currentMode = ActionMode.MoveTargeting;
            currentMoveTargets.Clear();

            HashSet<GridPosition> positions = movementControllerToSuspend.GetReachablePositionsForSelected();
            foreach (GridPosition position in positions)
            {
                currentMoveTargets.Add(position);
            }

            if (highlighter != null)
            {
                highlighter.ShowMoveRange(gridManager, currentMoveTargets);
            }

            SuspendMovementInput();
            ShowPrompt("Select a move tile.");
        }

        private void EnterBasicAttackTargeting()
        {
            ExitTargetingMode();
            currentMode = ActionMode.BasicAttackTargeting;
            ShowBasicAttackTargets();
            SuspendMovementInput();
            ShowPrompt("Select an enemy target.");
        }

        private void EnterSkillTargeting(int slotIndex)
        {
            ExitTargetingMode();
            currentMode = ActionMode.SkillTargeting;
            pendingSkillSlotIndex = slotIndex;
            ShowSkillTargets(slotIndex);
            SuspendMovementInput();
            ShowPrompt("Select a skill target.");
        }

        private void HandleTargetClick()
        {
            Unit clickedUnit = RaycastUnitUnderCursor();
            if (IsCurrentSelectedUnit(clickedUnit))
            {
                ClearSelectionAndExitModes();
                return;
            }

            if (currentMode == ActionMode.MoveTargeting)
            {
                HandleMoveTargetClick(RaycastTileUnderCursor());
                return;
            }

            if (currentMode == ActionMode.BasicAttackTargeting)
            {
                HandleBasicAttackTargetClick(clickedUnit);
                return;
            }

            if (currentMode == ActionMode.SkillTargeting)
            {
                HandleSkillTargetClick(clickedUnit);
            }
        }

        private void HandleMoveTargetClick(Tile tile)
        {
            if (tile == null || !currentMoveTargets.Contains(tile.Position))
            {
                ShowPrompt("Invalid move target.");
                return;
            }

            if (movementControllerToSuspend == null)
            {
                ShowPrompt("Move unavailable: movement controller unbound.");
                return;
            }

            bool success = movementControllerToSuspend.TryMoveSelectedUnitTo(tile);
            if (success)
            {
                ExitTargetingMode();
                RefreshSelectedHighlight();
                ClearPrompt();
                return;
            }

            ShowPrompt("Invalid move target.");
        }

        private void HandleBasicAttackTargetClick(Unit target)
        {
            if (target == null || target.Faction != UnitFaction.Enemy)
            {
                ShowPrompt("Invalid attack target.");
                return;
            }

            Unit attacker = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (attacker == null)
            {
                ShowPrompt("Select a unit first.");
                ExitTargetingMode();
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("Basic attack unavailable: CombatSystem unbound.");
                return;
            }

            bool success = combatSystem.TryBasicAttack(attacker, target);
            if (success)
            {
                ExitTargetingMode();
                ClearPrompt();
                return;
            }

            ShowPrompt("Invalid attack target.");
        }

        private void HandleSkillTargetClick(Unit target)
        {
            if (target == null)
            {
                ShowPrompt("Invalid skill target.");
                return;
            }

            Unit caster = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (caster == null)
            {
                ShowPrompt("Select a unit first.");
                ExitTargetingMode();
                return;
            }

            if (skillSystem == null)
            {
                ShowPrompt("Skill unavailable: SkillSystem unbound.");
                return;
            }

            bool success = skillSystem.TryUseSkill(caster, target, pendingSkillSlotIndex);
            if (success)
            {
                ExitTargetingMode();
                ClearPrompt();
                return;
            }

            ShowPrompt("Invalid skill target.");
        }

        private void ShowBasicAttackTargets()
        {
            if (highlighter == null)
            {
                return;
            }

            Unit attacker = selectionManager != null ? selectionManager.SelectedUnit : null;
            Unit[] enemies = GetEnemyTargetsForPreview();
            if (attacker == null || combatSystem == null || enemies == null)
            {
                highlighter.ClearActionHighlights();
                return;
            }

            List<Tile> targetTiles = new List<Tile>();
            for (int i = 0; i < enemies.Length; i++)
            {
                Unit enemy = enemies[i];
                string reason;
                if (enemy != null
                    && enemy.gameObject.activeInHierarchy
                    && enemy.IsAlive
                    && enemy.CurrentTile != null
                    && combatSystem.CanBasicAttack(attacker, enemy, out reason))
                {
                    targetTiles.Add(enemy.CurrentTile);
                }
            }

            highlighter.ShowAttackTargets(targetTiles);
        }

        private void ShowSkillTargets(int slotIndex)
        {
            if (highlighter == null)
            {
                return;
            }

            Unit caster = selectionManager != null ? selectionManager.SelectedUnit : null;
            Unit[] enemies = GetEnemyTargetsForPreview();
            if (caster == null || skillSystem == null || enemies == null)
            {
                highlighter.ClearActionHighlights();
                return;
            }

            List<Tile> targetTiles = new List<Tile>();
            for (int i = 0; i < enemies.Length; i++)
            {
                Unit enemy = enemies[i];
                string reason;
                if (enemy != null
                    && enemy.gameObject.activeInHierarchy
                    && enemy.IsAlive
                    && enemy.CurrentTile != null
                    && skillSystem.CanUseSkillOnTarget(caster, enemy, slotIndex, out reason))
                {
                    targetTiles.Add(enemy.CurrentTile);
                }
            }

            highlighter.ShowSkillTargets(targetTiles);
        }

        private Unit[] GetEnemyTargetsForPreview()
        {
            ActiveUnitProvider provider = ResolveActiveUnitProvider();
            Unit[] activeEnemies = provider != null ? provider.GetActiveAliveEnemies() : null;
            if (activeEnemies != null && activeEnemies.Length > 0)
            {
                return activeEnemies;
            }

            return enemyUnits;
        }

        private ActiveUnitProvider ResolveActiveUnitProvider()
        {
            if (activeUnitProvider == null)
            {
                activeUnitProvider = Object.FindFirstObjectByType<ActiveUnitProvider>();
            }

            return activeUnitProvider;
        }

        private bool IsCurrentSelectedUnit(Unit unit)
        {
            return unit != null && selectionManager != null && unit == selectionManager.SelectedUnit;
        }

        private void ClearSelectionAndExitModes()
        {
            if (selectionManager != null)
            {
                selectionManager.ClearSelection();
            }

            ExitTargetingMode();
            if (highlighter != null)
            {
                highlighter.Clear();
            }

            ShowPrompt("Free Select.", 1.5f);
        }

        private Unit RaycastUnitUnderCursor()
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                ShowPrompt("Basic attack unavailable: Camera unbound.");
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxRayDistance, targetLayerMask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<Unit>();
        }

        private Tile RaycastTileUnderCursor()
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                ShowPrompt("Move unavailable: Camera unbound.");
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxRayDistance, targetLayerMask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<Tile>();
        }

        private void CancelTargeting()
        {
            ActionMode canceledMode = currentMode;
            ExitTargetingMode();
            string message = canceledMode == ActionMode.MoveTargeting
                ? "Move canceled."
                : canceledMode == ActionMode.SkillTargeting ? "Skill targeting canceled." : "Basic attack canceled.";
            ShowPrompt(message, 1.5f);
        }

        private void ExitTargetingMode()
        {
            currentMode = ActionMode.None;
            pendingSkillSlotIndex = -1;
            currentMoveTargets.Clear();
            if (highlighter != null)
            {
                highlighter.ClearActionHighlights();
            }

            RestoreMovementInput();
        }

        private void RefreshSelectedHighlight()
        {
            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            Tile selectedTile = selectedUnit != null ? selectedUnit.CurrentTile : null;
            if (selectedUnit == lastHighlightedUnit && selectedTile == lastHighlightedTile)
            {
                return;
            }

            lastHighlightedUnit = selectedUnit;
            lastHighlightedTile = selectedTile;

            if (highlighter == null)
            {
                return;
            }

            if (selectedTile == null)
            {
                highlighter.ClearSelected();
                return;
            }

            highlighter.ShowSelected(selectedTile);
        }

        private void SuspendMovementInput()
        {
            if (movementControllerToSuspend == null)
            {
                return;
            }

            movementControllerWasEnabled = movementControllerToSuspend.enabled;
            movementControllerToSuspend.enabled = false;
            movementInputSuspended = true;
        }

        private void RestoreMovementInput()
        {
            if (!movementInputSuspended || movementControllerToSuspend == null)
            {
                return;
            }

            movementControllerToSuspend.enabled = movementControllerWasEnabled;
            movementInputSuspended = false;
        }

        private void ShowPrompt(string message)
        {
            ShowPrompt(message, 0f);
        }

        private void ShowPrompt(string message, float duration)
        {
            if (promptView != null)
            {
                promptView.ShowOverride(message, duration);
            }
        }

        private void ClearPrompt()
        {
            if (promptView != null)
            {
                promptView.ClearOverride();
            }
        }
    }
}
