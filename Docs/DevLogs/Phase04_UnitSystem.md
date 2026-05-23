# Phase 04 DevLog - Unit System and Data Model

## Date
2026-05-23

## Branch
phase/04-unit-system

## Unity version
Unity 6000.3.10f1

## Goal
Implement the minimal unit system and data model for basic stats, faction, runtime HP/death state, and tile occupancy/release.

## Files changed
- Assets/_BoneThrone/Scripts/Units/UnitFaction.cs
- Assets/_BoneThrone/Scripts/Units/UnitStats.cs
- Assets/_BoneThrone/Scripts/Units/UnitRuntimeState.cs
- Assets/_BoneThrone/Scripts/Units/UnitData.cs
- Assets/_BoneThrone/Scripts/Units/Unit.cs
- Assets/_BoneThrone/Scripts/Tests/UnitPlacementTester.cs

## Unity test result
- Compile: Pass
- Console red errors: None
- Play Mode: Pass
- Test scene: Assets/_BoneThrone/Scenes/GridTest.unity

## Passed tests
- [x] Unit runtime initializes from stats
- [x] UnitId <= 0 placement fails with warning
- [x] Unit can occupy target tile
- [x] Occupied tile returns CanEnter false
- [x] Second unit cannot overwrite occupied tile
- [x] Dead unit releases occupied tile
- [x] No movement, BFS, A*, combat, turns, skills, AI, UI, or networking implemented

## Notes
Phase 4 only validates unit state and tile occupancy. Real selection and movement will be handled in later phases.