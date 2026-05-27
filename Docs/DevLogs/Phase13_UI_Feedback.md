# Phase 13 DevLog - UI and Feedback

## Date
2026-05-25

## Scope
Implemented the first small Phase 13 battle HUD slice only. This change adds screen-space uGUI + TextMeshPro feedback for the existing single-player test flow. It does not add networking, world-space health bars, sound, VFX, Animator Controllers, formal icons, portraits, gameplay formula changes, or prefab visual changes.

## Added UI scripts
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
  - Coordinates the first battle HUD layer.
  - Reads `TurnManager`, `SelectionManager`, `LevelProgressionService`, `CombatLog`, and the configured player `Unit` references.
  - Builds a simple runtime TMP/uGUI layout if view references are missing.
  - Does not call movement, combat, skill, AI, key, stairs, level, or networking actions.
- `Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`
  - Displays current turn phase, current role, and turn index.
- `Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`
  - Displays player unit name, HP, level, moved/acted state, and alive/dead state.
- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
  - Displays Basic Attack, skill slot 0 state, and disabled placeholder slots.
  - Buttons are disabled and display-only in this first slice.
- `Assets/_BoneThrone/Scripts/UI/CombatFeedbackView.cs`
  - Displays recent `CombatLog` entries.
- `Assets/_BoneThrone/Scripts/UI/PromptView.cs`
  - Displays selected-unit, key/progression, and stairs confirmation prompts.

## BattleHUD prefab
- Added `Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab`.
- Prefab hierarchy:
  - `BattleHUD`
    - `RectTransform`
    - `BattleHUDController`
- The prefab intentionally keeps author-time hierarchy minimal.
- Runtime-generated child hierarchy:
  - `TurnBanner`
  - `SelectedUnitText`
  - `HeroPanel_0`
  - `HeroPanel_1`
  - `HeroPanel_2`
  - `HeroPanel_3`
  - `SkillBar`
    - `BasicAttack`
    - `SkillSlot0`
    - `SkillSlot1`
    - `SkillSlot2`
    - `Defend`
    - `Potion`
  - `CombatFeedback`
  - `Prompt`
- Runtime text uses `TextMeshProUGUI`; background panels and disabled display buttons use uGUI.
- Non-interactive generated text and panel images set `raycastTarget=false`.

## CombatLog feedback bridge
- Updated `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`.
- Added:
  - `CombatLog.Entry`
  - `CombatLog.EntryType`
  - `EntryAdded` event
  - `RecentEntries` cache
- Original `Debug.Log` and `Debug.LogWarning` behavior is preserved.
- Combat resolution, D20 formula, damage, death, action consumption, and targeting logic were not changed.

## GridTest scene changes
- Updated `Assets/_BoneThrone/Scenes/GridTest.unity`.
- Added a `BattleHUD.prefab` instance under the existing `Canvas`.
- Set the existing empty `Canvas` RectTransform scale to `{x:1,y:1,z:1}` so the HUD can render.
- Bound `BattleHUDController` scene references:
  - `turnManager` -> existing `TurnManager`
  - `selectionManager` -> existing `SelectionManager`
  - `progressionService` -> existing `LevelProgressionService`
  - `combatLog` -> existing `CombatLog`
  - `playerUnits[0]` -> Fighter
  - `playerUnits[1]` -> Ranger
  - `playerUnits[2]` -> Mage
  - `playerUnits[3]` -> Barbarian
- `stairs` is intentionally left unbound because the scene stairs object is a prefab instance component and should be checked in Unity Inspector before binding. The prompt still shows key/progression reasons through `LevelProgressionService`.
- No gameplay objects were moved, deleted, renamed, or visually changed.

## Placeholders
- Basic Attack is display-only.
- Skill slot 0 is display-only.
- Slot 1, Slot 2, Defend, and Potion are disabled placeholders.
- No FloatingHealthBar or world-space HP bar was added.
- No formal icons, portraits, sound, VFX, or animation controller was added.

## Manual Unity 6.3 test steps
1. Open the project in Unity 6.3 LTS.
2. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
3. Wait for compile/import to finish and confirm there are no red Console errors.
4. Enter Play Mode.
5. Confirm the battle HUD appears on the existing Canvas.
6. Confirm the top banner shows turn phase, current role, and turn index.
7. Click player units and confirm the selected-unit text and SkillBar update.
8. Confirm the four player panels show name, HP, level, moved/acted, and alive/dead state.
9. Use the existing movement flow and confirm the HUD does not block tile clicks.
10. Run the existing basic attack test and confirm CombatLog entries appear in the HUD.
11. Run the existing skill test and confirm HP/action/cooldown display updates.
12. Test key/stairs progression and confirm prompt text reports missing key or progression state.
13. Run movement, D20 attack, representative skill, enemy AI, key, stairs, transition, and upgrade regression checks.
14. Confirm Console has no red errors.

