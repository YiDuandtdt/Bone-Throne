# Phase 14.2 DevLog - System Design Document Update

## Date
2026-05-27

## Branch
phase/14-documentation-and-stabilization

## Scope
Documentation-only update for the current project system design state.

No gameplay code, Unity scenes, prefabs, ScriptableObject assets, packages, project settings, or KayKit source assets were modified.

## Files changed
- `Docs/ACTIVE_TASK.md`
- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`
- `Docs/DevLogs/Phase14.2_SystemDesignDocumentUpdate.md`

## Summary
Updated `Docs/ACTIVE_TASK.md` from Phase 14.1 to Phase 14.2 and rewrote it as UTF-8 Markdown because the existing file was not valid UTF-8 for patch-based editing.

Added a new current-state system design document:

- `Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md`

The new document keeps the Unity 6.3 LTS / URP / uGUI + TextMeshPro / NGO + Unity Transport direction from v2.1, but separates long-term goals from current implementation.

## Current state facts documented
- `GridTest.unity` is the current only real integrated validation scene.
- `MainMenu.unity` is still a placeholder.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes yet.
- There is no Boss, failure/victory screen, or LAN lobby yet.
- Current committed ScriptableObject gameplay data assets are SkillData only.
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

## v2.1 goals preserved as long-term direction
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

## Deferred items clarified
- Formal three-floor scenes.
- LAN lobby and gameplay synchronization.
- NetworkManager gameplay wiring.
- Full ActionCommand execution path.
- Boss, victory, defeat, reconnect, and retry flows.
- CharacterData / EnemyData / LevelData / RoomData asset pipelines.
- Skill slots 1 and 2.
- Defend and Potion actions.
- Formal stairs modal and final UI art.
- VFX, SFX, animation controllers, final icons, and portraits.

## Validation
No Unity Play Mode validation is required for this phase.

Manual repository validation:
1. Run `git status --short`.
2. Run `git diff -- Docs`.
3. Confirm only Docs files changed.
4. Confirm no `Assets/`, `Packages/`, or `ProjectSettings/` files changed.

## Rollback
To revert this documentation-only phase:

```powershell
git checkout -- Docs/ACTIVE_TASK.md
git rm Docs/骸骨王座_系统设计文档_Unity6.3LTS_v2.2_CurrentProjectState.md
git rm Docs/DevLogs/Phase14.2_SystemDesignDocumentUpdate.md
```
