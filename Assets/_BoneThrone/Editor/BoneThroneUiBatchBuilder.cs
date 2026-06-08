using System.Collections.Generic;
using System.IO;
using BoneThrone.Core;
using BoneThrone.UI;
using BoneThrone.Units;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoneThrone.Editor
{
    public static class BoneThroneUiBatchBuilder
    {
        private const string Art2dFolder = "Assets/_BoneThrone/Art/2D";
        private const string PlaceholderFolder = "Assets/_BoneThrone/Art/2D/UIPlaceholders";
        private const string UiPrefabFolder = "Assets/_BoneThrone/Prefabs/UI";
        private const string UiCommonFolder = "Assets/_BoneThrone/Prefabs/UI/Common";
        private const string UiBattleFolder = "Assets/_BoneThrone/Prefabs/UI/Battle";
        private const string UiMenuFolder = "Assets/_BoneThrone/Prefabs/UI/Menu";
        private const string UiOutcomeFolder = "Assets/_BoneThrone/Prefabs/UI/Outcome";

        private const string ImageSlotPrefabPath = UiCommonFolder + "/UI_ImageSlot.prefab";
        private const string ButtonBasePrefabPath = UiCommonFolder + "/UI_Button_Base.prefab";
        private const string BloodBarFramePrefabPath = UiCommonFolder + "/UI_BloodBarFrame.prefab";
        private const string HeroPanelPrefabPath = UiBattleFolder + "/UI_HeroPanel.prefab";
        private const string TurnBannerPrefabPath = UiBattleFolder + "/UI_TurnBanner.prefab";
        private const string PromptPanelPrefabPath = UiBattleFolder + "/UI_PromptPanel.prefab";
        private const string CombatLogPrefabPath = UiBattleFolder + "/UI_CombatLogPanel.prefab";
        private const string ActionSelectionPrefabPath = UiBattleFolder + "/UI_ActionSelectionPanel.prefab";
        private const string CharacterSelectionPrefabPath = UiMenuFolder + "/UI_CharacterSelectionPanel.prefab";
        private const string StartMenuPrefabPath = UiMenuFolder + "/UI_StartMenuCanvas.prefab";
        private const string EndMenuPrefabPath = UiMenuFolder + "/UI_EndMenuCanvas.prefab";
        private const string ResultPanelPrefabPath = UiOutcomeFolder + "/UI_GameResultCanvas.prefab";
        private const string SharedBattleHudPrefabPath = "Assets/_BoneThrone/Prefabs/UI/BattleHUD.prefab";
        private const string SharedEnemyHpBarPrefabPath = "Assets/_BoneThrone/Prefabs/UI/EnemyFloatingHealthBar.prefab";
        private const string BattleHudPrefabPath = UiBattleFolder + "/BattleHUD_BossTest.prefab";
        private const string EnemyHpBarPrefabPath = UiBattleFolder + "/EnemyFloatingHealthBar_BossTest.prefab";

        private const string StartMenuScenePath = "Assets/_BoneThrone/Scenes/StartMenu.unity";
        private const string EndMenuScenePath = "Assets/_BoneThrone/Scenes/EndMenu.unity";
        private const string BossTestScenePath = "Assets/_BoneThrone/Scenes/boss_test.unity";

        private sealed class PlaceholderPalette
        {
            public Sprite Gray00;
            public Sprite Gray20;
            public Sprite Gray40;
            public Sprite Gray60;
            public Sprite Gray80;
            public Sprite Gray100;
        }

        [MenuItem("BoneThrone/UI/Build Visual UI Prefabs And Scenes")]
        public static void BuildAll()
        {
            EnsureFolder("Assets/_BoneThrone/Art", "2D");
            EnsureFolder(Art2dFolder, "UIPlaceholders");
            EnsureFolder("Assets/_BoneThrone/Prefabs", "UI");
            EnsureFolder(UiPrefabFolder, "Common");
            EnsureFolder(UiPrefabFolder, "Battle");
            EnsureFolder(UiPrefabFolder, "Menu");
            EnsureFolder(UiPrefabFolder, "Outcome");
            EnsureFolder("Assets/_BoneThrone", "Editor");

            PlaceholderPalette palette = BuildPlaceholderPalette();
            BuildImageSlotPrefab(palette);
            BuildButtonBasePrefab(palette);
            BuildBloodBarFramePrefab(palette);
            BuildHeroPanelPrefab(palette);
            BuildTurnBannerPrefab(palette);
            BuildPromptPanelPrefab(palette);
            BuildCombatLogPrefab(palette);
            BuildActionSelectionPrefab(palette);
            BuildCharacterSelectionPrefab(palette);
            BuildGameResultPrefab(palette);
            BuildBattleHudPrefab(palette, SharedBattleHudPrefabPath, "BattleHUD");
            BuildBattleHudPrefab(palette, BattleHudPrefabPath, "BattleHUD_BossTest");
            BuildEnemyFloatingHealthBarPrefab(palette, SharedEnemyHpBarPrefabPath, "EnemyFloatingHealthBar");
            BuildEnemyFloatingHealthBarPrefab(palette, EnemyHpBarPrefabPath, "EnemyFloatingHealthBar_BossTest");
            BuildStartMenuPrefab(palette);
            BuildEndMenuPrefab(palette);

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UpdateStartMenuScene();
                UpdateEndMenuScene();
                UpdateBossTestScene();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BoneThroneUiBatchBuilder completed visual UI prefab and scene generation.");
        }

        private static PlaceholderPalette BuildPlaceholderPalette()
        {
            PlaceholderPalette palette = new PlaceholderPalette
            {
                Gray00 = CreateOrLoadSprite("ui_gray_00.png", 255),
                Gray20 = CreateOrLoadSprite("ui_gray_20.png", 204),
                Gray40 = CreateOrLoadSprite("ui_gray_40.png", 153),
                Gray60 = CreateOrLoadSprite("ui_gray_60.png", 102),
                Gray80 = CreateOrLoadSprite("ui_gray_80.png", 51),
                Gray100 = CreateOrLoadSprite("ui_gray_100.png", 0)
            };

            AssetDatabase.Refresh();
            palette.Gray00 = ReloadSprite("ui_gray_00.png");
            palette.Gray20 = ReloadSprite("ui_gray_20.png");
            palette.Gray40 = ReloadSprite("ui_gray_40.png");
            palette.Gray60 = ReloadSprite("ui_gray_60.png");
            palette.Gray80 = ReloadSprite("ui_gray_80.png");
            palette.Gray100 = ReloadSprite("ui_gray_100.png");
            return palette;
        }

        private static Sprite CreateOrLoadSprite(string fileName, byte channel)
        {
            string assetPath = PlaceholderFolder + "/" + fileName;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));

            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color32 color = new Color32(channel, channel, channel, 255);
            Color32[] pixels = new Color32[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return ReloadSprite(fileName);
        }

        private static Sprite ReloadSprite(string fileName)
        {
            string assetPath = PlaceholderFolder + "/" + fileName;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                return sprite;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite loadedSprite)
                {
                    return loadedSprite;
                }
            }

            return null;
        }

        private static void BuildButtonBasePrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_Button_Base");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 56f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray80;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            Button button = root.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.85f);
            button.colors = colors;
            button.targetGraphic = background;

            TMP_Text label = CreateText(root.transform, "Label", "Button", 22, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(label.rectTransform, new Vector2(12f, 8f), new Vector2(-12f, -8f));

            SavePrefab(root, ButtonBasePrefabPath);
        }

        private static void BuildImageSlotPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_ImageSlot");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(128f, 128f);

            Image image = root.AddComponent<Image>();
            image.sprite = palette.Gray40;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = Color.white;

            SavePrefab(root, ImageSlotPrefabPath);
        }

        private static void BuildBloodBarFramePrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_BloodBarFrame");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 20f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray100;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            GameObject fill = CreateRectObject("Fill");
            fill.transform.SetParent(root.transform, false);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = palette.Gray20;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Color.white;

            SavePrefab(root, BloodBarFramePrefabPath);
        }

        private static void BuildHeroPanelPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_HeroPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 150f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray80;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            HeroPanelView heroPanelView = root.AddComponent<HeroPanelView>();

            GameObject bloodBar = InstantiatePrefabAsset(BloodBarFramePrefabPath, root.transform, "BloodBar");
            RectTransform bloodBarRect = bloodBar.GetComponent<RectTransform>();
            bloodBarRect.anchorMin = new Vector2(0f, 1f);
            bloodBarRect.anchorMax = new Vector2(1f, 1f);
            bloodBarRect.pivot = new Vector2(0.5f, 1f);
            bloodBarRect.offsetMin = new Vector2(16f, -42f);
            bloodBarRect.offsetMax = new Vector2(-16f, -18f);

            TMP_Text statusText = CreateText(root.transform, "StatusText", "角色：未绑定\n生命：-- / --\n等级：--\n移动：-- | 行动：--\n回合：--\n状态：--", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
            Stretch(statusText.rectTransform, new Vector2(16f, 54f), new Vector2(-16f, -14f));
            heroPanelView.Bind(statusText);

            SavePrefab(root, HeroPanelPrefabPath);
        }

        private static void BuildTurnBannerPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_TurnBanner");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(720f, 64f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray80;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            TurnBannerView view = root.AddComponent<TurnBannerView>();
            TMP_Text text = CreateText(root.transform, "TurnText", "回合：未绑定", 28, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            Stretch(text.rectTransform, new Vector2(18f, 10f), new Vector2(-18f, -10f));
            view.Bind(text);

            SavePrefab(root, TurnBannerPrefabPath);
        }

        private static void BuildPromptPanelPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_PromptPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(720f, 52f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray60;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            PromptView view = root.AddComponent<PromptView>();
            TMP_Text text = CreateText(root.transform, "PromptText", "请选择一名玩家角色。", 20, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
            Stretch(text.rectTransform, new Vector2(16f, 8f), new Vector2(-16f, -8f));
            view.Bind(text);

            SavePrefab(root, PromptPanelPrefabPath);
        }

        private static void BuildCombatLogPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_CombatLogPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(520f, 360f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray100;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            CombatFeedbackView view = root.AddComponent<CombatFeedbackView>();
            TMP_Text text = CreateText(root.transform, "CombatLogText", "战斗日志：暂无记录", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);
            Stretch(text.rectTransform, new Vector2(18f, 14f), new Vector2(-18f, -14f));
            view.Bind(text);

            SavePrefab(root, CombatLogPrefabPath);
        }

        private static void BuildActionSelectionPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_ActionSelectionPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(980f, 108f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray80;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            SkillBarView view = root.AddComponent<SkillBarView>();
            string[] labels = { "Move", "Attack", "Skill 0", "Skill 1", "Skill 2", "Defend", "Potion", "End Turn" };
            Button[] buttons = new Button[labels.Length];
            TMP_Text[] texts = new TMP_Text[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                GameObject buttonRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, root.transform, "Button_" + labels[i].Replace(" ", string.Empty));
                LayoutElement element = buttonRoot.AddComponent<LayoutElement>();
                element.preferredWidth = i == labels.Length - 1 ? 128f : 112f;
                element.flexibleWidth = 1f;
                buttons[i] = buttonRoot.GetComponent<Button>();
                texts[i] = buttonRoot.GetComponentInChildren<TMP_Text>(true);
                if (texts[i] != null)
                {
                    texts[i].text = labels[i];
                }
            }

            view.Bind(texts[0], texts[1], texts[2], texts[3], texts[4], texts[5], texts[6], texts[7], buttons);
            SavePrefab(root, ActionSelectionPrefabPath);
        }

        private static void BuildCharacterSelectionPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateRectObject("UI_CharacterSelectionPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(920f, 240f);

            Image background = root.AddComponent<Image>();
            background.sprite = palette.Gray80;
            background.type = Image.Type.Sliced;
            background.color = Color.white;

            CreateText(root.transform, "Title", "Character Selection", 24, FontStyles.Bold, TextAlignmentOptions.TopLeft, Color.white);
            StretchTitle(root.transform.Find("Title") as RectTransform, 16f, 12f, 42f);

            GameObject gridRoot = CreateRectObject("Cards");
            gridRoot.transform.SetParent(root.transform, false);
            RectTransform gridRect = gridRoot.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = new Vector2(16f, 16f);
            gridRect.offsetMax = new Vector2(-16f, -52f);
            HorizontalLayoutGroup layout = gridRoot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            string[] roles = { "Fighter", "Ranger", "Mage", "Barbarian" };
            for (int i = 0; i < roles.Length; i++)
            {
                GameObject card = CreateRectObject(roles[i] + "_Card");
                card.transform.SetParent(gridRoot.transform, false);
                LayoutElement element = card.AddComponent<LayoutElement>();
                element.preferredWidth = 210f;
                element.flexibleWidth = 1f;
                Image cardImage = card.AddComponent<Image>();
                cardImage.sprite = palette.Gray60;
                cardImage.type = Image.Type.Sliced;
                cardImage.color = Color.white;
                TMP_Text roleText = CreateText(card.transform, "Label", roles[i], 22, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
                Stretch(roleText.rectTransform, new Vector2(10f, 10f), new Vector2(-10f, -10f));
            }

            SavePrefab(root, CharacterSelectionPrefabPath);
        }

        private static void BuildGameResultPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateCanvasRoot("UI_GameResultCanvas", 300);
            GameResultPanelController controller = root.AddComponent<GameResultPanelController>();

            GameObject blocker = CreateRectObject("Blocker");
            blocker.transform.SetParent(root.transform, false);
            RectTransform blockerRect = blocker.GetComponent<RectTransform>();
            Stretch(blockerRect, Vector2.zero, Vector2.zero);
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.sprite = palette.Gray100;
            blockerImage.color = new Color(0f, 0f, 0f, 0.78f);

            GameObject panel = CreateRectObject("PanelRoot");
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 360f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);
            panelImage.raycastTarget = true;

            TMP_Text titleText = CreateText(panel.transform, "TitleText", "Victory", 34, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.pivot = new Vector2(0.5f, 1f);
            titleText.rectTransform.offsetMin = new Vector2(24f, -70f);
            titleText.rectTransform.offsetMax = new Vector2(-24f, -18f);

            TMP_Text reasonText = CreateText(panel.transform, "ReasonText", "Outcome reason.", 22, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
            reasonText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            reasonText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            reasonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            reasonText.rectTransform.sizeDelta = new Vector2(-48f, 120f);
            reasonText.rectTransform.anchoredPosition = new Vector2(0f, 24f);

            GameObject buttonRow = CreateRectObject("Buttons");
            buttonRow.transform.SetParent(panel.transform, false);
            RectTransform rowRect = buttonRow.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0f);
            rowRect.anchorMax = new Vector2(0.5f, 0f);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.sizeDelta = new Vector2(420f, 64f);
            rowRect.anchoredPosition = new Vector2(0f, 28f);
            HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            GameObject retryRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttonRow.transform, "RetryButton");
            GameObject closeRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttonRow.transform, "CloseButton");
            Button retryButton = retryRoot.GetComponent<Button>();
            Button closeButton = closeRoot.GetComponent<Button>();
            TMP_Text retryLabel = retryRoot.GetComponentInChildren<TMP_Text>(true);
            TMP_Text closeLabel = closeRoot.GetComponentInChildren<TMP_Text>(true);
            if (retryLabel != null)
            {
                retryLabel.text = "Retry";
            }

            if (closeLabel != null)
            {
                closeLabel.text = "Close";
            }

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("root").objectReferenceValue = panel;
            serialized.FindProperty("titleText").objectReferenceValue = titleText;
            serialized.FindProperty("reasonText").objectReferenceValue = reasonText;
            serialized.FindProperty("retryButton").objectReferenceValue = retryButton;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            panel.SetActive(false);
            SavePrefab(root, ResultPanelPrefabPath);
        }

        private static void BuildBattleHudPrefab(PlaceholderPalette palette, string assetPath, string rootName)
        {
            GameObject root = CreateCanvasRoot(rootName, 200);
            BattleHUDController controller = root.AddComponent<BattleHUDController>();
            UIActionModeController actionModeController = root.AddComponent<UIActionModeController>();

            GameObject turnBannerObject = InstantiatePrefabAsset(TurnBannerPrefabPath, root.transform, "TurnBanner");
            RectTransform turnBannerRect = turnBannerObject.GetComponent<RectTransform>();
            turnBannerRect.anchorMin = new Vector2(0.5f, 1f);
            turnBannerRect.anchorMax = new Vector2(0.5f, 1f);
            turnBannerRect.pivot = new Vector2(0.5f, 1f);
            turnBannerRect.anchoredPosition = new Vector2(0f, -20f);

            GameObject promptObject = InstantiatePrefabAsset(PromptPanelPrefabPath, root.transform, "PromptPanel");
            RectTransform promptRect = promptObject.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0f);
            promptRect.anchorMax = new Vector2(0.5f, 0f);
            promptRect.pivot = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, 124f);

            GameObject actionPanelObject = InstantiatePrefabAsset(ActionSelectionPrefabPath, root.transform, "ActionSelectionPanel");
            RectTransform actionRect = actionPanelObject.GetComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.5f, 0f);
            actionRect.anchorMax = new Vector2(0.5f, 0f);
            actionRect.pivot = new Vector2(0.5f, 0f);
            actionRect.anchoredPosition = new Vector2(0f, 12f);

            GameObject combatLogObject = InstantiatePrefabAsset(CombatLogPrefabPath, root.transform, "CombatLogPanel");
            RectTransform combatLogRect = combatLogObject.GetComponent<RectTransform>();
            combatLogRect.anchorMin = new Vector2(1f, 0.5f);
            combatLogRect.anchorMax = new Vector2(1f, 0.5f);
            combatLogRect.pivot = new Vector2(1f, 0.5f);
            combatLogRect.anchoredPosition = new Vector2(-18f, 0f);

            HeroPanelView[] heroPanels = new HeroPanelView[4];
            for (int i = 0; i < heroPanels.Length; i++)
            {
                GameObject heroPanelObject = InstantiatePrefabAsset(HeroPanelPrefabPath, root.transform, "HeroPanel_" + i);
                RectTransform heroRect = heroPanelObject.GetComponent<RectTransform>();
                heroRect.anchorMin = new Vector2(0f, 0.5f);
                heroRect.anchorMax = new Vector2(0f, 0.5f);
                heroRect.pivot = new Vector2(0f, 0.5f);
                heroRect.anchoredPosition = new Vector2(18f, 255f - (i * 158f));
                heroPanels[i] = heroPanelObject.GetComponent<HeroPanelView>();
            }

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("actionModeController").objectReferenceValue = actionModeController;
            serialized.FindProperty("turnBannerView").objectReferenceValue = turnBannerObject.GetComponent<TurnBannerView>();
            serialized.FindProperty("skillBarView").objectReferenceValue = actionPanelObject.GetComponent<SkillBarView>();
            serialized.FindProperty("combatFeedbackView").objectReferenceValue = combatLogObject.GetComponent<CombatFeedbackView>();
            serialized.FindProperty("promptView").objectReferenceValue = promptObject.GetComponent<PromptView>();
            serialized.FindProperty("buildRuntimeLayoutIfMissing").boolValue = false;

            SerializedProperty heroPanelsProperty = serialized.FindProperty("heroPanels");
            heroPanelsProperty.arraySize = heroPanels.Length;
            for (int i = 0; i < heroPanels.Length; i++)
            {
                heroPanelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = heroPanels[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(root, assetPath);
        }

        private static void BuildEnemyFloatingHealthBarPrefab(PlaceholderPalette palette, string assetPath, string rootName)
        {
            GameObject root = CreateRectObject(rootName);
            root.layer = 5;
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(120f, 18f);
            rootRect.localScale = Vector3.one * 0.01f;

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10;

            EnemyFloatingHealthBarView view = root.AddComponent<EnemyFloatingHealthBarView>();

            GameObject backgroundRoot = CreateRectObject("Background");
            backgroundRoot.transform.SetParent(root.transform, false);
            Stretch(backgroundRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            Image backgroundImage = backgroundRoot.AddComponent<Image>();
            backgroundImage.sprite = palette.Gray100;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = new Color(1f, 1f, 1f, 0.88f);

            GameObject fillRoot = CreateRectObject("Fill");
            fillRoot.transform.SetParent(backgroundRoot.transform, false);
            RectTransform fillRect = fillRoot.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            Image fillImage = fillRoot.AddComponent<Image>();
            fillImage.sprite = palette.Gray20;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = Color.white;

            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("worldCanvas").objectReferenceValue = canvas;
            serialized.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            serialized.FindProperty("fillImage").objectReferenceValue = fillImage;
            serialized.FindProperty("fillRect").objectReferenceValue = fillRect;
            serialized.FindProperty("barRoot").objectReferenceValue = root;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, assetPath);
        }

        private static void BuildStartMenuPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateCanvasRoot("UI_StartMenuCanvas", 150);
            StartMenuController controller = root.AddComponent<StartMenuController>();

            GameObject background = CreateRectObject("Background");
            background.transform.SetParent(root.transform, false);
            Stretch(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.sprite = palette.Gray100;
            backgroundImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);

            GameObject panel = CreateRectObject("MainPanel");
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(1120f, 720f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.74f);

            TMP_Text title = CreateText(panel.transform, "Title", "Bone Throne", 56, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.offsetMin = new Vector2(24f, -90f);
            title.rectTransform.offsetMax = new Vector2(-24f, -16f);

            TMP_Text subtitle = CreateText(panel.transform, "Subtitle", "Single-player tactics entry scene", 24, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
            subtitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.rectTransform.offsetMin = new Vector2(32f, -142f);
            subtitle.rectTransform.offsetMax = new Vector2(-32f, -92f);

            GameObject characterSelection = InstantiatePrefabAsset(CharacterSelectionPrefabPath, panel.transform, "CharacterSelection");
            RectTransform characterSelectionRect = characterSelection.GetComponent<RectTransform>();
            characterSelectionRect.anchorMin = new Vector2(0.5f, 0.5f);
            characterSelectionRect.anchorMax = new Vector2(0.5f, 0.5f);
            characterSelectionRect.pivot = new Vector2(0.5f, 0.5f);
            characterSelectionRect.anchoredPosition = new Vector2(0f, 40f);

            TMP_Text note = CreateText(panel.transform, "Note", "Temporary grayscale UI placeholders are mounted here and will be replaced with final art later.", 20, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
            note.rectTransform.anchorMin = new Vector2(0f, 0f);
            note.rectTransform.anchorMax = new Vector2(1f, 0f);
            note.rectTransform.pivot = new Vector2(0.5f, 0f);
            note.rectTransform.offsetMin = new Vector2(28f, 122f);
            note.rectTransform.offsetMax = new Vector2(-28f, 182f);

            GameObject buttons = CreateRectObject("Buttons");
            buttons.transform.SetParent(panel.transform, false);
            RectTransform buttonsRect = buttons.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0f);
            buttonsRect.pivot = new Vector2(0.5f, 0f);
            buttonsRect.sizeDelta = new Vector2(420f, 68f);
            buttonsRect.anchoredPosition = new Vector2(0f, 28f);
            HorizontalLayoutGroup buttonLayout = buttons.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 16f;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;

            GameObject startButtonRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttons.transform, "StartButton");
            GameObject quitButtonRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttons.transform, "QuitButton");
            Button startButton = startButtonRoot.GetComponent<Button>();
            Button quitButton = quitButtonRoot.GetComponent<Button>();
            TMP_Text startLabel = startButtonRoot.GetComponentInChildren<TMP_Text>(true);
            TMP_Text quitLabel = quitButtonRoot.GetComponentInChildren<TMP_Text>(true);
            if (startLabel != null)
            {
                startLabel.text = "Enter Game";
            }

            if (quitLabel != null)
            {
                quitLabel.text = "Quit";
            }

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("titleText").objectReferenceValue = title;
            serialized.FindProperty("subtitleText").objectReferenceValue = subtitle;
            serialized.FindProperty("noteText").objectReferenceValue = note;
            serialized.FindProperty("startButton").objectReferenceValue = startButton;
            serialized.FindProperty("quitButton").objectReferenceValue = quitButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, StartMenuPrefabPath);
        }

        private static void BuildEndMenuPrefab(PlaceholderPalette palette)
        {
            GameObject root = CreateCanvasRoot("UI_EndMenuCanvas", 150);
            EndMenuController controller = root.AddComponent<EndMenuController>();

            GameObject background = CreateRectObject("Background");
            background.transform.SetParent(root.transform, false);
            Stretch(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.sprite = palette.Gray100;
            backgroundImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);

            GameObject panel = CreateRectObject("MainPanel");
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(920f, 520f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.74f);

            TMP_Text title = CreateText(panel.transform, "Title", "Run Complete", 50, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.offsetMin = new Vector2(24f, -90f);
            title.rectTransform.offsetMax = new Vector2(-24f, -20f);

            TMP_Text summary = CreateText(panel.transform, "Summary", "Victory / ending placeholder scene.", 24, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
            summary.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            summary.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            summary.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            summary.rectTransform.sizeDelta = new Vector2(-48f, 120f);
            summary.rectTransform.anchoredPosition = new Vector2(0f, 40f);

            GameObject buttons = CreateRectObject("Buttons");
            buttons.transform.SetParent(panel.transform, false);
            RectTransform buttonsRect = buttons.GetComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.5f, 0f);
            buttonsRect.anchorMax = new Vector2(0.5f, 0f);
            buttonsRect.pivot = new Vector2(0.5f, 0f);
            buttonsRect.sizeDelta = new Vector2(420f, 68f);
            buttonsRect.anchoredPosition = new Vector2(0f, 32f);
            HorizontalLayoutGroup buttonLayout = buttons.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 16f;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;

            GameObject returnButtonRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttons.transform, "ReturnToMenuButton");
            GameObject quitButtonRoot = InstantiatePrefabAsset(ButtonBasePrefabPath, buttons.transform, "QuitButton");
            Button returnButton = returnButtonRoot.GetComponent<Button>();
            Button quitButton = quitButtonRoot.GetComponent<Button>();
            TMP_Text returnLabel = returnButtonRoot.GetComponentInChildren<TMP_Text>(true);
            TMP_Text quitLabel = quitButtonRoot.GetComponentInChildren<TMP_Text>(true);
            if (returnLabel != null)
            {
                returnLabel.text = "Back To Start";
            }

            if (quitLabel != null)
            {
                quitLabel.text = "Quit";
            }

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("titleText").objectReferenceValue = title;
            serialized.FindProperty("summaryText").objectReferenceValue = summary;
            serialized.FindProperty("returnToMenuButton").objectReferenceValue = returnButton;
            serialized.FindProperty("quitButton").objectReferenceValue = quitButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, EndMenuPrefabPath);
        }

        private static void UpdateStartMenuScene()
        {
            if (!File.Exists(StartMenuScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(StartMenuScenePath, OpenSceneMode.Single);
            RemoveObjectsOfType<StartMenuController>();
            EnsureEventSystem();
            InstantiatePrefabAsset(StartMenuPrefabPath, null, "UI_StartMenuCanvas");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateEndMenuScene()
        {
            if (!File.Exists(EndMenuScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(EndMenuScenePath, OpenSceneMode.Single);
            RemoveObjectsOfType<EndMenuController>();
            EnsureEventSystem();
            InstantiatePrefabAsset(EndMenuPrefabPath, null, "UI_EndMenuCanvas");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateBossTestScene()
        {
            if (!File.Exists(BossTestScenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(BossTestScenePath, OpenSceneMode.Single);
            EnsureEventSystem();

            GameOutcomeService outcomeService = Object.FindFirstObjectByType<GameOutcomeService>();
            if (outcomeService == null)
            {
                GameObject outcomeObject = new GameObject("BossTestOutcomeService");
                outcomeService = outcomeObject.AddComponent<GameOutcomeService>();
            }

            RetryCurrentSceneController retryController = Object.FindFirstObjectByType<RetryCurrentSceneController>();
            if (retryController == null)
            {
                GameObject retryObject = new GameObject("BossTestRetryController");
                retryController = retryObject.AddComponent<RetryCurrentSceneController>();
            }

            BattleOutcomeAutoEvaluator evaluator = Object.FindFirstObjectByType<BattleOutcomeAutoEvaluator>();
            if (evaluator == null)
            {
                GameObject evaluatorObject = new GameObject("BossTestOutcomeEvaluator");
                evaluator = evaluatorObject.AddComponent<BattleOutcomeAutoEvaluator>();
            }

            RemoveObjectsOfType<GameResultPanelController>();
            GameObject resultPrefabRoot = InstantiatePrefabAsset(ResultPanelPrefabPath, null, "UI_GameResultCanvas");
            GameResultPanelController resultPanel = resultPrefabRoot != null ? resultPrefabRoot.GetComponent<GameResultPanelController>() : null;

            RemoveObjectsOfType<BattleHUDController>();
            InstantiatePrefabAsset(SharedBattleHudPrefabPath, null, "BattleHUD");

            BindBossTestOutcomeObjects(outcomeService, retryController, evaluator, resultPanel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BindBossTestOutcomeObjects(
            GameOutcomeService outcomeService,
            RetryCurrentSceneController retryController,
            BattleOutcomeAutoEvaluator evaluator,
            GameResultPanelController resultPanel)
        {
            if (retryController != null)
            {
                SerializedObject serialized = new SerializedObject(retryController);
                serialized.FindProperty("outcomeService").objectReferenceValue = outcomeService;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            if (evaluator != null)
            {
                List<Unit> players = new List<Unit>();
                List<Unit> enemies = new List<Unit>();
                Unit[] sceneUnits = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < sceneUnits.Length; i++)
                {
                    if (sceneUnits[i] == null)
                    {
                        continue;
                    }

                    if (sceneUnits[i].Faction == UnitFaction.Player)
                    {
                        players.Add(sceneUnits[i]);
                    }
                    else if (sceneUnits[i].Faction == UnitFaction.Enemy)
                    {
                        enemies.Add(sceneUnits[i]);
                    }
                }

                SerializedObject serialized = new SerializedObject(evaluator);
                serialized.FindProperty("outcomeService").objectReferenceValue = outcomeService;
                serialized.FindProperty("defeatReason").stringValue = "玩家队伍全员倒下。";
                serialized.FindProperty("victoryReason").stringValue = "所有追踪敌人已被击败。";
                SetObjectArray(serialized.FindProperty("trackedPlayerUnits"), players);
                SetObjectArray(serialized.FindProperty("trackedVictoryUnits"), enemies);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            if (resultPanel != null)
            {
                SerializedObject serialized = new SerializedObject(resultPanel);
                serialized.FindProperty("outcomeService").objectReferenceValue = outcomeService;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetObjectArray<T>(SerializedProperty arrayProperty, IList<T> values)
            where T : Object
        {
            arrayProperty.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void RemoveObjectsOfType<T>()
            where T : Component
        {
            T[] objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    Object.DestroyImmediate(objects[i].gameObject);
                }
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject InstantiatePrefabAsset(string assetPath, Transform parent, string overrideName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance != null && !string.IsNullOrEmpty(overrideName))
            {
                instance.name = overrideName;
            }

            return instance;
        }

        private static void SavePrefab(GameObject root, string assetPath)
        {
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateCanvasRoot(string name, int sortingOrder)
        {
            GameObject root = CreateRectObject(name);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            RectTransform rectTransform = root.GetComponent<RectTransform>();
            NormalizeRectTransform(rectTransform);
            Stretch(rectTransform, Vector2.zero, Vector2.zero);
            return root;
        }

        private static GameObject CreateRectObject(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = 5;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            NormalizeRectTransform(rectTransform);
            return gameObject;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string value,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.layer = 5;
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.font = GetDefaultFontAsset();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.enableAutoSizing = false;
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static TMP_FontAsset GetDefaultFontAsset()
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            const string fallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fallbackPath);
        }

        private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            NormalizeRectTransform(rectTransform);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void StretchTitle(RectTransform rectTransform, float left, float right, float height)
        {
            NormalizeRectTransform(rectTransform);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.offsetMin = new Vector2(left, -height);
            rectTransform.offsetMax = new Vector2(-right, -8f);
        }

        private static void NormalizeRectTransform(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string combined = parent.TrimEnd('/') + "/" + child;
            if (!AssetDatabase.IsValidFolder(combined))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
