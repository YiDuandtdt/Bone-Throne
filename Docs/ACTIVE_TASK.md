# ACTIVE_TASK.md

## Current phase
Phase 14.15-D - Player Foot Tile Indicator

## Goal
Add a lightweight player foot tile indicator to the current debug highlighter flow.

Rules:

- Alive player units show their current tile as white by default.
- Enemy tiles do not become white.
- Dead player units do not keep a white tile marker.
- Player movement refreshes the marker so the old tile returns to its baseline and the new tile becomes white.
- Selected, move, attack, and skill highlights can temporarily override the white marker.
- Clearing highlights reapplies the white player foot tile baseline.
- No scene, prefab, material asset, KayKit, combat, skill, or turn-rule changes.

## Allowed files
- `Assets/_BoneThrone/Scripts/Movement/MovementDebugHighlighter.cs`
- `Assets/_BoneThrone/Scripts/Movement/PlayerMovementController.cs`
- `Docs/DevLogs/Phase14.15D_PlayerFootTileIndicator.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify `DamageResolver.cs`.
- Do not modify `SkillEffectExecutor.cs`.
- Do not modify `SkillSystem.cs`.
- Do not modify `SkillTargetingService.cs`.
- Do not modify `CombatSystem.cs`.
- Do not modify SkillData assets.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Rogue`.
- Do not modify `Skeleton_Golem`.
- Do not modify Ranger visual or identity.
- Do not implement Defend.
- Do not implement Potion.
- Do not rebuild skills.
- Do not introduce initiative, AP, networking, behavior trees, or complex UI art.
- Do not modify camera controls.
- Do not modify `ActiveUnitProvider` behavior.

## Required output
After implementation, report:

1. Actual modified files.
2. Whether `MovementDebugHighlighter` was modified.
3. Whether `PlayerMovementController`, `BattleHUDController`, or `TurnManager` were modified.
4. How player foot tile white is implemented.
5. How enemy tiles are avoided.
6. How old/new tile colors update after movement.
7. How Clear restores player foot tile white.
8. Whether scene / prefab / assets were modified.
9. Unity Play Mode test steps.
10. Risks and rollback.

## Validation
Implementation phase.

Manual checks:

1. Confirm only allowed files changed for this phase.
2. Confirm no scene, prefab, SkillData, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. Run C# compilation.
4. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
5. Enter Play Mode and validate player foot tile indicators.
