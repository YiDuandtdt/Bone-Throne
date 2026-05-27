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
        [SerializeField] private int maxVisibleEntries = 5;
        [SerializeField] private float lineSpacing = 8f;

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
            visibleEntries.Add("Combat Log: Unbound");
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
                visibleEntries.Add("Combat Log: No combat yet");
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

        private string Format(CombatLog.Entry entry)
        {
            if (entry == null)
            {
                return "Combat: N/A";
            }

            return entry.Message;
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
            logText.text = string.Join("\n", visibleEntries);
        }

        private void ConfigureText()
        {
            if (logText == null)
            {
                return;
            }

            logText.richText = true;
            logText.lineSpacing = lineSpacing;
            logText.raycastTarget = false;
        }
    }
}
