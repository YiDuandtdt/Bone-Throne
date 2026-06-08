using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Editor
{
    public static class SmileySansFontInstaller
    {
        private const string FontFilePath = "Assets/_BoneThrone/Art/Fonts/SmileySans.ttf";
        private const string FontAssetPath = "Assets/_BoneThrone/Art/Fonts/SmileySans SDF.asset";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        [MenuItem("BoneThrone/Fonts/Install Smiley Sans")]
        public static void InstallSmileySans()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Smiley Sans install was skipped because Unity is in Play Mode or entering Play Mode.");
                return;
            }

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontFilePath);
            if (sourceFont == null)
            {
                Debug.LogError("Smiley Sans source font not found at " + FontFilePath);
                return;
            }

            TMP_FontAsset fontAsset = EnsureFontAsset(sourceFont);
            if (fontAsset == null)
            {
                Debug.LogError("Failed to create or load TMP font asset for Smiley Sans.");
                return;
            }

            AssignDefaultFont(fontAsset);
            ReplaceFontReferences(fontAsset, SceneManager.GetActiveScene().path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Smiley Sans font installation completed.");
        }

        private static TMP_FontAsset EnsureFontAsset(Font sourceFont)
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null)
            {
                string existingSourcePath = AssetDatabase.GetAssetPath(existing.sourceFontFile);
                if (existingSourcePath == FontFilePath)
                {
                    return existing;
                }

                AssetDatabase.DeleteAsset(FontAssetPath);
                AssetDatabase.Refresh();
            }

            TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);

            if (created == null)
            {
                return null;
            }

            created.name = "SmileySans SDF";
            AssetDatabase.CreateAsset(created, FontAssetPath);

            if (created.atlasTextures != null)
            {
                for (int i = 0; i < created.atlasTextures.Length; i++)
                {
                    if (created.atlasTextures[i] != null && AssetDatabase.GetAssetPath(created.atlasTextures[i]) != FontAssetPath)
                    {
                        AssetDatabase.AddObjectToAsset(created.atlasTextures[i], created);
                    }
                }
            }

            if (created.material != null && AssetDatabase.GetAssetPath(created.material) != FontAssetPath)
            {
                AssetDatabase.AddObjectToAsset(created.material, created);
            }

            EditorUtility.SetDirty(created);
            return created;
        }

        private static void AssignDefaultFont(TMP_FontAsset fontAsset)
        {
            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            if (settings != null)
            {
                TMP_Settings.defaultFontAsset = fontAsset;
                EditorUtility.SetDirty(settings);
            }
        }

        private static void ReplaceFontReferences(TMP_FontAsset fontAsset, string activeScenePath)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab t:Scene");
            HashSet<string> processedAssets = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path) || !processedAssets.Add(path))
                {
                    continue;
                }

                if (path.EndsWith(".prefab"))
                {
                    ReplaceFontInPrefab(path, fontAsset);
                    continue;
                }

                if (path.EndsWith(".unity"))
                {
                    if (!string.IsNullOrEmpty(activeScenePath) && path == activeScenePath)
                    {
                        Debug.Log("Skipping active open scene during Smiley Sans install: " + path);
                        continue;
                    }

                    ReplaceFontInScene(path, fontAsset);
                }
            }
        }

        private static void ReplaceFontInPrefab(string path, TMP_FontAsset fontAsset)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = ReplaceFontInHierarchy(root, fontAsset);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ReplaceFontInScene(string path, TMP_FontAsset fontAsset)
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                bool changed = false;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    changed |= ReplaceFontInHierarchy(roots[i], fontAsset);
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        private static bool ReplaceFontInHierarchy(GameObject root, TMP_FontAsset fontAsset)
        {
            bool changed = false;
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null || text.font == fontAsset)
                {
                    continue;
                }

                text.font = fontAsset;
                if (fontAsset.material != null)
                {
                    text.fontSharedMaterial = fontAsset.material;
                }

                EditorUtility.SetDirty(text);
                changed = true;
            }

            return changed;
        }
    }
}
