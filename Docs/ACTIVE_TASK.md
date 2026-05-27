# ACTIVE_TASK.md

## Current phase
Phase 14.9 - Scope Correction and Functional Backlog Reopen

## Goal
Correct the Phase 14 documentation closure wording and reopen the original Phase 14 functional backlog.

The previous Phase 14.8 documentation closure only completed the documentation / stabilization preparation subcycle. It must not be interpreted as the full completion of Phase 14.

This phase must clarify that the following Phase 14 functional items remain:
- GridTest camera controls: middle mouse drag and mouse wheel zoom
- Active enemy provider / scene and UI auto enemy collection
- Skill SO cleanup
- Actual final regression after functional changes
- Real final Phase 14 handover after those functional items are complete

## Allowed files
- Docs/ACTIVE_TASK.md
- Docs/Phase14_FinalHandoverAndClosure.md
- Docs/DevLogs/Phase14.8_FinalHandoverAndClosure.md
- Docs/DevLogs/Phase14.9_ScopeCorrection.md

## Forbidden changes
- Do not modify gameplay code.
- Do not modify scenes.
- Do not modify prefabs.
- Do not modify ScriptableObject assets.
- Do not modify Packages or ProjectSettings.
- Do not implement camera controls yet.
- Do not implement active enemy provider yet.
- Do not clean Skill SO assets yet.

## Required output
Update the Phase 14 closure wording so it clearly states:
1. Phase 14 documentation/stabilization preparation is complete.
2. Phase 14 functional implementation is not complete.
3. The original Phase 14 functional backlog remains open.
4. The project must continue with Phase 14.10, not Phase 15.
5. Phase 15 must not start until Phase 14 functional items are completed and revalidated.
6. The remaining functional backlog must be treated as the next Phase 14.10+ workstream.

## Validation
Documentation-only phase.

Manual checks:
1. git diff only shows Docs changes.
2. No Assets, Packages, ProjectSettings, Library, Temp, Obj, Logs, or UserSettings changes.
3. No code implementation is included.
4. No Phase 15 recommendation is written as the immediate next implementation step.
