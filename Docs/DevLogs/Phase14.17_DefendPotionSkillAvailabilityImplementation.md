# Phase 14.17 - Defend / Potion / Skill Availability Implementation

## Scope

Phase 14.17 implements:

- Skill Slot 0 / 1 / 2 availability UI.
- Defend as a self action that consumes action and gives one-hit damage reduction.
- Potion as a self action that consumes action and heals the selected player.

This phase does not modify SkillData, player prefabs, enemy prefabs, scene files, KayKit assets, skill formulas, `SkillSystem`, `SkillTargetingService`, `SkillEffectExecutor`, or `CombatSystem`.

## Files changed

- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`
- `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs`
- `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs.meta`
- `Assets/_BoneThrone/Scripts/Combat/UnitDefenseState.cs`
- `Assets/_BoneThrone/Scripts/Combat/UnitDefenseState.cs.meta`
- `Assets/_BoneThrone/Scripts/Items/PotionSystem.cs`
- `Assets/_BoneThrone/Scripts/Items/PotionSystem.cs.meta`
- `Assets/_BoneThrone/Scripts/Items/UnitPotionState.cs`
- `Assets/_BoneThrone/Scripts/Items/UnitPotionState.cs.meta`
- `Assets/_BoneThrone/Scripts/Items.meta`
- `Docs/ACTIVE_TASK.md`
- `Docs/DevLogs/Phase14.17_DefendPotionSkillAvailabilityImplementation.md`

`UnitRuntimeState.cs` was not modified because the existing `SetCurrentHp(...)` API is enough when `PotionSystem` clamps HP before setting it.

## Skill availability UI

`SkillBarView` now makes Skill Slot 0 / 1 / 2 interactable only when the selected unit has:

- a skill in that slot
- the skill unlocked by level
- no cooldown
- not already acted
- not ended
- currently in PlayerTurn

Empty, Locked, and Cooldown slots are greyed out or cooldown-colored and are not clickable. Ready skills keep the normal enabled color.

`UIActionModeController` keeps its existing second-line checks for empty, locked, cooldown, acted, and ended units.

## Defend

Added `UnitDefenseState`:

- stores `IsDefending`
- stores flat reduction
- `SetDefending(int reduction)`
- `ClearDefending()`
- `TryConsumeReduction(...)`

Added `DefendSystem`:

- `TryDefend(Unit unit)`
- validates selected alive player through `ActionPermissionService.CanAct(...)`
- sets one-hit defend reduction, default `2`
- calls `UnitTurnState.MarkActed()`
- logs Defend through `CombatLog`
- does not mark moved
- does not end turn
- does not modify HP
- does not use target selection

## DamageResolver defend integration

`DamageResolver.ApplyDamage(...)` now checks `UnitDefenseState` on the target.

If defending:

- incoming damage is reduced by the flat reduction
- final damage has a minimum of `1` when incoming damage is above zero
- `CombatLog.LogDamageReduced(...)` records the reduction
- defend state is cleared after that one damage event

No-defend damage and death behavior are unchanged.

Because Basic Attack and Skill damage both route through `DamageResolver`, Defend applies to both paths without modifying `CombatSystem`, `SkillSystem`, `SkillTargetingService`, or `SkillEffectExecutor`.

## Potion

Added `UnitPotionState`:

- `initialPotionCount = 1`
- `currentPotionCount`
- `CurrentPotionCount`
- `HasPotion`
- `EnsureInitialized()`
- `TryConsumePotion()`
- `ResetForTest()`

Added `PotionSystem`:

- `TryUsePotion(Unit unit)`
- validates selected alive player through `ActionPermissionService.CanAct(...)`
- requires potion count above zero
- rejects full HP
- heals a fixed `4 HP`, clamped to max HP
- consumes one potion
- calls `UnitTurnState.MarkActed()`
- logs potion use and heal through `CombatLog`
- does not mark moved
- does not end turn
- does not use target selection
- does not implement a backpack

Potion state is added at runtime when needed, so no player prefab or scene edits are required.

## CombatLog

`CombatLog.EntryType` now includes:

- `Defend`
- `DamageReduced`
- `Potion`
- `Heal`
- `PotionRejected`

New methods:

- `LogDefend(...)`
- `LogDefendRejected(...)`
- `LogDamageReduced(...)`
- `LogPotionUsed(...)`
- `LogPotionRejected(...)`

Logs remain structured through `AddEntry(...)`; no string parsing was added.

## UI integration

`SkillBarView` now exposes:

- `DefendClicked`
- `PotionClicked`

Defend and Potion are no longer disabled placeholders.

`BattleHUDController` now:

- resolves or runtime-adds `DefendSystem`
- resolves or runtime-adds `PotionSystem`
- subscribes to Defend / Potion events
- validates selected self-action context
- cancels active targeting before using Defend or Potion
- calls the appropriate system

The HUD does not directly call `MarkActed`, `MarkMoved`, `SetDefending`, potion consumption, HP mutation, or End Turn.

## Explicit non-changes

No changes were made to:

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
- the 12 formal skill formulas
- free player turn order
- End Turn rules
- player foot tile indicator
- camera controls
- `ActiveUnitProvider` behavior
- backpack systems
- taunt
- counterattack
- guard ally
- enemy skills
- networking
- initiative
- AP
- behavior trees

## Unity Play Mode test steps

1. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
2. Confirm there are no compile errors.
3. Enter Play Mode.
4. Select a Level 1 player and confirm locked Skill Slot 1 / 2 are grey and not clickable.
5. Confirm empty skill slots are grey and not clickable.
6. Confirm ready Skill Slot 0 is enabled and enters targeting.
7. Use a skill and confirm that slot becomes cooldown-colored/disabled.
8. Confirm `UIActionModeController` still rejects locked/cooldown/empty skills if invoked.
9. Select an alive player that has not acted and click Defend.
10. Confirm Defend logs feedback and consumes action.
11. Confirm the same player cannot Basic Attack, Skill, Potion, or Defend again that turn.
12. Confirm Defend does not automatically End Turn.
13. Let the defended unit take Basic Attack damage and confirm damage is reduced once, then defend clears.
14. Confirm Skill damage also routes through the same reduction path when targeting a defended unit.
15. Damage a player, then click Potion.
16. Confirm Potion heals up to max HP, consumes action, decrements count, and logs feedback.
17. Confirm Potion is disabled at full HP, with no potions, after acted, or after ended.
18. Confirm free PlayerTurn order, End Turn, EnemyTurn, player foot tile white marker, camera controls, ActiveUnitProvider, Room / Key / Stairs / LevelUp, and Skill Slot 0 / 1 / 2 regressions.
19. Confirm Console has no new red errors.

## Validation notes

`dotnet build Assembly-CSharp.csproj` was attempted after implementation, but the generated Unity `.csproj` did not yet include the newly added Phase 14.17 scripts. The external build therefore failed to resolve `DefendSystem`, `PotionSystem`, and the new `BoneThrone.Items` namespace from the generated project file.

The `.csproj` file was not modified because generated IDE project files are outside this phase's allowed files and Unity normally regenerates them after importing new scripts.

Unity Editor compilation and Play Mode validation still need to be run after Unity imports the new scripts.

## Risks

- `DamageResolver` is now aware of `UnitDefenseState`, so all damage paths should be checked carefully.
- Defend clears on the first damage event, including splash damage.
- Potion state is runtime-added, which avoids prefab edits but is still prototype state.
- SkillBar UI disable is not the only safety layer; system-side validation remains required.
- Full-HP Potion is disabled, so manual tests may need to damage a player before validating Potion use.

## Rollback

To roll back Phase 14.17:

1. Revert `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`.
2. Revert `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`.
3. Revert `Assets/_BoneThrone/Scripts/Combat/CombatLog.cs`.
4. Revert `Assets/_BoneThrone/Scripts/Combat/DamageResolver.cs`.
5. Delete `Assets/_BoneThrone/Scripts/Combat/DefendSystem.cs` and `.meta`.
6. Delete `Assets/_BoneThrone/Scripts/Combat/UnitDefenseState.cs` and `.meta`.
7. Delete `Assets/_BoneThrone/Scripts/Items/`.
8. Delete `Assets/_BoneThrone/Scripts/Items.meta`.
9. Revert `Docs/ACTIVE_TASK.md`.
10. Delete `Docs/DevLogs/Phase14.17_DefendPotionSkillAvailabilityImplementation.md`.

No SkillData, prefab, scene, KayKit, skill formula, camera, ActiveUnitProvider, free-turn-order, End Turn, or foot-tile rollback is needed because those systems were not changed.
