using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FrontierSettlementBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Creates frontier settlement content, bootstrap trading post, and playtest steps.
// PLACEMENT: Batch entry for milestone 1.8.1 settlement services polish.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific content under Assets/CCS/Survival/.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_FrontierSettlementBootstrapSetup
    {
        private const string ContentRoot = "Assets/CCS/Survival/Content/Settlements";
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements";
        private const string TradingPostDefinitionPath = ContentRoot + "/CCS_Settlement_TestTradingPost.asset";
        private const string DefaultSettlementProfilePath = ProfilesRoot + "/CCS_DefaultSettlementProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string LogPrefix = "[CCS_FrontierSettlementBootstrapSetup]";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_VendorDefinition generalStore = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            CCS_VendorDefinition stableVendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            CCS_VendorDefinition gunsmithVendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GunsmithVendorPath);
            if (generalStore == null || stableVendor == null || gunsmithVendor == null)
            {
                Debug.LogError($"{LogPrefix} Missing vendor assets. Run economy, mounts, and firearms bootstraps first.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SettlementDefinition tradingPost = EnsureTradingPostDefinition();
            CCS_SettlementProfile settlementProfile = EnsureSettlementProfile(tradingPost);
            EnsureBootstrapGameplayServiceHost(settlementProfile);
            EnsureBootstrapTradingPostScene(tradingPost, generalStore, stableVendor, gunsmithVendor);
            EnsurePlaytestSettlementSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier settlement bootstrap complete (1.8.1).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Settlements");
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

        private static CCS_SettlementDefinition EnsureTradingPostDefinition()
        {
            CCS_SettlementDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementDefinition>(TradingPostDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_SettlementDefinition>();
                AssetDatabase.CreateAsset(definition, TradingPostDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("settlementId").stringValue = CCS_SettlementContentIds.TestTradingPostSettlementId;
            serialized.FindProperty("displayName").stringValue = "Frontier Test Trading Post";
            serialized.FindProperty("description").stringValue =
                "Bootstrap trading post with general store, stable, gunsmith, and blacksmith industry routing.";
            serialized.FindProperty("settlementType").enumValueIndex = (int)CCS_SettlementType.TradingPost;
            serialized.FindProperty("defaultWorldPosition").vector3Value = new Vector3(24f, 0f, 20f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_SettlementProfile EnsureSettlementProfile(CCS_SettlementDefinition tradingPost)
        {
            CCS_SettlementProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementProfile>();
                AssetDatabase.CreateAsset(profile, DefaultSettlementProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.settlements.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement catalog for bootstrap trading post discovery.";
            serialized.FindProperty("profileVersion").stringValue = "1.8.1";
            SerializedProperty definitions = serialized.FindProperty("settlementDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = tradingPost;
            serialized.FindProperty("defaultDiscoverRadius").floatValue = 14f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_SettlementProfile profile)
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
            serializedHost.FindProperty("settlementProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureBootstrapTradingPostScene(
            CCS_SettlementDefinition tradingPost,
            CCS_VendorDefinition generalStore,
            CCS_VendorDefinition stableVendor,
            CCS_VendorDefinition gunsmithVendor)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existing = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            GameObject tradingPostRoot;
            if (existing != null)
            {
                tradingPostRoot = existing.gameObject;
            }
            else
            {
                tradingPostRoot = new GameObject(CCS_SettlementContentIds.TestTradingPostObjectName);
                tradingPostRoot.transform.SetParent(sceneRoot, false);
            }

            tradingPostRoot.transform.position = new Vector3(24f, 0f, 20f);

            CCS_SettlementLocation location = tradingPostRoot.GetComponent<CCS_SettlementLocation>();
            if (location == null)
            {
                location = tradingPostRoot.AddComponent<CCS_SettlementLocation>();
            }

            SerializedObject serializedLocation = new SerializedObject(location);
            serializedLocation.FindProperty("settlementDefinition").objectReferenceValue = tradingPost;
            serializedLocation.FindProperty("discoverRadius").floatValue = 14f;
            serializedLocation.FindProperty("autoDiscoverOnProximity").boolValue = true;
            serializedLocation.ApplyModifiedPropertiesWithoutUndo();

            EnsureServicePoint(
                tradingPostRoot.transform,
                "CCS_TestTradingPost_GeneralStore",
                CCS_SettlementContentIds.GeneralStoreServicePointId,
                CCS_SettlementServicePointType.GeneralStore,
                location,
                generalStore,
                string.Empty,
                new Vector3(0f, 0.5f, 0f),
                "General Store");

            EnsureServicePoint(
                tradingPostRoot.transform,
                "CCS_TestTradingPost_Stable",
                CCS_SettlementContentIds.StableServicePointId,
                CCS_SettlementServicePointType.Stable,
                location,
                stableVendor,
                string.Empty,
                new Vector3(4f, 0.5f, 0f),
                "Stable");

            EnsureServicePoint(
                tradingPostRoot.transform,
                "CCS_TestTradingPost_Gunsmith",
                CCS_SettlementContentIds.GunsmithServicePointId,
                CCS_SettlementServicePointType.Gunsmith,
                location,
                gunsmithVendor,
                string.Empty,
                new Vector3(8f, 0.5f, 0f),
                "Gunsmith");

            EnsureServicePoint(
                tradingPostRoot.transform,
                "CCS_TestTradingPost_Blacksmith",
                CCS_SettlementContentIds.BlacksmithServicePointId,
                CCS_SettlementServicePointType.Blacksmith,
                location,
                null,
                "Refine and forge services require a Primitive Forge at camp.",
                new Vector3(12f, 0.5f, 0f),
                "Blacksmith",
                true,
                string.Empty,
                false,
                -1,
                CCS_SettlementServiceRouteType.Industry);

            EditorUtility.SetDirty(tradingPostRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureServicePoint(
            Transform parent,
            string objectName,
            string servicePointId,
            CCS_SettlementServicePointType servicePointType,
            CCS_SettlementLocation settlementLocation,
            CCS_VendorDefinition vendorDefinition,
            string placeholderMessage,
            Vector3 localPosition,
            string displayName,
            bool pointIsAvailable = true,
            string pointUnavailableReason = "",
            bool requireSettlementDiscovered = false,
            int requiredCampTier = -1,
            CCS_SettlementServiceRouteType routeOverride = CCS_SettlementServiceRouteType.Unknown)
        {
            Transform existing = parent.Find(objectName);
            GameObject serviceObject;
            if (existing != null)
            {
                serviceObject = existing.gameObject;
            }
            else
            {
                serviceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                serviceObject.name = objectName;
                serviceObject.transform.SetParent(parent, false);
            }

            serviceObject.transform.localPosition = localPosition;
            serviceObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);

            Rigidbody rigidbody = serviceObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = serviceObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            CCS_SettlementServicePoint servicePoint = serviceObject.GetComponent<CCS_SettlementServicePoint>();
            if (servicePoint == null)
            {
                servicePoint = serviceObject.AddComponent<CCS_SettlementServicePoint>();
            }

            SerializedObject serialized = new SerializedObject(servicePoint);
            serialized.FindProperty("servicePointId").stringValue = servicePointId;
            serialized.FindProperty("servicePointType").enumValueIndex = (int)servicePointType;
            serialized.FindProperty("settlementLocation").objectReferenceValue = settlementLocation;
            serialized.FindProperty("vendorDefinition").objectReferenceValue = vendorDefinition;
            serialized.FindProperty("placeholderMessage").stringValue = placeholderMessage ?? string.Empty;
            serialized.FindProperty("isAvailable").boolValue = pointIsAvailable;
            serialized.FindProperty("unavailableReason").stringValue = pointUnavailableReason ?? string.Empty;
            serialized.FindProperty("requiredSettlementDiscovered").boolValue = requireSettlementDiscovered;
            serialized.FindProperty("requiredCampTier").intValue = requiredCampTier;
            serialized.FindProperty("routeOverride").enumValueIndex = (int)routeOverride;
            serialized.FindProperty("interactionDistance").floatValue = 3f;
            serialized.FindProperty("interactionDisplayNameOverride").stringValue = displayName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serviceObject);
        }

        private static void EnsurePlaytestSettlementSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.discover",
                "Discover trading post",
                CCS_PlaytestStepType.DiscoverTradingPost,
                "Walk within range of CCS_TestTradingPost or interact with a service point.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.generalstore",
                "Interact with General Store service point",
                CCS_PlaytestStepType.InteractGeneralStoreServicePoint,
                "Interact with the General Store cube at the trading post.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.stable",
                "Interact with Stable service point",
                CCS_PlaytestStepType.InteractStableServicePoint,
                "Interact with the Stable cube at the trading post.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.gunsmith",
                "Interact with Gunsmith service point",
                CCS_PlaytestStepType.InteractGunsmithServicePoint,
                "Interact with the Gunsmith cube at the trading post.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.blacksmith",
                "Interact with Blacksmith service point",
                CCS_PlaytestStepType.InteractBlacksmithServicePoint,
                "Interact with the Blacksmith cube at the trading post.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.blacksmith.routing",
                "Verify blacksmith industry routing",
                CCS_PlaytestStepType.VerifySettlementBlacksmithRouting,
                "Confirm blacksmith opens industry service summary panel.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.vendor.routing",
                "Verify settlement vendor routing",
                CCS_PlaytestStepType.VerifySettlementVendorRouting,
                "Buy or sell through a settlement service point vendor (V/H/Shift+V).");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.save",
                "Save settlement discovery",
                CCS_PlaytestStepType.SaveSettlementDiscovery,
                "Press F5 after discovering the trading post.");
            InsertStep(
                profile,
                "ccs.survival.playtest.settlement.load.verify",
                "Verify settlement discovery persists",
                CCS_PlaytestStepType.VerifySettlementDiscoveryAfterLoad,
                "Press F9 and confirm trading post remains discovered.");
            EditorUtility.SetDirty(profile);
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
