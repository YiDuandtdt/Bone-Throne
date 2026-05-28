# ACTIVE_TASK.md

## Current phase
Phase 14.19-E - Restore SkillBar UI From Last Known Good Baseline

## Goal
Restore SkillBar UI code to the last known good committed baseline before Phase 14.19-B/C/D working-tree UI layout changes, while preserving player foot tile white / ended grey behavior.

## Allowed files
- `Assets/_BoneThrone/Scripts/UI/SkillBarView.cs`
- `Assets/_BoneThrone/Scripts/UI/BattleHUDController.cs`
- `Docs/DevLogs/Phase14.19E_RestoreSkillBarUIFromGoodBaseline.md`
- `Docs/ACTIVE_TASK.md`

## Forbidden changes
- Do not modify SkillData assets.
- Do not modify Player prefabs.
- Do not modify enemy prefabs.
- Do not modify scene files, including `GridTest.unity`.
- Do not modify `SkillSystem.cs`.
- Do not modify `SkillTargetingService.cs`.
- Do not modify `CombatSystem.cs`.
- Do not modify UI scripts.
- Do not modify KayKit original assets.
- Do not modify `Skeleton_Rogue` or `Skeleton_Golem`.
- Do not modify Ranger visual or identity.
- Do not modify Defend / Potion action economy.
- Do not modify End Turn / free player turn order.
- Do not modify player foot tile indicator.
- Do not modify camera controls.
- Do not modify `ActiveUnitProvider` behavior.
- Do not introduce mana, skill trees, inventory, extra movement, slow, shield, lifesteal, summon, taunt AI, counterattack, guard ally, complex status framework, complex line targeting, target-tile casting, networking, initiative, AP, or behavior trees.

## Validation
Implementation phase.

Manual checks:

1. Confirm only allowed files changed.
2. Confirm no scene, prefab, OnlyTest SkillData, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. Let Unity import new scripts and compile.
4. Open `Assets/_BoneThrone/Scenes/GridTest.unity`.
5. Enter Play Mode and validate the SkillBar matches the pre-14.19-B good UI baseline, all eight buttons and action states still work, and End Turn still immediately repaints the ended player's foot tile grey.
