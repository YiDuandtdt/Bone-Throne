# Phase 16.14 - Boss Only Victory EndMenu Fix

## Scope

- Fixed `boss_test` victory being triggered by killing ordinary enemies.
- Disabled the victory popup path by default.
- Added `EndMenu` to Build Settings so the configured victory transition can load.

## Implementation

- `BattleOutcomeAutoEvaluator`
  - Victory now requires a Boss-like enemy.
  - If tracked enemies do not include the Boss, the evaluator searches scene enemies, including inactive units.
  - Missing Boss no longer falls back to "all tracked enemies defeated".
  - Boss victory is blocked until the Boss fight runtime has started through `BossGateProgressionState`.
  - After the existing victory delay, the evaluator re-checks the Boss condition, sets victory, and loads `EndMenu`.

- `GameOutcomeService`
  - Victory still plays the configured victory music.
  - Victory no longer spawns the green outcome popup by default.
  - Defeat popup behavior remains enabled for the demo defeat flow.

- `ProjectSettings/EditorBuildSettings.asset`
  - Added `Assets/_BoneThrone/Scenes/EndMenu.unity`.

## Validation

- Ran `dotnet build Assembly-CSharp.csproj`: 0 errors, existing Inspector serialization warnings only.
- Ran targeted `git diff --check`; only Git's LF-to-CRLF notice appeared for `ProjectSettings/EditorBuildSettings.asset`.

## Unity Verification

1. Open `boss_test` and enter Play Mode.
2. Kill an ordinary enemy before entering the Boss room.
3. Confirm no victory popup appears and no EndMenu transition happens.
4. Clear the outside room, pick up the Boss key, open the Boss door, and enter the Boss room.
5. Kill the Boss.
6. Confirm there is a short delay, the victory music plays, and the scene transitions to `EndMenu`.
7. Confirm the green victory popup never appears.
8. Click the demo defeat button and confirm defeat popup behavior still works.

## Risk And Rollback

- Risk: if a future scene uses `BattleOutcomeAutoEvaluator` without a Boss but still expects "all enemies defeated" victory, it must disable `Victory Requires Boss Unit`.
- Rollback: revert `BattleOutcomeAutoEvaluator`, `GameOutcomeService`, `ProjectSettings/EditorBuildSettings.asset`, and this DevLog.
