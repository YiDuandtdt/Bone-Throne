# AGENTS.md - BoneThrone Codex Rules

## Project
This repository is a Unity 6.3 LTS project for 《骸骨王座》, a 2.5D low-poly DND-style turn-based tactics demo.
The authoritative design documents are in /Docs. Always read them before planning changes.

## Hard boundaries
Do not add open-world systems, equipment affixes, character customization, dialogue trees, full DND rules, behavior trees, complex fog of war, account systems, ranked matchmaking, or internet lobby in the first LAN milestone.
Keep single-player mode intact while adding multiplayer support.
LAN 4-player mode is the target; internet support is only architecture preparation.

## Unity 6.3 rules
Use URP / Universal 3D.
Use uGUI + TextMeshPro for battle UI.
Use Netcode for GameObjects + Unity Transport for LAN multiplayer unless the user changes the stack.
Multiplayer Play Mode is allowed for early testing, but final LAN validation needs builds or separate devices.

## Coding rules
Keep changes small and phase-scoped.
Prefer clear serialized fields over hard-coded scene lookups.
Avoid changing Unity scenes or prefabs unless the task explicitly asks for it.
Never edit Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Architecture rules
Single-player should run without NetworkManager.
Multiplayer should use host-authoritative ActionCommand flow.
The host validates actions; clients request actions.
Turn order in multiplayer: Warrior -> Ranger -> Mage -> Barbarian -> Enemy round.
Exactly four players are required to start LAN multiplayer.

## Testing expectations
For each code change, provide what changed, Unity Inspector setup, Play Mode steps, expected result, risks, and rollback instructions.
