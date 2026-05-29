继续执行 Unity 6.3 LTS 项目《Bone Throne / 骸骨王座》。

当前 Phase 15.8 - Character Animator Controllers and Animation State Machines 已完成并提交。

现在请只更新文档任务状态，不要修改任何 code / prefab / scene / material / SkillData。

允许修改文件：

* Docs/ACTIVE_TASK.md

目标：
将当前阶段更新为：

Phase 15.9 - Animation Integration with Movement / Combat / Skill / Potion

请在 ACTIVE_TASK.md 中写清：

## Current phase

Phase 15.9 - Animation Integration with Movement / Combat / Skill / Potion

## Goal

Connect the Phase 15.8 Animator Controllers to runtime presentation so units can visually respond to movement, basic attack, skill cast, hit, defend, potion use, and death. Animation must remain presentation-only and must not drive gameplay logic.

## Allowed files

Phase 15.9 方案阶段先不允许改文件。后续实现阶段可能涉及：

* Assets/_BoneThrone/Scripts/Units/UnitAnimationController.cs
* Assets/_BoneThrone/Scripts/Movement/* only if needed for MoveSpeed presentation
* Assets/_BoneThrone/Scripts/Combat/* only if needed to trigger BasicAttack / Hit presentation
* Assets/_BoneThrone/Scripts/Skills/* only if needed to trigger Skill presentation
* Assets/_BoneThrone/Scripts/Items/* only if needed to trigger UsePotion presentation
* Assets/_BoneThrone/Prefabs/Units/Players/*.prefab only if UnitAnimationController must be attached
* Assets/_BoneThrone/Prefabs/Units/Enemies/*.prefab only if UnitAnimationController must be attached
* Assets/_BoneThrone/Prefabs/Units/Boss/*.prefab only if UnitAnimationController must be attached
* Docs/DevLogs/Phase15.9_AnimationIntegration.md

## Forbidden changes

* Do not modify gameplay rules.
* Do not let animation events drive damage, cooldown, MarkMoved, MarkActed, End Turn, death, or turn progression.
* Do not modify CombatSystem damage rules.
* Do not modify SkillSystem resolution rules.
* Do not modify TurnManager turn rules.
* Do not modify PotionSystem potion rules.
* Do not modify SkillData.
* Do not modify KayKit source assets.
* Do not modify GridTest.unity.
* Do not create formal Level scenes.
* Do not modify LAN / Networking.
* Do not add animation events.
* Do not treat animation as authority; animation is presentation only.

## Notes

Phase 15.8 created:

* BT_Player_Medium.controller
* BT_Skeleton_Medium.controller
* BT_Boss_Large.controller

Phase 15.9 should first produce an implementation plan before writing code.

完成后输出：

1. 实际修改文件。
2. ACTIVE_TASK.md 改了什么。
3. 明确说明没有修改 code / prefab / scene / material / SkillData。
4. 下一步 Phase 15.9 方案 Prompt 建议。
