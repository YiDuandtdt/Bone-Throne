using BoneThrone.Turns;
using BoneThrone.Units;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays a single player unit's readable battle status.
    /// </summary>
    public sealed class HeroPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;

        public void Bind(TMP_Text text)
        {
            statusText = text;
        }

        public void Refresh(Unit unit)
        {
            Refresh(unit, null);
        }

        public void Refresh(Unit unit, Unit selectedUnit)
        {
            if (statusText == null)
            {
                return;
            }

            if (unit == null)
            {
                statusText.text = "角色：未绑定\n生命：-- / --\n等级：--\n移动：-- | 行动：--";
                return;
            }

            string displayName = string.IsNullOrEmpty(unit.DisplayName) ? unit.RoleId.ToString() : unit.DisplayName;
            string currentHp = unit.RuntimeState != null ? unit.RuntimeState.CurrentHp.ToString() : "--";
            string maxHp = unit.Stats != null ? unit.Stats.GetClampedMaxHp().ToString() : "--";
            string level = unit.Stats != null ? unit.Stats.Level.ToString() : "--";
            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            string moved = turnState != null ? turnState.HasMoved.ToString() : "--";
            string acted = turnState != null ? turnState.HasActed.ToString() : "--";
            string turnStatus = FormatTurnStatus(unit, selectedUnit, turnState);
            string life = unit.IsAlive ? "存活" : "死亡";

            statusText.text = displayName
                + "\n生命：" + currentHp + " / " + maxHp
                + "\n等级：" + level
                + "\n移动：" + moved + " | 行动：" + acted
                + "\n回合：" + turnStatus
                + "\n状态：" + life;
        }

        private static string FormatTurnStatus(Unit unit, Unit selectedUnit, UnitTurnState turnState)
        {
            if (unit == null || turnState == null)
            {
                return "--";
            }

            if (turnState.HasEnded)
            {
                return "已结束";
            }

            if (unit == selectedUnit || turnState.HasMoved || turnState.HasActed)
            {
                return "进行中";
            }

            return "未开始";
        }
    }
}
