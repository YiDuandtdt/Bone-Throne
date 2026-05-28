# ACTIVE_TASK.md

## Current Phase

Phase 15.5 - Interactable Prefab Completion Pass

## Phase 15.4 Completion Note

Phase 15.4 - Environment Prefabization Pass is complete and closed.

Phase 15.4 created project-owned Environment prefabs under:

- `Assets/_BoneThrone/Prefabs/Environment/`

Phase 15.4 closeout is documented in:

- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.4_EnvironmentPrefabizationPass.md`

## Goal

Implement only the Phase 15.5 slice: Interactable Prefab Completion Pass.

Phase 15.5 should review and complete project-owned Interactable prefabs based on existing project prefabs and actual scanned source assets.

## Reference Documents

Phase 15.5 should use:

- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/Phase15_EnvironmentPrefabizationSummary.md`
- `Docs/DevLogs/Phase15.4_EnvironmentPrefabizationPass.md`

## Phase 15.5 Scope Direction

- Phase 15.5 only handles Interactable prefabs.
- Phase 15.5 should prioritize current project interactables and actual scanned interactable-looking candidates.
- Current project interactables include `HealthPotion`, `Key`, and `Stairs`.
- Scanned interactable-looking candidates include chest / door / supply-looking candidates if they actually exist in the imported assets.
- Phase 15.5 must not modify KayKit original resources.
- Phase 15.5 must not process Environment prefabs except for read-only reference.
- Phase 15.5 must not process Characters.
- Phase 15.5 must not process Weapons / Equipment.
- Phase 15.5 must not process Animations / Animator Controllers.
- Phase 15.5 must not modify Turn, Combat, Skill, Potion, LAN, or Networking systems.
- Phase 15.5 must not create formal Level scenes.
- `GridTest.unity` remains the regression baseline and must not be converted into a formal level.

## Allowed Files

Phase 15.5 allowed files must be confirmed before implementation.

Expected direction:

- Project-owned interactable prefab paths under `Assets/_BoneThrone/Prefabs/Interactables/`
- Existing minimal interactable scripts only if Phase 15.5 explicitly proves a script change is required
- A Phase 15.5 DevLog under `Docs/DevLogs/`

Do not expand this scope without first explaining why.

## Forbidden Changes

- Do not modify prefab files during planning or review unless Phase 15.5 explicitly authorizes an interactable prefab edit.
- Do not modify Art / KayKit original resources.
- Do not modify scenes.
- Do not modify C# code unless Phase 15.5 explicitly approves a minimal interactable-only script change.
- Do not modify materials.
- Do not modify SkillData.
- Do not process Environment as a creation/modification target in Phase 15.5.
- Do not process Characters.
- Do not process Weapons / Equipment.
- Do not process Animations / Animator Controllers.
- Do not create formal Level scenes.
- Do not change the single-player PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.
- Do not modify LAN / Networking.
- Do not implement Boss content.

## Validation Direction

Phase 15.5 validation should be defined before implementation.

Expected validation direction:

1. Confirm selected Interactable prefab candidates come from existing project prefabs or actual scanned source assets.
2. Confirm any created or modified Interactable prefab lives under `_BoneThrone/Prefabs/Interactables`.
3. Confirm no KayKit original resources were modified.
4. Confirm no Environment, Character, Weapon, Animation, SkillData, LAN, Boss, or formal Level scene work was introduced.
5. Confirm `GridTest.unity` remains a regression baseline and is not converted into a formal level.
6. Confirm single-player free-order PlayerTurn remains unchanged.