## Known risks
- Unity may need to reserialize the new prefab instance after first open.
- If TMP package resources are missing, runtime-generated `TextMeshProUGUI` text may need TMP Essentials imported.
- The `stairs` reference is now bound in GridTest after Unity reserialized the prefab instance; verify it in Inspector if stairs prompts do not appear.
- UI placement is first-pass prototype layout and may need visual adjustment after Play Mode.
- The existing `GraphicRaycaster` remains on Canvas; generated display text/images disable raycast targets, but future interactive HUD work must keep tile-click blocking in mind.

## First fix patch notes
- Increased the runtime HUD text sizes and panel dimensions for clearer Game view readability.
- Normalized the BattleHUD root and generated UI node RectTransform scales to `{x:1,y:1,z:1}`.
- Switched the GridTest CanvasScaler to a 1920x1080 reference with width/height matching so the HUD is less blurry in typical full-HD Game views.
- Replaced broad `N/A` display text with clearer fallbacks:
  - Missing hero binding: `Hero: Unbound`
  - Missing HP: `HP: -- / --`
  - Missing moved/acted state: `Moved: -- | Acted: --`
  - Empty combat log: `Combat Log: No combat yet`
  - Missing stairs binding: `Stairs: Unbound`
- Added a player-unit fallback path in `BattleHUDController`: if the local `playerUnits` array slot is empty, the HUD reads `LevelProgressionService.PlayerUnits` for that slot.
- Bound the GridTest HUD stairs reference after Unity reserialized the prefab instance, so stairs confirmation can be displayed once that interaction is tested.
- Still not added: FloatingHealthBar, world-space HP bars, icons, portraits, sound, VFX, Animator Controllers, networking, or gameplay formula changes.
- Follow-up tests still needed from Play Mode: D20 CombatLog, Key, Stairs confirmation/transition, and Enemy AI regression.

## HUD polish patch notes
- Updated `TurnBannerView` fallback display only.
- Player turn with `TurnManager.CurrentRole=None` now shows `Turn: Player Turn | Actor: Free Select` instead of `Role: None`.
- The banner no longer displays `Index: 0`, because the current local single-player tester flow does not expose a reliable HUD-facing turn index.
- Enemy phase displays `Turn: Enemy Turn`.
- Unknown or unstarted phase displays `Turn: -- | Actor: --`.
- SkillBar remains display-only/prototype; it does not trigger attacks, skills, items, or defense.
- Basic attack, skills, and Enemy AI are still validated through the existing tester, ContextMenu, and test-entry flows for this phase.
- No formal modal confirmation box was added; stairs continue to use the existing prompt/log/confirmation-pending behavior.
- Still not added: FloatingHealthBar, world-space HP bars, formal icons, portraits, sound, VFX, Animator Controllers, networking, or gameplay formula changes.

