# 骸骨王座 系统设计文档 Unity 6.3 LTS v2.2 Current Project State

## 0. Document purpose

This document updates the original v2.1 system design into a current-state design document for the real Unity 6.3 LTS project.

It does not claim that future systems are already complete. It separates:

- current implemented state
- test-only / placeholder state
- deferred future state
- Phase 14 safety rules
- do-not-touch rules

The original `骸骨王座_系统设计文档_Unity6.3LTS_v2.1.docx` remains the long-term design baseline. This v2.2 Markdown document is the current authoritative system-state reference for Phase 14 documentation and stabilization.

## 1. Technical baseline

The following v2.1 decisions remain valid as long-term project rules:

- Engine: Unity 6.3 LTS.
- Rendering: URP / Universal 3D.
- Battle UI: uGUI + TextMeshPro.
- LAN target stack: Netcode for GameObjects + Unity Transport.
- LAN testing: Multiplayer Play Mode is allowed for early tests; final LAN validation needs builds or separate devices.
- Networking direction: host-authoritative ActionCommand flow.
- LAN player target: exactly four players before multiplayer start.
- Fixed LAN actor order: Fighter -> Ranger -> Mage -> Barbarian -> Enemy round.
- Internet support remains architecture preparation only, not a first LAN milestone feature.

These are targets and boundaries. They do not mean that LAN lobby, role slots, synchronized combat, or online services are currently implemented.

## 2. Current project truth snapshot

The real project is currently a single-player integrated tactics test flow plus documentation/stabilization work.

Current true state:

- `Assets/_BoneThrone/Scenes/GridTest.unity` is the current only real integrated validation scene.
- `Assets/_BoneThrone/Scenes/MainMenu.unity` exists but is still a placeholder.
- There are no formal `Level_01`, `Level_02`, or `Level_03` scenes yet.
- There is no Boss encounter yet.
- There is no failure screen or victory screen yet.
- There is no LAN lobby, Ready flow, role slots, or NetworkManager gameplay wiring yet.
- Current committed ScriptableObject gameplay data assets are SkillData only.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are not formally assetized yet.
- The four player characters currently implement only Slot 0 representative skills.
- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` exist as architecture skeletons and are not connected to the actual gameplay flow yet.

## 3. Repository structure

Important current project directories:

- `Assets/_BoneThrone/Scripts/Core`: command/session/snapshot skeletons.
- `Assets/_BoneThrone/Scripts/Grid`: square grid, tile state, grid registration.
- `Assets/_BoneThrone/Scripts/Units`: unit identity, stats, runtime HP, tile occupancy.
- `Assets/_BoneThrone/Scripts/Movement`: selection, BFS, A*, movement, debug highlights.
- `Assets/_BoneThrone/Scripts/Turns`: local turn state, fixed role order reservation.
- `Assets/_BoneThrone/Scripts/Combat`: D20, range query, damage, combat log, basic attack.
- `Assets/_BoneThrone/Scripts/AI`: simple enemy action planner.
- `Assets/_BoneThrone/Scripts/Rooms`: room state, shadow, trigger, enemy activation.
- `Assets/_BoneThrone/Scripts/Levels`: placeholder level progression and party level-up.
- `Assets/_BoneThrone/Scripts/Interactables`: key and stairs interactions.
- `Assets/_BoneThrone/Scripts/Skills`: SkillData, runtime cooldowns, targeting, execution, representative effects.
- `Assets/_BoneThrone/Scripts/UI`: BattleHUD, action targeting, feedback, enemy floating HP bar.
- `Assets/_BoneThrone/Scripts/Tests`: ContextMenu test helpers.
- `Assets/_BoneThrone/Scripts/Networking`: currently placeholder only.

## 4. Scenes

### Current implemented scene

`Assets/_BoneThrone/Scenes/GridTest.unity`

Purpose:

- Integrated validation scene for grid, movement, unit placement, turn flags, combat, enemy AI, room shadow, key/stairs progression, representative skills, BattleHUD, combat feedback, and enemy floating HP bars.
- This is the scene that should be used for current Play Mode regression checks.

Current status:

- Real integrated validation scene.
- Not a final production level.
- Contains test rigs and ContextMenu helper flows.

### Placeholder scene

`Assets/_BoneThrone/Scenes/MainMenu.unity`

Purpose:

- Future entry point placeholder.

Current status:

- No formal mode selection flow.
- No LAN create/join UI.
- No role slots or Ready flow.

### Deferred formal scenes

The following formal scenes are not implemented yet:

- `Level_01_EntranceCorridor`
- `Level_02_CourtyardResidence`
- `Level_03_TreasureVault`
- `Lobby`
- Boss / victory / defeat scenes

## 5. Prefabs and visual rules

### Player prefabs

Current project-specific gameplay player prefabs:

- `Assets/_BoneThrone/Prefabs/Units/Players/Fighter.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Players/Barbarian.prefab`

Current common structure:

- Root gameplay components include `Unit`, `UnitTurnState`, `SkillRuntime`, `Rigidbody`, and `MeshCollider`.
- Visuals are nested under a `Visual` child and wrap KayKit source prefabs.
- `Unit.currentTile` is intentionally assigned by scene/runtime placement, not by the prefab asset.

Important visual rule:

- Player Ranger intentionally uses the KayKit Adventurers Rogue visual.
- Do not change Player Ranger back to the Adventurers Ranger visual.

### Enemy prefabs

Current normal enemy prefabs:

- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Warrior.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab`
- `Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab`

