# ACTIVE_TASK.md

## Current phase
Phase 7 - D20 Basic Attack Combat

## Goal
Implement the minimal basic attack combat loop for the Unity 6.3 LTS tactics demo.

This phase should allow a player Unit to attack a target Unit within basic attack range, roll a D20, resolve hit/miss by D20 + attackModifier >= target defense, apply baseDamage on hit, mark the attacker as acted, and release Tile occupancy when a target dies.

## Unity version
Unity 6000.3.10f1 / Unity 6.3 LTS series

## Allowed files
- Assets/_BoneThrone/Scripts/Core/**
- Assets/_BoneThrone/Scripts/Grid/**
- Assets/_BoneThrone/Scripts/Units/**
- Assets/_BoneThrone/Scripts/Movement/**
- Assets/_BoneThrone/Scripts/Turns/**
- Assets/_BoneThrone/Scripts/Combat/**
- Assets/_BoneThrone/Scripts/Tests/**
- Docs/ACTIVE_TASK.md
- Docs/DevLogs/Phase07_D20BasicCombat.md

## Forbidden changes
- Do not implement skills, cooldowns, enemy AI behavior, room progression, fog/shadow rooms, stairs, keys, level switching, UI HUD, LAN multiplayer, lobby, NetworkManager, or Netcode synchronization in this phase.
- Do not implement skill targeting or skill effects.
- Do not implement enemy AI decision making.
- Do not implement formal UI buttons or HUD panels.
- Do not modify Packages, ProjectSettings, Library, Temp, Obj, Logs, UserSettings, or generated IDE files.
- Do not add large art/audio/model assets.
- Do not convert gameplay classes to NetworkBehaviour.
- Do not require NetworkManager.

## Required scope
Codex may propose and implement only a small set of combat-related scripts, preferably 4-6 files.

Expected files may include:
1. D20Roller.cs
   - Rolls 1-20.
   - Can support deterministic seed or debug override only if simple.
   - Does not reference networking.

2. CombatLog.cs
   - Records or prints combat results.
   - Can initially use Debug.Log.
   - Does not implement UI HUD.

3. DamageResolver.cs
   - Applies damage to Unit runtime HP.
   - If HP <= 0, calls existing Unit death/release logic.
   - Does not implement damage types, armor types, buffs, or skills.

4. CombatSystem.cs
   - Validates basic attack range.
   - Rolls D20.
   - Checks D20 + attackModifier >= target defense.
   - Applies baseDamage on hit.
   - Marks attacker UnitTurnState as acted after a valid attack attempt.
   - Does not implement skills, AI, UI, networking.

5. CombatInputTester.cs
   - Temporary Play Mode controller or ContextMenu test helper.
   - Allows selecting attacker and target for manual combat testing.
   - Clearly marked as temporary/test helper.

Optional only if necessary:
6. AttackRangeService.cs
   - Computes simple Manhattan attack distance.
   - Four-direction/grid-distance based.
   - No AOE, no skill range, no line of sight unless explicitly minimal.

## Architecture rules
- Use namespace BoneThrone.Combat for combat scripts.
- Use BoneThrone.Grid, BoneThrone.Units, and BoneThrone.Turns only when necessary.
- Do not reference Netcode.
- Do not inherit NetworkBehaviour.
- Keep combat independent from skills, AI, rooms, levels, UI, and networking.
- Use existing UnitStats fields: attackModifier, defense, baseDamage.
- Use existing UnitRuntimeState currentHp / isDead if available.
- Use existing Unit.MarkDeadAndReleaseTile() or equivalent death method.
- Preserve Phase 5 movement and Phase 6 turn behavior.
- If integrating with UnitTurnState, do it only to MarkActed after a valid attack attempt.
- Do not implement Host authority yet, but keep D20 rolling centralized in D20Roller for future Host authority.

## Acceptance tests in Unity
1. Unity 6.3 LTS opens the project without compile errors.
2. Console has no red compile errors.
3. A player Unit can attack a target Unit within range.
4. D20 roll is logged.
5. Hit is resolved by D20 + attackModifier >= target defense.
6. On hit, target currentHp decreases by attacker baseDamage.
7. On miss, target currentHp does not decrease.
8. If target HP <= 0, target is marked dead and its Tile is released.
9. Attacker UnitTurnState is marked HasActed after a valid attack attempt.
10. A Unit that has already acted cannot attack again if ActionPermissionService is bound.
11. Out-of-range attack is rejected.
12. Dead attacker or dead target cannot participate.
13. No skills, cooldowns, enemy AI behavior, rooms, UI HUD, or networking is implemented.
14. Git status does not include Library, Temp, Obj, Logs, UserSettings, or generated IDE files.

## Expected Codex output for this phase
Codex should first perform a read-only scan and output:
1. Current repository status.
2. Proposed files, limited to 4-6 combat-related files.
3. Responsibility of each file.
4. How D20 rolling is centralized.
5. How hit/miss is resolved using UnitStats.
6. How damage and death reuse existing Unit runtime/death logic.
7. How Phase 6 UnitTurnState is marked acted without implementing skills.
8. Unity scene setup instructions for manual testing.
9. Risks and rollback method.

Codex must not write code until explicitly confirmed.