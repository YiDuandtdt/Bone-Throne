# Phase 15.18 - Victory / Defeat / Retry Minimal Implementation

## Summary

Implemented a minimal non-scene outcome service and UI bridge.

No outcome trigger logic was added.

## Files Added

- `Assets/_BoneThrone/Scripts/Core/GameOutcome.cs`
- `Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs`
- `Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs`
- `Docs/Phase15_VictoryDefeatRetryFlowPlan.md`

## Behavior

`GameOutcomeService` stores `None`, `Victory`, or `Defeat`, broadcasts outcome changes, and raises retry requests.

`GameResultPanelController` displays the current outcome and routes retry button clicks into `GameOutcomeService.RequestRetry()`.

Retry is only an event. It does not reload scenes or reset gameplay state.

## Deferred

- `GameResultPanel.prefab`
- Victory trigger
- Defeat trigger
- Retry reload / reset controller
- Boss victory
- party wipe integration
- LAN outcome replication

## Validation

`dotnet build Assembly-CSharp.csproj` succeeded with 0 warnings and 0 errors.

No scene files were modified.
