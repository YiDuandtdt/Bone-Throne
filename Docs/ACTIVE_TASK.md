# ACTIVE_TASK.md

## Current phase
Phase 14.1 - Current Project State Audit

## Goal
Scan the current Unity 6.3 LTS Bone Throne project and produce an accurate project state audit before updating design documents or implementing stabilization features.

This phase must identify:
- completed systems
- current scenes
- current prefabs
- current ScriptableObject data assets
- current UI / combat / skill / movement / room / level systems
- placeholder or test-only systems
- differences between the real project and the original design documents
- rules that must be reflected in updated Phase 14 documentation

## Allowed work
Read-only scan and documentation planning only.

Codex may inspect:
- AGENTS.md
- Docs/
- Assets/_BoneThrone/Scripts/
- Assets/_BoneThrone/Scenes/
- Assets/_BoneThrone/Prefabs/
- Assets/_BoneThrone/Data/
- Packages/manifest.json
- ProjectSettings when relevant

## Allowed file changes
None for the first Codex pass.

After the audit plan is reviewed and approved, documentation-only files may be added or updated, such as:
- Docs/Phase14_ProjectStateAudit.md
- Docs/DevLogs/Phase14.1_ProjectStateAudit.md

## Forbidden changes
- Do not write gameplay code.
- Do not modify prefabs.
- Do not modify Unity scenes.
- Do not modify ScriptableObject assets.
- Do not modify combat formulas.
- Do not modify DamageResolver.
- Do not modify SkillEffectExecutor.
- Do not modify TurnManager.
- Do not modify Room / Level systems.
- Do not modify Networking.
- Do not rename Skeleton_Golem.
- Do not use Skeleton_Golem as a normal enemy.
- Do not rename Skeleton_Rogue back to Skeleton_Golem.
- Do not change Player Ranger visual back to Adventurers Ranger.
- Do not modify KayKit original assets.

## Required output
Codex must output an audit report with:
1. Current branch and repository structure summary.
2. Current completed systems.
3. Current scenes and their apparent purpose.
4. Current prefabs and their apparent purpose.
5. Current data assets / ScriptableObjects.
6. Current UI systems.
7. Current combat systems.
8. Current skill systems.
9. Current movement / grid systems.
10. Current room / level / stairs / key systems.
11. Current enemy and player prefab rules.
12. Current known placeholder / deferred systems.
13. Differences from the original Unity 6.3 design documents.
14. What must be updated in the new system design document.
15. What must be updated in the new vibecoding development document.
16. Recommended next Phase 14.2 / 14.3 documentation update plan.
17. Risks and things that must not be changed.

## Unity validation
No Play Mode validation is required in this read-only audit phase.

Manual validation:
1. Unity 6.3 LTS project still opens.
2. Console has no new compile errors.
3. Git diff only contains ACTIVE_TASK.md after manual update.