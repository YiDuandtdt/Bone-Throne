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

## Rollback
- Revert scripts:
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/TurnBannerView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/HeroPanelView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/CombatFeedbackView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/UI/PromptView.cs`
  - `git checkout -- Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- Revert UI prefab:
  - `git checkout -- Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab`
- Revert scene:
  - `git checkout -- Assets/_BoneThrone/Scenes/GridTest.unity`
- Remove this DevLog if reverting the whole phase slice:
  - `git checkout -- Docs/DevLogs/Phase13_UI_Feedback.md`
