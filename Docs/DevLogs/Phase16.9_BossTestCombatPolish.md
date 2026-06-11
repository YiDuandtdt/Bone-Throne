# Phase 16.9 - Boss Test Combat Polish

## Scope

This pass addresses the reported `boss_test` polish issues without changing formal level scenes.

## What Changed

- Boss attacks now reuse the axe chop cue with lower pitch and higher volume.
- Boss attack animation playback is slowed through `UnitAnimationController` temporary animator-speed overrides.
- Boss AI now clears stale preview intent, evaluates whether moving improves the attack, waits for movement visual completion, then recalculates and resolves the attack.
- Boss intent scoring prefers hitting more players, then lower-HP players.
- Boss intent highlights darken player foot-tile colors instead of covering them.
- Ordinary enemy attacks now wait longer before the next enemy action, so attack presentation can finish before later movement starts.
- Battle targeting and movement clicks now use `Physics.RaycastAll` and select the intended tile/unit through the model stack.
- `boss_test` gets a runtime-only "演示失败" button that kills all player units and triggers defeat.
- Key pickup VFX root scale is doubled.

## Inspector Setup

- No new required scene wiring.
- `BattleHUDController.showBossTestDefeatButton` defaults on and only shows when the active scene name is `boss_test`.
- `EnemyTurnRunner.enemyAttackRecoveryDelay` can be tuned if enemy attacks still feel too fast.
- `MovementDebugHighlighter.bossIntentFootTileDarken` can be tuned for stronger/weaker foot-tile darkening.

## Play Mode Steps

1. Open `Assets/_BoneThrone/Scenes/boss_test.unity`.
2. Enter Play Mode, clear the outer enemies, collect the Boss key, open the door, and enter the Boss room.
3. During PlayerTurn, confirm Boss warning tiles do not fully cover player foot colors.
4. Click Move and choose tiles partly covered by player/Boss models.
5. Trigger EnemyTurn and watch ordinary enemies and Boss attack pacing.
6. Press the `演示失败` button in `boss_test`.

## Expected Result

- Click targeting remains usable even when a model overlaps the clicked tile.
- Boss moves before attacking when movement improves the eventual attack.
- Boss attacks sound slower, louder, and heavier.
- Enemy attacks finish presentation before the next enemy action.
- The `演示失败` button immediately kills all players and shows defeat.
- Key pickup VFX appears twice as large.

## Risks

- Boss movement/attack evaluation is still a compact heuristic, not a full tactical planner.
- `RaycastAll` allocates on click, but this is input-only and not a per-frame hot path.
- Runtime-created test UI depends on the active scene being named `boss_test`.

## Rollback

Restore these files:

- `Assets/_BoneThrone/Scripts/AI/EnemyAIController.cs`
- `Assets/_BoneThrone/Scripts/AI/EnemyTurnRunner.cs`
- `Assets/_BoneThrone/Scripts/Audio/BTAudioService.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatSystem.cs`
- `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
- `Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs`
- `Assets/_BoneThrone/Resources/BoneThroneVFX/Interaction/BT_VFX_KeyPickup.prefab`
- `Docs/DevLogs/Phase16.9_BossTestCombatPolish.md`