## Phase 13.3 Basic Attack targeting patch notes
- Added `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
  - Stores the current UI action mode: `None` or `BasicAttackTargeting`.
  - Receives the Basic Attack button click from `SkillBarView`.
  - Reads `SelectionManager.SelectedUnit`.
  - Uses the configured camera and Physics Raycast to find the clicked `Unit`.
  - Calls only `CombatSystem.TryBasicAttack(attacker, target)` for execution.
  - Does not directly change HP, cooldown, `HasActed`, D20 rolls, damage, death, AI, room, key, stairs, or level state.
- Updated `SkillBarView`.
  - Basic Attack is now clickable and emits a UI intent event.
  - Skill Slot 0 remains display-only and does not trigger skill gameplay.
  - Slot 1, Slot 2, Defend, and Potion remain disabled placeholders.
- Updated `BattleHUDController`.
  - Wires `SkillBarView` to `UIActionModeController`.
  - Passes Inspector-configured `CombatSystem`, input camera, target layer mask, and `PlayerMovementController` reference into the action mode controller.
- Updated `PromptView`.
  - Added lightweight override prompts for targeting, invalid target, cancel, and missing binding states.
- Updated `BattleHUD.prefab`.
  - Added `UIActionModeController` to the root `BattleHUD` object.
  - Kept the author-time hierarchy minimal; child HUD layout is still runtime-generated.
- Updated `GridTest.unity`.
  - Bound the HUD `combatSystem` reference to the existing scene `CombatSystem`.
  - Bound the HUD `actionInputCamera` reference to the existing scene camera.
  - Bound the HUD `movementControllerToSuspend` reference to the existing `PlayerMovementController`.
  - Existing gameplay objects, player/enemy visuals, key, stairs, AI, room, and level objects were not moved, renamed, deleted, or visually changed.
- Targeting input conflict handling:
  - Entering Basic Attack targeting temporarily disables the bound `PlayerMovementController`.
  - Success, cancel, or controller disable restores the movement controller to its prior enabled state.
  - This prevents targeting-mode clicks on empty tiles or friendly units from triggering movement.
- Cancel options:
  - Right mouse button.
  - Escape.
  - Clicking Basic Attack again while targeting.
- Still not added:
  - Skill Slot 0 targeting.
  - Defend or Potion actions.
  - Formal modal confirmation boxes.
  - FloatingHealthBar or world-space HP bars.
  - Sound, VFX, Animator Controllers, formal icons, portraits, or networking.

## Phase 13.3 manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. With no selected unit, click `Basic Attack`; expected Prompt: `Select a unit first.`
4. Select a living player unit that has not acted.
5. Click `Basic Attack`; expected Prompt: `Select an enemy target.`
6. While targeting, click empty ground; expected Prompt: `Invalid attack target.`, no movement, no action consumption.
7. While targeting, click a friendly unit; expected Prompt: `Invalid attack target.`, no movement, no action consumption.
8. While targeting, click an enemy unit in range; expected `CombatSystem.TryBasicAttack` runs, CombatLog shows D20 feedback, HUD refreshes HP/action state, movement input is restored.
9. Repeat with an out-of-range enemy; expected rejected CombatLog/prompt feedback, no action consumption, targeting remains active.
10. Enter targeting again and cancel with right-click, Escape, and clicking `Basic Attack` again; expected no action consumption and movement input restored.
11. After cancel or successful attack, confirm normal tile movement still works.
12. Re-run Key/Stairs, Skill Slot 0 display-only, and Enemy AI regression checks.

## Phase 13.4 Skill Slot 0 targeting patch notes
- Extended `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
  - Added `SkillTargeting` alongside `BasicAttackTargeting`.
  - Added an Inspector-configurable `SkillSystem` reference.
  - Stores `pendingSkillSlotIndex`, currently only slot `0`.
  - Skill targeting uses the same camera, Physics Raycast, and movement-input suspend/restore path as Basic Attack targeting.
  - `HandleTargetClick` now dispatches by mode:
    - `BasicAttackTargeting` calls only `CombatSystem.TryBasicAttack(attacker, target)`.
    - `SkillTargeting` calls only `SkillSystem.TryUseSkill(selectedUnit, target, 0)`.
  - UI still does not directly change HP, cooldowns, `HasActed`, formulas, death, AI, room, key, stairs, level, or networking state.
- Updated `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`.
  - Skill Slot 0 is now clickable and emits a UI intent event.
  - Basic Attack click behavior is preserved.
  - Slot 1, Slot 2, Defend, and Potion remain disabled placeholders.
