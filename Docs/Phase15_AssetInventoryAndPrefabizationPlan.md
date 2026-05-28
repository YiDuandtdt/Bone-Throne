# Phase 15.3 - Asset Inventory and Prefabization Plan

## Goal and Boundaries

Phase 15.3 scans the assets currently imported in the Unity project and turns the actual repository state into a prefabization plan for Phase 15.4 through Phase 15.8.

This phase is planning only. It does not create prefabs, modify scenes, edit materials, change SkillData, or touch gameplay code.

Phase 15.3 follows the Phase 15 rebaseline rule: prefab types must come from the current project scan. Do not assume fixed categories before reading the actual imported assets.

## Scanned Directories

- `Assets/_BoneThrone/Art/`
- `Assets/_BoneThrone/Audio/`
- `Assets/_BoneThrone/Data/`
- `Assets/_BoneThrone/Materials/`
- `Assets/_BoneThrone/Prefabs/`
- `Assets/_BoneThrone/Scenes/`
- `Assets/_BoneThrone/Settings/`
- `Assets/_BoneThrone/Scripts/`
- `Assets/TextMesh Pro/`
- `Assets/` top-level package directories
- `Docs/ACTIVE_TASK.md`
- `Docs/Phase15_Plan.md`
- `Docs/DevLogs/Phase15.1_HealthPotionPrefabAndPickup.md`
- `Docs/DevLogs/Phase15.2_SequentialEnemyTurnActions.md`

## Asset Package Inventory

### Project Art Root

`Assets/_BoneThrone/Art/` currently contains:

- `Animations`
- `Avatar`
- `KayKit - Adventurers (for Unity)`
- `KayKit - Dungeon Remastered Pack (for Unity)`
- `KayKit - Skeletons (for Unity)`

The scanned art tree contains imported models, prefabs, animations, textures, materials, avatar assets, and metadata. The main scanned file types are `.fbx`, `.prefab`, `.anim`, `.png`, `.mat`, `.asset`, and `.mask`.

### KayKit Adventurers

Source path:

- `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/`

Observed content:

- Character prefabs: `Barbarian`, `Barbarian_Large`, `Druid`, `Engineer`, `Knight`, `Mage`, `Ranger`, `Rogue`, `Rogue_Hooded`.
- Accessory prefabs: arrows, axes, bows, crossbows, daggers, staff, wand, shields, quiver, spellbooks, potions, ammo crates, mugs, smoke bomb, turret base, and related variants.

Use rule:

- Treat this package as original third-party source art.
- Do not modify its original prefabs, models, materials, textures, or animation assets.
- If a game-ready object is needed, create a project-owned wrapper or prefab variant under `Assets/_BoneThrone/Prefabs/`.

### KayKit Dungeon Remastered

Source path:

- `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/`

Observed content includes actual scanned prefab names for:

- Structural and layout pieces: `floor_*`, `wall_*`, `ceiling_tile`, `column`, `pillar`, `post`, `scaffold_*`, `stairs*`.
- Dungeon props and dressing: `barrel_*`, `crate_*`, `box_*`, `bucket`, `rocks*`, `rubble_*`, `banner_*`, `bench`, `chair`, `stool`, `table_*`, `shelf*`, `book*`, `bookcase*`, `bed_*`, `plate*`, `candle*`, `torch*`, `keg*`, `trunk_*`, `sword_shield*`.
- Interactable-looking source props: `chest`, `chest_gold`, `chest_large`, `chest_large_gold`, `chest_mimic`, `key`, `key_gold`, `keyring`, `keyring_hanging`, `stairs`.
- Potion bottle source props: `bottle_A_*`, `bottle_B_*`, `bottle_C_*`.

Use rule:

- This package is the main source for Phase 15.4 environment prefabization.
- Do not edit KayKit original assets.
- Project gameplay-ready versions must live under `Assets/_BoneThrone/Prefabs/`.

### KayKit Skeletons

Source path:

- `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/`

Observed content:

- Character prefabs: `Skeleton_Golem`, `Skeleton_Mage`, `Skeleton_Minion`, `Skeleton_Necromancer`, `Skeleton_Rogue`, `Skeleton_Warrior`.
- Accessory prefabs: `Skeleton_Arrow`, `Skeleton_Axe`, `Skeleton_Blade`, `Skeleton_Crossbow`, `Skeleton_Dagger`, `Skeleton_Golem_Axe`, `Skeleton_Mace`, `Skeleton_Quiver`, `Skeleton_Scythe`, `Skeleton_Shield_*`, `Skeleton_Staff`, and related variants.

Use rule:

- `Skeleton_Golem` is reserved for a Boss or heavy boss role and must not be used as a normal enemy prefab.
- `Skeleton_Rogue` is the ordinary Rogue enemy naming.
- Do not modify KayKit Skeletons original assets.

## Project-Owned Prefab Inventory

### Interactables

Path:

- `Assets/_BoneThrone/Prefabs/Interactables/`

Current prefabs:

- `HealthPotion.prefab`
- `Key.prefab`
- `Stairs.prefab`

Notes:

- `HealthPotion.prefab` is the Phase 15.1 pickup prefab and uses project-owned pickup behavior with a KayKit bottle visual and project-owned red material.
- `Key.prefab` and `Stairs.prefab` are existing project interactables and are candidates for Phase 15.5 review.

### UI

Path:

- `Assets/_BoneThrone/Prefabs/UI/`

Current prefabs:

- `BattleHUD.prefab`
- `EnemyFloatingHealthBar.prefab`

Notes:

- UI prefabs are recorded for completeness.
- They are not Phase 15.4 environment candidates.

### Enemy Units

Path:

- `Assets/_BoneThrone/Prefabs/Units/Enemies/`

Current prefabs:

- `Skeleton_Mage.prefab`
- `Skeleton_Minion.prefab`
- `Skeleton_Rogue.prefab`
- `Skeleton_Warrior.prefab`

Notes:

- These are Phase 15.6 character prefab completion candidates.
- `Skeleton_Rogue` remains the ordinary Rogue enemy naming.
- `Skeleton_Golem` is not present as a project-owned normal enemy prefab and must remain reserved for Boss or heavy boss planning.

### Player Units

Path:

- `Assets/_BoneThrone/Prefabs/Units/Players/`

Current prefabs:

- `Barbarian.prefab`
- `Fighter.prefab`
- `Mage.prefab`
- `Ranger.prefab`

Notes:

- These are Phase 15.6 character prefab completion candidates.
- Ranger gameplay identity remains `Ranger`, even if a Rogue visual source is used or inspected later.

## Scene Inventory

Path:

- `Assets/_BoneThrone/Scenes/`

Current scenes:

- `GridTest.unity`
- `MainMenu.unity`

Scene rules:

- `GridTest.unity` remains the regression baseline scene.
- `GridTest.unity` must not be converted into a formal level.
- `MainMenu.unity` is recorded as existing scene state only.
- Phase 15.3 does not modify any scene.
- Formal `Level_01`, `Level_02`, and `Level_03` scenes do not start until Phase 15.13.

## Materials Inventory

Path:

- `Assets/_BoneThrone/Materials/`

Current project-owned material:

- `HealthPotion_Red.mat`

Rules:

- `HealthPotion_Red.mat` is a project-owned material override created for the Phase 15.1 Health Potion visual.
- Do not modify KayKit original materials.
- Future material overrides should live under `Assets/_BoneThrone/Materials/` or another project-owned `_BoneThrone` path, not inside KayKit source directories.

## Data and SkillData Inventory

Paths:

- `Assets/_BoneThrone/Data/Skills/`
- `Assets/_BoneThrone/Data/OnlyTest/Skills/`

Observed SkillData assets include class skill assets for Barbarian, Fighter, Mage, and Ranger in both the active `Skills` folder and the `OnlyTest/Skills` folder.

Rules:

