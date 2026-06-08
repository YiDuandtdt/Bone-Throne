using System.Collections.Generic;
using BoneThrone.Combat;
using TMPro;
using UnityEngine;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays recent CombatLog feedback entries as readable HUD text.
    /// </summary>
    public sealed class CombatFeedbackView : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;
        [SerializeField] private int maxVisibleEntries = 10;
        [SerializeField] [Min(0f)] private float lineSpacing = 16f;

        private readonly List<string> visibleEntries = new List<string>();

        public void Bind(TMP_Text text)
        {
            logText = text;
            ConfigureText();
            Render();
        }

        public void ShowUnbound()
        {
            visibleEntries.Clear();
            visibleEntries.Add("战斗日志：未绑定");
            Render();
        }

        public void SeedFrom(CombatLog combatLog)
        {
            visibleEntries.Clear();
            if (combatLog == null)
            {
                ShowUnbound();
                return;
            }

            IReadOnlyList<CombatLog.Entry> entries = combatLog.RecentEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                visibleEntries.Add(Format(entries[i]));
            }

            if (visibleEntries.Count == 0)
            {
                visibleEntries.Add("战斗日志：暂无记录");
            }

            Trim();
            Render();
        }

        public void AddEntry(CombatLog.Entry entry)
        {
            visibleEntries.Add(Format(entry));
            Trim();
            Render();
        }

        private static string Format(CombatLog.Entry entry)
        {
            return entry == null ? "战斗记录不可用。" : entry.Message;
        }

        private void Trim()
        {
            int limit = Mathf.Max(1, maxVisibleEntries);
            while (visibleEntries.Count > limit)
            {
                visibleEntries.RemoveAt(0);
            }
        }

        private void Render()
        {
            if (logText == null)
            {
                return;
            }

            ConfigureText();
            EnsureEntriesFit();
            logText.text = string.Join("\n", visibleEntries);
            logText.ForceMeshUpdate();
        }

        private void ConfigureText()
        {
            if (logText == null)
            {
                return;
            }

            logText.raycastTarget = false;
            logText.textWrappingMode = TextWrappingModes.Normal;
            logText.overflowMode = TextOverflowModes.Truncate;
            logText.richText = true;
            logText.lineSpacing = lineSpacing;
            logText.verticalAlignment = VerticalAlignmentOptions.Bottom;
        }

        private void EnsureEntriesFit()
        {
            RectTransform rectTransform = logText.rectTransform;
            if (rectTransform == null)
            {
                return;
            }

            Rect rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            while (visibleEntries.Count > 1 && IsTextTooTall(string.Join("\n", visibleEntries), width, height))
            {
                visibleEntries.RemoveAt(0);
            }
        }

        private bool IsTextTooTall(string text, float width, float height)
        {
            Vector2 preferred = logText.GetPreferredValues(text, width, 0f);
            return preferred.y > height + 0.5f;
        }
    }
}