Current common structure:

- Root gameplay components include `Unit`, `UnitTurnState`, `Rigidbody`, and `MeshCollider`.
- Visuals are nested under `Visual`.
- Enemy floating HP bars are attached to these ordinary enemy prefabs.
- Prefab `unitId` values are placeholders and must be overridden per scene instance or future spawning flow when multiple enemies exist.

Important enemy naming rule:

- `Skeleton_Rogue` is the normal rogue enemy.
- `Skeleton_Golem` is reserved for a future Boss / heavy Boss.
- Do not use `Skeleton_Golem` as a normal enemy.
- Do not rename `Skeleton_Rogue` back to `Skeleton_Golem`.

### UI prefabs

Current UI prefabs:

- `Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab`
- `Assets/_BoneThrone/Prefabs/UI/EnemyFloatingHealthBar.prefab`

### Interactable prefabs

Current interactable prefabs:

- `Assets/_BoneThrone/Prefabs/Interactables/Key.prefab`
- `Assets/_BoneThrone/Prefabs/Interactables/Stairs.prefab`

### KayKit source asset rule

KayKit original assets under `Assets/_BoneThrone/Art/` are imported source assets. Do not modify KayKit original resources directly. Project-specific gameplay prefabs should wrap those sources instead.

## 6. Data assets and ScriptableObjects

Current committed ScriptableObject gameplay data assets are SkillData only:

- `Assets/_BoneThrone/Data/OnlyTest/Skills/fighter_shield_bash.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/ranger_precision_shot.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/mage_fireball.asset`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/barbarian_heavy_cleave.asset`

Current status:

- `SkillData` type exists and has committed test assets.
- `UnitData` type exists, but no formal UnitData asset set is currently used as the main data pipeline.
- `CharacterData`, `EnemyData`, `LevelData`, and `RoomData` are not formally implemented or assetized yet.
- Current character and enemy stats are mainly serialized on gameplay prefabs / scene instances.

Deferred data goal:

- Move toward data-driven content using CharacterData, EnemyData, LevelData, and RoomData after documentation and stabilization.

## 7. Current implemented systems

### Core skeleton

Implemented:

- `ActionCommand`
- `GameStateSnapshot`
- `IGameSession`
- `LocalGameSession`
- `GameMode`, `RoleId`, `ActionCommandType`, and `GameSessionState`

Current limitation:

- These are architecture skeletons.
- The actual gameplay flow does not yet route all player actions through ActionCommand / IGameSession.

### Grid

Implemented:

- Square `GridPosition`.
- `Tile` walkable / occupied / occupant id state.
- `GridManager` registration and lookup.

### Unit

Implemented:

- Unit identity, display name, role, faction, stats, runtime HP, alive/dead state.
- Tile placement, occupancy, release, and death release.
- Max HP level-up support in `UnitStats`.

### Movement

Implemented:

- Player unit selection.
- Four-direction BFS movement range query.
- Four-direction A* pathfinding.
- Instant movement to final tile.
- Tile occupancy update.
- Debug highlight colors for selected, move, attack, and skill targets.

Current limitation:

- Movement is instant transform placement, not animated path playback.
- Highlighting is material-color debug/prototype behavior.

### Turn

Implemented:

- PlayerTurn / EnemyTurn phase enum.
- Per-unit `hasMoved` and `hasActed`.
- Optional fixed role order reservation.
- Single-player free selection remains supported.

Current limitation:

- EnemyTurn does not run a formal scheduler.
- Cooldown ticking is not fully connected to the turn-end flow.

### Combat

Implemented:

- D20 roll service.
- Basic attack range query.
- D20 hit formula: `D20 + AttackModifier >= Defense`.
- Damage through `DamageResolver`.
- Death through Unit runtime state and tile release.
- Valid basic attack marks acted.
- `CombatLog` structured feedback entries.

Safety rule:

- UI must not directly change HP or death state.
- CombatLog must remain structured-event based and must not be rebuilt by parsing display strings.

### Skills

Implemented:

- SkillData definitions.
- SkillRuntime slots and cooldowns.
- SkillTargetingService validation.
- SkillSystem execution.
- SkillEffectExecutor representative role-skill dispatch.
- Structured SkillEffectResult feedback.

Current Slot 0 skills:

- Fighter: Shield Bash.
- Ranger: Precision Shot.
- Mage: Fireball.
- Barbarian: Heavy Cleave.

Current limitation:

- Only Slot 0 representative skills are implemented.
- Slot 1 and Slot 2 are deferred.
- Skill matching still relies on current display-name matching.
- Fireball splash depends on manually bound `knownUnits`.

### Enemy AI

Implemented:

- Select nearest alive player by Manhattan distance.
- Attack if in basic attack range.
- Move closer if out of range.
- Skip safely when no valid target/path/action exists.

Current limitation:

- Not a formal enemy round scheduler.
- No behavior tree.
- No role-specific enemy AI for Rogue/Mage/Necromancer/Boss.

### Room / level / key / stairs

Implemented:

- Room states: Unentered, Entered, CombatActive, Cleared.
- Room shadow show/hide.
- Pre-placed enemy activation and spawn tile placement.
- Shared key state.
- Stairs interaction with two-click confirmation.
- Placeholder level index/root switching.
- Party level-up on next-level transition.

Current limitation:

- No formal three-floor scene loading.
- No formal room data assets.
- No formal modal confirmation UI.
- No Boss key / Boss door.

### UI and feedback

Implemented:

- BattleHUD.
- Turn banner.
- Hero panels.
- Skill bar.
- Prompt view.
- Combat feedback panel.
- UI action mode controller.
- Enemy floating HP bars.

Current UI action flow:

`UI intent -> UIActionModeController -> gameplay service`

Examples:

- Move calls `PlayerMovementController.TryMoveSelectedUnitTo`.
- Basic Attack calls `CombatSystem.TryBasicAttack`.
- Skill Slot 0 calls `SkillSystem.TryUseSkill`.

Safety rule:

- UI must not directly change HP.
- UI must not directly change cooldown.
- UI must not directly call `MarkActed`.
- UI must not directly call `MarkMoved`.

## 8. Manual Inspector dependencies

The current project still depends on several manually bound arrays and references:

- `GridManager.initialTiles`
- `TurnManager.playerUnits`
- `LevelProgressionService.playerUnits`
- `LevelProgressionService.requiredClearedRooms`
- `RoomEnemyActivator.assignedEnemies`
- `RoomEnemyActivator.spawnTiles`
- `SkillEffectExecutor.knownUnits`
- `BattleHUDController.playerUnits`
- `BattleHUDController.enemyUnits`
- `UIActionModeController.enemyUnits`
- Key / Stairs scene references to progression and selection services

Risk:

- Missing or stale Inspector bindings can break Fireball splash, UI target highlights, room activation, level progression, HUD hero panels, or stairs prompts.

## 9. Test-only / placeholder state

The following are test-only or placeholder:

- `GridTest.unity` is an integrated validation scene, not final production level content.
- `MainMenu.unity` is placeholder.
- ContextMenu tester scripts under `Assets/_BoneThrone/Scripts/Tests/` are validation helpers.
- `GridTestBuilder` is a test helper and uses reflection to assign `GridManager.initialTiles`.
- Networking folder is placeholder only.
- `LocalGameSession` is a skeleton and does not execute gameplay rules.
- `GameStateSnapshot` is a placeholder snapshot and does not yet collect real state.
- Skill Slot 1, Skill Slot 2, Defend, and Potion are UI placeholders.
- Level switching is placeholder index/root switching, not formal scene loading.
- Stairs confirmation is prompt/two-click based, not a formal modal.
- Enemy HP bar colors and HUD layout are prototype readability work, not final art.

## 10. Deferred future state

The following remain future work and must not be documented as complete:

- Formal `Level_01`, `Level_02`, and `Level_03` scenes.
- Data-driven LevelData and RoomData content.
- CharacterData and EnemyData asset pipelines.
- Full three-floor dungeon progression.
- Boss / heavy Boss implementation.
- Victory screen.
- Defeat screen and retry current floor.
- LAN lobby.
- Ready flow.
- Role slots.
- NetworkManager gameplay prefab.
- NetworkBehaviour / RPC / NetworkVariable gameplay synchronization.
- Host-authoritative command router.
- Full ActionCommand execution path.
- Disconnection pause / reconnect UI.
- Skill slots 1 and 2.
- Defend action.
- Potion action.
- Formal modal confirmation for stairs.
- Boss key and Boss door.
- Necromancer / elite enemies.
- Formal animation controllers.
- VFX, SFX, music, final icons, and portraits.

## 11. Mismatches from original v2.1 plan

The v2.1 document described the intended full product. The real project is earlier and narrower.

Important mismatches:

- v2.1 describes three formal floors; current project has only `GridTest.unity`.
- v2.1 describes MainMenu mode selection; current MainMenu is placeholder.
- v2.1 describes LAN Host/Client flow; current networking is not implemented beyond packages and skeleton abstractions.
- v2.1 describes Boss / victory / defeat; current project has none of these.
- v2.1 recommends CharacterData / EnemyData / LevelData / RoomData; current committed gameplay data assets are SkillData only.
- v2.1 describes three skills per hero; current implementation has Slot 0 representative skills only.
- v2.1 describes formal stairs Outline and confirmation box; current stairs use hover material hooks and two-click/prompt behavior.
- v2.1 describes enemies spawning after room entry; current room flow activates pre-placed enemies and places them on assigned spawn tiles.
- v2.1 describes all gameplay actions as ActionCommand flow; current UI and testers call local gameplay services directly.

## 12. Updated architecture direction

Preserve the v2.1 architecture direction:

- Single-player must remain playable without NetworkManager.
- Future multiplayer should be host-authoritative.
- Clients should request actions; host validates actions.
- D20, HP changes, deaths, cooldowns, room state, key state, enemy AI, and win/loss state should eventually be host-owned in LAN.
- Gameplay rules should not depend directly on LAN/IP/Relay transport APIs.

Current bridge to future:

- `ActionCommand`, `IGameSession`, and `GameStateSnapshot` already exist as skeletons.
- Future phases should connect existing local gameplay services to command routing gradually.
- Do not rewrite stable single-player systems just to add networking.

## 13. Phase 14 safety rules

During Phase 14 documentation and stabilization:

- Do not write gameplay code unless a later phase explicitly authorizes it.
- Do not modify scenes unless explicitly requested.
- Do not modify prefabs unless explicitly requested.
- Do not modify ScriptableObject assets unless explicitly requested.
- Do not modify KayKit original assets.
- Do not modify combat formulas casually.
- Do not modify `DamageResolver` casually.
- Do not modify `SkillEffectExecutor` formulas casually.
- Do not change `CombatLog` back to string-parsing behavior.
- Do not call execution APIs only to calculate highlights.
- Do not make UI directly mutate gameplay state.

Execution APIs that must not be used for highlight preview:

- `CombatSystem.TryBasicAttack`
- `SkillSystem.TryUseSkill`
- movement execution methods that consume move state

Use query-style methods for preview/highlight behavior.

## 14. Do-not-touch rules

Do not touch:

- `Skeleton_Golem` as a normal enemy.
- `Skeleton_Rogue` name or normal-enemy identity.
- Player Ranger Adventurers Rogue visual.
- KayKit original source resources.
- `DamageResolver` unless the task explicitly enters combat formula work.
- `SkillEffectExecutor` formulas unless the task explicitly enters skill formula work.
- `CombatLog` structured event model.
- `GridTest.unity` without explicit scene-edit approval.
- Any `Assets/`, `Packages/`, or `ProjectSettings/` file during documentation-only phases.

## 15. Recommended roadmap after Phase 14.2

Recommended order:

1. Phase 14.3: update the Vibecoding development document to match this current-state system design.
2. Phase 14.4: write a stabilization plan without code changes.
3. Phase 14.5: document manual Inspector bindings and regression checklists.
4. Phase 14.6: only after approval, consider small code stabilization tasks such as cooldown turn flow or enemy turn scheduling.
5. Later phases: formalize data assets, content scenes, Boss, victory/defeat, and LAN systems in separate narrow slices.

## 16. Validation notes

This document is documentation-only. No Unity Play Mode validation is required for this phase.

Manual repository validation:

1. Run `git status --short`.
2. Run `git diff -- Docs`.
3. Confirm only Docs files changed.
4. Confirm no `Assets/`, `Packages/`, or `ProjectSettings/` files changed.

Rollback:

- Revert or delete this document if the v2.2 current-state design needs to be regenerated.
- Do not use any Unity asset rollback command unless a later phase actually modifies Unity assets.
