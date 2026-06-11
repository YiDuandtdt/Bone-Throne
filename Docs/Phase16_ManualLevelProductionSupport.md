# Phase 16.0 - Manual Level Production Support

## 1. Purpose

Phase 16.0 defines a manual production checklist for `Level_01`, `Level_02`, and `Level_03`.

Formal level scenes are user-owned. Codex should support planning, review, checklist work, and small non-scene fixes only when explicitly approved.

This document does not authorize Codex to edit scenes, prefabs, scripts, SkillData, KayKit source assets, or ScriptableObject assets.

## 2. Manual Level Production Principles

- Treat `GridTest.unity` as the regression baseline, not as a formal level.
- Keep formal scenes readable, with clear root groups and room names.
- Build one room slice at a time, then test before expanding.
- Prefer explicit Inspector references over hidden runtime lookup.
- Keep tactical readability ahead of dressing density.
- Keep the single-player PlayerTurn free-order rule intact.
- Do not turn `Skeleton_Golem` or `Skeleton_Golem_Boss` into normal enemies.
- Keep Ranger gameplay identity as Ranger even if visuals use Rogue assets.
- Keep Boss fight, full outcome flow, LAN, chest loot, and general door systems deferred until explicitly scoped.

## 3. Codex Scope Going Forward

Codex may help with:

- documentation
- checklists
- scene review notes
- screenshot / hierarchy review
- missing-reference triage
- narrow code fixes when approved
- narrow prefab fixes when approved
- non-scene system preparation
- read-only Unity-Skills review if available

Codex must not directly:

- create formal level scenes
- modify `Level_01`, `Level_02`, or `Level_03`
- modify `GridTest.unity`
- wire level scene managers
- place rooms, tiles, units, interactables, UI, BossDoor, BossKey, or SupplyPoint
- copy GridTest into formal levels
- modify prefabs without explicit approval
- modify C# code without explicit approval
- modify SkillData
- modify KayKit source assets
- create ScriptableObject assets

## 4. Level_01 Manual Repair Checklist

Use this checklist when cleaning up the current `Level_01` manually:

- Decide whether to keep, rebuild, or replace the Phase 15.14 first-pass scene contents.
- Rename hierarchy roots into stable production groups.
- Remove temporary test-only objects that are not meant for the formal slice.
- Confirm the four player units are present, alive, and selectable.
- Confirm PlayerTurn allows free selection among alive not-ended player units.
- Confirm grid tiles are walkable / blocked as intended.
- Confirm unit starting tiles are not accidentally occupied by the wrong unit id.
- Confirm at least one combat room can be entered and cleared.
- Confirm room shadow / black fog objects do not block clicks unintentionally.
- Confirm enemies are inactive before intended room entry if using `RoomEnemyActivator`.
- Confirm key and stairs rules are simple and readable.
- Confirm UI / BattleHUD references are present.
- Confirm camera framing and lighting are usable.
- Confirm no missing scripts, missing prefabs, or missing material references.

## 5. Level_02 / Level_03 Manual Production Checklist

For new manual scene production:

- Start from an empty or clean scene, not from an uncontrolled copy of `GridTest`.
- Create clear root groups before placing gameplay objects.
- Build a small room loop first, then expand.
- Add grid and room systems before dressing.
- Add players, enemies, interactables, and UI after the grid is validated.
- Add environment dressing after movement lanes and click targets are readable.
- Keep `Skeleton_Rogue` as a normal enemy option.
- Keep `Skeleton_Golem` / `Skeleton_Golem_Boss` as boss / heavy boss placeholders only.
- For `Level_03`, reserve boss route space without implementing Boss fight unless a later phase approves it.
- Run Play Mode after each room is added.
- Keep scene diffs intentional and reviewable.

## 6. Required Scene Systems Checklist

Each formal scene should eventually have:

- a clear scene root object
- grid root / grid manager
- tile root with inspectable tile objects
- selection manager
- movement system
- turn manager
- enemy turn runner
- combat system
- damage resolver dependencies
- skill system
- defend / potion systems if used by the HUD
- camera
- lighting suitable for URP
- UI Canvas / BattleHUD
- room roots
- room trigger objects where needed
- room shadow / black fog objects where needed
- progression service objects only if the scene uses key / stairs / gate progression

Avoid hidden singleton assumptions. If an object needs a reference, assign it explicitly where practical.

## 7. Player / Enemy Checklist

Players:

- Fighter, Ranger, Mage, and Barbarian exist if the scene is a full party slice.
- All player units use `UnitFaction.Player`.
- Unit ids are unique.
- Runtime HP initializes correctly.
- `UnitTurnState` and action state are present where needed.
- Skill loadouts match class identity.
- Ranger remains gameplay Ranger.
- Animators and visuals are assigned.

Enemies:

- Enemy units use `UnitFaction.Enemy`.
- Enemy unit ids do not collide with player ids.
- Regular enemies should be `Skeleton_Minion`, `Skeleton_Warrior`, `Skeleton_Rogue`, `Skeleton_Mage`, or `Skeleton_Necromancer`.
- `Skeleton_Golem` and `Skeleton_Golem_Boss` are not used as ordinary enemies.
- Enemy spawn tiles are valid and not blocked.
- Room enemies are assigned to the intended `RoomEnemyActivator`.
- Enemies activate only when the room is entered, if that rule is intended.

## 8. Interactable Checklist

