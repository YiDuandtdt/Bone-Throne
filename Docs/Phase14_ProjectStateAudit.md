# Phase 14.1 Current Project State Audit

## 1. Repository / branch summary
当前分支：`phase/14-documentation-and-stabilization`，跟踪 `origin/phase/14-documentation-and-stabilization`。工作区已有用户变更：`Docs/ACTIVE_TASK.md` modified，本轮只读扫描，未修改文件。

重要目录：
- `Assets/_BoneThrone/Scripts/`：核心玩法脚本，已分为 `Core/Grid/Units/Movement/Turns/Combat/AI/Rooms/Levels/Interactables/Skills/UI/Tests/Networking`。
- `Assets/_BoneThrone/Scenes/`：`GridTest.unity`、`MainMenu.unity`。
- `Assets/_BoneThrone/Prefabs/`：玩家、普通敌人、Key、Stairs、BattleHUD、EnemyFloatingHealthBar。
- `Assets/_BoneThrone/Data/OnlyTest/Skills/`：当前唯一已提交 ScriptableObject 数据资产。
- `Docs/DevLogs/`：Phase 00 到 Phase 13 记录完整，Phase 13 UI/feedback 记录很长，是当前真实状态的重要依据。
- `Packages/manifest.json`：URP、TextMeshPro/uGUI、Netcode for GameObjects、Unity Transport、Multiplayer Play Mode/Tools/Center 均已安装。

## 2. Completed systems
已完成到“单机测试闭环 + UI 原型反馈 + prefab wrapping”级别：

- Core 架构骨架：`ActionCommand`、`GameStateSnapshot`、`IGameSession`、`LocalGameSession` 已存在，但目前仍是占位/事件边界，没有驱动实际玩法。
- Grid：`GridPosition`、`Tile`、`GridManager` 完成坐标、walkable、occupancy、注册查询。
- Unit：`Unit`、`UnitStats`、`UnitRuntimeState`、`UnitData`、`UnitFaction` 完成基础属性、HP、死亡、Tile 释放。
- Movement：`SelectionManager`、`MovementRangeFinder` BFS、`Pathfinder` A*、`UnitMover`、`PlayerMovementController`、`MovementDebugHighlighter` 完成选择、四方向移动、占位、高亮。
- Turn：`TurnManager`、`TurnOrderService`、`ActionPermissionService`、`UnitTurnState` 完成 player/enemy phase、moved/acted、固定顺序预留。
- Combat：`D20Roller`、`AttackRangeService`、`DamageResolver`、`CombatSystem`、`CombatLog` 完成 D20 普攻、伤害、死亡、结构化 UI log 事件。
- Enemy AI：最近目标、靠近、普攻、跳过逻辑已完成，但不是正式 EnemyTurn scheduler。
- Room/Level：房间进入、阴影、敌人激活、钥匙、楼梯二次确认、placeholder 切层、自动升级已完成。
- Skill：`SkillData`、`SkillRuntime`、`SkillTargetingService`、`SkillSystem`、`SkillEffectExecutor` 和四职业 slot0 代表技能已完成。
- UI：BattleHUD、TurnBanner、HeroPanel、SkillBar、Prompt、CombatFeedback、UI action targeting、enemy floating HP bar 已完成第一版。

## 3. Scenes
- `Assets/_BoneThrone/Scenes/GridTest.unity`：当前主要集成/测试场景。包含 20x20 左右 Tile 网格、四名玩家、普通敌人、Room_1、Room_1_Shadow、RoomTrigger、Key/Stairs、CombatTestRig、MovementTester、Phase10_Progression、BattleHUD、Canvas、各种 ContextMenu tester。当前相当于“唯一真实 gameplay regression scene”。
- `Assets/_BoneThrone/Scenes/MainMenu.unity`：存在 Main Camera、Directional Light、Global Volume；未看到正式主菜单 UI/模式选择系统。应视为 placeholder/未来入口场景。
- 未发现正式三层场景：`Level_01/02/03`、`Lobby`、Boss 场景等尚未落地。

## 4. Prefabs
Player：
- `Fighter.prefab`：Unit + UnitTurnState + SkillRuntime + Rigidbody + MeshCollider；Knight visual；slot0 `fighter_shield_bash`。
- `Ranger.prefab`：Ranger 玩法身份，但视觉明确使用 Adventurers `Rogue.prefab`，这是当前真实规则，不能改回 Adventurers Ranger。
- `Mage.prefab`：Mage visual；slot0 `mage_fireball`。
- `Barbarian.prefab`：Barbarian visual；slot0 `barbarian_heavy_cleave`。

