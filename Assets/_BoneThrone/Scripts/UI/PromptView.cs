using BoneThrone.Core;
using BoneThrone.Interactables;
using BoneThrone.Levels;
using BoneThrone.Units;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays lightweight local prompts derived from readable gameplay state.
    /// </summary>
    public sealed class PromptView : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;

        private string overrideText;
        private float overrideExpiresAt = -1f;

        public void Bind(TMP_Text text)
        {
            promptText = text;
        }

        public void ShowOverride(string message)
        {
            ShowOverride(message, 0f);
        }

        public void ShowOverride(string message, float duration)
        {
            overrideText = ToPlayerText(message);
            overrideExpiresAt = duration > 0f ? Time.time + duration : -1f;
            SetPrompt(overrideText);
        }

        public void ClearOverride()
        {
            overrideText = null;
            overrideExpiresAt = -1f;
        }

        public void Refresh(Unit selectedUnit, LevelProgressionService progressionService, InteractableStairs stairs)
        {
            if (promptText == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(overrideText))
            {
                if (overrideExpiresAt <= 0f || Time.time < overrideExpiresAt)
                {
                    promptText.text = overrideText;
                    return;
                }

                ClearOverride();
            }

            if (stairs != null && stairs.ConfirmationPending)
            {
                promptText.text = "楼梯：再次点击即可进入下一层。";
                return;
            }

            if (progressionService != null)
            {
                string reason;
                if (!progressionService.CanEnterNextLevel(out reason))
                {
                    promptText.text = "进度：" + ToPlayerText(reason);
                    return;
                }
            }

            if (selectedUnit == null)
            {
                promptText.text = "请选择一名玩家角色。";
                return;
            }

            promptText.text = "当前选择：" + BoneThroneTextUtility.GetUnitDisplayName(selectedUnit) + "。";
        }

        private void SetPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
            }
        }

        private static string ToPlayerText(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            switch (BoneThroneTextUtility.NormalizeKey(message))
            {
                case "thepartydoesnothavethesharedkey":
                    return "队伍还没有获得共用钥匙。";
                case "progressionconditionssatisfied":
                    return "进度条件已满足。";
                case "aleveltransitionisalreadyinprogress":
                    return "正在切换关卡，请稍等。";
                case "selectaunitfirst":
                case "selectaplayerunitfirst":
                    return "请先选择一名玩家角色。";
                case "selectamovetile":
                    return "请选择一个移动目标格。";
                case "selectanenemytarget":
                    return "请选择一个敌方目标。";
                case "selectaskilltarget":
                    return "请选择一个技能目标。";
                case "invalidmovetarget":
                    return "无效移动目标。";
                case "invalidattacktarget":
                    return "无效攻击目标。";
                case "invalidskilltarget":
                    return "无效技能目标。";
                case "freechoice":
                    return "自由选择。";
                case "selectedunitisnotaplayerunit":
                    return "当前选择的不是玩家角色。";
                case "selectedunitisdead":
                    return "当前角色已经倒下。";
                case "selectedunithasnoturnstate":
                    return "当前角色缺少回合状态。";
                case "selectedunithasalreadyended":
                    return "当前角色本回合已经结束。";
                case "selectedunithasalreadyacted":
                    return "当前角色本回合已经行动过。";
                case "selectedunithasalreadymoved":
                    return "当前角色本回合已经移动过。";
                case "moveunavailableactionmodeunbound":
                    return "移动暂不可用：行动模式未绑定。";
                case "moveunavailablemovementcontrollermissing":
                    return "移动暂不可用：移动控制器未绑定。";
                case "moveunavailablegridsystemmissing":
                    return "移动暂不可用：网格系统未绑定。";
                case "moveunavailablecameramissing":
                    return "移动暂不可用：摄像机未绑定。";
                case "basicattackunavailableactionmodeunbound":
                    return "普通攻击暂不可用：行动模式未绑定。";
                case "basicattackunavailablecombatsystemmissing":
                    return "普通攻击暂不可用：战斗系统未绑定。";
                case "basicattackunavailablecameramissing":
                    return "普通攻击暂不可用：摄像机未绑定。";
                case "skillunavailableactionmodeunbound":
                    return "技能暂不可用：行动模式未绑定。";
                case "skillunavailableskillsystemmissing":
                    return "技能暂不可用：技能系统未绑定。";
                case "noskillruntimeoncaster":
                case "casterhasnoskillruntimecomponent":
                    return "当前角色没有技能组件。";
                case "skilllocked":
                    return "技能尚未解锁。";
                case "skilloncooldown":
                    return "技能正在冷却。";
                case "demodefeat":
                    return "演示失败。";
                case "demovictory":
                    return "演示胜利。";
                case "targetisoutofbasicattackrange":
                    return "目标超出普通攻击范围。";
                case "targetisoutofskillrange":
                    return "目标超出技能范围。";
                default:
                    return EnsureChinesePunctuation(message);
            }
        }

        private static string EnsureChinesePunctuation(string message)
        {
            if (message.EndsWith("。", System.StringComparison.Ordinal)
                || message.EndsWith("！", System.StringComparison.Ordinal)
                || message.EndsWith("？", System.StringComparison.Ordinal)
                || message.EndsWith(".", System.StringComparison.Ordinal)
                || message.EndsWith("!", System.StringComparison.Ordinal)
                || message.EndsWith("?", System.StringComparison.Ordinal))
            {
                return message;
            }

            return message + "。";
        }
    }
}
