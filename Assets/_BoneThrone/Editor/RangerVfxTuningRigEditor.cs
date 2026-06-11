using System.IO;
using BoneThrone.Units;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoneThrone.Editor
{
    [CustomEditor(typeof(RangerVfxTuningRig))]
    public sealed class RangerVfxTuningRigEditor : UnityEditor.Editor
    {
        private const string RangerPrefabPath = "Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab";
        private const string TargetPrefabPath = "Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Minion.prefab";
        private const string ArrowPrefabPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Ranger/BTVFX_Ranger_Arrow_KayKit.prefab";
        private const string Skill1PrefabPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Ranger/1.prefab";
        private const string Skill2PrefabPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Ranger/2.prefab";
        private const string Skill3PrefabPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Ranger/3.prefab";
        private const string TuningScenePath = "Assets/_BoneThrone/Scenes/VFX_Tuning.unity";
        private const string TuningSceneRootName = "BoneThrone_VFX_Tuning_Scene_Root";
        private const string RangerRigName = "Ranger_VFX_TuningRig";
        private const string MageRigName = "Mage_VFX_TuningRig";
        private const string MagePrefabPath = "Assets/_BoneThrone/Prefabs/Units/Players/Mage.prefab";
        private const string SkeletonMagePrefabPath = "Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Mage.prefab";
        private const string SkeletonRoguePrefabPath = "Assets/_BoneThrone/Prefabs/Units/Enemies/Skeleton_Rogue.prefab";
        private const string MageBasicImpactPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Mage/BTVFX_Mage_Basic_ImpactBlue.prefab";
        private const string MageFireballImpactPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Mage/BTVFX_Mage_Fireball_ImpactFire.prefab";
        private const string MageFrostImpactPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Mage/BTVFX_Mage_FrostBolt_ImpactBlue.prefab";
        private const string MageArcaneImpactPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Mage/BTVFX_Mage_ArcaneBurst_ImpactBlue.prefab";
        private const string MageArcaneNovaPath = "Assets/_BoneThrone/Art/VFX_Runtime/Skills/Mage/BTVFX_Mage_ArcaneBurst_NovaBlue.prefab";
        private const string EnemyDarkSpellPath = "Assets/_BoneThrone/Art/VFX_Runtime/Boss/BTVFX_Boss_DarkImpact_SoulPurple.prefab";
        private static readonly Vector3 ArrowMeshForwardOffsetEuler = Vector3.zero;

        [InitializeOnLoadMethod]
        private static void ScheduleLegacyRigCleanup()
        {
            EditorApplication.delayCall += AutoCleanGeneratedRigsOutsideTuningScene;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This rig uses real scene GameObjects. Move/rotate/scale Arrow_Preview_REAL_PREFAB and Skill1/2/3_REAL_PREFAB directly in Scene view, then apply the captured transforms to the Ranger prefab.",
                MessageType.Info);

            RangerVfxTuningRig rig = (RangerVfxTuningRig)target;
            if (GUILayout.Button("Refresh Real Particle Preview"))
            {
                RefreshParticlePreviews(rig);
            }

            if (GUILayout.Button("Apply Scene Transforms To Ranger Prefab"))
            {
                ApplyCapturedValues(rig);
            }
        }

        [MenuItem("BoneThrone/VFX/Ranger/Create Real Tuning Rig")]
        private static void CreateRealTuningRig()
        {
            OpenDedicatedTuningScene();
        }

        [MenuItem("BoneThrone/VFX/Tuning/Open Dedicated VFX Tuning Scene")]
        private static void OpenDedicatedTuningScene()
        {
            CleanGeneratedRigsOutsideTuningScene();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene;
            if (File.Exists(TuningScenePath))
            {
                scene = EditorSceneManager.OpenScene(TuningScenePath, OpenSceneMode.Single);
                CleanGeneratedRigsInScene(scene);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            BuildDedicatedTuningScene(scene);
            EditorSceneManager.SaveScene(scene, TuningScenePath);
            Selection.activeGameObject = GameObject.Find(TuningSceneRootName);
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        [MenuItem("BoneThrone/VFX/Tuning/Clean Current Scene Generated Rigs")]
        private static void CleanCurrentSceneGeneratedRigs()
        {
            int removedCount = CleanGeneratedRigsInScene(SceneManager.GetActiveScene());
            Debug.Log($"Removed {removedCount} generated VFX tuning root object(s) from the current scene.");
        }

        [MenuItem("BoneThrone/VFX/Tuning/Apply Current Tuning Scene To Runtime Prefabs")]
        private static void ApplyCurrentTuningSceneToRuntimePrefabs()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TuningScenePath)
            {
                Debug.LogError($"Open {TuningScenePath} before applying VFX tuning values. Current scene is '{activeScene.path}'.");
                return;
            }

            int appliedCount = 0;
            if (ApplyRangerPreviewValuesFromHierarchy())
            {
                appliedCount++;
            }
            else
            {
                RangerVfxTuningRig rangerRig = Object.FindFirstObjectByType<RangerVfxTuningRig>();
                if (rangerRig != null)
                {
                    ApplyCapturedValues(rangerRig);
                    appliedCount++;
                }
                else
                {
                    Debug.LogWarning("No Ranger VFX preview hierarchy or Ranger_VFX_TuningRig component found in the current tuning scene.");
                }
            }

            if (ApplyMagePreviewValues())
            {
                appliedCount++;
            }

            if (ApplyEnemyReusableValues())
            {
                appliedCount++;
            }

            Debug.Log($"Applied VFX tuning scene values to runtime prefabs. Applied groups={appliedCount}.");
        }

        private static void AutoCleanGeneratedRigsOutsideTuningScene()
        {
            if (Application.isPlaying)
            {
                return;
            }

            CleanGeneratedRigsOutsideTuningScene();
        }

        private static void CleanGeneratedRigsOutsideTuningScene()
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.IsValid() || scene.path == TuningScenePath)
                {
                    continue;
                }

                CleanGeneratedRigsInScene(scene);
            }
        }

        private static int CleanGeneratedRigsInScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return 0;
            }

            int removedCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = roots.Length - 1; i >= 0; i--)
            {
                GameObject root = roots[i];
                if (root == null || !IsGeneratedTuningRootName(root.name))
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(root);
                removedCount++;
            }

            if (removedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            return removedCount;
        }

        private static bool IsGeneratedTuningRootName(string objectName)
        {
            return objectName == TuningSceneRootName ||
                   objectName == RangerRigName ||
                   objectName == MageRigName;
        }

        private static void BuildDedicatedTuningScene(Scene scene)
        {
            GameObject root = new GameObject(TuningSceneRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            CreateLighting(root.transform);
            CreateReferenceFloor(root.transform);
            CreateRangerRig(root.transform, new Vector3(-3.5f, 0f, 0f));
            CreateMagePreviewRig(root.transform, new Vector3(3.5f, 0f, 0f));
            CreateEnemyReusablePreviewRig(root.transform, new Vector3(3.5f, 0f, -4f));
        }

        private static void CreateRangerRig(Transform parent, Vector3 offset)
        {
            GameObject rangerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RangerPrefabPath);
            GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);
            GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
            GameObject skill1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Skill1PrefabPath);
            GameObject skill2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Skill2PrefabPath);
            GameObject skill3Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Skill3PrefabPath);

            if (rangerPrefab == null || targetPrefab == null || arrowPrefab == null)
            {
                Debug.LogError("Cannot create Ranger VFX tuning rig because Ranger, target, or arrow prefab path is missing.");
                return;
            }

            RangerHitPresentationConfig sourceConfig = rangerPrefab.GetComponent<RangerHitPresentationConfig>();
            CurrentRangerVfxSettings currentSettings = CurrentRangerVfxSettings.Read(sourceConfig);

            GameObject root = new GameObject(RangerRigName);
            root.transform.SetParent(parent, true);
            root.transform.position = offset;

            Transform rangerOrigin = CreateEmptyChild(root.transform, "Ranger_Origin_ATTACKER_POSITION", offset);
            GameObject target = InstantiatePrefab(targetPrefab, root.transform, "Target_REAL_Skeleton_Minion");
            target.transform.position = offset + new Vector3(2f, 0f, 0f);
            target.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            Transform targetAnchor = ResolveTargetAnchor(target.transform);

            Vector3 direction = Vector3.right;
            Quaternion arrowBaseRotation = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 impactPosition = targetAnchor.TransformPoint(currentSettings.EmbeddedArrowLocalOffset);
            Vector3 startPosition = impactPosition - direction * Mathf.Max(0f, currentSettings.ArrowStartDistanceFromTarget);

            Transform arrowStart = CreateEmptyChild(root.transform, "Arrow_Start_MOVE_THIS", startPosition);
            Transform arrowImpact = CreateEmptyChild(root.transform, "Arrow_Impact_STICKS_HERE", impactPosition);

            GameObject arrow = InstantiatePrefab(arrowPrefab, root.transform, "Arrow_Preview_REAL_PREFAB_MOVE_ROTATE_SCALE");
            arrow.transform.SetPositionAndRotation(startPosition, arrowBaseRotation * Quaternion.Euler(currentSettings.ArrowRotationOffsetEuler));
            arrow.transform.localScale = currentSettings.ArrowWorldScale;

            GameObject skill1 = InstantiatePrefab(skill1Prefab, root.transform, "Skill1_Precision_REAL_PREFAB_MOVE_ROTATE_SCALE");
            GameObject skill2 = InstantiatePrefab(skill2Prefab, root.transform, "Skill2_Quick_REAL_PREFAB_MOVE_ROTATE_SCALE");
            GameObject skill3 = InstantiatePrefab(skill3Prefab, root.transform, "Skill3_Piercing_REAL_PREFAB_MOVE_ROTATE_SCALE");
            ApplyAttachmentTransform(targetAnchor, skill1.transform, currentSettings.PrecisionShot);
            ApplyAttachmentTransform(targetAnchor, skill2.transform, currentSettings.QuickShot);
            ApplyAttachmentTransform(targetAnchor, skill3.transform, currentSettings.PiercingArrow);

            RangerVfxTuningRig rig = root.AddComponent<RangerVfxTuningRig>();
            rig.Initialize(
                rangerPrefab,
                rangerOrigin,
                target.transform,
                targetAnchor,
                arrowStart,
                arrowImpact,
                arrow.transform,
                skill1.transform,
                skill2.transform,
                skill3.transform);

            RefreshParticlePreviews(rig);
        }

        private static void CreateMagePreviewRig(Transform parent, Vector3 offset)
        {
            GameObject magePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MagePrefabPath);
            GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath);
            GameObject basicImpactPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MageBasicImpactPath);
            GameObject fireballImpactPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MageFireballImpactPath);
            GameObject frostImpactPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MageFrostImpactPath);
            GameObject darkSkillPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyDarkSpellPath);

            GameObject root = new GameObject(MageRigName);
            root.transform.SetParent(parent, true);
            root.transform.position = offset;

            if (magePrefab != null)
            {
                GameObject mage = InstantiatePrefab(magePrefab, root.transform, "Mage_REAL_Attacker_Reference");
                mage.transform.position = offset;
                mage.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            }

            GameObject target = InstantiatePrefab(targetPrefab, root.transform, "Target_REAL_Skeleton_For_Mage_VFX");
            target.transform.position = offset + new Vector3(2f, 0f, 0f);
            target.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            Transform targetAnchor = ResolveTargetAnchor(target.transform);
            Vector3 chest = targetAnchor != null ? targetAnchor.TransformPoint(new Vector3(0f, 1.05f, 0f)) : target.transform.position + Vector3.up;

            GameObject basicImpact = InstantiatePrefab(basicImpactPrefab, root.transform, "Mage_BasicAttack_Impact_REAL_PREFAB");
            basicImpact.transform.position = chest + new Vector3(-0.65f, 0f, 0f);
            GameObject fireballImpact = InstantiatePrefab(fireballImpactPrefab, root.transform, "Mage_Skill1_Fireball_Impact_REAL_PREFAB");
            fireballImpact.transform.position = chest;
            GameObject frostImpact = InstantiatePrefab(frostImpactPrefab, root.transform, "Mage_Skill2_FrostBolt_Impact_REAL_PREFAB");
            frostImpact.transform.position = chest + new Vector3(0.65f, 0f, 0f);
            GameObject darkSkillImpact = InstantiatePrefab(darkSkillPrefab, root.transform, "Mage_Skill3_DarkSpell_REAL_PREFAB_MOVE_ROTATE_SCALE");
            darkSkillImpact.transform.position = chest + new Vector3(0f, 0f, 0.65f);

            SimulateParticles(basicImpact.transform, 0.35f);
            SimulateParticles(fireballImpact.transform, 0.35f);
            SimulateParticles(frostImpact.transform, 0.35f);
            SimulateParticles(darkSkillImpact.transform, 0.35f);
        }

        private static void CreateEnemyReusablePreviewRig(Transform parent, Vector3 offset)
        {
            GameObject roguePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SkeletonRoguePrefabPath);
            GameObject magePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SkeletonMagePrefabPath);
            GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath);
            GameObject darkSpellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyDarkSpellPath);

            GameObject root = new GameObject("Enemy_Reusable_VFX_Only_Arrow_And_DarkSpell");
            root.transform.SetParent(parent, true);
            root.transform.position = offset;

            GameObject rogue = InstantiatePrefab(roguePrefab, root.transform, "Enemy_RangerLike_Skeleton_Rogue_Reference");
            rogue.transform.position = offset;
            rogue.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

            GameObject mage = InstantiatePrefab(magePrefab, root.transform, "Enemy_Mage_Skeleton_Mage_Reference");
            mage.transform.position = offset + new Vector3(0f, 0f, 1.6f);
            mage.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);

            GameObject arrow = InstantiatePrefab(arrowPrefab, root.transform, "Enemy_Reusable_NormalArrow_REAL_PREFAB");
            arrow.transform.position = offset + new Vector3(2f, 1.05f, 0f);
            arrow.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
            arrow.transform.localScale = Vector3.one;

            GameObject darkSpell = InstantiatePrefab(darkSpellPrefab, root.transform, "Enemy_Reusable_BasicDarkSpell_REAL_PREFAB");
            darkSpell.transform.position = offset + new Vector3(2f, 1.05f, 1.6f);
            SimulateParticles(darkSpell.transform, 0.35f);
        }

        private static void CreateLighting(Transform parent)
        {
            GameObject lightObject = new GameObject("Preview_Directional_Light");
            lightObject.transform.SetParent(parent, true);
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static void CreateReferenceFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Preview_Reference_Floor_NotGameplay";
            floor.transform.SetParent(parent, true);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            Collider collider = floor.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void ApplyCapturedValues(RangerVfxTuningRig rig)
        {
            string error = "Ranger VFX tuning rig is missing.";
            RangerVfxTuningSnapshot snapshot = default;
            if (rig == null || !rig.TryCapture(out snapshot, out error))
            {
                Debug.LogError(error);
                return;
            }

            GameObject prefabAsset = rig.RangerPrefabAsset != null
                ? rig.RangerPrefabAsset
                : AssetDatabase.LoadAssetAtPath<GameObject>(RangerPrefabPath);
            string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Ranger prefab asset is missing. Assign Assets/_BoneThrone/Prefabs/Units/Players/Ranger.prefab on the rig.");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RangerHitPresentationConfig config = prefabRoot.GetComponent<RangerHitPresentationConfig>();
                if (config == null)
                {
                    Debug.LogError($"Ranger prefab does not contain {nameof(RangerHitPresentationConfig)}: {prefabPath}");
                    return;
                }

                SerializedObject serializedConfig = new SerializedObject(config);
                SetBool(serializedConfig, "allowAnyUnitWithThisComponent", false);
                SetObject(serializedConfig, "basicAttackArrowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath));
                SetObject(serializedConfig, "skillArrowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath));
                SetObject(serializedConfig, "precisionShotEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill1PrefabPath));
                SetObject(serializedConfig, "quickShotEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill2PrefabPath));
                SetObject(serializedConfig, "piercingArrowEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill3PrefabPath));
                SetFloat(serializedConfig, "arrowStartDistanceFromTarget", snapshot.ArrowStartDistanceFromTarget);
                SetVector3(serializedConfig, "arrowRotationOffsetEuler", snapshot.ArrowRotationOffsetEuler);
                SetVector3(serializedConfig, "arrowWorldScale", snapshot.ArrowWorldScale);
                SetVector3(serializedConfig, "embeddedArrowLocalOffset", snapshot.EmbeddedArrowLocalOffset);
                SetAttachment(serializedConfig, "precisionShot", snapshot.PrecisionShot);
                SetAttachment(serializedConfig, "quickShot", snapshot.QuickShot);
                SetAttachment(serializedConfig, "piercingArrow", snapshot.PiercingArrow);
                serializedConfig.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Applied Ranger VFX tuning values to {prefabPath}. Test Ranger basic attack and skills in Play Mode.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool ApplyRangerPreviewValuesFromHierarchy()
        {
            GameObject rangerRig = GameObject.Find(RangerRigName);
            if (rangerRig == null)
            {
                return false;
            }

            RangerVfxTuningSnapshot snapshot;
            string error;
            if (!TryCaptureRangerSnapshotFromHierarchy(rangerRig.transform, out snapshot, out error))
            {
                Debug.LogWarning(error);
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(RangerPrefabPath);
            try
            {
                RangerHitPresentationConfig config = GetOrAddComponent<RangerHitPresentationConfig>(prefabRoot);
                SerializedObject serializedConfig = new SerializedObject(config);
                SetBool(serializedConfig, "allowAnyUnitWithThisComponent", false);
                SetObject(serializedConfig, "basicAttackArrowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath));
                SetObject(serializedConfig, "skillArrowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath));
                SetObject(serializedConfig, "precisionShotEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill1PrefabPath));
                SetObject(serializedConfig, "quickShotEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill2PrefabPath));
                SetObject(serializedConfig, "piercingArrowEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Skill3PrefabPath));
                SetFloat(serializedConfig, "arrowStartDistanceFromTarget", snapshot.ArrowStartDistanceFromTarget);
                SetVector3(serializedConfig, "arrowRotationOffsetEuler", snapshot.ArrowRotationOffsetEuler);
                SetVector3(serializedConfig, "arrowWorldScale", snapshot.ArrowWorldScale);
                SetVector3(serializedConfig, "embeddedArrowLocalOffset", snapshot.EmbeddedArrowLocalOffset);
                SetAttachment(serializedConfig, "precisionShot", snapshot.PrecisionShot);
                SetAttachment(serializedConfig, "quickShot", snapshot.QuickShot);
                SetAttachment(serializedConfig, "piercingArrow", snapshot.PiercingArrow);
                serializedConfig.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, RangerPrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Applied Ranger VFX values from visible hierarchy previews to {RangerPrefabPath}.");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool TryCaptureRangerSnapshotFromHierarchy(Transform rangerRig, out RangerVfxTuningSnapshot snapshot, out string error)
        {
            snapshot = default;
            error = string.Empty;

            Transform rangerOrigin = FindDeepChildByExactOrParts(rangerRig, "Ranger_Origin_ATTACKER_POSITION", "Ranger", "Origin");
            Transform target = FindDeepChildByExactOrParts(rangerRig, "Target_REAL_Skeleton_Minion", "Target", "REAL", "Skeleton");
            Transform arrowStart = FindDeepChildByExactOrParts(rangerRig, "Arrow_Start_MOVE_THIS", "Arrow", "Start");
            Transform arrowImpact = FindDeepChildByExactOrParts(rangerRig, "Arrow_Impact_STICKS_HERE", "Arrow", "Impact");
            Transform arrowPreview = FindDeepChildByExactOrParts(rangerRig, "Arrow_Preview_REAL_PREFAB_MOVE_ROTATE_SCALE", "Arrow", "Preview");
            Transform skill1Preview = FindDeepChildByExactOrParts(rangerRig, "Skill1_Precision_REAL_PREFAB_MOVE_ROTATE_SCALE", "Skill1");
            Transform skill2Preview = FindDeepChildByExactOrParts(rangerRig, "Skill2_Quick_REAL_PREFAB_MOVE_ROTATE_SCALE", "Skill2");
            Transform skill3Preview = FindDeepChildByExactOrParts(rangerRig, "Skill3_Piercing_REAL_PREFAB_MOVE_ROTATE_SCALE", "Skill3");
            Transform targetAnchor = ResolveTargetAnchor(target);

            if (targetAnchor == null || arrowStart == null || arrowImpact == null || arrowPreview == null)
            {
                error = "Ranger VFX apply skipped: required visible preview objects are missing. Need Target, Arrow_Start, Arrow_Impact, and Arrow_Preview under Ranger_VFX_TuningRig.";
                return false;
            }

            Vector3 originPosition = rangerOrigin != null ? rangerOrigin.position : rangerRig.position;
            Vector3 direction = arrowImpact.position - originPosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = targetAnchor.position - originPosition;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                error = "Ranger VFX apply skipped: cannot calculate attack direction from current preview positions.";
                return false;
            }

            direction.Normalize();

            snapshot.ArrowStartDistanceFromTarget = Mathf.Max(0f, Vector3.Dot(arrowImpact.position - arrowStart.position, direction));
            snapshot.ArrowRotationOffsetEuler = ArrowMeshForwardOffsetEuler;
            snapshot.ArrowWorldScale = ClampScaleVector(arrowPreview.lossyScale);
            snapshot.EmbeddedArrowLocalOffset = CaptureCenteredArrowLocalOffset(targetAnchor, arrowImpact.position);
            snapshot.PrecisionShot = CaptureAttachment(targetAnchor, skill1Preview);
            snapshot.QuickShot = CaptureAttachment(targetAnchor, skill2Preview);
            snapshot.PiercingArrow = CaptureAttachment(targetAnchor, skill3Preview);
            return true;
        }

        private static bool ApplyMagePreviewValues()
        {
            GameObject mageRig = GameObject.Find(MageRigName);
            if (mageRig == null)
            {
                Debug.LogWarning("No Mage_VFX_TuningRig found in the current tuning scene.");
                return false;
            }

            Transform target = FindDeepChildByExactOrParts(mageRig.transform, "Target_REAL_Skeleton_For_Mage_VFX", "Target", "REAL", "Skeleton");
            Transform targetAnchor = ResolveTargetAnchor(target);
            if (targetAnchor == null)
            {
                Debug.LogWarning("Mage tuning target anchor is missing.");
                return false;
            }

            Transform basicPreview = FindDeepChildByExactOrParts(mageRig.transform, "Mage_BasicAttack_Impact_REAL_PREFAB", "BasicAttack");
            Transform fireballPreview = FindDeepChildByExactOrParts(mageRig.transform, "Mage_Skill1_Fireball_Impact_REAL_PREFAB", "Skill1");
            Transform frostPreview = FindDeepChildByExactOrParts(mageRig.transform, "Mage_Skill2_FrostBolt_Impact_REAL_PREFAB", "Skill2");
            Transform darkSkillPreview = FindDeepChildByExactOrParts(mageRig.transform, "Mage_Skill3_DarkSpell_REAL_PREFAB_MOVE_ROTATE_SCALE", "Skill3");

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(MagePrefabPath);
            try
            {
                MageHitPresentationConfig config = GetOrAddComponent<MageHitPresentationConfig>(prefabRoot);
                SerializedObject serializedConfig = new SerializedObject(config);
                SetObject(serializedConfig, "basicAttackImpactEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MageBasicImpactPath));
                SetAttachment(serializedConfig, "basicAttack", CaptureAttachment(targetAnchor, basicPreview));
                SetObject(serializedConfig, "fireballImpactEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MageFireballImpactPath));
                SetAttachment(serializedConfig, "fireball", CaptureAttachment(targetAnchor, fireballPreview));
                SetObject(serializedConfig, "frostBoltImpactEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MageFrostImpactPath));
                SetAttachment(serializedConfig, "frostBolt", CaptureAttachment(targetAnchor, frostPreview));
                SetObject(serializedConfig, "arcaneBurstImpactEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EnemyDarkSpellPath));
                SetAttachment(serializedConfig, "arcaneBurst", CaptureAttachment(targetAnchor, darkSkillPreview));
                SetObject(serializedConfig, "arcaneBurstAreaEffectPrefab", null);
                SetAttachment(serializedConfig, "arcaneBurstArea", CaptureAttachment(targetAnchor, darkSkillPreview));
                serializedConfig.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, MagePrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Applied Mage VFX tuning values to {MagePrefabPath}. Mage skill 3 now uses the enemy dark spell prefab.");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool ApplyEnemyReusableValues()
        {
            GameObject enemyRig = GameObject.Find("Enemy_Reusable_VFX_Only_Arrow_And_DarkSpell");
            if (enemyRig == null)
            {
                Debug.LogWarning("No Enemy_Reusable_VFX_Only_Arrow_And_DarkSpell found in the current tuning scene.");
                return false;
            }

            bool appliedArrow = ApplyEnemyArrowValues(enemyRig.transform);
            bool appliedDarkSpell = ApplyEnemyDarkSpellValues(enemyRig.transform);
            return appliedArrow || appliedDarkSpell;
        }

        private static bool ApplyEnemyArrowValues(Transform enemyRig)
        {
            Transform target = FindDeepChildByExactOrParts(enemyRig, "Enemy_RangerLike_Skeleton_Rogue_Reference", "Rogue", "Reference");
            Transform arrowPreview = FindDeepChildByExactOrParts(enemyRig, "Enemy_Reusable_NormalArrow_REAL_PREFAB", "NormalArrow");
            Transform targetAnchor = ResolveTargetAnchor(target);
            if (targetAnchor == null || arrowPreview == null)
            {
                Debug.LogWarning("Enemy reusable arrow preview or rogue reference is missing.");
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(SkeletonRoguePrefabPath);
            try
            {
                RangerHitPresentationConfig config = GetOrAddComponent<RangerHitPresentationConfig>(prefabRoot);
                SerializedObject serializedConfig = new SerializedObject(config);
                SetBool(serializedConfig, "allowAnyUnitWithThisComponent", true);
                SetObject(serializedConfig, "basicAttackArrowPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ArrowPrefabPath));
                SetObject(serializedConfig, "skillArrowPrefab", null);
                SetFloat(serializedConfig, "arrowStartDistanceFromTarget", 0.5f);
                SetVector3(serializedConfig, "arrowWorldScale", ClampScaleVector(arrowPreview.lossyScale));
                SetVector3(serializedConfig, "embeddedArrowLocalOffset", CaptureCenteredArrowLocalOffset(targetAnchor, arrowPreview.position));
                SetVector3(serializedConfig, "arrowRotationOffsetEuler", ArrowMeshForwardOffsetEuler);
                serializedConfig.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, SkeletonRoguePrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Applied enemy reusable normal arrow values to {SkeletonRoguePrefabPath}.");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool ApplyEnemyDarkSpellValues(Transform enemyRig)
        {
            Transform target = FindDeepChildByExactOrParts(enemyRig, "Enemy_Mage_Skeleton_Mage_Reference", "Mage", "Reference");
            Transform darkSpellPreview = FindDeepChildByExactOrParts(enemyRig, "Enemy_Reusable_BasicDarkSpell_REAL_PREFAB", "DarkSpell");
            Transform targetAnchor = ResolveTargetAnchor(target);
            if (targetAnchor == null || darkSpellPreview == null)
            {
                Debug.LogWarning("Enemy reusable dark spell preview or mage reference is missing.");
                return false;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(SkeletonMagePrefabPath);
            try
            {
                MageHitPresentationConfig config = GetOrAddComponent<MageHitPresentationConfig>(prefabRoot);
                SerializedObject serializedConfig = new SerializedObject(config);
                SetObject(serializedConfig, "basicAttackImpactEffectPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EnemyDarkSpellPath));
                SetAttachment(serializedConfig, "basicAttack", CaptureAttachment(targetAnchor, darkSpellPreview));
                serializedConfig.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, SkeletonMagePrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Applied enemy reusable basic dark spell values to {SkeletonMagePrefabPath}.");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void RefreshParticlePreviews(RangerVfxTuningRig rig)
        {
            if (rig == null)
            {
                return;
            }

            SimulateParticles(rig.PrecisionShotPreview, rig.ParticlePreviewTime);
            SimulateParticles(rig.QuickShotPreview, rig.ParticlePreviewTime);
            SimulateParticles(rig.PiercingArrowPreview, rig.ParticlePreviewTime);
            SceneView.RepaintAll();
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, string instanceName)
        {
            GameObject instance;
            if (prefab != null)
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                instance = new GameObject(instanceName);
            }

            instance.name = instanceName;
            instance.transform.SetParent(parent, true);
            instance.SetActive(true);
            return instance;
        }

        private static Transform CreateEmptyChild(Transform parent, string childName, Vector3 position)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent, true);
            child.transform.position = position;
            return child.transform;
        }

        private static Transform ResolveTargetAnchor(Transform targetRoot)
        {
            Animator animator = targetRoot != null ? targetRoot.GetComponentInChildren<Animator>(true) : null;
            return animator != null ? animator.transform : targetRoot;
        }

        private static void ApplyAttachmentTransform(Transform targetAnchor, Transform preview, RangerVfxAttachmentSnapshot snapshot)
        {
            if (targetAnchor == null || preview == null)
            {
                return;
            }

            preview.position = targetAnchor.TransformPoint(snapshot.LocalOffset);
            preview.rotation = targetAnchor.rotation * Quaternion.Euler(snapshot.LocalEulerAngles);
            preview.localScale = snapshot.LocalScale;
        }

        private static void SimulateParticles(Transform root, float previewTime)
        {
            if (root == null)
            {
                return;
            }

            ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.gameObject.SetActive(true);
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Clear(true);
                particleSystem.useAutoRandomSeed = false;
                particleSystem.randomSeed = 1u;
                particleSystem.Simulate(Mathf.Max(0f, previewTime), true, true, true);
            }
        }

        private static void SetAttachment(SerializedObject serializedObject, string fieldPrefix, RangerVfxAttachmentSnapshot snapshot)
        {
            SetVector3(serializedObject, fieldPrefix + "LocalOffset", snapshot.LocalOffset);
            SetVector3(serializedObject, fieldPrefix + "LocalEulerAngles", snapshot.LocalEulerAngles);
            SetVector3(serializedObject, fieldPrefix + "LocalScale", snapshot.LocalScale);
        }

        private static RangerVfxAttachmentSnapshot CaptureAttachment(Transform targetAnchor, Transform preview)
        {
            if (targetAnchor == null || preview == null)
            {
                return new RangerVfxAttachmentSnapshot
                {
                    LocalOffset = new Vector3(0f, 1.05f, 0f),
                    LocalEulerAngles = Vector3.zero,
                    LocalScale = Vector3.one
                };
            }

            Quaternion localRotation = Quaternion.Inverse(targetAnchor.rotation) * preview.rotation;
            return new RangerVfxAttachmentSnapshot
            {
                LocalOffset = targetAnchor.InverseTransformPoint(preview.position),
                LocalEulerAngles = localRotation.eulerAngles,
                LocalScale = ClampScaleVector(preview.lossyScale)
            };
        }

        private static Vector3 CaptureCenteredArrowLocalOffset(Transform targetAnchor, Vector3 previewWorldPosition)
        {
            if (targetAnchor == null)
            {
                return new Vector3(0f, 1.05f, 0f);
            }

            Vector3 localOffset = targetAnchor.InverseTransformPoint(previewWorldPosition);
            return new Vector3(0f, localOffset.y, 0f);
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeepChildByExactOrParts(Transform root, string exactName, params string[] requiredNameParts)
        {
            Transform exact = FindDeepChild(root, exactName);
            return exact != null ? exact : FindDeepChildContaining(root, requiredNameParts);
        }

        private static Transform FindDeepChildContaining(Transform root, params string[] requiredNameParts)
        {
            if (root == null)
            {
                return null;
            }

            if (NameContainsAll(root.name, requiredNameParts))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChildContaining(root.GetChild(i), requiredNameParts);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool NameContainsAll(string objectName, string[] requiredNameParts)
        {
            if (string.IsNullOrEmpty(objectName) || requiredNameParts == null || requiredNameParts.Length == 0)
            {
                return false;
            }

            string lowerName = objectName.ToLowerInvariant();
            for (int i = 0; i < requiredNameParts.Length; i++)
            {
                string part = requiredNameParts[i];
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                if (!lowerName.Contains(part.ToLowerInvariant()))
                {
                    return false;
                }
            }

            return true;
        }

        private static T GetOrAddComponent<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : root.AddComponent<T>();
        }

        private static Vector3 ClampScaleVector(Vector3 value)
        {
            const float minimumScale = 0.01f;
            return new Vector3(
                Mathf.Max(minimumScale, Mathf.Abs(value.x)),
                Mathf.Max(minimumScale, Mathf.Abs(value.y)),
                Mathf.Max(minimumScale, Mathf.Abs(value.z)));
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector3Value = value;
            }
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private sealed class CurrentRangerVfxSettings
        {
            public float ArrowStartDistanceFromTarget = 0.5f;
            public Vector3 ArrowRotationOffsetEuler = ArrowMeshForwardOffsetEuler;
            public Vector3 ArrowWorldScale = Vector3.one;
            public Vector3 EmbeddedArrowLocalOffset = new Vector3(0f, 1.05f, 0f);
            public RangerVfxAttachmentSnapshot PrecisionShot = DefaultAttachment();
            public RangerVfxAttachmentSnapshot QuickShot = DefaultAttachment();
            public RangerVfxAttachmentSnapshot PiercingArrow = DefaultAttachment();

            public static CurrentRangerVfxSettings Read(RangerHitPresentationConfig config)
            {
                CurrentRangerVfxSettings settings = new CurrentRangerVfxSettings();
                if (config == null)
                {
                    return settings;
                }

                SerializedObject serializedConfig = new SerializedObject(config);
                settings.ArrowStartDistanceFromTarget = ReadFloat(serializedConfig, "arrowStartDistanceFromTarget", settings.ArrowStartDistanceFromTarget);
                settings.ArrowRotationOffsetEuler = ReadVector3(serializedConfig, "arrowRotationOffsetEuler", settings.ArrowRotationOffsetEuler);
                settings.ArrowWorldScale = ReadVector3(serializedConfig, "arrowWorldScale", settings.ArrowWorldScale);
                settings.EmbeddedArrowLocalOffset = ReadVector3(serializedConfig, "embeddedArrowLocalOffset", settings.EmbeddedArrowLocalOffset);
                settings.PrecisionShot = ReadAttachment(serializedConfig, "precisionShot");
                settings.QuickShot = ReadAttachment(serializedConfig, "quickShot");
                settings.PiercingArrow = ReadAttachment(serializedConfig, "piercingArrow");
                return settings;
            }

            private static RangerVfxAttachmentSnapshot ReadAttachment(SerializedObject serializedObject, string fieldPrefix)
            {
                return new RangerVfxAttachmentSnapshot
                {
                    LocalOffset = ReadVector3(serializedObject, fieldPrefix + "LocalOffset", new Vector3(0f, 1.05f, 0f)),
                    LocalEulerAngles = ReadVector3(serializedObject, fieldPrefix + "LocalEulerAngles", Vector3.zero),
                    LocalScale = ReadVector3(serializedObject, fieldPrefix + "LocalScale", Vector3.one)
                };
            }

            private static RangerVfxAttachmentSnapshot DefaultAttachment()
            {
                return new RangerVfxAttachmentSnapshot
                {
                    LocalOffset = new Vector3(0f, 1.05f, 0f),
                    LocalEulerAngles = Vector3.zero,
                    LocalScale = Vector3.one
                };
            }

            private static float ReadFloat(SerializedObject serializedObject, string propertyName, float fallback)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                return property != null ? property.floatValue : fallback;
            }

            private static Vector3 ReadVector3(SerializedObject serializedObject, string propertyName, Vector3 fallback)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                return property != null ? property.vector3Value : fallback;
            }
        }
    }
}