- Phase 15.3 records SkillData state only.
- Do not modify SkillData in Phase 15.3.
- Phase 15.4 through 15.8 should not change skill balance or skill data unless a later phase explicitly authorizes it.

## Animation, Avatar, and Controller Inventory

Paths:

- `Assets/_BoneThrone/Art/Animations/`
- `Assets/_BoneThrone/Art/Avatar/`

Observed content:

- `.anim` animation clips are present for general motion, movement, melee combat, ranged combat, spellcasting, unarmed actions, hit/death, item use, pickup, and skeleton-specific actions.
- `Assets/_BoneThrone/Art/Avatar/CharacterAvatar.asset` exists.
- Animation source FBX and mannequin assets also exist under the animation art tree.

Rules:

- Phase 15.3 does not create or modify Animator Controllers.
- Animation controller and state machine work belongs to Phase 15.8.
- Gameplay integration with movement, combat, skills, and potion actions belongs to Phase 15.9.

## Audio and Settings Inventory

### Audio

Path:

- `Assets/_BoneThrone/Audio/`

Current state:

- No gameplay audio assets were found beyond placeholder repository files.

### Settings

Path:

- `Assets/_BoneThrone/Settings/`

Current state:

- No project-specific setting assets were found beyond placeholder repository files.

## Phase 15.4 - Environment Prefabization Candidates

Phase 15.4 should only create project-owned environment wrappers or prefab variants from environment assets that were actually found in `KayKit - Dungeon Remastered Pack (for Unity)`.

Candidate source groups from the scan:

### Dungeon Layout Pieces

- Source path: `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/`
- Examples from scan: `floor_*`, `wall_*`, `ceiling_tile`, `column`, `pillar`, `post`, `scaffold_*`, `stairs*`
- Suggested target path: `Assets/_BoneThrone/Prefabs/Environment/`
- Wrapper or variant: Project-owned wrapper or prefab variant.
- Dependencies: Mesh/visual from KayKit source prefab; optional collider if needed for validation.
- Validation: Instantiate in a temporary controlled test context or inspect prefab composition without modifying formal scenes unless the phase explicitly allows a validation scene change.
- Risks: Overbuilding categories, editing KayKit source assets, inconsistent pivots/colliders, creating gameplay blockers too early.

### Dungeon Dressing Props

- Source path: `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/`
- Examples from scan: `barrel_*`, `crate_*`, `box_*`, `bucket`, `rocks*`, `rubble_*`, `banner_*`, `bench`, `chair`, `stool`, `table_*`, `shelf*`, `book*`, `bookcase*`, `bed_*`, `plate*`, `candle*`, `torch*`, `keg*`, `trunk_*`, `sword_shield*`
- Suggested target path: `Assets/_BoneThrone/Prefabs/Environment/Props/` or another project-owned environment path chosen in Phase 15.4.
- Wrapper or variant: Project-owned wrapper or prefab variant.
- Dependencies: KayKit visual prefab only unless Phase 15.4 explicitly chooses simple static colliders.
- Validation: Confirm prefab opens cleanly, visual renders, and no KayKit source assets are modified.
- Risks: Mixing interactable-looking props with static environment props; future phases may need to move some props into Interactables.

Phase 15.4 should not process Characters, Weapons, Interactables, Animation Controllers, SkillData, LAN, Boss, or formal Level scenes.

## Phase 15.5 - Interactable Prefab Completion Candidates

Phase 15.5 should review project-owned interactables and interactable-looking source props from the scan.

### Existing Project Interactables

- Source path: `Assets/_BoneThrone/Prefabs/Interactables/`
- Current prefabs: `HealthPotion.prefab`, `Key.prefab`, `Stairs.prefab`
- Suggested target path: Keep under `Assets/_BoneThrone/Prefabs/Interactables/`
- Wrapper or variant: Already project-owned prefabs.
- Dependencies: Existing interactable scripts in `Assets/_BoneThrone/Scripts/Interactables/` and related systems.
- Validation: Verify click/pickup/transition behavior in the approved test context for that phase.
- Risks: Accidentally changing Phase 15.1 Health Potion behavior, consuming actions on pickup, or converting GridTest into a formal level.

