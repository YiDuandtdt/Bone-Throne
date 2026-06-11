# Phase 15 - Victory / Defeat / Retry Flow Plan

## Goal

This plan records the minimal outcome flow implemented during the Phase 15.16-15.19 combined closure.

The implementation is deliberately local and non-scene:

- no formal level scene changes
- no `GridTest.unity` changes
- no scene reload
- no gameplay reset
- no Turn / Combat / Skill / Potion refactor
- no LAN or networking behavior

## Implemented Runtime Contract

### GameOutcome

Path:

- `Assets/_BoneThrone/Scripts/Core/GameOutcome.cs`

Values:

- `None`
- `Victory`
- `Defeat`

### GameOutcomeService

Path:

- `Assets/_BoneThrone/Scripts/Core/GameOutcomeService.cs`

Responsibilities:

- store the current outcome
- expose `HasOutcome` and `LastReason`
- broadcast outcome changes
- broadcast retry requests

Public surface:

```csharp
public GameOutcome CurrentOutcome { get; }
public bool HasOutcome { get; }
public string LastReason { get; }
public event Action<GameOutcome, string> OutcomeChanged;
public event Action RetryRequested;
public bool SetVictory(string reason = null);
public bool SetDefeat(string reason = null);
public void ClearOutcome();
public void RequestRetry();
```

Rules:

- `SetVictory` and `SetDefeat` return `false` if an outcome is already set.
- `ClearOutcome` resets the state to `None` and broadcasts the change.
- `RequestRetry` only raises `RetryRequested`.
- The service does not reload scenes, reset units, reset turns, or change progression.

### GameResultPanelController

Path:

- `Assets/_BoneThrone/Scripts/UI/GameResultPanelController.cs`

Responsibilities:

- subscribe to `GameOutcomeService.OutcomeChanged`
- show Victory / Defeat title and reason
- hide on `None`
- call `GameOutcomeService.RequestRetry()` when retry is clicked
- hide the panel when close is clicked

Rules:

- Retry is only a request event.
- Close hides the panel and does not clear the outcome.
- Missing references fail safely.
- If `outcomeService` is not assigned, it attempts one runtime lookup.

## Deferred UI Prefab

`Assets/_BoneThrone/Prefabs/UI/GameResultPanel.prefab` was deferred.

Reason:

- Creating a robust uGUI + TMP prefab as raw YAML without Unity Editor import verification is riskier than the value it provides in this closure pass.
- The script contract is ready for a user-built Canvas panel or a later Unity Editor-verified prefab pass.

Recommended manual setup:

1. Add a `GameOutcomeService` GameObject to the user-owned scene when outcome flow is needed.
2. Add a Canvas panel with title text, reason text, retry button, and close button.
3. Attach `GameResultPanelController`.
4. Assign `root`, `titleText`, `reasonText`, `retryButton`, `closeButton`, and `outcomeService`.
5. Wire whatever future reset/reload behavior should respond to `RetryRequested`.

## Deferred Outcome Triggers

The service does not decide when victory or defeat happens.

Deferred trigger sources:

- all enemies defeated in a boss room
- party wiped
- stairs / final exit
- formal level progression service
- future Boss fight controller
- future retry / scene reload controller

## Validation

Compile validation:

```powershell
dotnet build Assembly-CSharp.csproj
```

Result:

- succeeded
- 0 warnings
- 0 errors

Unity Play Mode validation remains manual because no formal scene was modified.
