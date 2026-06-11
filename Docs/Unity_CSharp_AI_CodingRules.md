# Unity C# AI Coding Rules

This document is adapted from `D:/Users/Admin/Downloads/unity-cs-rules.mdc` for the BoneThrone project.

The source `.mdc` file is a Cursor project rule file. In Cursor, it is normally placed under `.cursor/rules/*.mdc`; when its front matter contains `alwaysApply: true`, Cursor automatically applies it inside that project. This repository keeps the rule in `Docs/` first because `AGENTS.md` and `Docs/` are the authoritative Codex entry points for BoneThrone.

## 0. How To Use

Before modifying C# scripts, prefab component fields, Inspector wiring, runtime gameplay logic, UI listeners, events, or Unity lifecycle code, read:

1. `AGENTS.md`
2. `README.md`
3. `Docs/ACTIVE_TASK.md`
4. The relevant phase/design documents under `Docs/`
5. This file
6. The scripts, prefabs, scenes, ScriptableObjects, or UI assets directly involved in the task

Before editing, confirm:

- What the change is meant to fix or enable.
- Which scripts, modules, scenes, prefabs, or Inspector fields may be affected.
- Whether the task is allowed by the current phase scope.
- How the change should be verified in Unity.
- Whether the change touches a `data -> command/event -> UI/visual feedback` chain.

If the context is not clear, read existing code and docs first. Do not guess across scene or prefab ownership boundaries.

## 1. Optional Cursor Setup

If the user later wants Cursor to apply this rule automatically, create:

```text
.cursor/rules/unity-cs-rules.mdc
```

Use front matter similar to:

```text
---
description: BoneThrone Unity + C# coding rules
alwaysApply: true
---
```

Then copy or adapt the body of this document into that `.mdc` file.

## 2. Unity Project Basics

- Default scripts inherit from `MonoBehaviour`, unless the existing architecture clearly uses plain C# services, data types, or editor-only helpers.
- Do not put Unity runtime logic in constructors.
- Use Unity lifecycle methods correctly: `Awake`, `OnEnable`, `Start`, `Update`, `FixedUpdate`, `LateUpdate`, `OnDisable`, and `OnDestroy`.
- Assume Unity API calls run on the main thread.
- Avoid heavy work, repeated lookups, LINQ, closures, avoidable allocations, disk access, or network operations in `Update`.
- Respect the current project boundary: single-player must stay intact while LAN multiplayer support remains phase-scoped.

## 3. Lifecycle Methods Stay Small

Lifecycle methods should coordinate work, not contain full gameplay systems.

Preferred:

```csharp
private void Update()
{
    HandleInput();
    UpdateMovement();
    UpdateAnimationState();
}
```

Avoid placing complete movement, turn, combat, UI, or state-machine logic directly inside a lifecycle method.

Rules:

- Put Unity lifecycle methods near the top of the class.
- Use a `#region Unity Lifecycle` block when it improves scanning.
- Methods called from `Awake` or `Start` should describe the concrete action. Prefer names such as `CacheComponents`, `FindBattleHud`, `ResetRuntimeState`, or `ValidateBindings`.
- Avoid vague initialization names when a more concrete method name is available.
- Use `#region Initialization` for grouped setup helpers when useful.

## 4. Naming

- Classes, methods, and properties: `PascalCase`
- Local variables: `camelCase`
- Constants: `UPPER_SNAKE_CASE`
- Private fields: `_camelCase`
- Boolean fields and properties: prefer `isXxx`, `hasXxx`, or `canXxx`

The source `.mdc` asks for `m_xxx` parameter names. BoneThrone's existing scripts generally use normal C# parameter naming, so new code should follow the surrounding file's style. Do not introduce `m_xxx` into a file unless that local module already uses it.

## 5. Null Checks

For normal C# objects, prefer pattern matching:

```csharp
if (obj is null)
{
}

if (obj is not null)
{
}
```

