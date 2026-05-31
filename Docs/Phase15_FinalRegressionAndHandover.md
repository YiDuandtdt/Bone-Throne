# Phase 15 Final Regression And Handover

## Summary

Phase 15 is closed with a non-scene handover.

The final closure added minimal reusable support for:

- BossKey
- BossDoor
- SupplyPoint
- Victory / Defeat / Retry outcome state

Formal level scenes remain user-owned.

Codex did not modify:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`

Note:

- `Level_01.unity` still has a pre-existing Phase 15.14 working tree diff.
- This closure did not edit, revert, or include that scene change.

## Unity-Skills Status

Unity-Skills unavailable.

Fallback used:

- ordinary file scans
- YAML prefab inspection
- C# compile check
- git scene diff checks
- documentation review

## Implemented Files

Scripts:

- `Assets/_BoneThrone/Scripts/Core/GameOutcome.cs`
- `Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs`
- `Assets/_BoneThrone/Scripts/Levels/BossGateProgressionState.cs`
- `Assets/_BoneThrone/Scripts/Interactables/BossKeyItem.cs`
- `Assets/_BoneThrone/Scripts/Interactables/BossDoor.cs`
- `Assets/_BoneThrone/Scripts/Interactables/SupplyPoint.cs`
- `Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs`

Prefab assets:

- `Assets/_BoneThrone/Prefabs/Interactables/BossKey.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/BossDoor.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/SupplyPoint.prefab`

Documentation:

- `Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/Phase15_BossGateSupplyMinimalContract.md`
- `Docs/Phase15_VictoryDefeatRetryFlowPlan.md`
- `Docs/Phase15_FinalRegressionAndHandover.md`
- `Docs/DevLogs/Phase15.16_BossDoorBossKeySupplyPointPreparation.md`
- `Docs/DevLogs/Phase15.17_BossGateSupplyMinimalContractReview.md`
- `Docs/DevLogs/Phase15.18_VictoryDefeatRetryMinimalImplementation.md`
- `Docs/DevLogs/Phase15.19_FinalRegressionAndHandover.md`
- `Docs/ACTIVE_TASK.md`

## Non-Goals Preserved

Not implemented:

- Boss fight
- Boss AI
- Boss skills
- BossDoor scene placement
- BossKey scene placement
- SupplyPoint scene placement
- formal level scene wiring
- scene loading
- gameplay reset / retry implementation
- Victory / Defeat trigger logic
- LAN / Networking
- SkillData changes
- KayKit source changes
- chest loot
- general door lock / unlock system
- SupplyPoint heal / revive behavior

## Manual Scene Handover

When the user is ready, manual formal scene setup can use these components:

- Add one `BossGateProgressionState` in the relevant user-owned scene or room root.
- Place `BossKey.prefab` and assign its `progressionState`.
- Place `BossDoor.prefab` and assign the same `progressionState`.
- Place `SupplyPoint.prefab` and assign `progressionState` plus explicit `targetUnits` when possible.
- Add `GameOutcomeService` and a manually built result panel only when outcome flow is needed.

Inspector references are preferred. Runtime lookup exists only as a fallback.

## Regression Checklist

Required checks before accepting a future scene integration:

- `GridTest.unity` has no new diff.
- `Level_01.unity`, `Level_02.unity`, and `Level_03.unity` changes are user-authored or explicitly approved.
- Single-player PlayerTurn remains free-order.
- `Skeleton_Golem` / `Skeleton_Golem_Boss` are not used as ordinary enemies.
- `Skeleton_Rogue` remains the regular Rogue enemy.
- Ranger gameplay identity remains Ranger.
- No SkillData changed.
- No KayKit source changed.
- BossDoor does not start a Boss fight.
- SupplyPoint grants potions only and does not call `PotionSystem.TryUsePotion`.
- Retry remains an event request until a later reset/reload phase.

## Validation Performed

Pre-checks:

```powershell
git status --short --untracked-files=all
git diff -- Assets/_BoneThrone/Scenes/GridTest.unity
git diff -- Assets/_BoneThrone/Scenes/Level_01.unity
git diff -- Assets/_BoneThrone/Scenes/Level_02.unity
git diff -- Assets/_BoneThrone/Scenes/Level_03.unity
```

Results:

- `GridTest.unity`: no diff
- `Level_02.unity`: no diff
- `Level_03.unity`: no diff
- `Level_01.unity`: pre-existing Phase 15.14 diff, untouched in this closure

Compile:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- succeeded
- 0 warnings
- 0 errors

## Rollback

Rollback this closure without touching the pre-existing `Level_01.unity` diff:

```powershell
git restore -- Docs/ACTIVE_TASK.md Docs/Phase15_BossDoorBossKeySupplyPointPreparation.md Docs/Phase15_BossGateSupplyMinimalContract.md
Remove-Item -LiteralPath Assets/_BoneThrone/Scripts/Core/GameOutcome.cs,Assets/_BoneThrone/Scripts/Core/GameOutcome.cs.meta,Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs,Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs.meta,Assets/_BoneThrone/Scripts/Levels/BossGateProgressionState.cs,Assets/_BoneThrone/Scripts/Levels/BossGateProgressionState.cs.meta,Assets/_BoneThrone/Scripts/Interactables/BossKeyItem.cs,Assets/_BoneThrone/Scripts/Interactables/BossKeyItem.cs.meta,Assets/_BoneThrone/Scripts/Interactables/BossDoor.cs,Assets/_BoneThrone/Scripts/Interactables/BossDoor.cs.meta,Assets/_BoneThrone/Scripts/Interactables/SupplyPoint.cs,Assets/_BoneThrone/Scripts/Interactables/SupplyPoint.cs.meta,Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs,Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs.meta,Assets/_BoneThrone/Prefabs/Interactables/BossKey.prefab,Assets/_BoneThrone/Prefabs/Interactables/BossKey.prefab.meta,Assets/_BoneThrone/Prefabs/Interactables/BossDoor.prefab,Assets/_BoneThrone/Prefabs/Interactables/BossDoor.prefab.meta,Assets/_BoneThrone/Prefabs/Interactables/SupplyPoint.prefab,Assets/_BoneThrone/Prefabs/Interactables/SupplyPoint.prefab.meta,Docs/Phase15_VictoryDefeatRetryFlowPlan.md,Docs/Phase15_FinalRegressionAndHandover.md,Docs/DevLogs/Phase15.18_VictoryDefeatRetryMinimalImplementation.md,Docs/DevLogs/Phase15.19_FinalRegressionAndHandover.md -Force
```