Enemy：
- `Skeleton_Minion.prefab`
- `Skeleton_Warrior.prefab`
- `Skeleton_Mage.prefab`
- `Skeleton_Rogue.prefab`
四个普通敌人都带 Unit/UnitTurnState/Rigidbody/MeshCollider/Visual/EnemyFloatingHealthBar。未发现项目自有 `Skeleton_Golem` 普通敌人 prefab；KayKit 原始 `Skeleton_Golem.prefab` 只存在于 Art 源资源，应保留给未来 Boss。

UI：
- `BattleHUD.prefab`
- `EnemyFloatingHealthBar.prefab`

Interactable：
- `Key.prefab`
- `Stairs.prefab`

Tile / Room：
- 未发现正式 `Tile.prefab` 或 `Room.prefab` 在 `Assets/_BoneThrone/Prefabs/` 下。当前 Tile/Room 主要是 `GridTest.unity` 场景对象和测试构建器流程。

## 5. Data assets / ScriptableObjects
当前已提交 SO 数据资产只有 SkillData 测试资产：
- `fighter_shield_bash.asset`：id 1，range 1，cooldown 2，guaranteedDamage 3。
- `ranger_precision_shot.asset`：id 2，range 4，cooldown 2，guaranteedDamage 3。
- `barbarian_heavy_cleave.asset`：id 3，range 1，cooldown 2，guaranteedDamage 3。
- `mage_fireball.asset`：id 4，range 3，cooldown 2，guaranteedDamage 3。

现状：
- `SkillData` 类型存在且有资产。
- `UnitData` 类型存在，但未发现正式 UnitData/CharacterData/EnemyData 资产。
- 未发现 `CharacterData`、`EnemyData`、`LevelData`、`RoomData` 类型或资产。
- 当前角色/敌人主要靠 prefab 上的 serialized `UnitStats`，不是设计文档建议的数据资产驱动。

## 6. Grid / movement / selection state
当前网格是 square grid，四方向规则。`MovementRangeFinder` 用 BFS 查可达格；`Pathfinder` 用四方向 A*；`UnitMover` 只做最终落点瞬移和 Tile 占位更新，没有动画路径播放。

选择逻辑：
- 只能选择 alive Player unit。
- 点击已选中单位会清选。
- 直接点 Tile 不再移动；必须通过 HUD 的 `Move` action 进入移动模式。

高亮：
- selected 蓝色。
- move range 绿色。
- basic attack target 红色。
- skill target 黄色。
高亮是 `MovementDebugHighlighter` 直接改 Renderer material color，仍是 debug/prototype 表现，不是最终美术系统。

## 7. Turn / combat state
`TurnOrderService` 固定顺序是 `Fighter -> Ranger -> Mage -> Barbarian -> Enemy`，符合未来 LAN 目标。单机默认仍支持 free select；`ActionPermissionService.requireCurrentRole` 可选开启角色顺序限制。

`CombatSystem`：
- 普攻校验 attacker/target alive、opposing faction、tile、range、turn gate。
- D20 由 `D20Roller` 产生。
- 命中公式：`D20 + AttackModifier >= Defense`。
- 命中伤害：attacker `BaseDamage`。
- `DamageResolver.ApplyDamage` 统一扣 HP；死亡时 `Unit.MarkDeadAndReleaseTile`。
- 合法攻击无论命中/未命中都会 `MarkActed`。
- UI 不直接扣血、不直接 MarkActed。

注意：Enemy AI 调用同一个 `CombatSystem.TryBasicAttack`，如果 TurnManager/ActionPermissionService 绑定为 player-only gate，敌人攻击可能被拒，这是 DevLog 已记录风险。

## 8. Skill state
`SkillData` 定义 id、displayName、unlockLevel、range、cooldown、guaranteedDamage、targetType。`SkillRuntime` 保存 3 个 slot 和 cooldown 数组。`SkillTargetingService` 做 unlock/cooldown/target type/alive/tile/range 校验。

`SkillSystem.TryUseSkill`：
- 校验 turn gate。
- 调 `SkillTargetingService`。
- 调 `SkillEffectExecutor` 或 fallback guaranteed damage。
- 成功后 `StartCooldown`、`MarkActed`。
- 写 CombatLog skill damage/death feedback。
- UI 只调用 `SkillSystem.TryUseSkill(selectedUnit, target, 0)`，不直接改 cooldown 或 acted。

`SkillEffectExecutor`：
- 仍通过 caster `RoleId` + `SkillData.DisplayName` normalize 匹配技能，未来应改为稳定 string id 或 enum id。
- Fighter Shield Bash：`GuaranteedDamage + 1`。
- Ranger Precision Shot：`GuaranteedDamage + 2`。
- Mage Fireball：主目标 `GuaranteedDamage`，相邻有效敌人 splash 1；依赖手动 `knownUnits` 数组。
- Barbarian Heavy Cleave：`GuaranteedDamage + 2`，半血或以下再 +1。
- Phase 13.6-B 已加入 `SkillEffectResult` 结构化 damage entries，Fireball splash 不靠解析字符串写 UI log。