- Updated `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
  - Added the `SkillSystem` scene reference.
  - Subscribes to `SkillBarView.SkillSlot0Clicked`.
  - Passes `SkillSystem` into `UIActionModeController`.
- Updated `Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab`.
  - Saved the new `SkillSystem` serialized fields on the HUD/action-mode components.
- Updated `Assets/_BoneThrone/Scenes/GridTest.unity`.
  - Bound the HUD `skillSystem` reference to the existing scene `SkillSystem`.
  - Did not move, delete, rename, or visually change gameplay objects.
- Supported Skill Slot 0 flow:
  - No selected unit: `Select a unit first.`
  - Dead selected unit: `Selected unit is dead.`
  - Already acted selected unit: `Selected unit has already acted.`
  - Missing `SkillRuntime`: `No SkillRuntime on selected unit.`
  - Empty slot 0: `No skill in slot 0.`
  - Locked skill: `Skill locked.`
  - Cooldown: `Skill on cooldown.`
  - Missing `SkillSystem`: `Skill unavailable: SkillSystem unbound.`
  - Valid start: `Select a skill target.`
  - Invalid target: `Invalid skill target.`
- Cancel options:
  - Right mouse button.
  - Escape.
  - Clicking Skill Slot 0 again while skill targeting.
- Still not added:
  - Tile target support.
  - Skill Slot 1 or Slot 2 actions.
  - Defend or Potion actions.
  - Formal modal confirmation boxes.
  - FloatingHealthBar or world-space HP bars.
  - Sound, VFX, Animator Controllers, formal icons, portraits, or networking.
- Known logging note:
  - `SkillSystem` currently logs skill success/rejection to Console, not to `CombatLog`, so HUD CombatFeedback may not show skill-specific entries in this slice.

## Phase 13.4 manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Run Basic Attack targeting regression: enter targeting, attack, invalid target, cancel, and confirm movement input restores.
4. With no selected unit, click `Skill Slot 0`; expected Prompt: `Select a unit first.`
5. Select a living player unit with a ready slot 0 skill.
6. Click `Skill Slot 0`; expected Prompt: `Select a skill target.`
7. While skill targeting, click empty ground; expected Prompt: `Invalid skill target.`, no movement, no action consumption.
8. While skill targeting, click an invalid Unit target; expected `SkillSystem.TryUseSkill` rejects, no direct UI state mutation, targeting remains active.
9. While skill targeting, click a legal Unit target; expected `SkillSystem.TryUseSkill(selectedUnit, target, 0)` succeeds, HP/acted/cooldown HUD data refreshes, movement input restores.
10. Enter skill targeting again and cancel with right-click, Escape, and clicking `Skill Slot 0` again; expected no action consumption and movement input restored.
11. Confirm Skill Slot 1, Skill Slot 2, Defend, and Potion remain disabled placeholders.
12. Re-run movement, Enemy AI, Key/Stairs, and Console red-error checks.

## Phase 13.5 Move targeting and tile highlight patch notes
- First Phase 13.5 slice only:
  - Added a HUD `Move` button to the left of `Basic Attack`.
  - Added `MoveTargeting` to `UIActionModeController`.
  - Added selected-unit current tile blue highlight.
  - Added MoveTargeting green move-range highlight.
  - Changed player unit click behavior so selecting a player unit no longer automatically shows the green movement range.
- Updated `SkillBarView`.
  - Runtime action order is now `Move`, `BasicAttack`, `SkillSlot0`, `SkillSlot1`, `SkillSlot2`, `Defend`, `Potion`.
  - `Move`, `BasicAttack`, and `SkillSlot0` emit UI intent events.
  - Slot 1, Slot 2, Defend, and Potion remain disabled placeholders.
- Updated `UIActionModeController`.
  - Added `MoveTargeting`.
  - Move button toggles MoveTargeting.
  - MoveTargeting uses the existing `PlayerMovementController` public movement entry point.
  - MoveTargeting shares the existing camera, Physics Raycast, and movement-input suspend/restore path.
  - Entering Basic Attack or Skill targeting exits MoveTargeting and clears green action highlights.
- Updated `PlayerMovementController` with minimal movement entry changes only.
  - Player unit clicks still call `SelectionManager.TrySelect`.
  - Unit selection no longer calls movement range refresh automatically.
  - Direct tile clicks outside HUD MoveTargeting no longer move the unit.
  - Added public `GetReachablePositionsForSelected`.
  - Added public `TryMoveSelectedUnitTo(Tile tile)`, reusing the existing reachable range, pathfinding, `UnitMover.TryMove`, and `UnitTurnState.MarkMoved` logic.
  - Movement range, pathfinding, occupancy, and moved-state formulas were not changed.
- Updated `MovementDebugHighlighter`.
  - Added selected blue highlight support.
  - Added separate action-highlight clearing for move-range green.
  - `ClearActionHighlights` removes green movement range while preserving selected blue.
  - `ClearSelected` removes selected blue.
- Updated `BattleHUD.prefab` and `GridTest.unity`.
  - Bound the HUD `gridManager` reference to the existing scene `GridManager`.
  - Bound the HUD `movementHighlighter` reference to the existing `MovementDebugHighlighter`.
  - Existing `PlayerMovementController`, camera, CombatSystem, SkillSystem, player/enemy visuals, key, stairs, AI, room, and level objects were not moved, renamed, deleted, or visually changed.
- Still not added:
  - Attack red highlight.
  - Skill yellow highlight.
  - AttackRangeService highlight query.
  - SkillTargetingService highlight query.
  - Tile-target skills.
  - Skill Slot 1 or Slot 2 actions.
  - Defend or Potion actions.
  - Sound, VFX, Animator Controllers, formal icons, portraits, or networking.

## Phase 13.5 manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Click a living player unit; expected only selected blue on its current tile, no green movement range.
4. Click empty or reachable tiles before pressing `Move`; expected no movement.
5. Click `Move`; expected Prompt `Select a move tile.` and green movement range.
6. Click an invalid tile while MoveTargeting; expected `Invalid move target.`, no movement, no `HasMoved` change.
7. Click a valid green tile while MoveTargeting; expected existing movement succeeds, `HasMoved` updates through existing logic, green clears, blue moves to the unit's new tile.
8. Enter MoveTargeting and cancel with right-click and Escape; expected green clears and selected blue remains.
9. Enter MoveTargeting, then click `Basic Attack`; expected green clears and Basic Attack targeting still works.
10. Enter MoveTargeting, then click `Skill Slot 0`; expected green clears and Skill Slot 0 targeting still works.
11. Confirm Slot 1, Slot 2, Defend, and Potion remain disabled placeholders.
12. Re-run Basic Attack targeting, Skill Slot 0 targeting, Enemy AI, Key/Stairs, and Console red-error checks.

## Phase 13.5 Basic Attack red highlight and selection cleanup patch notes
- Added a read-only `CombatSystem.CanBasicAttack(Unit attacker, Unit target, out string reason)` query.
  - It checks attacker/target presence, alive state, self-targeting, opposing factions, current tiles, turn gate, required combat services, and basic attack range.
  - It does not roll D20, write `CombatLog`, apply damage, release tiles, change `UnitTurnState`, mark acted, touch cooldowns, or mutate gameplay state.
  - `TryBasicAttack` behavior and logging semantics were not changed.
- Updated `UIActionModeController`.
  - BasicAttackTargeting now uses Inspector-bound `enemyUnits` and `CombatSystem.CanBasicAttack` to collect valid enemy current tiles.
  - Valid basic attack target tiles are sent to the highlighter as red action highlights.
  - Real attacks still call only `CombatSystem.TryBasicAttack(attacker, target)` after the player clicks a target.
  - Invalid target clicks keep targeting active and preserve red highlights.
  - Clearing selection during any action mode exits the mode, clears highlights, and restores movement input.
- Updated `MovementDebugHighlighter`.
  - Preserves selected blue and move green.
  - Adds attack red as the current action highlight color.
  - `ClearActionHighlights` clears green/red action highlights while preserving selected blue.
  - `Clear` clears selected and action highlights together.
- Updated selected-unit UI.
  - The runtime HUD no longer creates or refreshes the independent top-left `SelectedUnitText`.
  - `TurnBannerView` now receives the current selected unit.
  - During player turn, selected unit display name takes priority in the Actor area.
  - With no selected unit, the banner falls back to `Actor: Free Select`.
  - Enemy turn remains `Turn: Enemy Turn`.
- Updated selection behavior.
  - Clicking a different living player unit still selects it.
  - Clicking the currently selected player unit again clears selection.
  - Clearing selection removes selected blue and any action highlights without changing HP, `HasMoved`, `HasActed`, cooldowns, formulas, AI, room, key, stairs, or level state.
- Updated `BattleHUD.prefab` and `GridTest.unity`.
  - Added the serialized enemy unit array used by red attack targeting.
  - Bound the GridTest HUD/action-mode enemy array to the existing scene enemy unit instances.
  - Did not move, delete, rename, or visually change gameplay objects or KayKit assets.
- Still not added:
  - Skill yellow highlight.
  - Tile target skills.
  - Skill Slot 1 or Slot 2 actions.
  - Defend or Potion actions.
  - FloatingHealthBar or world-space HP bars.
  - Sound, VFX, Animator Controllers, formal icons, portraits, networking, or UI Toolkit.

## Phase 13.5 red highlight manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Click a living player unit; expected selected blue only, with no green or red action highlight.
4. Confirm the old top-left independent selected-unit text is not shown.
5. Confirm TurnBanner Actor shows the selected unit display name.
6. Click the same selected player unit again; expected selection clears, blue clears, Prompt returns to free selection, and Actor returns to `Free Select`.
7. Select a player unit and click `Basic Attack`; expected valid enemy target tiles turn red.
8. Confirm out-of-range or invalid enemy targets do not turn red.
9. Click a red enemy; expected existing D20 basic attack flow runs through `TryBasicAttack`, CombatLog updates, and red clears.
10. While BasicAttackTargeting, click empty ground or an invalid target; expected no action consumption, Prompt `Invalid attack target`, red remains.
11. While BasicAttackTargeting, cancel with right-click and Escape; expected red clears and selected blue remains.
12. While BasicAttackTargeting, click `Move`; expected red clears and green move range appears.
13. While BasicAttackTargeting, click `Skill Slot 0`; expected red clears and existing skill targeting still works.
14. Re-run Move, Skill Slot 0, Enemy AI, Key/Stairs, and Console red-error regression checks.

## Phase 13.5 Skill Slot 0 yellow highlight patch notes
- Added `SkillSystem.CanUseSkillOnTarget(Unit caster, Unit target, int slotIndex, out string reason)`.
  - It is a read-only query for UI feedback.
  - It checks caster/target presence, caster alive state, `SkillRuntime`, turn gate, `SkillTargetingService`, `DamageResolver`, and then reuses `SkillTargetingService.CanUseSkill`.
  - It does not execute skill effects, write `CombatLog`, apply damage, change cooldowns, change `UnitTurnState`, mark acted, or mutate gameplay state.
  - `SkillSystem.TryUseSkill` behavior and logging semantics were not changed.
- Updated `UIActionModeController`.
  - Entering SkillTargeting for slot 0 now traverses the Inspector-bound `enemyUnits`.
  - Null, inactive, dead, and untiled enemies are skipped.
  - Each candidate is checked through `SkillSystem.CanUseSkillOnTarget`.
  - Valid enemy current tiles are highlighted yellow.
  - Real skill execution still calls only `SkillSystem.TryUseSkill(selectedUnit, target, 0)` after the player clicks a Unit target.
  - Invalid target clicks keep targeting active and preserve yellow highlights.
- Updated `MovementDebugHighlighter`.
  - Added skill yellow action highlight support.
  - Green move range, red basic attack targets, and yellow skill targets share the same action highlight layer.
  - `ClearActionHighlights` clears green/red/yellow while preserving selected blue.
  - `ClearSelected` clears only selected blue.
  - `Clear` clears selected blue and all action highlights.
- Existing cleanup behavior remains:
  - Successful skill use clears yellow and restores movement input.
  - Right-click, Escape, or clicking Skill Slot 0 again cancels and clears yellow.
  - Switching Skill -> Move clears yellow and shows green.
  - Switching Skill -> Basic Attack clears yellow and shows red.
  - Clearing selection removes selected blue and all action highlights.
- Still not added:
  - Tile target skills.
  - Skill Slot 1 or Slot 2 actions.
  - Defend or Potion actions.
  - FloatingHealthBar or world-space HP bars.
  - Sound, VFX, Animator Controllers, networking, UI Toolkit, or gameplay visual-rule changes.

## Phase 13.5 yellow highlight manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Click a living player unit; expected selected blue only.
4. Click `Move`; expected green move range still appears.
5. Cancel, then click `Basic Attack`; expected red attack targets still appear.
6. Cancel, then click `Skill Slot 0`; expected valid skill target enemies show yellow.
7. Confirm enemies outside slot 0 range, invalid by target type, dead, inactive, or missing current tile do not show yellow.
8. Click a yellow enemy; expected existing `SkillSystem.TryUseSkill(selectedUnit, target, 0)` succeeds, yellow clears, HP/action/cooldown HUD state refreshes.
9. Click an invalid enemy while SkillTargeting; expected no action consumption, no direct UI mutation, Prompt `Invalid skill target`, yellow remains.
10. Click empty ground while SkillTargeting; expected no movement, no action consumption, yellow remains.
11. Cancel SkillTargeting with right-click and Escape; expected yellow clears and selected blue remains.
12. Enter SkillTargeting, then click `Move`; expected yellow clears and green appears.
13. Enter SkillTargeting, then click `Basic Attack`; expected yellow clears and red appears.
14. Enter SkillTargeting, then click the currently selected player unit; expected selection clears and all blue/green/red/yellow highlights clear.
15. Re-run Move, Basic Attack, Enemy AI, Key/Stairs, and Console red-error regression checks.

## Phase 13.6 Skill feedback CombatLog patch notes
- Extended `CombatLog` with skill-specific entries:
  - `SkillUse`
  - `SkillEffect`
  - `SkillRejected`
  - `SkillCooldown`
  - Skill death continues to reuse the existing `Death` entry.
- Added `CombatLog` methods for skill feedback:
  - `LogSkillRejected`
  - `LogSkillUse`
  - `LogSkillEffect`
  - `LogSkillCooldown`
- Updated `SkillSystem`.
  - Added an Inspector-configurable `CombatLog` reference.
  - Rejected skill use keeps the existing `Debug.LogWarning` and now also pushes the reason into CombatLog when bound.
  - Successful skill use now logs skill name, caster, target, effect summary, target remaining HP, death, and cooldown after the existing effect/cooldown/acted logic runs.
  - `TryUseSkill` validation order, effect execution, cooldown start, `MarkActed`, and return value semantics were not changed.
  - If `CombatLog` is unbound, skills continue to execute with the existing Console logs.
- Updated `GridTest.unity`.
  - Bound the existing scene `CombatLog` to the existing scene `SkillSystem`.
  - Did not move, delete, rename, or visually change gameplay objects.
- UI responsibility remains unchanged.
  - `UIActionModeController` still only calls `SkillSystem.TryUseSkill`.
  - `CombatFeedbackView` still only displays `CombatLog.Entry.Message`.
  - No UI script directly writes CombatLog or reads skill internals.
- Not changed:
  - SkillData ScriptableObjects.
  - Skill asset names.
  - Skill values.
  - Skill damage formulas.
  - Skill effect formulas.
  - Skill cooldown rules.
  - Skill unlock rules.
  - `SkillEffectExecutor`.
  - Enemy AI, Room, Level, Key, Stairs, visuals, sound, VFX, Animator, networking, or UI Toolkit.

## Phase 13.6 manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Select a player unit with a ready Skill Slot 0.
4. Click `Skill Slot 0`; expected yellow target highlights still appear.
5. Click a valid yellow target; expected CombatLog UI shows skill name, caster, target, effect summary, target HP, and cooldown if cooldown is greater than 0.
6. Use a skill to kill a target; expected CombatLog UI also shows the existing death entry.
7. Click an invalid skill target; expected CombatLog UI shows a skill rejected reason and the action is not consumed.
8. Confirm HP, acted state, cooldown display, and yellow highlight cleanup are unchanged.
9. Re-run Basic Attack CombatLog: expected D20, hit/miss, damage, death still appear.
10. Re-run Move, Basic Attack red highlight, Skill yellow highlight, Enemy AI, Key/Stairs, and Console red-error checks.

## Phase 13.6-A CombatLog UI polish and result-only filtering patch notes
- Updated CombatLog UI runtime layout.
  - Runtime `CombatFeedback` area is taller: `560 x 300`.
  - TMP rich text is enabled for generated HUD text.
  - `CombatFeedbackView` enables rich text and uses increased line spacing for more readable rows.
  - CombatLog background and text remain non-interactive and keep `raycastTarget=false`.
- Updated CombatLog UI semantics to result-only entries.
  - Basic attack attempts / D20 details stay in Console only.
  - Basic attack rejected entries stay in Console only.
  - Basic attack miss entries stay in Console only for this polish slice.
  - Skill rejected entries stay in Console only.
  - Skill use process entries stay in Console only.
  - Skill cooldown entries stay in Console only.
  - CombatLog UI shows basic attack damage, skill effect/damage results, and death.
- Updated result formatting.
  - Basic attack damage format: `Fighter attacked Skeleton Warrior, dealt 6 damage.`
  - Skill result format uses existing effect summary without parsing it: `Mage used Fireball on Skeleton Mage. Mage Fireball dealt guaranteed damage 8. TargetHP=2.`
  - Death format uses TMP rich text: `<b>Skeleton Mage died.</b>`.
  - Display names prefer `Unit.DisplayName`, then `RoleId`, then `Unit {id}`.
- Updated `SkillSystem` logging.
  - Skill success no longer adds a separate skill-use process entry to the HUD.
  - Skill cooldown no longer appears in the HUD.
  - Skill rejected no longer appears in the HUD.
  - `TryUseSkill` validation order, effect execution, cooldown, `MarkActed`, return values, damage formulas, effect formulas, cooldown rules, and unlock rules were not changed.
- Not changed in this slice:
  - Group / splash structured result logging.
  - `SkillEffectExecutor`.
  - Fighter/Ranger/Mage/Barbarian skill effect files.
  - `DamageResolver`.
  - SkillData ScriptableObjects, skill assets, skill values, Enemy AI, Room, Level, Key, Stairs, visuals, sound, VFX, Animator, networking, or UI Toolkit.
- Follow-up:
  - Phase 13.6-B should add structured feedback for group/splash skill damage instead of parsing effect summary strings.

## Phase 13.6-A manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Confirm the CombatLog panel is taller and log rows have more comfortable spacing.
4. Basic Attack a valid enemy; expected CombatLog UI shows only the damage result, not D20/attempt details.
5. Cause a Basic Attack death; expected CombatLog UI shows a bold death entry.
6. Use Skill Slot 0 on a valid enemy; expected CombatLog UI shows the skill effect/damage result.
7. Cause a skill death; expected CombatLog UI shows a bold death entry.
8. Try an invalid skill target; expected Prompt/Console feedback only, no CombatLog UI entry.
9. Confirm skill cooldown does not appear in CombatLog UI.
10. Confirm Console still contains debugging information for rejected/process entries.
11. Re-run Move, Basic Attack red highlight, Skill yellow highlight, Enemy AI, Key/Stairs, and Console red-error checks.

## Phase 13.6-B structured skill damage log patch notes
- Added `Assets/_BoneThrone/Scripts/Skills/SkillEffectResult.cs`.
  - `SkillEffectResult` stores a feedback-only `Summary`, structured `DamageEntries`, `AnyTargetDied`, and `PrimaryTargetDied`.
  - `SkillDamageLogEntry` stores target unit, damage, remaining HP, death state, and whether the hit was the primary target.
  - These structures do not read scene objects directly and do not mutate gameplay state.
- Updated `SkillEffectExecutor`.
  - Added a structured `TryExecute(..., out SkillEffectResult result)` entry point.
  - Kept the old `TryExecute(..., out string resultLog)` entry point for compatibility; it delegates to the structured result and returns `result.Summary`.
  - Existing skill selection by caster role and skill name is preserved.
  - Existing fallback damage still uses `skill.GuaranteedDamage`.
- Updated existing damage skill effect files.
  - Fighter Shield Bash records one primary damage entry after its existing `ApplyDamage` call.
  - Ranger Precision Shot records one primary damage entry after its existing `ApplyDamage` call.
  - Barbarian Heavy Cleave records one primary damage entry after its existing `ApplyDamage` call.
  - Mage Fireball records the primary damage entry and one splash entry per adjacent valid enemy.
  - In each case, remaining HP and death state are read immediately after `DamageResolver.ApplyDamage`.
- Updated Mage Fireball feedback.
  - Primary target damage remains `skill.GuaranteedDamage`.
  - Splash target damage remains `1`.
  - Splash target filtering, knownUnits dependency, and traversal order are unchanged.
  - Each splash-damaged target can now appear as its own CombatLog UI row.
- Updated `SkillSystem`.
  - Skill execution now consumes the structured result.
  - CombatLog writes one skill damage row per `SkillDamageLogEntry`.
  - Death still reuses the existing rich-text bold `CombatLog.LogDeath`.
  - If no damage entries exist, only Console summary remains; no forced UI damage row is created.
  - Cooldown start, `MarkActed`, validation, return value semantics, and rejection behavior are unchanged.
- Updated `CombatLog`.
  - Added `LogSkillDamage(Unit caster, Unit target, SkillData skill, int damage, int remainingHp, bool isPrimaryTarget)`.
  - Primary format: `Mage used Fireball on Skeleton Mage, dealt 6 damage.`
  - Splash format: `Mage used Fireball on Skeleton Warrior, dealt 1 splash damage.`
  - Rejected, cooldown, miss, and D20 attempt entries remain filtered out of the CombatLog UI.
- Not changed in this slice:
  - `DamageResolver` damage calculation.
  - SkillData ScriptableObjects.
  - Skill asset names.
  - Skill values.
  - Skill damage/effect formulas.
  - Skill cooldown rules.
  - Skill unlock rules.
  - Unit, UnitStats, UnitRuntimeState data structures.
  - Enemy AI, Room, Level, Key, Stairs, visuals, sound, VFX, Animator, networking, or UI Toolkit.

## Phase 13.6-B manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Let Unity regenerate project files after importing `SkillEffectResult.cs`, then confirm there are no red compile errors.
3. Enter Play Mode.
4. Use Fighter Shield Bash; expected one skill damage row and bold death if it kills.
5. Use Ranger Precision Shot; expected one skill damage row and bold death if it kills.
6. Use Barbarian Heavy Cleave; expected one skill damage row and bold death if it kills.
7. Use Mage Fireball on a target with adjacent enemies; expected one primary Fireball damage row plus one splash damage row for each valid adjacent enemy.
8. Kill a splash target with Fireball; expected that target also gets a bold death row.
9. Use Fireball with no adjacent valid enemies; expected only the primary target damage row and no red Console errors.
10. Confirm cooldown, acted state, HP refresh, selected blue, move green, attack red, and skill yellow behavior are unchanged.
11. Re-run Basic Attack result-only CombatLog, Enemy AI, Key/Stairs, and Console red-error checks.

## Phase 13.6-C CombatLog final readability patch notes
- Increased the runtime CombatLog panel again.
  - Runtime `CombatFeedback` area is now `580 x 380`.
  - `CombatFeedbackView` default visible entry count is now `10`.
  - Existing TMP rich text, line spacing, and non-interactive `raycastTarget=false` behavior are preserved.
- Added concise Basic Attack D20 UI feedback.
  - The long attack attempt remains Console-only.
  - CombatLog UI now gets a compact roll row: `Fighter rolled D20: 15 + 5 = 20.`
  - This uses the already-computed roll and attack modifier from `CombatSystem`; no formula or roll calculation changed.
- Updated Basic Attack damage result format.
  - Damage rows now include remaining HP: `Fighter attacked Skeleton Warrior, dealt 6 damage. TargetHP=8.`
  - Death remains a separate rich-text bold row.
- Existing filtering remains.
  - Rejected, skill rejected, cooldown, and miss entries stay out of the CombatLog UI.
  - Skill damage and splash damage result-only rows are unchanged.
- Not changed in this slice:
  - Basic attack hit formula.
  - D20 roll calculation.
  - Damage formula.
  - Death logic.
  - Skill formulas.
  - `DamageResolver`.
  - `SkillEffectExecutor`.
  - SkillData ScriptableObjects, Enemy AI, Room, Level, Key, Stairs, visuals, Floating HP Bar, networking, or UI Toolkit.

## Phase 13.6-C manual Unity 6.3 test steps
1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Enter Play Mode and confirm there are no red Console errors.
3. Confirm the CombatLog panel is taller and can show more entries.
4. Basic Attack a valid enemy; expected one concise D20 row and one damage row with `TargetHP=`.
5. Confirm the old long attack attempt text remains Console-only and does not appear in the UI.
6. Cause a Basic Attack death; expected the damage row plus a separate bold death row.
7. Confirm rejected, skill rejected, cooldown, and miss entries do not appear in the UI.
8. Use Skill Slot 0 and Mage Fireball; expected existing skill damage and splash damage rows remain unchanged.
9. Re-run Move, Basic Attack red highlight, Skill yellow highlight, Enemy AI, Key/Stairs, and Console red-error checks.

## Rollback
- Revert scripts:
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/CombatFeedbackView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/PromptView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/SkillSystem.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/SkillEffectExecutor.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/FighterSkillEffects.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/RangerSkillEffects.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/MageSkillEffects.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Skills/BarbarianSkillEffects.cs`
- Revert UI prefab:
  - `git checkout -- Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab`
- Revert scene:
  - `git checkout -- Assets/_BoneThrone/Scenes/GridTest.unity`
- Remove this DevLog if reverting the whole phase slice:
  - `git checkout -- Docs/DevLogs/Phase13_UI_Feedback.md`
- Remove the structured skill result files if reverting Phase 13.6-B:
  - `git rm Assets/_BoneThrone/Scripts/Skills/SkillEffectResult.cs Assets/_BoneThrone/Scripts/Skills/SkillEffectResult.cs.meta`
