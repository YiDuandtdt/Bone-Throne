# ACTIVE_TASK.md

## Current phase
Phase 14.17 - Defend / Potion / Skill Availability Implementation

## Goal
Implement the Phase 14.16 plan:

- Skill Slot 0 / 1 / 2 availability: Empty / Locked / Cooldown are disabled and greyed out; Ready is clickable.
- Defend action: selected player self-action, consumes action, does not end turn, gives one-hit flat damage reduction.
- Potion action: selected player self-action, consumes action, does not end turn, heals self, each player defaults to one potion.

## Allowed files
- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Assets/_BoneThrone/Scripts/UI/UIActionModeController.cs`
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
- `Docs/DevLogs/Phase14.17_DefendPotionSkillAvailabilityImplementation.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify `SkillEffectExecutor.cs`.
- Do not modify `SkillSystem.cs`.
- Do not modify `SkillTargetingService.cs`.
- Do not modify `CombatSystem.cs`.
- Do not modify SkillData assets.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Rogue` or `Skeleton_Golem`.
- Do not modify Ranger visual or identity.
- Do not modify the 12 formal skill formulas.
- Do not modify free player turn order, End Turn rules, player foot tile indicator, camera controls, or `ActiveUnitProvider` behavior.
- Do not implement a backpack system, multiple potion types, taunt, counterattack, guard ally, skill rebuild, enemy skills, networking, initiative, AP, or behavior trees.

## Validation
Implementation phase.

Manual checks:

1. Confirm only allowed files changed.
2. Confirm no scene, prefab, SkillData, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. Run C# compilation.
4. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
5. Enter Play Mode and validate skill availability, Defend, Potion, and regressions.
