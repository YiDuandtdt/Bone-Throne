# Phase 16.25 - Final Test Polish

Date: 2026-06-10

## Problem

Before the final manual test pass, Level 3 raised boss-room surfaces made range highlights hard to read, and late enemy attack audio needed better synchronization.

## Change

- Added runtime raised-surface overlays to `MovementDebugHighlighter`.
- Move, attack, skill, selected, and boss-intent highlights now create translucent overlay quads above the detected surface height.
- Overlay height probes skip units so character colliders do not lift the range marker.
- Delayed the boss attack SFX by 1.5 seconds, from 0.18 seconds to 1.68 seconds.
- Added a one-second enemy-only AxeChop delay on top of the existing serialized axe delay, so the current 0.8-second scene value becomes 1.8 seconds for axe skeleton attacks.
- Routed `Skeleton_Warrior` and `Skeleton_Minion` basic attacks to `AxeChop` instead of the generic heavy-hit cue, since both enemy prefabs use `RoleId.Enemy`.

## Final Loop Check

- Build Settings include `StartMenu`, `IntroStory`, `Level_1`, `Level_2`, `Level_3_final`, and `EndMenu`.
- `Level_1` transitions to `Level_2`.
- `Level_2` transitions to `Level_3_final`.
- `Level_3_final` transitions to `EndMenu`.
- `Level_3_final` has a `Skeleton_Golem_Boss`, has stairs, skips the shared key requirement, and keeps a required boss-defeated room gate before stairs can complete the level.
- No `BattleOutcomeAutoEvaluator` component is present in the three formal level scenes, so killing the third-level boss should not directly trigger victory outside the stairs flow.

## Verification

- `dotnet build .\Assembly-CSharp.csproj --no-restore` passed with 0 errors.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors.
- `git diff --check` passed for the edited scripts.

## Risk

Raised range overlays depend on collider/raycast surface detection when raised geometry is separate from the tile renderer. If a decorative platform has no collider, the fallback remains the tile renderer bounds height, but the brighter overlay still improves visibility.

## Rollback

Revert `MovementDebugHighlighter.cs`, `BTAudioService.cs`, `CombatSystem.cs`, and `EnemyAIController.cs` to remove the raised overlays and restore the previous attack-audio timing.
