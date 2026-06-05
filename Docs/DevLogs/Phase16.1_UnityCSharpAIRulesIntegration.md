# Phase 16.1 - Unity C# AI Rules Integration

## Purpose

Integrate the user's `unity-cs-rules.mdc` guidance into BoneThrone's project documentation.

The source file is a Cursor project rule file. It is normally used from `.cursor/rules/*.mdc`, and `alwaysApply: true` means Cursor applies it automatically inside the project. BoneThrone currently uses `AGENTS.md` and `Docs/` as its AI collaboration entry points, so this pass stores the adapted rule in `Docs/` instead of creating a Cursor-only rule directory.

## Files Added

- `Docs/Unity_CSharp_AI_CodingRules.md`
- `Docs/DevLogs/Phase16.1_UnityCSharpAIRulesIntegration.md`

## Files Updated

- `AGENTS.md`
- `Docs/ACTIVE_TASK.md`

## Summary

- Adapted the original rule from a generic/AP_Eyes-oriented Cursor rule into BoneThrone-specific wording.
- Preserved the key rule topics: coding pre-read, Unity lifecycle order, naming, null checks, serialized fields, optional Inspector references, performance, interface/event architecture, comments, documentation updates, and output expectations.
- Adjusted project-specific architecture wording toward BoneThrone's turn-based tactics flow, UI feedback, ActionCommand direction, and current phase boundaries.
- Documented optional future Cursor setup through `.cursor/rules/unity-cs-rules.mdc`.

## Verification

- Documentation-only change.
- No Unity scenes, prefabs, packages, scripts, or generated files were edited.
- `rg` should show the new rule referenced from `AGENTS.md` and `Docs/ACTIVE_TASK.md`.

## Risks

- This rule is not automatically applied by Cursor unless a matching `.cursor/rules/*.mdc` file is created later.
- The rule intentionally preserves BoneThrone's existing parameter naming style instead of forcing the source `.mdc` `m_xxx` parameter convention across the project.

## Rollback

- Remove `Docs/Unity_CSharp_AI_CodingRules.md`.
- Remove this DevLog.
- Remove the references added to `AGENTS.md` and `Docs/ACTIVE_TASK.md`.
