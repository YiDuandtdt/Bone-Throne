#if UNITY_EDITOR
using BoneThrone.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.Editor
{
    /// <summary>
    /// Repairs reusable battle HUD prefabs on demand without overwriting manual prefab layout edits on editor reload.
    /// </summary>
    public static class BattleHudPrefabRepairUtility
    {
        private static readonly Color HealthFillColor = new Color(0.9f, 0.03f, 0.02f, 1f);
        private static readonly Color BossPanelColor = new Color(0.08f, 0.03f, 0.02f, 0.86f);
        private static readonly Color HealthBackgroundColor = new Color(1f, 1f, 1f, 0.28f);
        private static readonly Vector2 ReferenceCanvasSize = new Vector2(1920f, 1080f);

        private const string RootHudPrefabPath = "Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab";
        private const string BossTestHudPrefabPath = "Assets/_BoneThrone/Prefabs/UI/Battle/BattleHUD_BossTest.prefab";

        [MenuItem("BoneThrone/UI/Repair Battle HUD Prefabs")]
        public static void RepairBattleHudPrefabs()
        {
            RepairBattleHudPrefab(RootHudPrefabPath);
            RepairBattleHudPrefab(BossTestHudPrefabPath);
            AssetDatabase.SaveAssets();
        }

        private static void RepairBattleHudPrefab(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null)
            {
                return;
            }

            bool changed = false;
            try
            {
                changed |= NormalizeRootRect(root);
                changed |= EnsureBossHealthPanel(root.transform);
                changed |= ConfigureHeroPanelBars(root);
                changed |= ConfigureControllerFallback(root);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    Debug.Log("Battle HUD prefab repaired: " + prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool NormalizeRootRect(GameObject root)
        {
            RectTransform rect = root.GetComponent<RectTransform>();
            if (rect == null)
            {
                return false;
            }

            bool changed = false;
            if (rect.localScale != Vector3.one)
            {
                rect.localScale = Vector3.one;
                changed = true;
            }

            Vector2 center = new Vector2(0.5f, 0.5f);
            if (rect.anchorMin != center)
            {
                rect.anchorMin = center;
                changed = true;
            }

            if (rect.anchorMax != center)
            {
                rect.anchorMax = center;
                changed = true;
            }

            if (rect.pivot != center)
            {
                rect.pivot = center;
                changed = true;
            }

            if (rect.anchoredPosition != Vector2.zero)
            {
                rect.anchoredPosition = Vector2.zero;
                changed = true;
            }

            if (rect.sizeDelta != ReferenceCanvasSize)
            {
                rect.sizeDelta = ReferenceCanvasSize;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureBossHealthPanel(Transform root)
        {
            Transform existing = FindChildByName(root, "BossHealthPanel");
            if (existing != null)
            {
                bool changed = ConfigureBossHealthPanel(existing.gameObject, overwriteLayout: false);
                changed |= EnsureBossText(existing, "BossHealthNameText", "Boss", TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0.45f, 1f), new Vector2(18f, -2f), new Vector2(-8f, -4f));
                changed |= EnsureBossText(existing, "BossHealthValueText", "-- / --", TextAlignmentOptions.Right, new Vector2(0.55f, 0.5f), new Vector2(1f, 1f), new Vector2(8f, -2f), new Vector2(-18f, -4f));
                changed |= EnsureBossHealthBar(existing);
                return changed;
            }

            GameObject panel = new GameObject("BossHealthPanel", typeof(RectTransform));
            panel.transform.SetParent(root, false);
            panel.layer = root.gameObject.layer;
            ConfigureBossHealthPanel(panel, overwriteLayout: true);

            CreateBossText(panel.transform, "BossHealthNameText", "Boss", TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0.45f, 1f), new Vector2(18f, -2f), new Vector2(-8f, -4f));
            CreateBossText(panel.transform, "BossHealthValueText", "-- / --", TextAlignmentOptions.Right, new Vector2(0.55f, 0.5f), new Vector2(1f, 1f), new Vector2(8f, -2f), new Vector2(-18f, -4f));
            CreateHealthBar(panel.transform, "BossHealthBar", "BossHealthFill", new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f), new Vector2(-32f, 22f), new Vector2(0f, 8f));
            return true;
        }

        private static bool ConfigureBossHealthPanel(GameObject panel, bool overwriteLayout)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            bool changed = false;
            int expectedLayer = panel.transform.parent != null ? panel.transform.parent.gameObject.layer : 5;
            if (panel.layer != expectedLayer)
            {
                panel.layer = expectedLayer;
                changed = true;
            }

            if (overwriteLayout)
            {
                changed |= SetRectTransform(rect, Vector3.one, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(720f, 54f), new Vector2(0f, -92f));
            }

            Image image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.AddComponent<Image>();
                changed = true;
            }

            if (image.color != BossPanelColor)
            {
                image.color = BossPanelColor;
                changed = true;
            }

            if (image.raycastTarget)
            {
                image.raycastTarget = false;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureBossText(
            Transform parent,
            string name,
            string value,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            if (FindChildByName(parent, name) != null)
            {
                return false;
            }

            CreateBossText(parent, name, value, alignment, anchorMin, anchorMax, offsetMin, offsetMax);
            return true;
        }

        private static bool EnsureBossHealthBar(Transform parent)
        {
            if (FindChildByName(parent, "BossHealthFill") != null)
            {
                return false;
            }

            CreateHealthBar(parent, "BossHealthBar", "BossHealthFill", new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0f), new Vector2(-32f, 22f), new Vector2(0f, 8f));
            return true;
        }

        private static void CreateBossText(
            Transform parent,
            string name,
            string value,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            textObject.layer = parent.gameObject.layer;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = 20f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static GameObject CreateHealthBar(
            Transform parent,
            string name,
            string fillName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position)
        {
            GameObject bar = new GameObject(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            bar.layer = parent.gameObject.layer;
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.localScale = Vector3.one;
            barRect.anchorMin = anchorMin;
            barRect.anchorMax = anchorMax;
            barRect.pivot = pivot;
            barRect.sizeDelta = size;
            barRect.anchoredPosition = position;

            Image background = bar.AddComponent<Image>();
            background.color = HealthBackgroundColor;
            background.raycastTarget = false;

            GameObject fill = new GameObject(fillName, typeof(RectTransform));
            fill.transform.SetParent(bar.transform, false);
            fill.layer = bar.layer;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.localScale = Vector3.one;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            fillRect.pivot = new Vector2(0f, 0.5f);

            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = HealthFillColor;
            fillImage.raycastTarget = false;
            return bar;
        }

        private static bool ConfigureHeroPanelBars(GameObject root)
        {
            bool changed = false;
            HeroPanelView[] panels = root.GetComponentsInChildren<HeroPanelView>(true);
            for (int i = 0; i < panels.Length; i++)
            {
                Image[] images = panels[i].GetComponentsInChildren<Image>(true);
                for (int j = 0; j < images.Length; j++)
                {
                    Image image = images[j];
                    if (image != null && image.gameObject.name == "Fill" && image.color != HealthFillColor)
                    {
                        image.color = HealthFillColor;
                        image.raycastTarget = false;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static bool SetRectTransform(
            RectTransform rect,
            Vector3 localScale,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition)
        {
            bool changed = false;

            if (rect.localScale != localScale)
            {
                rect.localScale = localScale;
                changed = true;
            }

            if (rect.anchorMin != anchorMin)
            {
                rect.anchorMin = anchorMin;
                changed = true;
            }

            if (rect.anchorMax != anchorMax)
            {
                rect.anchorMax = anchorMax;
                changed = true;
            }

            if (rect.pivot != pivot)
            {
                rect.pivot = pivot;
                changed = true;
            }

            if (rect.sizeDelta != sizeDelta)
            {
                rect.sizeDelta = sizeDelta;
                changed = true;
            }

            if (rect.anchoredPosition != anchoredPosition)
            {
                rect.anchoredPosition = anchoredPosition;
                changed = true;
            }

            return changed;
        }

        private static bool ConfigureControllerFallback(GameObject root)
        {
            BattleHUDController controller = root.GetComponent<BattleHUDController>();
            if (controller == null)
            {
                return false;
            }

            SerializedObject serializedController = new SerializedObject(controller);
            bool changed = false;
            changed |= SetObjectReference(serializedController, "bossHealthRoot", FindChildByName(root.transform, "BossHealthPanel")?.gameObject);
            changed |= SetObjectReference(serializedController, "bossHealthFillImage", FindChildByName(root.transform, "BossHealthFill")?.GetComponent<Image>());
            changed |= SetObjectReference(serializedController, "bossHealthFillRect", FindChildByName(root.transform, "BossHealthFill")?.GetComponent<RectTransform>());
            changed |= SetObjectReference(serializedController, "bossHealthNameText", FindChildByName(root.transform, "BossHealthNameText")?.GetComponent<TMP_Text>());
            changed |= SetObjectReference(serializedController, "bossHealthValueText", FindChildByName(root.transform, "BossHealthValueText")?.GetComponent<TMP_Text>());

            if (changed)
            {
                serializedController.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindChildByName(root.GetChild(i), childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
#endif