### Interactable-Looking Dungeon Sources

- Source path: `Assets/_BoneThrone/Art/KayKit - Dungeon Remastered Pack (for Unity)/Prefabs/`
- Examples from scan: `chest*`, `key*`, `keyring*`, `stairs*`, `bottle_*`
- Suggested target path: `Assets/_BoneThrone/Prefabs/Interactables/`
- Wrapper or variant: Project-owned wrapper or prefab variant.
- Dependencies: Existing interactable scripts or later small scripts only if explicitly authorized by Phase 15.5.
- Validation: Prefab inspector review plus minimal Play Mode checks when Phase 15.5 authorizes implementation.
- Risks: Implementing door, chest, or supply point systems too early; modifying KayKit source prefabs; duplicating Health Potion behavior.

Do not implement door, chest, or supply point behavior in Phase 15.3. Do not pre-commit to interactable types that were not scanned.

## Phase 15.6 - Character Prefab Completion Candidates

### Player Prefabs

- Source path: `Assets/_BoneThrone/Prefabs/Units/Players/`
- Current prefabs: `Barbarian.prefab`, `Fighter.prefab`, `Mage.prefab`, `Ranger.prefab`
- Visual source candidates: KayKit Adventurers character prefabs.
- Suggested target path: Keep under `Assets/_BoneThrone/Prefabs/Units/Players/`
- Wrapper or variant: Existing project-owned prefabs.
- Dependencies: Existing Unit, movement, selection, health, potion, and skill references.
- Validation: Confirm all four players can spawn/select/move/act/end turn under the single-player free-order PlayerTurn rule.
- Risks: Breaking Phase 14 combat baseline, changing Ranger identity, removing required components.

### Enemy Prefabs

- Source path: `Assets/_BoneThrone/Prefabs/Units/Enemies/`
- Current prefabs: `Skeleton_Mage.prefab`, `Skeleton_Minion.prefab`, `Skeleton_Rogue.prefab`, `Skeleton_Warrior.prefab`
- Visual source candidates: KayKit Skeletons character prefabs.
- Suggested target path: Keep under `Assets/_BoneThrone/Prefabs/Units/Enemies/`
- Wrapper or variant: Existing project-owned prefabs.
- Dependencies: Existing Unit, health, movement, enemy AI, status, and combat references.
- Validation: Confirm each enemy can participate in Phase 15.2 sequential EnemyTurn without fixed-order player changes.
- Risks: Treating `Skeleton_Golem` as a normal enemy, renaming `Skeleton_Rogue`, changing EnemyAI behavior.

Rules:

- `Skeleton_Golem` remains Boss or heavy boss reserve.
- `Skeleton_Rogue` is the ordinary Rogue enemy naming.
- Ranger gameplay identity remains `Ranger`.

## Phase 15.7 - Weapon / Equipment Attachment Candidates

Phase 15.7 should use only scanned weapon and equipment assets.

### Adventurer Equipment

- Source path: `Assets/_BoneThrone/Art/KayKit - Adventurers (for Unity)/Prefabs/Accessories/`
- Examples from scan: `axe_*`, `bow*`, `crossbow_*`, `dagger`, `staff`, `wand`, `shield_*`, `quiver`, `spellbook_*`, `arrow_*`
- Suggested target path: `Assets/_BoneThrone/Prefabs/Equipment/` or character-local child objects inside project-owned character prefabs, decided in Phase 15.7.
- Wrapper or variant: Project-owned wrapper or prefab variant.
- Dependencies: Character hand/socket transforms, visual-only attachment rules, later animation checks.
- Validation: Inspect equipment placement on target character prefabs without changing skill behavior.
- Risks: Misaligned sockets, weapon clipping, changing gameplay stats when the pass should be visual/attachment focused.

### Skeleton Equipment

