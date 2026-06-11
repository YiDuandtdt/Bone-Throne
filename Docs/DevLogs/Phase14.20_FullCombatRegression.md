# Phase 14.20 - Full Combat Regression

## Summary
Phase 14.20 is documentation-only. No gameplay code was written, no systems were changed, and no fixes were attempted.

## Actual Modified Files
- `Docs/Phase14_FullCombatRegression.md`
- `Docs/DevLogs/Phase14.20_FullCombatRegression.md`
- `Docs/ACTIVE_TASK.md`

## What Changed
Created the full manual combat regression checklist for Unity Play Mode testing. The document covers:
- UI regression.
- Free-order PlayerTurn.
- Player foot tile white / ended grey markers.
- End Turn.
- Defend.
- Potion.
- Skill availability.
- All 12 rebuilt formal skills.
- Stun / Bleed / DamageAmplify / Knockback.
- EnemyTurn.
- CombatLog.
- HP bars.
- Room / Key / Stairs / LevelUp.
- Camera controls.
- ActiveUnitProvider.
- Issue logging and final result summary.

## What Did Not Change
- No C# files changed.
- No gameplay code changed.
- No SkillData assets changed.
- No prefabs changed.
- No scenes changed.
- No Packages or ProjectSettings changed.
- No KayKit, art, materials, or UI prefab / scene layout changed.

## Next Step
The user should manually execute `Docs/Phase14_FullCombatRegression.md` in Unity Play Mode using `Assets/_BoneThrone/Scenes/GridTest.unity`.

If every required test passes, proceed to Phase 14.21 Final Handover.

If any test fails or is blocked, proceed to Phase 14.20-A Minimal Regression Fix with the recorded Issue ID(s).
