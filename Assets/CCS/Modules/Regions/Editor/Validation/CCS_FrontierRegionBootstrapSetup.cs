using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FrontierRegionBootstrapSetup
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Creates frontier region content, bootstrap volumes, and playtest steps.
// PLACEMENT: Batch entry for milestone 1.9.0 frontier region foundation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific content under Assets/CCS/Survival/.
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    public static class CCS_FrontierRegionBootstrapSetup
    {
        private const string ContentRoot = "Assets/CCS/Survival/Content/Regions";
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Regions";
        private const string DefaultRegionProfilePath = ProfilesRoot + "/CCS_DefaultRegionProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string LogPrefix = "[CCS_FrontierRegionBootstrapSetup]";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_RegionDefinition pineRidgeForest = EnsurePineRidgeForestDefinition();
            CCS_RegionDefinition brokenCreek = EnsureBrokenCreekDefinition();
            CCS_RegionDefinition ironRidgeMine = EnsureIronRidgeMineDefinition();
            CCS_RegionDefinition tradingPostRegion = EnsureFrontierTradingPostRegionDefinition();
            CCS_RegionProfile regionProfile = EnsureRegionProfile(
                pineRidgeForest,
                brokenCreek,
                ironRidgeMine,
                tradingPostRegion);
            EnsureBootstrapGameplayServiceHost(regionProfile);
            EnsureBootstrapRegionVolumes(
                pineRidgeForest,
                brokenCreek,
                ironRidgeMine,
                tradingPostRegion);
            EnsurePlaytestRegionSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier region bootstrap complete (1.9.0).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ContentRoot);
            EnsureFolder(ProfilesRoot);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static CCS_RegionDefinition EnsurePineRidgeForestDefinition()
        {
            const string assetPath = ContentRoot + "/CCS_Region_PineRidgeForest.asset";
            CCS_RegionDefinition definition = LoadOrCreateDefinition(assetPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("regionId").stringValue = CCS_RegionContentIds.PineRidgeForestRegionId;
            serialized.FindProperty("displayName").stringValue = "Pine Ridge Forest";
            serialized.FindProperty("description").stringValue =
                "Timber-rich forest region with frontier wood and wildlife resources.";
            serialized.FindProperty("regionType").enumValueIndex = (int)CCS_RegionType.Forest;
            serialized.FindProperty("defaultWorldPosition").vector3Value = new Vector3(-16f, 0f, -12f);
            SetStringArray(serialized.FindProperty("settlementIds"), new string[0]);
            SetStringArray(
                serialized.FindProperty("resourceMetadataTags"),
                new[]
                {
                    "ccs.survival.resource.wood",
                    "ccs.survival.resource.wildlife",
                    "ccs.survival.resource.herbs"
                });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_RegionDefinition EnsureBrokenCreekDefinition()
        {
            const string assetPath = ContentRoot + "/CCS_Region_BrokenCreek.asset";
            CCS_RegionDefinition definition = LoadOrCreateDefinition(assetPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("regionId").stringValue = CCS_RegionContentIds.BrokenCreekRegionId;
            serialized.FindProperty("displayName").stringValue = "Broken Creek";
            serialized.FindProperty("description").stringValue =
                "Creek region with fishing, clay, and frontier water resources.";
            serialized.FindProperty("regionType").enumValueIndex = (int)CCS_RegionType.Creek;
            serialized.FindProperty("defaultWorldPosition").vector3Value = new Vector3(-30f, 0f, 8f);
            SetStringArray(serialized.FindProperty("settlementIds"), new string[0]);
            SetStringArray(
                serialized.FindProperty("resourceMetadataTags"),
                new[]
                {
                    "ccs.survival.resource.fish",
                    "ccs.survival.resource.clay",
                    "ccs.survival.resource.water"
                });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_RegionDefinition EnsureIronRidgeMineDefinition()
        {
            const string assetPath = ContentRoot + "/CCS_Region_IronRidgeMine.asset";
            CCS_RegionDefinition definition = LoadOrCreateDefinition(assetPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("regionId").stringValue = CCS_RegionContentIds.IronRidgeMineRegionId;
            serialized.FindProperty("displayName").stringValue = "Iron Ridge Mine";
            serialized.FindProperty("description").stringValue =
                "Mining region with iron, coal, and stone frontier deposits.";
            serialized.FindProperty("regionType").enumValueIndex = (int)CCS_RegionType.Mine;
            serialized.FindProperty("defaultWorldPosition").vector3Value = new Vector3(48f, 0f, 32f);
            SetStringArray(serialized.FindProperty("settlementIds"), new string[0]);
            SetStringArray(
                serialized.FindProperty("resourceMetadataTags"),
                new[]
                {
                    "ccs.survival.resource.ironore",
                    "ccs.survival.resource.coal",
                    "ccs.survival.resource.stone"
                });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_RegionDefinition EnsureFrontierTradingPostRegionDefinition()
        {
            const string assetPath = ContentRoot + "/CCS_Region_FrontierTradingPost.asset";
            CCS_RegionDefinition definition = LoadOrCreateDefinition(assetPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("regionId").stringValue = CCS_RegionContentIds.FrontierTradingPostRegionId;
            serialized.FindProperty("displayName").stringValue = "Frontier Trading Post Region";
            serialized.FindProperty("description").stringValue =
                "Service hub region containing the bootstrap frontier trading post.";
            serialized.FindProperty("regionType").enumValueIndex = (int)CCS_RegionType.TradingPost;
            serialized.FindProperty("defaultWorldPosition").vector3Value = new Vector3(24f, 0f, 20f);
            SetStringArray(
                serialized.FindProperty("settlementIds"),
                new[] { CCS_SettlementContentIds.TestTradingPostSettlementId });
            SetStringArray(
                serialized.FindProperty("resourceMetadataTags"),
                new[]
                {
                    "ccs.survival.resource.trade",
                    "ccs.survival.resource.supplies",
                    "ccs.survival.resource.services"
                });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_RegionProfile EnsureRegionProfile(params CCS_RegionDefinition[] definitions)
        {
            CCS_RegionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_RegionProfile>(DefaultRegionProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_RegionProfile>();
                AssetDatabase.CreateAsset(profile, DefaultRegionProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.regions.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Region Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier region catalog for bootstrap world organization.";
            serialized.FindProperty("profileVersion").stringValue = "1.9.0";
            SerializedProperty definitionProperty = serialized.FindProperty("regionDefinitions");
            definitionProperty.arraySize = definitions.Length;
            for (int index = 0; index < definitions.Length; index++)
            {
                definitionProperty.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_RegionProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("regionProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureBootstrapRegionVolumes(
            CCS_RegionDefinition pineRidgeForest,
            CCS_RegionDefinition brokenCreek,
            CCS_RegionDefinition ironRidgeMine,
            CCS_RegionDefinition tradingPostRegion)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            EnsureRegionVolume(
                sceneRoot,
                CCS_RegionContentIds.PineRidgeForestVolumeObjectName,
                pineRidgeForest,
                new Vector3(-16f, 1f, -12f),
                new Vector3(24f, 4f, 24f));
            EnsureRegionVolume(
                sceneRoot,
                CCS_RegionContentIds.BrokenCreekVolumeObjectName,
                brokenCreek,
                new Vector3(-30f, 1f, 8f),
                new Vector3(18f, 4f, 18f));
            EnsureRegionVolume(
                sceneRoot,
                CCS_RegionContentIds.IronRidgeMineVolumeObjectName,
                ironRidgeMine,
                new Vector3(48f, 1f, 32f),
                new Vector3(20f, 4f, 20f));
            EnsureRegionVolume(
                sceneRoot,
                CCS_RegionContentIds.FrontierTradingPostRegionVolumeObjectName,
                tradingPostRegion,
                new Vector3(24f, 1f, 20f),
                new Vector3(24f, 4f, 24f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureRegionVolume(
            Transform sceneRoot,
            string objectName,
            CCS_RegionDefinition regionDefinition,
            Vector3 worldPosition,
            Vector3 volumeScale)
        {
            Transform existing = sceneRoot.Find(objectName);
            GameObject volumeObject;
            if (existing != null)
            {
                volumeObject = existing.gameObject;
            }
            else
            {
                volumeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                volumeObject.name = objectName;
                volumeObject.transform.SetParent(sceneRoot, false);
            }

            volumeObject.transform.position = worldPosition;
            volumeObject.transform.localScale = volumeScale;

            Rigidbody rigidbody = volumeObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = volumeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            Renderer renderer = volumeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            CCS_RegionVolume regionVolume = volumeObject.GetComponent<CCS_RegionVolume>();
            if (regionVolume == null)
            {
                regionVolume = volumeObject.AddComponent<CCS_RegionVolume>();
            }

            SerializedObject serialized = new SerializedObject(regionVolume);
            serialized.FindProperty("regionDefinition").objectReferenceValue = regionDefinition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(volumeObject);
        }

        private static void EnsurePlaytestRegionSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.region.pineridge",
                "Discover Pine Ridge Forest region",
                CCS_PlaytestStepType.DiscoverPineRidgeForestRegion,
                "Enter the Pine Ridge Forest trigger volume.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.brokencreek",
                "Discover Broken Creek region",
                CCS_PlaytestStepType.DiscoverBrokenCreekRegion,
                "Enter the Broken Creek trigger volume.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.ironridge",
                "Discover Iron Ridge Mine region",
                CCS_PlaytestStepType.DiscoverIronRidgeMineRegion,
                "Enter the Iron Ridge Mine trigger volume.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.tradingpost",
                "Discover Frontier Trading Post region",
                CCS_PlaytestStepType.DiscoverFrontierTradingPostRegion,
                "Enter the Frontier Trading Post region trigger volume.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.verify.all",
                "Verify all frontier regions discovered",
                CCS_PlaytestStepType.VerifyAllRegionsDiscovered,
                "Confirm all four bootstrap regions are discovered.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.save",
                "Save region discovery",
                CCS_PlaytestStepType.SaveRegionDiscovery,
                "Press F5 after discovering all regions.");
            InsertStep(
                profile,
                "ccs.survival.playtest.region.load.verify",
                "Verify region discovery persists",
                CCS_PlaytestStepType.VerifyRegionDiscoveryAfterLoad,
                "Press F9 and confirm all region discoveries remain.");
            EditorUtility.SetDirty(profile);
        }

        private static CCS_RegionDefinition LoadOrCreateDefinition(string assetPath)
        {
            CCS_RegionDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RegionDefinition>(assetPath);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<CCS_RegionDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);
            return definition;
        }

        private static void SetStringArray(SerializedProperty property, string[] values)
        {
            property.arraySize = values?.Length ?? 0;
            for (int index = 0; index < property.arraySize; index++)
            {
                property.GetArrayElementAtIndex(index).stringValue = values[index];
            }
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("stepDefinitions");
            for (int index = 0; index < steps.arraySize; index++)
            {
                if (steps.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return;
                }
            }

            steps.InsertArrayElementAtIndex(steps.arraySize);
            SerializedProperty step = steps.GetArrayElementAtIndex(steps.arraySize - 1);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue = instructionText;
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == "CCS_BuildVerificationScene")
                {
                    return roots[index].transform;
                }
            }

            return null;
        }
    }
}
