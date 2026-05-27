# ACTIVE_TASK.md

## Current phase
Phase 14.2 - System Design Document Update

## Goal
Update the Bone Throne Unity 6.3 LTS system design documentation so it reflects the current real project state instead of the original from-zero plan.

This phase must clearly distinguish:
- current implemented state
- test-only / placeholder state
- deferred future state
- Phase 14 safety rules
- do-not-touch rules

## Required sources
Codex must use:
- `AGENTS.md`
- `Docs/Phase14_ProjectStateAudit.md` when readable, without copying any corrupted text
- `Docs/DevLogs/`
- original system design document `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.1.docx`
- current repository scan results from Phase 14.1

## Allowed work
Documentation-only updates under `Docs/`.

Allowed files for this phase:
- `Docs/ACTIVE_TASK.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/DevLogs/Phase14.2_SystemDesignDocumentUpdate.md`

## Forbidden changes
- Do not write gameplay code.
- Do not modify any file under `Assets/_BoneThrone/Scripts/`.
- Do not modify Unity scenes.
- Do not modify prefabs.
- Do not modify ScriptableObject assets.
- Do not modify `Assets/`, `Packages/`, or `ProjectSettings/`.
- Do not modify the original v2.1 `.docx`.
- Do not modify combat formulas.
- Do not modify `DamageResolver`.
- Do not modify `SkillEffectExecutor`.
- Do not modify `TurnManager`.
- Do not modify Room / Level systems.
- Do not modify Networking.
- Do not rename `Skeleton_Golem`.
- Do not use `Skeleton_Golem` as a normal enemy.
- Do not rename `Skeleton_Rogue` back to `Skeleton_Golem`.
- Do not change Player Ranger visual back to Adventurers Ranger.
- Do not modify KayKit original assets.

## Required documentation facts
The updated system design document must state:
- `GridTest.unity` is the current only real integrated validation scene.
- `MainMenu.unity` is still a placeholder.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes yet.
- There is no Boss, failure/victory screen, or LAN lobby yet.
- Current committed ScriptableObject data assets are SkillData only.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are not formally assetized yet.
- The four player characters currently implement only Slot 0 representative skills.
- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` are architecture skeletons and are not connected to the actual gameplay flow yet.
- Current UI action flow is `UI intent -> UIActionModeController -> gameplay service`.
- UI must not directly change HP, cooldown, `MarkActed`, or `MarkMoved`.
- `CombatLog` uses structured events and must not be implemented by parsing strings.
- Fireball splash uses structured `SkillEffectResult`.
- Fireball splash and UI target highlights still rely on manually bound Inspector arrays such as `knownUnits` and `enemyUnits`.
- `Skeleton_Rogue` is the normal rogue enemy.
- `Skeleton_Golem` is reserved for a future Boss / heavy Boss and must not be used as a normal enemy.
- Player Ranger intentionally uses the Adventurers Rogue visual and must not be changed back to Adventurers Ranger.
- KayKit original assets must not be modified.

## Long-term goals to preserve but mark as not yet complete
- Unity 6.3 LTS.
- URP / Universal 3D.
- uGUI + TextMeshPro.
- Netcode for GameObjects + Unity Transport.
- Host-authoritative ActionCommand direction.
- LAN four-player target.
- Fighter -> Ranger -> Mage -> Barbarian -> Enemy turn order.
- Formal three-floor dungeon content.
- Boss / Victory / Defeat.
- MainMenu / Lobby / Ready / Role Slots.
- CharacterData / EnemyData / LevelData / RoomData data-driven content.

## Unity validation
No Play Mode validation is required in this documentation-only phase.

Manual validation:
1. Run `git status --short`.
2. Run `git diff -- Docs`.
3. Confirm only the allowed Docs files changed.
4. Confirm no `Assets/`, `Packages/`, or `ProjectSettings/` files changed.