Unity objects are special. `UnityEngine.Object` overloads null comparison, and destroyed or unassigned Inspector references can behave differently from plain C# objects. For `SerializeField` references, scene objects, prefab components, and other Unity objects, choose the check that matches Unity lifetime behavior instead of applying ordinary C# null rules mechanically.

## 6. Serialization And Inspector Fields

- Prefer `[SerializeField] private` fields over public fields for Inspector exposure.
- Keep Inspector field names readable and specific.
- Use `[Header]` and `[Tooltip]` for fields that designers, level builders, UI wiring, or prefab maintainers must understand.
- Tooltips should state what happens when a field is empty, who assigns it, and whether runtime fallback lookup is allowed.
- Do not change scenes or prefabs unless the task explicitly allows it.
- When a task does change prefab or Inspector-facing fields, report the affected assets and Unity verification steps.

## 7. Optional References And Fallback Lookup

When a `[SerializeField]` reference is allowed to be empty and resolved by code, handle it carefully.

Reason:

- Unity treats an unassigned Inspector reference differently from a normal C# null.
- A simple null check may appear to pass, but using the reference can still throw `UnassignedReferenceException`.
- `GameObject.FindWithTag` can throw if the tag is not defined.

Rules:

- Do not use an optional serialized reference without protection.
- If code tries a serialized reference first and then falls back to a tag/name search, wrap the actual use of the reference in `try/catch (UnassignedReferenceException)`.
- Wrap tag fallback lookup in its own `try/catch` when the tag may not exist.
- If the final dependency cannot be resolved, log a clear error and disable the component when continuing would cause repeated null errors.
- Explain optional lookup behavior in the field's `[Tooltip]`.

## 8. Code Structure And Performance

- Prefer composition over inheritance unless the local architecture already establishes inheritance.
- Keep abstractions small and phase-scoped.
- Cache component references.
- Avoid unnecessary per-frame allocation.
- Avoid LINQ, closures, and temporary collection churn in hot runtime paths.
- Use pooling only when there is a real repeated allocation problem or the project already has a suitable pool.
- Keep code changes narrow; do not perform unrelated refactors.

## 9. Interfaces And Events

For BoneThrone, apply the original rule's `data -> UI` idea to gameplay and UI flows:

- Data/runtime sources such as unit state, combat state, turn state, inventory/key state, room progression, or level result state should broadcast real changes after they happen.
- UI and visual feedback listeners should subscribe in `OnEnable` and unsubscribe in `OnDisable`.
- Avoid polling UI every frame when an event or explicit refresh can represent the change.
- Do not make data sources implement listener-style UI interfaces only so UI can subscribe.
- Use interfaces when there are multiple implementations, a clear command boundary, or useful test seams. Do not introduce broad service interfaces only for ceremony.
- Host-authoritative multiplayer commands must preserve the ActionCommand direction described in `AGENTS.md`: clients request actions, host validates and applies them.

## 10. Comments And Regions

- Comments may be in Chinese or English, but match the surrounding file when possible.
- Comments should explain why something exists, not restate what the code already says.
- Use XML comments on public APIs when they help callers understand behavior.
- Interface files should stay easy to scan; prefer a type-level summary and concise member signatures unless the surrounding code uses more detailed XML.
- Use regions sparingly for Unity lifecycle, initialization, event subscription, or large Inspector field groups.

## 11. Documentation Updates

For meaningful code changes, update the documentation that owns the concept:

- Use `README.md` for stable code structure and module responsibility.
- Use `Docs/ACTIVE_TASK.md` for current phase status when the phase changes.
- Use `Docs/DevLogs/` for implementation notes, verification, risks, and rollback instructions.
- Do not turn `README.md` into a changelog.

Each code handoff should include:

- What changed.
- Which Inspector or prefab setup is required.
- Play Mode or build validation steps.
- Expected result.
- Risks and rollback notes.

## 12. Output Expectations

- Optimize for Unity usage first.
- Respect the current phase boundaries before editing.
- Keep single-player behavior intact unless explicitly scoped otherwise.
- Keep explanations concise and technical.
- Prefer small, verifiable changes over broad rewrites.
