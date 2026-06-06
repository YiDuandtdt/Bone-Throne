using BoneThrone.Turns;
using BoneThrone.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Displays a single player unit's readable battle status.
    /// </summary>
    public sealed class HeroPanelView : MonoBehaviour
    {
        private const string RuntimeContentRootName = "RuntimeHeroPanelContent";
        private const string OuterBorderRootName = "RuntimeHeroPanelOuterBorder";
        private const string InnerBorderRootName = "RuntimeHeroPanelInnerBorder";
        private const float ContentInset = 5f;
        private const float OuterBorderThickness = 1.4f;
        private const float InnerBorderThickness = 1.4f;

        private static readonly Color HealthFillColor = new Color(0.9f, 0.03f, 0.02f, 1f);
        private static readonly Color PopupFrameColor = new Color(0.16f, 0.1f, 0.08f, 0.94f);
        private static readonly Color ContentBackgroundColor = new Color(0.12f, 0.09f, 0.08f, 0.92f);
        private static readonly Color InnerBorderNormalColor = new Color(1f, 1f, 1f, 0.24f);
        private static readonly Color InnerBorderSelectedColor = new Color(1f, 1f, 1f, 0.62f);

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private RectTransform healthFillRect;
        [SerializeField] private bool autoBuildFrameVisuals = true;

        private RectTransform contentRoot;
        private Image[] innerBorderImages;
        private bool frameVisualsReady;

        private void Awake()
        {
            EnsureFrameVisuals();
            ResolveHealthBarReferences();
        }

        public void Bind(TMP_Text text)
        {
            EnsureFrameVisuals();
            statusText = text;
            MoveTransformIntoContent(statusText != null ? statusText.transform : null);
            ResolveHealthBarReferences();
        }

        public void Bind(TMP_Text text, Image fillImage, RectTransform fillRect)
        {
            EnsureFrameVisuals();
            statusText = text;
            healthFillImage = fillImage;
            healthFillRect = fillRect;
            MoveTransformIntoContent(statusText != null ? statusText.transform : null);
            MoveTransformIntoContent(healthFillRect != null ? healthFillRect.parent : null);
            ConfigureHealthBar();
        }

        public void Refresh(Unit unit)
        {
            Refresh(unit, null);
        }

        public void Refresh(Unit unit, Unit selectedUnit)
        {
            EnsureFrameVisuals();
            ApplySelectionFrame(unit != null && unit == selectedUnit);
            ResolveHealthBarReferences();

            if (statusText == null)
            {
                return;
            }

            if (unit == null)
            {
                statusText.text = "角色：未绑定\n生命：- / --\n等级：-\n移动：- | 行动：-";
                ApplyHealthFill(0f);
                return;
            }

            string displayName = string.IsNullOrEmpty(unit.DisplayName) ? unit.RoleId.ToString() : unit.DisplayName;
            int currentHpValue = unit.RuntimeState != null ? Mathf.Max(0, unit.RuntimeState.CurrentHp) : 0;
            int maxHpValue = unit.Stats != null ? Mathf.Max(1, unit.Stats.GetClampedMaxHp()) : 1;
            string currentHp = unit.RuntimeState != null ? currentHpValue.ToString() : "--";
            string maxHp = unit.Stats != null ? maxHpValue.ToString() : "--";
            string level = unit.Stats != null ? unit.Stats.Level.ToString() : "--";
            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            string moved = turnState != null ? turnState.HasMoved.ToString() : "--";
            string acted = turnState != null ? turnState.HasActed.ToString() : "--";
            string turnStatus = FormatTurnStatus(unit, selectedUnit, turnState);
            string life = unit.IsAlive ? "存活" : "死亡";
            ApplyHealthFill(unit.RuntimeState != null && unit.Stats != null ? (float)currentHpValue / maxHpValue : 0f);

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

        private void ResolveHealthBarReferences()
        {
            if (healthFillImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (image != null && image.gameObject.name == "Fill")
                    {
                        healthFillImage = image;
                        break;
                    }
                }
            }

            if (healthFillRect == null && healthFillImage != null)
            {
                healthFillRect = healthFillImage.rectTransform;
            }

            ConfigureHealthBar();
        }

        private void ConfigureHealthBar()
        {
            if (healthFillImage != null)
            {
                healthFillImage.color = HealthFillColor;
                healthFillImage.raycastTarget = false;
            }

            if (healthFillRect == null)
            {
                return;
            }

            healthFillRect.anchorMin = new Vector2(0f, 0f);
            healthFillRect.pivot = new Vector2(0f, 0.5f);
            healthFillRect.anchoredPosition = Vector2.zero;
            healthFillRect.sizeDelta = Vector2.zero;
        }

        private void ApplyHealthFill(float ratio)
        {
            if (healthFillRect == null)
            {
                return;
            }

            healthFillRect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
        }

        private void EnsureFrameVisuals()
        {
            if (!autoBuildFrameVisuals)
            {
                return;
            }

            if (frameVisualsReady && contentRoot != null && innerBorderImages != null)
            {
                return;
            }

            RectTransform rootRect = GetComponent<RectTransform>();
            if (rootRect == null)
            {
                return;
            }

            Image rootBackground = GetComponent<Image>();
            if (rootBackground == null)
            {
                rootBackground = gameObject.AddComponent<Image>();
            }

            rootBackground.color = PopupFrameColor;
            rootBackground.raycastTarget = false;

            contentRoot = FindDirectChildRect(RuntimeContentRootName);
            if (contentRoot == null)
            {
                GameObject contentObject = new GameObject(RuntimeContentRootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                contentObject.transform.SetParent(transform, false);
                contentRoot = contentObject.GetComponent<RectTransform>();
                contentRoot.SetAsFirstSibling();
            }

            ConfigureStretchRect(contentRoot, ContentInset, ContentInset, -ContentInset, -ContentInset);
            Image contentBackground = contentRoot.GetComponent<Image>();
            if (contentBackground != null)
            {
                contentBackground.color = ContentBackgroundColor;
                contentBackground.raycastTarget = false;
            }

            MoveExistingContentChildren();
            CreateOrUpdateBorder(transform, OuterBorderRootName, PopupFrameColor, OuterBorderThickness);
            innerBorderImages = CreateOrUpdateBorder(contentRoot, InnerBorderRootName, InnerBorderNormalColor, InnerBorderThickness);
            ApplySelectionFrame(false);
            frameVisualsReady = true;
        }

        private void MoveExistingContentChildren()
        {
            if (contentRoot == null)
            {
                return;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null || child == contentRoot || IsRuntimeFrameChild(child.name))
                {
                    continue;
                }

                child.SetParent(contentRoot, false);
            }
        }

        private void MoveTransformIntoContent(Transform child)
        {
            if (child == null || contentRoot == null || child == contentRoot || child.parent == contentRoot)
            {
                return;
            }

            if (child.parent == transform && !IsRuntimeFrameChild(child.name))
            {
                child.SetParent(contentRoot, false);
            }
        }

        private RectTransform FindDirectChildRect(string childName)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private void ApplySelectionFrame(bool isSelected)
        {
            if (innerBorderImages == null)
            {
                return;
            }

            Color color = isSelected ? InnerBorderSelectedColor : InnerBorderNormalColor;
            for (int i = 0; i < innerBorderImages.Length; i++)
            {
                if (innerBorderImages[i] != null)
                {
                    innerBorderImages[i].color = color;
                }
            }
        }

        private static bool IsRuntimeFrameChild(string childName)
        {
            return childName == RuntimeContentRootName
                || childName == OuterBorderRootName
                || childName == InnerBorderRootName;
        }

        private static Image[] CreateOrUpdateBorder(Transform parent, string borderName, Color color, float thickness)
        {
            RectTransform borderRoot = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == borderName)
                {
                    borderRoot = child as RectTransform;
                    break;
                }
            }

            if (borderRoot == null)
            {
                GameObject borderObject = new GameObject(borderName, typeof(RectTransform));
                borderObject.transform.SetParent(parent, false);
                borderRoot = borderObject.GetComponent<RectTransform>();
            }

            ConfigureStretchRect(borderRoot, 0f, 0f, 0f, 0f);
            borderRoot.SetAsLastSibling();

            return new[]
            {
                CreateOrUpdateBorderStrip(borderRoot, "Top", color, BorderSide.Top, thickness),
                CreateOrUpdateBorderStrip(borderRoot, "Bottom", color, BorderSide.Bottom, thickness),
                CreateOrUpdateBorderStrip(borderRoot, "Left", color, BorderSide.Left, thickness),
                CreateOrUpdateBorderStrip(borderRoot, "Right", color, BorderSide.Right, thickness)
            };
        }

        private static Image CreateOrUpdateBorderStrip(RectTransform parent, string stripName, Color color, BorderSide side, float thickness)
        {
            RectTransform stripRect = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == stripName)
                {
                    stripRect = child as RectTransform;
                    break;
                }
            }

            if (stripRect == null)
            {
                GameObject stripObject = new GameObject(stripName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                stripObject.transform.SetParent(parent, false);
                stripRect = stripObject.GetComponent<RectTransform>();
            }

            ConfigureBorderStripRect(stripRect, side, thickness);
            Image stripImage = stripRect.GetComponent<Image>();
            if (stripImage != null)
            {
                stripImage.color = color;
                stripImage.raycastTarget = false;
            }

            return stripImage;
        }

        private static void ConfigureStretchRect(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
        }

        private static void ConfigureBorderStripRect(RectTransform rect, BorderSide side, float thickness)
        {
            rect.localScale = Vector3.one;

            switch (side)
            {
                case BorderSide.Top:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(0f, thickness);
                    break;
                case BorderSide.Bottom:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(0f, thickness);
                    break;
                case BorderSide.Left:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(thickness, 0f);
                    break;
                case BorderSide.Right:
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(thickness, 0f);
                    break;
            }
        }

        private enum BorderSide
        {
            Top,
            Bottom,
            Left,
            Right
        }
    }
}
