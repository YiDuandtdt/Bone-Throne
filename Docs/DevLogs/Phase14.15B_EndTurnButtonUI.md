# Phase 14.15-B - End Turn Button UI

## Scope

Phase 14.15-B adds a manual End Turn button to the BattleHUD / SkillBar flow so players can advance the actor turn lifecycle implemented in Phase 14.15.

The button only sends UI intent to `BattleHUDController`, which safely calls `TurnManager.EndCurrentActorTurn()` during `PlayerTurn`.

## Files changed

- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.15B_EndTurnButtonUI.md`

## End Turn button creation

The runtime SkillBar layout now creates eight buttons in this order:

1. Move
2. BasicAttack
3. SkillSlot0
4. SkillSlot1
5. SkillSlot2
6. Defend
7. Potion
8. End Turn

The existing indices `0` through `6` keep their previous meanings. Defend and Potion remain disabled placeholder buttons. End Turn is the new supported action at index `7`.

If an existing SkillBar is already present at runtime, `SkillBarView.EnsureEndTurnButton()` can add the End Turn button without modifying the scene or prefab.

## SkillBarView event

`SkillBarView` now exposes:

- `EndTurnClicked`

The view binds the End Turn button to this event. `SkillBarView` does not reference `TurnManager` and does not mutate HP, cooldown, moved state, or acted state.

`SetEndTurnInteractable(bool interactable)` lets the HUD enable End Turn during `PlayerTurn` and disable it during `EnemyTurn`.

## BattleHUDController handler

`BattleHUDController` now:

- ensures the End Turn button exists after runtime layout setup
- subscribes to `SkillBarView.EndTurnClicked`
- unsubscribes on disable
- enables End Turn only while `turnManager.CurrentPhase == TurnPhase.PlayerTurn`
- handles End Turn through `HandleEndTurnClicked()`

`HandleEndTurnClicked()`:

1. returns safely if `TurnManager` is missing
2. returns safely if the current phase is not `PlayerTurn`
3. asks `UIActionModeController` to cancel current targeting
4. calls `turnManager.EndCurrentActorTurn()`

It does not directly call `MarkMoved`, `MarkActed`, cooldown ticking, or `EnemyTurnRunner`.

## UIActionModeController targeting cleanup

`UIActionModeController` now has:

- `CancelTargetingForExternalAction()`

This public method exits the current Move / BasicAttack / Skill targeting mode by reusing the existing internal cleanup path. It clears action highlights and restores suspended movement input before End Turn advances the actor.

Right-click and Escape cancel behavior is unchanged.

## Explicit non-changes

No changes were made to:

- `DamageResolver.cs`
- `SkillEffectExecutor.cs`
- `SkillSystem.cs`
- `SkillTargetingService.cs`
- `CombatSystem.cs`
- SkillData assets
- Player prefabs
- enemy prefabs
- scene files, including `GridTest.unity`
- KayKit original assets
- `Skeleton_Rogue`
- `Skeleton_Golem`
- Ranger visual / identity
- camera controls
- `ActiveUnitProvider`
- Defend implementation
- Potion implementation
- skill formulas or skill rebuild work

No automatic end-turn behavior was added after movement, basic attacks, or skills.

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Confirm the SkillBar shows `End Turn` after Potion.
5. During Fighter `PlayerTurn`, click End Turn and confirm the turn advances to Ranger.
6. During Ranger `PlayerTurn`, move once, then click End Turn and confirm the turn advances to Mage.
7. During Mage `PlayerTurn`, use Basic Attack, then click End Turn and confirm the turn advances to Barbarian.
8. During Barbarian `PlayerTurn`, use Skill Slot 0 / 1 / 2 if available, then click End Turn and confirm the turn advances to EnemyTurn.
9. Confirm EnemyTurn runs through `TurnManager` / `EnemyTurnRunner` and returns to Fighter.
10. Enter Move targeting, then click End Turn and confirm move highlights clear before the turn advances.
11. Enter BasicAttack targeting, then click End Turn and confirm attack highlights clear before the turn advances.
12. Enter Skill targeting, then click End Turn and confirm skill highlights clear before the turn advances.
13. Confirm End Turn is disabled or non-responsive during EnemyTurn.
14. Confirm End Turn does not directly change HP, cooldown, moved state, or acted state.
15. Confirm Skill Slot 0 / 1 / 2 targeting and execution still work.
16. Confirm Defend and Potion remain disabled placeholders.
17. Confirm camera controls, ActiveUnitProvider, CombatLog, Enemy HP Bar, Room / Key / Stairs / LevelUp still work.
18. Confirm Console has no new red errors.

## Risks

- Existing scene or prefab SkillBars with custom layout may need runtime augmentation to display the new button; this phase handles that without editing scene or prefab assets.
- The SkillBar now contains eight buttons, so text width should be checked in Play Mode.
- If `TurnManager` is unbound, End Turn safely shows a prompt and does not advance.
- If a targeting mode is active, End Turn exits it before advancing; this should be checked for movement input restoration.

## Rollback

To roll back Phase 14.15-B:

1. Revert `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`.
2. Revert `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
3. Revert `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`.
4. Revert `Docs/ACTIVE_TASK.md`.
5. Delete `Docs/DevLogs/Phase14.15B_EndTurnButtonUI.md`.

No scene, prefab, SkillData, combat, skill formula, camera, ActiveUnitProvider, Defend, or Potion rollback is needed because those systems were not changed.
