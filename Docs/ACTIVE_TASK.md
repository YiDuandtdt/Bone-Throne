# ACTIVE TASK

## Current Phase

Phase 16.0 - Manual Level Production Support and Review Checklist

## Status

Phase 16.0 is a docs-only support phase for user-owned formal level production.

Codex is not responsible for building, modifying, or wiring formal level scenes.

Formal `Level_01`, `Level_02`, and `Level_03` remain manually owned by the user.

## Phase 16.0 Output

Created:

- `Docs/Phase16_ManualLevelProductionSupport.md`
- `Docs/DevLogs/Phase16.0_ManualLevelProductionSupport.md`
- `Docs/Unity_CSharp_AI_CodingRules.md`
- `Docs/DevLogs/Phase16.1_UnityCSharpAIRulesIntegration.md`

Updated:

- `Docs/ACTIVE_TASK.md`
- `AGENTS.md`

The new support document provides:

- manual formal level production principles
- Codex allowed / forbidden scope
- `Level_01` manual repair checklist
- `Level_02` / `Level_03` manual production checklist
- required scene system object checklist
- player / enemy / interactable / UI / room / grid review checklist
- BossKey / BossDoor / SupplyPoint manual placement checklist
- Victory / Defeat / Retry follow-up checklist
- Unity-Skills read-only review flow
- Git checks before commit
- rollback and risk notes

Additional AI coding-rule integration:

- `Docs/Unity_CSharp_AI_CodingRules.md` adapts the user's `unity-cs-rules.mdc` into BoneThrone-specific Unity C# guidance.
- It should be read before work that touches C# scripts, Inspector fields, Unity lifecycle methods, UI listeners, events, runtime performance, or documentation handoff.

## Scene Boundary

Do not modify:

- `Assets/_BoneThrone/Scenes/GridTest.unity`
- `Assets/_BoneThrone/Scenes/Level_01.unity`
- `Assets/_BoneThrone/Scenes/Level_02.unity`
- `Assets/_BoneThrone/Scenes/Level_03.unity`

Do not directly:

- create formal level scenes
- wire scene managers
- place room, grid, unit, enemy, interactable, UI, BossGate, fog, or progression objects
- copy `GridTest` into formal levels

## Codex Allowed Work

Allowed when phase-scoped:

- documentation
- checklists
- review notes
- screenshot / hierarchy review
- read-only Unity-Skills review if available
- narrow non-scene code / prefab fixes only when explicitly approved

## Standing Rules

- Preserve `GridTest.unity` regression baseline.
- Preserve single-player free-order PlayerTurn.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
- Do not modify SkillData unless explicitly approved.
- Do not modify KayKit source assets.
- Do not create ScriptableObject assets unless explicitly approved.
- `Skeleton_Golem` / `Skeleton_Golem_Boss` are Boss / heavy boss placeholders only.
- `Skeleton_Rogue` is the ordinary Rogue enemy.
- Ranger gameplay identity remains Ranger.

## Next Phase Candidate

Next phase should be selected after the user manually reviews or revises formal level scenes.

Safe candidates:

- manual scene review report
- docs-only checklist refinement
- read-only Unity-Skills scene audit
- UI result panel prefab pass only if explicitly approved
- narrow non-scene bug fix

Avoid:

- direct formal scene construction
- automatic formal scene wiring
- Boss fight implementation without explicit scope
- LAN / Networking without explicit scope
