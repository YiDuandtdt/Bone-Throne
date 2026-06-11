using BoneThrone.Core;
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
        [System.Serializable]
        private sealed class RolePortraitSprite
        {
            [SerializeField] private RoleId roleId = RoleId.None;
            [SerializeField] private Sprite portraitSprite;

            public RoleId RoleId => roleId;
            public Sprite PortraitSprite => portraitSprite;
        }

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private RectTransform healthFillRect;
        [SerializeField] private Image missingHealthImage;
        [SerializeField] private RectTransform missingHealthRect;

        [Header("Replaceable Images")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image selectedFrameImage;
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Sprite selectedFrameSprite;
        [SerializeField] private RolePortraitSprite[] rolePortraitSprites;

        private void Awake()
        {
            ResolveReplaceableImages();
            ResolveHealthBarReferences();
        }

        public void Bind(TMP_Text text)
        {
            statusText = text;
            ResolveHealthBarReferences();
        }

        public void Bind(TMP_Text text, Image fillImage, RectTransform fillRect)
        {
            statusText = text;
            healthFillImage = fillImage;
            healthFillRect = fillRect;
            ResolveHealthBarReferences();
        }

        public void Refresh(Unit unit)
        {
            Refresh(unit, null);
        }

        public void Refresh(Unit unit, Unit selectedUnit)
        {
            ResolveReplaceableImages();
            ApplyReplaceableImages(unit, unit != null && unit == selectedUnit);
            ResolveHealthBarReferences();

            if (statusText == null)
            {
                return;
            }

            if (unit == null)
            {
                statusText.text = "角色：未绑定\n生命：-- / --\n等级：--\n移动：-- | 行动：--\n回合：--\n状态：--";
                ApplyHealthFill(0f);
                return;
            }

            int currentHpValue = unit.RuntimeState != null ? Mathf.Max(0, unit.RuntimeState.CurrentHp) : 0;
            int maxHpValue = unit.Stats != null ? Mathf.Max(1, unit.Stats.GetClampedMaxHp()) : 1;
            string currentHp = unit.RuntimeState != null ? currentHpValue.ToString() : "--";
            string maxHp = unit.Stats != null ? maxHpValue.ToString() : "--";
            string level = unit.Stats != null ? unit.Stats.Level.ToString() : "--";
            UnitTurnState turnState = unit.GetComponent<UnitTurnState>();
            string moved = FormatYesNo(turnState != null && turnState.HasMoved);
            string acted = FormatYesNo(turnState != null && turnState.HasActed);
            string turnStatus = FormatTurnStatus(unit, selectedUnit, turnState);
            string life = unit.IsAlive ? "存活" : "倒下";
            ApplyHealthFill(unit.RuntimeState != null && unit.Stats != null ? (float)currentHpValue / maxHpValue : 0f);

            statusText.text = BoneThroneTextUtility.GetUnitDisplayName(unit)
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

        private static string FormatYesNo(bool value)
        {
            return value ? "是" : "否";
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

            if (missingHealthImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (image != null && (image.gameObject.name == "MissingFill" || image.gameObject.name == "HealthBackground"))
                    {
                        missingHealthImage = image;
                        break;
                    }
                }
            }

            if (missingHealthRect == null && missingHealthImage != null)
            {
                missingHealthRect = missingHealthImage.rectTransform;
            }

            ConfigureHealthBar();
        }

        private void ResolveReplaceableImages()
        {
            if (frameImage == null)
            {
                frameImage = GetComponent<Image>();
            }

            if (portraitImage == null || selectedFrameImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    Image image = images[i];
                    if (image == null)
                    {
                        continue;
                    }

                    if (portraitImage == null && image.gameObject.name == "PortraitImage")
                    {
                        portraitImage = image;
                    }

                    if (selectedFrameImage == null && image.gameObject.name == "SelectedFrameImage")
                    {
                        selectedFrameImage = image;
                    }
                }
            }
        }

        private void ApplyReplaceableImages(Unit unit, bool isSelected)
        {
            ApplySprite(frameImage, frameSprite);

            if (portraitImage != null)
            {
                Sprite portraitSprite = ResolvePortraitSprite(unit);
                portraitImage.sprite = portraitSprite;
                portraitImage.color = portraitSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                portraitImage.raycastTarget = false;
                portraitImage.preserveAspect = true;
            }

            if (selectedFrameImage != null)
            {
                ApplySprite(selectedFrameImage, selectedFrameSprite);
                selectedFrameImage.gameObject.SetActive(isSelected);
            }
        }

        private Sprite ResolvePortraitSprite(Unit unit)
        {
            if (unit == null || rolePortraitSprites == null)
            {
                return null;
            }

            for (int i = 0; i < rolePortraitSprites.Length; i++)
            {
                RolePortraitSprite entry = rolePortraitSprites[i];
                if (entry != null && entry.RoleId == unit.RoleId)
                {
                    return entry.PortraitSprite;
                }
            }

            return null;
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
        }

        private void ConfigureHealthBar()
        {
            if (healthFillImage != null)
            {
                healthFillImage.raycastTarget = false;
                healthFillImage.type = Image.Type.Filled;
                healthFillImage.fillMethod = Image.FillMethod.Horizontal;
                healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            if (missingHealthImage != null)
            {
                missingHealthImage.raycastTarget = false;
                missingHealthImage.type = Image.Type.Filled;
                missingHealthImage.fillMethod = Image.FillMethod.Horizontal;
                missingHealthImage.fillOrigin = (int)Image.OriginHorizontal.Right;
            }

            // The health bar artwork is authored in the prefab. Runtime refreshes only
            // adjust Image.fillAmount so the red/gray segments cannot drift away from it.
        }

        private void ApplyHealthFill(float ratio)
        {
            float clampedRatio = Mathf.Clamp01(ratio);

            if (healthFillImage != null)
            {
                healthFillImage.fillAmount = clampedRatio;
            }

            if (missingHealthImage != null)
            {
                missingHealthImage.fillAmount = 1f - clampedRatio;
            }
        }

    }
}
