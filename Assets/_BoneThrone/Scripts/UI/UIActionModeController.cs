using System.Collections.Generic;
using BoneThrone.Combat;
using BoneThrone.Grid;
using BoneThrone.Movement;
using BoneThrone.Skills;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

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

        public void CancelTargetingForExternalAction()
        {
            if (currentMode == ActionMode.None)
            {
                return;
            }

            ExitTargetingMode();
            ClearPrompt();
        }

        private void Update()
        {
            RefreshSelectedHighlight();

            if (currentMode != ActionMode.None && (selectionManager == null || selectionManager.SelectedUnit == null))
            {
                ExitTargetingMode();
                ShowPrompt("请先选择一名玩家角色。", 1.5f);
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

            Vector2 pointerPosition;
            if (!BTPrimaryPointerInput.TryGetPrimaryClick(out pointerPosition))
            {
                return;
            }

            if (BTPrimaryPointerInput.IsPointerOverUi(pointerPosition))
            {
                return;
            }

            HandleTargetClick(pointerPosition);
        }

        public void HandleMoveButtonClicked()
        {
            if (currentMode == ActionMode.MoveTargeting)
            {
                CancelTargeting();
                return;
            }

            ExitCurrentTargetingForModeSwitch();

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("请先选择一名玩家角色。");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("当前角色已经倒下。");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasEnded)
            {
                ShowPrompt("当前角色本回合已经结束。");
                return;
            }

            if (turnState != null && turnState.HasMoved)
            {
                ShowPrompt("当前角色本回合已经移动过。");
                return;
            }

            if (movementControllerToSuspend == null)
            {
                ShowPrompt("移动暂不可用：移动控制器未绑定。");
                return;
            }

            if (gridManager == null)
            {
                ShowPrompt("移动暂不可用：网格系统未绑定。");
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

            ExitCurrentTargetingForModeSwitch();

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("请先选择一名玩家角色。");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("当前角色已经倒下。");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasEnded)
            {
                ShowPrompt("当前角色本回合已经结束。");
                return;
            }

            if (turnState != null && turnState.HasActed)
            {
                ShowPrompt("当前角色本回合已经行动过。");
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("普通攻击暂不可用：战斗系统未绑定。");
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

            ExitCurrentTargetingForModeSwitch();

            Unit selectedUnit = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (selectedUnit == null)
            {
                ShowPrompt("请先选择一名玩家角色。");
                return;
            }

            if (!selectedUnit.IsAlive)
            {
                ShowPrompt("当前角色已经倒下。");
                return;
            }

            UnitTurnState turnState = selectedUnit.GetComponent<UnitTurnState>();
            if (turnState != null && turnState.HasEnded)
            {
                ShowPrompt("当前角色本回合已经结束。");
                return;
            }

            if (turnState != null && turnState.HasActed)
            {
                ShowPrompt("当前角色本回合已经行动过。");
                return;
            }

            SkillRuntime runtime = selectedUnit.GetComponent<SkillRuntime>();
            if (runtime == null)
            {
                ShowPrompt("当前角色没有技能组件。");
                return;
            }

            if (!runtime.HasSkill(slotIndex))
            {
                ShowPrompt("第 " + (slotIndex + 1) + " 个技能槽没有技能。");
                return;
            }

            if (!runtime.IsUnlocked(selectedUnit, slotIndex))
            {
                ShowPrompt("技能尚未解锁。");
                return;
            }

            if (runtime.IsOnCooldown(slotIndex))
            {
                ShowPrompt("技能正在冷却。");
                return;
            }

            if (skillSystem == null)
            {
                ShowPrompt("技能暂不可用：技能系统未绑定。");
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
            ShowPrompt("请选择一个移动目标格。");
        }

        private void EnterBasicAttackTargeting()
        {
            ExitTargetingMode();
            currentMode = ActionMode.BasicAttackTargeting;
            ShowBasicAttackTargets();
            SuspendMovementInput();
            ShowPrompt("请选择一个敌方目标。");
        }

        private void EnterSkillTargeting(int slotIndex)
        {
            ExitTargetingMode();
            currentMode = ActionMode.SkillTargeting;
            pendingSkillSlotIndex = slotIndex;
            ShowSkillTargets(slotIndex);
            SuspendMovementInput();
            ShowPrompt("请选择一个技能目标。");
        }

        private void HandleTargetClick(Vector2 pointerPosition)
        {
            if (currentMode == ActionMode.MoveTargeting)
            {
                HandleMoveTargetClick(RaycastTileAtScreenPosition(pointerPosition));
                return;
            }

            Unit clickedUnit = RaycastTargetUnitAtScreenPosition(pointerPosition);
            if (IsCurrentSelectedUnit(clickedUnit))
            {
                ClearSelectionAndExitModes();
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
                ShowPrompt("无效移动目标。");
                return;
            }

            if (movementControllerToSuspend == null)
            {
                ShowPrompt("移动暂不可用：移动控制器未绑定。");
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

            ShowPrompt("无效移动目标。");
        }

        private void HandleBasicAttackTargetClick(Unit target)
        {
            if (target == null || target.Faction != UnitFaction.Enemy)
            {
                ShowPrompt("无效攻击目标。");
                return;
            }

            Unit attacker = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (attacker == null)
            {
                ShowPrompt("请先选择一名玩家角色。");
                ExitTargetingMode();
                return;
            }

            if (combatSystem == null)
            {
                ShowPrompt("普通攻击暂不可用：战斗系统未绑定。");
                return;
            }

            bool success = combatSystem.TryBasicAttack(attacker, target);
            if (success)
            {
                ExitTargetingMode();
                ClearPrompt();
                return;
            }

            ShowPrompt("无效攻击目标。");
        }

        private void HandleSkillTargetClick(Unit target)
        {
            if (target == null)
            {
                ShowPrompt("无效技能目标。");
                return;
            }

            Unit caster = selectionManager != null ? selectionManager.SelectedUnit : null;
            if (caster == null)
            {
                ShowPrompt("请先选择一名玩家角色。");
                ExitTargetingMode();
                return;
            }

            if (skillSystem == null)
            {
                ShowPrompt("技能暂不可用：技能系统未绑定。");
                return;
            }

            bool success = skillSystem.TryUseSkill(caster, target, pendingSkillSlotIndex);
            if (success)
            {
                ExitTargetingMode();
                ClearPrompt();
                return;
            }

            ShowPrompt("无效技能目标。");
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

            List<Tile> rangeTiles = BuildManhattanRangeTiles(attacker, GetBasicAttackRange(attacker));
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

            highlighter.ShowAttackRangeAndTargets(rangeTiles, targetTiles);
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

            List<Tile> rangeTiles = new List<Tile>();
            string rangeReason;
            if (!skillSystem.TryFillSkillRangeTiles(caster, slotIndex, gridManager, rangeTiles, out rangeReason))
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

            highlighter.ShowSkillRangeAndTargets(rangeTiles, targetTiles);
        }

        private List<Tile> BuildManhattanRangeTiles(Unit originUnit, int range)
        {
            List<Tile> rangeTiles = new List<Tile>();
            if (gridManager == null || originUnit == null || originUnit.CurrentTile == null)
            {
                return rangeTiles;
            }

            int clampedRange = Mathf.Max(0, range);
            List<Tile> registeredTiles = new List<Tile>();
            gridManager.FillRegisteredTiles(registeredTiles);
            GridPosition origin = originUnit.CurrentTile.Position;
            for (int i = 0; i < registeredTiles.Count; i++)
            {
                Tile tile = registeredTiles[i];
                if (tile == null || tile == originUnit.CurrentTile)
                {
                    continue;
                }

                GridPosition position = tile.Position;
                int distance = Mathf.Abs(position.X - origin.X) + Mathf.Abs(position.Y - origin.Y);
                if (distance <= clampedRange)
                {
                    rangeTiles.Add(tile);
                }
            }

            return rangeTiles;
        }

        private static int GetBasicAttackRange(Unit attacker)
        {
            return attacker != null && attacker.Stats != null ? attacker.Stats.BasicAttackRange : 1;
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

            ShowPrompt("自由选择。", 1.5f);
        }

        private Unit RaycastUnitAtScreenPosition(Vector2 screenPosition)
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                ShowPrompt("普通攻击暂不可用：摄像机未绑定。");
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, targetLayerMask, QueryTriggerInteraction.Ignore);
            SortHitsByDistance(hits);
            for (int i = 0; i < hits.Length; i++)
            {
                Unit unit = hits[i].collider != null ? hits[i].collider.GetComponentInParent<Unit>() : null;
                if (unit != null)
                {
                    return unit;
                }
            }

            return null;
        }

        private Unit RaycastTargetUnitAtScreenPosition(Vector2 screenPosition)
        {
            Tile tile = RaycastTileAtScreenPosition(screenPosition);
            Unit unitOnTile = FindUnitOnTile(tile);
            if (unitOnTile != null)
            {
                return unitOnTile;
            }

            return RaycastUnitAtScreenPosition(screenPosition);
        }

        private Tile RaycastTileAtScreenPosition(Vector2 screenPosition)
        {
            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                ShowPrompt("移动暂不可用：摄像机未绑定。");
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, targetLayerMask, QueryTriggerInteraction.Ignore);
            SortHitsByDistance(hits);
            for (int i = 0; i < hits.Length; i++)
            {
                Tile tile = hits[i].collider != null ? hits[i].collider.GetComponentInParent<Tile>() : null;
                if (tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        private Unit FindUnitOnTile(Tile tile)
        {
            if (tile == null)
            {
                return null;
            }

            ActiveUnitProvider provider = ResolveActiveUnitProvider();
            Unit[] activeUnits = provider != null ? provider.GetActiveAliveUnits() : null;
            Unit found = FindUnitOnTile(tile, activeUnits);
            if (found != null)
            {
                return found;
            }

            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            return FindUnitOnTile(tile, units);
        }

        private static Unit FindUnitOnTile(Tile tile, Unit[] units)
        {
            if (tile == null || units == null)
            {
                return null;
            }

            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit != null && unit.IsAlive && unit.CurrentTile == tile)
                {
                    return unit;
                }
            }

            return null;
        }

        private static void SortHitsByDistance(RaycastHit[] hits)
        {
            if (hits == null || hits.Length < 2)
            {
                return;
            }

            System.Array.Sort(hits, CompareRaycastHitsByDistance);
        }

        private static int CompareRaycastHitsByDistance(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }

        private void CancelTargeting()
        {
            ActionMode canceledMode = currentMode;
            ExitTargetingMode();
            string message = canceledMode == ActionMode.MoveTargeting
                ? "移动已取消。"
                : canceledMode == ActionMode.SkillTargeting ? "技能目标选择已取消。" : "普通攻击已取消。";
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

        private void ExitCurrentTargetingForModeSwitch()
        {
            if (currentMode == ActionMode.None)
            {
                return;
            }

            ExitTargetingMode();
            ClearPrompt();
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