- Source path: `Assets/_BoneThrone/Art/KayKit - Skeletons (for Unity)/Prefabs/Accessories/`
- Examples from scan: `Skeleton_Axe`, `Skeleton_Blade`, `Skeleton_Crossbow`, `Skeleton_Dagger`, `Skeleton_Mace`, `Skeleton_Quiver`, `Skeleton_Scythe`, `Skeleton_Shield_*`, `Skeleton_Staff`
- Suggested target path: `Assets/_BoneThrone/Prefabs/Equipment/Enemies/` or enemy-local child objects inside project-owned enemy prefabs, decided in Phase 15.7.
- Wrapper or variant: Project-owned wrapper or prefab variant.
- Dependencies: Enemy hand/socket transforms, enemy visual identity, later animation checks.
- Validation: Inspect enemy prefabs and run a minimal EnemyTurn visual check only when Phase 15.7 authorizes prefab edits.
- Risks: Using `Skeleton_Golem_Axe` for normal enemies, breaking existing enemy prefab references.

## Phase 15.8 - Animation Controller Candidates

Phase 15.8 should be based on existing animation resources only.

Candidate animation groups from the scan:

- Source path: `Assets/_BoneThrone/Art/Animations/`
- General: idle, hit, death, interact, pickup, use item, spawn.
- Movement: walking, running, dodge, jump.
- Combat melee: one-handed, two-handed, block, unarmed.
- Combat ranged: bow, crossbow, magic, shooting, aiming.
- Spellcasting: long cast, raise, shoot, summon, casting loops.
- Skeleton-specific: skeleton death, awaken, inactive pose, spawn, skeleton walk.
- Avatar path: `Assets/_BoneThrone/Art/Avatar/CharacterAvatar.asset`
- Suggested target path: `Assets/_BoneThrone/Art/AnimatorControllers/` or another project-owned controller path chosen in Phase 15.8.
- Wrapper or variant: Project-owned Animator Controllers and avatar/controller assignments.
- Dependencies: Existing character prefabs, Animator components, movement/combat/skill/potion event hooks.
- Validation: Animator state transitions in Unity Play Mode after Phase 15.8 implementation.
- Risks: Creating controllers before character prefab completion, breaking existing idle visuals, mismatching rig/avatar types.

Phase 15.8 does not perform Phase 15.9 gameplay integration.

## Global Prohibitions for This Plan

- Do not modify KayKit original resources.
- Do not modify C# gameplay code.
- Do not modify scenes.
- Do not modify prefabs.
- Do not modify materials.
- Do not modify SkillData.
- Do not create `Level_01`, `Level_02`, or `Level_03`.
- Do not change the `GridTest.unity` regression baseline.
- Do not convert `GridTest.unity` into a formal level.
- Do not change the single-player free-order PlayerTurn rule.
- Do not use Fighter -> Ranger -> Mage -> Barbarian fixed-order for single-player.

## Git Check After Phase 15.3

Expected changed files:

- `Docs/Phase15_AssetInventoryAndPrefabizationPlan.md`
- `Docs/DevLogs/Phase15.3_AssetInventoryAndPrefabizationPlan.md`
- `Docs/ACTIVE_TASK.md`

Recommended checks:

```powershell
git status --short
git diff -- Docs/Phase15_AssetInventoryAndPrefabizationPlan.md
git diff -- Docs/DevLogs/Phase15.3_AssetInventoryAndPrefabizationPlan.md
git diff -- Docs/ACTIVE_TASK.md
```

## Rollback

Because Phase 15.3 is docs-only, rollback should only affect the three allowed documentation files:

```powershell
git restore -- Docs/ACTIVE_TASK.md
git restore -- Docs/Phase15_AssetInventoryAndPrefabizationPlan.md
git restore -- Docs/DevLogs/Phase15.3_AssetInventoryAndPrefabizationPlan.md
```

If the two Phase 15.3 documents are untracked, delete those untracked files after confirming with `git status --short`.
