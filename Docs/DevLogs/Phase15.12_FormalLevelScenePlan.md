# Phase 15.12 - Formal Level Scene Plan

## Summary

Phase 15.12 produced a docs-only formal level scene plan for future `Level_01`, `Level_02`, and `Level_03` construction.

This phase did not create or modify any Unity scenes. It only records planning boundaries, three-floor structure, room type plan, environment / interactable / unit usage rules, deferred systems, and Phase 15.13 inputs.

## Unity-Skills Note

Unity-Skills was requested for read-only project assistance.

Result:

- Unity-Skills unavailable.
- The local UnitySkills REST health endpoint did not respond.

Fallback used:

- ordinary file scan
- documentation scan
- prefab and scene YAML reference inspection
- docs-only planning

## Actual Added / Modified Files

Added:

- `Docs/Phase15_FormalLevelScenePlan.md`
- `Docs/DevLogs/Phase15.12_FormalLevelScenePlan.md`

Modified:

- `Docs/ACTIVE_TASK.md`

## Planned Three-Level Structure

### Level_01

- Entrance / onboarding floor.
- Suggested 4 to 5 rooms.
- Main enemies: `Skeleton_Minion`, `Skeleton_Warrior`.
- Optional light `Skeleton_Rogue` or `Skeleton_Mage`.
- Validates movement, basic combat, potion, key, stairs, and simple room progression.
- No Boss.

### Level_02

- Medium difficulty escalation floor.
- Suggested 5 to 7 rooms.
- Introduces more `Skeleton_Rogue` and `Skeleton_Mage`.
- Allows optional rooms and visual-only Chest / Doorway candidates.
- No Boss.

### Level_03

- Final planned floor for current three-floor structure.
- Suggested 6 to 8 rooms.
- Introduces `Skeleton_Necromancer`.
- May reserve placeholder location for `Skeleton_Golem_Boss`.
- Boss fight, Boss AI, BossDoor, BossKey, SupplyPoint, Victory, and Defeat remain deferred.

## Non-Changes

This phase did not:

- create `Level_01`, `Level_02`, or `Level_03`
- create any formal level scene
- modify `GridTest.unity`
- modify C# code
- modify prefabs
- modify SkillData
- modify KayKit source
- create ScriptableObject assets
- modify materials
- modify animation clips or Animator Controllers
- implement Boss / BossDoor / BossKey / SupplyPoint
- implement Victory / Defeat / Retry
- implement LAN / Networking
- change the single-player free-order PlayerTurn rule

## Next Phase

Next phase:

- Phase 15.13 - Level_01 / Level_02 / Level_03 Scene Setup

Phase 15.13 should begin only after this plan is accepted. It should create scenes in a narrow, staged way and must keep `GridTest.unity` unchanged as the regression baseline.

## Risks

- Creating formal scenes too early can create large diffs.
- Copying manager objects from GridTest can lose references or carry test-only helpers.
- Environment prefab blocking semantics are not fully defined yet.
- Room, Level, and Data responsibilities can become mixed if implemented together.
- Boss, Door, Chest, or SupplyPoint work can expand scope if not deferred.
- Formal scene behavior can drift from GridTest regression if validation criteria are not explicit.

## Rollback

Rollback Phase 15.12 docs changes with:

```powershell
git restore -- Docs/ACTIVE_TASK.md
Remove-Item -LiteralPath "Docs/Phase15_FormalLevelScenePlan.md" -Force
Remove-Item -LiteralPath "Docs/DevLogs/Phase15.12_FormalLevelScenePlan.md" -Force
```

Before rollback, inspect the worktree:

```powershell
git status --short
```