## 9. UI / feedback state
Battle HUD 使用 uGUI + TextMeshPro，符合项目规则。`BattleHUDController` 可运行时生成 UI 层级，也可绑定 prefab view。

当前 UI：
- TurnBanner：显示 Player/Enemy turn 和 actor/free select。
- HeroPanel x4：显示 HP、Level、Moved、Acted、Alive/Dead。
- SkillBar：Move、Basic Attack、Skill Slot 0 可交互；Slot1、Slot2、Defend、Potion 仍是 placeholder。
- Prompt：显示选择、进度条件、楼梯二次确认、targeting/cancel/invalid prompt。
- CombatFeedback：显示 `CombatLog.Entry`，不是解析 Debug 字符串。
- EnemyFloatingHealthBar：挂在四个普通敌人 prefab 下，读取 Unit HP，死亡隐藏，world-space canvas，raycast disabled。

Action UI 安全边界：
- Move 调 `PlayerMovementController.TryMoveSelectedUnitTo`。
- Basic Attack 调 `CombatSystem.TryBasicAttack`。
- Skill Slot 0 调 `SkillSystem.TryUseSkill`。
- UI 不直接扣血、不直接改 cooldown、不直接 MarkActed/MarkMoved；Move 的 MarkMoved 仍在 movement controller 内部完成。

## 10. Room / level / key / stairs state
Room：
- `RoomController` 状态：Unentered、Entered、CombatActive、Cleared。
- `RoomShadowController` 控制 shadowRoot active/inactive，是简单半透明遮罩，不是复杂 fog of war。
- `RoomEnemyActivator` 使用手动 `assignedEnemies` 和 `spawnTiles`，进入房间时 SetActive 并 `TryPlaceOnTile`。
- Room clear 通过 assigned enemies 是否 dead/active 判断。

Level/key/stairs：
- `LevelProgressionService` 管共享 key、required rooms、party level-up、进入下一层条件。
- `KeyItem` 收集后设置 shared key，可 deactivate。
- `InteractableStairs` 支持 hover material feedback 和二次点击确认；还没有正式 modal confirmation box。
- `LevelManager` 是 placeholder root switching/index progression，不是真正三层 scene loading。
- 每次进入下一层，living player units level up，MaxHP +2，HP refill，max level 3。

## 11. Enemy / player visual rules
必须保留的真实规则：
- `Skeleton_Rogue` 是普通敌人 Rogue prefab，不能改名回 `Skeleton_Golem`。
- `Skeleton_Golem` 只存在 KayKit 原始 Art 资源中，应保留为未来 Boss/重型 Boss 候选，不要当普通敌人使用。
- 不要修改 KayKit 原始资源；项目 gameplay prefab 通过 Visual child 包装 KayKit prefab。
- Player `Ranger.prefab` 的玩法身份是 Ranger，但视觉采用 Adventurers Rogue，这是当前最终偏好，不能改回 Adventurers Ranger。
- 四个普通敌人都已有 `EnemyFloatingHealthBar`，但 `Skeleton_Golem` 没有、也不应在 Boss 阶段前动它。

## 12. Placeholder / deferred items
- Networking 文件夹只有 `.gitkeep`；NGO/Transport 包已装，但没有 LAN lobby、NetworkManager prefab、NetworkBehaviour、RPC、NetworkVariable。
- `ActionCommand`/`IGameSession` 存在，但实际玩法仍主要由 MonoBehaviour 直接调用，不是统一 command router。
- MainMenu 是 placeholder；没有 ModeManager、Lobby、role slot、Ready。
- 没有正式三层 LevelData/RoomData/scene loading。
- 没有 Boss、Necromancer、失败/胜利界面、断线暂停。
- Slot1/Slot2/Defend/Potion 是 UI placeholder。
- 技能 cooldown tick 未接入完整 turn-end flow，仍依赖 tester/手动调用。
- EnemyTurn 没有正式 scheduler。
- 楼梯没有正式确认 modal，只有 prompt + 二次点击。
- 没有动画控制器、VFX、SFX、正式 icon/portrait。
- 没有正式 CharacterData/EnemyData/LevelData/RoomData 资产。