Common interactables:

- HealthPotion has a reachable position and collider.
- Key uses ordinary floor progression only.
- Stairs point to the current placeholder progression behavior only if that behavior is desired.
- Chest remains visual-only unless a later loot phase approves behavior.
- Doorway remains visual-only unless a later door phase approves behavior.

Inspector checks:

- Required services are assigned.
- Click / trigger colliders do not overlap in confusing ways.
- Interactable visuals are readable from the camera angle.
- One-shot objects have sensible consumed / collected defaults.

## 9. UI Checklist

- Canvas is present and active.
- BattleHUD is present and wired.
- Hero panels update for all player units.
- Skill bar updates when selected unit changes.
- Turn banner updates when turns advance.
- Prompt text is readable and not occluded.
- Combat feedback appears for attacks / skills.
- Enemy floating health bars appear only when intended.
- Result panel is deferred unless manually built and wired.

## 10. Room / Grid / Fog Checklist

Grid:

- Tile count matches intended room size.
- Walkable and blocked tiles are correct.
- Tile occupancy is clean at scene start.
- Tile visual height does not fight with fog planes or props.

Rooms:

- Room roots have clear names, such as `Room_01_Start`.
- Triggers cover intended entry spaces only.
- `RoomController` references its shadow and enemy activator when used.
- Cleared state is based on the correct assigned enemies.

Fog / shadow:

- Black fog / shadow meshes sit above or in front of rooms without blocking required interaction.
- Materials use transparent rendering and readable alpha.
- Fog objects can be disabled or faded later.
- Do not implement complex fog of war for the first LAN milestone.

## 11. BossKey / BossDoor / SupplyPoint Manual Placement Checklist

Boss gate state:

- Add one `BossGateProgressionState` for the intended boss gate area.
- Keep it local to the scene / level root.
- Do not bind it to Boss fight logic yet.

BossKey:

- Place `BossKey.prefab` manually.
- Assign its `progressionState`.
- Confirm `consumeOnCollect` is intended.
- Confirm it does not replace ordinary floor key behavior.

BossDoor:

- Place `BossDoor.prefab` manually.
- Assign the same `progressionState`.
- Assign or verify `doorBlocker`.
- Configure locked / opened visuals if needed.
- Confirm it only gates access and does not start Boss fight.

SupplyPoint:

- Place `SupplyPoint.prefab` manually.
- Assign `progressionState` if the point should mark boss preparation state.
- Prefer explicit `targetUnits` references.
- Keep first pass potion-only.
- Do not revive, heal, grant loot, advance turns, or call `PotionSystem.TryUsePotion`.

## 12. Victory / Defeat / Retry Follow-Up Checklist

Outcome service:

- Add `GameOutcomeService` only when a scene needs outcome state.
- Let future systems call `SetVictory` or `SetDefeat`.
- Keep outcome triggers separate from UI display.

Result panel:

- Manually build a Canvas panel with title, reason, retry, and close controls.
- Attach `GameResultPanelController`.
- Assign `root`, `titleText`, `reasonText`, `retryButton`, `closeButton`, and `outcomeService`.

Retry:

- Current retry behavior is event-only.
- A future reset / reload controller should decide what retry means.
- Do not silently reload scenes from the panel controller.

Deferred:

- party wipe detection
- boss victory detection
- final floor victory
- retry reset / reload
- LAN outcome replication

## 13. Unity-Skills Read-Only Review Flow

If Unity-Skills REST is available, use it for read-only review first:

1. Check editor health and compile state.
2. Inspect loaded scene names.
3. Get scene hierarchy summaries.
4. Search for missing scripts and missing prefab references.
5. Search for unassigned required component references.
6. List selected objects or targeted room roots when reviewing a screenshot / hierarchy.
7. Export review notes only.

Do not use Unity-Skills to mutate formal scenes unless the user explicitly overrides the Phase 15.15 boundary.

If Unity-Skills is unavailable, use fallback review:

- file scan
- scene YAML grep
- Unity Console screenshot / user report
- manual Inspector checklist

## 14. Git Checks Before Commit

Run these before accepting formal scene work:

```powershell
git status --short --untracked-files=all
git diff -- Assets/_BoneThrone/Scenes/GridTest.unity
git diff -- Assets/_BoneThrone/Scenes/Level_01.unity
git diff -- Assets/_BoneThrone/Scenes/Level_02.unity
git diff -- Assets/_BoneThrone/Scenes/Level_03.unity
git diff --name-only
```

For Codex docs-only phases, expected changed files should be docs only.

For user-owned scene phases, scene diffs are allowed only when intentionally made by the user.

## 15. Rollback And Risks

Docs-only rollback for Phase 16.0:

```powershell
git restore -- Docs/ACTIVE_TASK.md
Remove-Item -LiteralPath Docs/Phase16_ManualLevelProductionSupport.md,Docs/DevLogs/Phase16.0_ManualLevelProductionSupport.md -Force
```

Manual scene rollback should be handled separately by the user because formal scene content is user-owned.

Risks:

- Scene diffs can become large and hard to review.
- Unassigned Inspector references can fail only at Play Mode.
- Fog / shadow planes can block clicks if colliders are left enabled.
- Environment dressing can obscure tactical tiles.
- Boss gate objects can be mistaken for Boss fight implementation if responsibilities are not kept narrow.
- Retry can become destructive if scene reload / reset rules are added without a clear contract.