## 13. Mismatches with original docs
- 原始文档路线把 Phase 14 定为三层内容配置；当前项目 Phase 14 实际改为 documentation/stabilization audit。
- 文档要求三层地下城、Boss、失败胜利、LAN 四人联机；当前真实工程仍是单机 GridTest 集成场景。
- 文档建议 `CharacterData/EnemyData/SkillData/LevelData/RoomData`；当前只有 SkillData 资产，角色/敌人靠 prefab serialized stats。
- 文档要求所有玩法走 `ActionCommand`；当前 Core 只有骨架，UI/测试器直接调用 gameplay services。
- 文档要求每名角色 3 技能，当前只实现 slot0 代表技能。
- 文档描述 Ranger 攻击范围 4、Mage 3 等；当前 `UnitStats` 没有 attack range 字段，普攻统一由 `AttackRangeService.basicAttackRange` 控制。
- 文档要求楼梯 Outline + 确认框；当前是 hover material + 二次点击 prompt，无正式 modal。
- 文档要求敌人差异化 AI；当前 AI 是最近 alive player + basic attack/move，没有 Rogue/Mage 特化行为。
- 文档要求进入房间后敌人才生成；当前实际是预放 enemies，进入时 SetActive/Place，不是实例化 spawn wave。

## 14. Must update in System Design Document
Phase 14.2 新系统设计文档必须写入：
- 当前真实阶段状态：Phase 0-13 已完成，Phase 14 是文档同步/稳定化，不是内容配置。
- `GridTest.unity` 是当前唯一集成验证场景；MainMenu 仍 placeholder。
- 当前数据策略：SkillData 已有，Character/Enemy/Level/Room data asset 尚未实现。
- 当前 combat 规则和边界：D20、DamageResolver、CombatLog structured entries、UI 禁止直接改 gameplay state。
- 当前 skill 真实公式、Fireball splash 依赖 `knownUnits`，不要解析字符串。
- 当前 UI action flow：UI -> action mode -> gameplay service。
- 当前 visual rules：Ranger uses Rogue visual；Skeleton_Rogue normal enemy；Skeleton_Golem reserved Boss。
- 当前手动 Inspector arrays 风险。
- 当前未实现 networking/lobby/Boss/failure/victory/three-level content。
- Phase 14 后续应先文档真实化，再做 stabilization，不能直接进入 Boss/LAN 大改。

## 15. Must update in Vibecoding Development Document
Phase 14.3 Vibecoding 文档必须更新：
- 阶段路线改为“当前真实项目继续开发路线”，不要再从零重复 Phase 3-13。
- 所有新任务 prompt 必须先读 DevLogs，尤其 Phase12.6 和 Phase13。
- 增加硬性禁止：不要动 Skeleton_Golem、不要改 Ranger visual、不要让 UI 直接扣血/改 cooldown/MarkActed/MarkMoved、不要为高亮调用执行型 API。
- 写入当前真实文件清单和推荐小步任务边界。
- 增加 Inspector 手动绑定清单模板：enemyUnits、knownUnits、playerUnits、assignedEnemies、spawnTiles、requiredClearedRooms、initialTiles。
- 增加 regression checklist：Move/Attack/Skill0/Fireball splash/CombatLog/HP bar/Room/Key/Stairs/LevelUp/EnemyAI。
- 明确文档更新阶段不得修改 scene/prefab/asset/code。

## 16. Recommended next steps
- Phase 14.2：更新新版系统设计文档，冻结“当前真实工程状态 + 新路线”。
- Phase 14.3：更新新版 Vibecoding 开发文档，重写后续 prompt 和验收模板。
- Phase 14.4：只做 stabilization plan，不写代码，确定最小修复优先级。
- Phase 14.5：处理文档指出的低风险稳定化，如测试清单、Inspector 绑定说明、KnownIssues。
- Phase 14.6：再考虑小范围代码稳定化，例如 cooldown turn flow 或 enemy turn scheduler，但必须单独批准。
- Phase 14.7：进入下一阶段前做 Unity Play Mode 回归和 DevLog 更新。

## 17. Risks / do-not-touch list
高风险：
- `DamageResolver`、`SkillEffectExecutor`、四职业 skill formula 文件，后续不要随手改公式。
- `CombatSystem.TryBasicAttack`、`SkillSystem.TryUseSkill` 是执行型 API，不能为了高亮调用。
- `CombatLog` 已结构化，不要回退到解析字符串。
- `GridTest.unity` 是唯一集成场景，后续修改前必须明确授权。
- 手动数组缺漏会导致 Fireball splash、HUD target highlight、room activation、level progression 失效。
- Enemy AI 与 turn gate 的绑定方式会影响敌人攻击。
- Enemy HP bar 会自动替换 stale parent Unit 引用，prefab/scene override 要谨慎。

Do-not-touch：
- KayKit 原始 Art 资源。
- KayKit `Skeleton_Golem.prefab`，Boss 阶段前不要改。
- 项目 `Skeleton_Rogue.prefab`，不要改名。
- `Ranger.prefab` 的 Adventurers Rogue visual，不要改回 Ranger visual。
- `Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab` 的 HP bar 和 Visual child，非明确任务不动。
- `Library/Temp/Obj/Logs/UserSettings/.vs` 和生成 IDE 文件。