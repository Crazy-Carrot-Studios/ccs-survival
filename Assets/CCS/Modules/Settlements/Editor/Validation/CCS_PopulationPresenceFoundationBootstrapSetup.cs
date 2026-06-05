using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates population presence profile, scene anchors, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 NPC population placeholder foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_PopulationPresenceFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_PopulationPresenceFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.0.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_PopulationPresenceContentIds.PresenceProfilesRoot);

            CCS_PopulationPresenceProfile presenceProfile = EnsurePresenceProfile();
            EnsureWorldSimulationPresenceProfile(presenceProfile);
            EnsureBootstrapPopulationAnchors();
            EnsurePlaytestPopulationPresenceSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Population presence bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_PopulationPresenceProfile EnsurePresenceProfile()
        {
            CCS_PopulationPresenceProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PopulationPresenceProfile>(
                CCS_PopulationPresenceContentIds.DefaultPresenceProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_PopulationPresenceProfile>();
                AssetDatabase.CreateAsset(profile, CCS_PopulationPresenceContentIds.DefaultPresenceProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_PopulationPresenceContentIds.DefaultPresenceProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Population Presence Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Bootstrap anchors mapping workforce population to visible placeholder actors.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty anchors = serialized.FindProperty("anchorDefinitions");
            anchors.arraySize = 8;
            SetAnchor(anchors, 0, CCS_PopulationPresenceContentIds.TradingPostMerchantsAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementPopulationCategory.Merchants,
                "Merchants", 1, 4, 2.5f, "ccs.survival.business.generalstore");
            SetAnchor(anchors, 1, CCS_PopulationPresenceContentIds.TradingPostLaborersAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId, CCS_SettlementPopulationCategory.Laborers,
                "Laborers", 1, 4, 3f, string.Empty);
            SetAnchor(anchors, 2, CCS_PopulationPresenceContentIds.BrokenCreekFarmersAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_SettlementPopulationCategory.Farmers, "Farmers", 1, 4, 2.5f, "ccs.survival.business.farmsupply");
            SetAnchor(anchors, 3, CCS_PopulationPresenceContentIds.BrokenCreekRanchersAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_SettlementPopulationCategory.Ranchers, "Ranchers", 1, 4, 2.5f, "ccs.survival.business.stable");
            SetAnchor(anchors, 4, CCS_PopulationPresenceContentIds.IronRidgeMinersAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_SettlementPopulationCategory.Miners, "Miners", 1, 4, 2.5f, "ccs.survival.business.miningsupplier");
            SetAnchor(anchors, 5, CCS_PopulationPresenceContentIds.IronRidgeLaborersAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_SettlementPopulationCategory.Laborers, "Laborers", 1, 4, 2.5f, string.Empty);
            SetAnchor(anchors, 6, CCS_PopulationPresenceContentIds.PineRidgeLumberWorkersAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_SettlementPopulationCategory.LumberWorkers, "Lumber Workers", 1, 4, 2.5f,
                "ccs.survival.business.lumberyard");
            SetAnchor(anchors, 7, CCS_PopulationPresenceContentIds.PineRidgeLaborersAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_SettlementPopulationCategory.Laborers, "Laborers", 1, 4, 2.5f, string.Empty);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetAnchor(
            SerializedProperty anchors,
            int index,
            string anchorId,
            string settlementId,
            CCS_SettlementPopulationCategory category,
            string displayName,
            int minimumCount,
            int maxVisible,
            float spawnRadius,
            string businessId)
        {
            SerializedProperty entry = anchors.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("anchorId").stringValue = anchorId;
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("workforceCategory").enumValueIndex = (int)category;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
            entry.FindPropertyRelative("minimumPopulationCount").intValue = minimumCount;
            entry.FindPropertyRelative("maxVisibleActors").intValue = maxVisible;
            entry.FindPropertyRelative("spawnRadius").floatValue = spawnRadius;
            entry.FindPropertyRelative("businessId").stringValue = businessId;
        }

        private static void EnsureWorldSimulationPresenceProfile(CCS_PopulationPresenceProfile presenceProfile)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(WorldSimulationProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing world simulation profile.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("settlementPopulationPresenceProfile").objectReferenceValue = presenceProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapPopulationAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            Transform tradingPost = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPost != null)
            {
                EnsurePopulationAnchor(tradingPost, "CCS_PopulationPresence_Merchants",
                    CCS_PopulationPresenceContentIds.TradingPostMerchantsAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementPopulationCategory.Merchants, "Merchants", 1, 4, 2.5f,
                    new Vector3(-2f, 0f, -1f));
                EnsurePopulationAnchor(tradingPost, "CCS_PopulationPresence_Laborers",
                    CCS_PopulationPresenceContentIds.TradingPostLaborersAnchorId,
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    CCS_SettlementPopulationCategory.Laborers, "Laborers", 1, 4, 3f,
                    new Vector3(5f, 0f, 2f));
            }

            EnsureCampAnchors(sceneRoot, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            EnsureCampAnchors(sceneRoot, CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            EnsureCampAnchors(sceneRoot, CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureCampAnchors(Transform sceneRoot, string objectName, string settlementId)
        {
            Transform campRoot = sceneRoot.Find(objectName);
            if (campRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing settlement root '{objectName}'.");
                return;
            }

            if (string.Equals(settlementId, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_Farmers",
                    CCS_PopulationPresenceContentIds.BrokenCreekFarmersAnchorId, settlementId,
                    CCS_SettlementPopulationCategory.Farmers, "Farmers", 1, 4, 2.5f, new Vector3(1f, 0f, -4f));
                EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_Ranchers",
                    CCS_PopulationPresenceContentIds.BrokenCreekRanchersAnchorId, settlementId,
                    CCS_SettlementPopulationCategory.Ranchers, "Ranchers", 1, 4, 2.5f, new Vector3(4f, 0f, -3f));
                return;
            }

            if (string.Equals(settlementId, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_Miners",
                    CCS_PopulationPresenceContentIds.IronRidgeMinersAnchorId, settlementId,
                    CCS_SettlementPopulationCategory.Miners, "Miners", 1, 4, 2.5f, new Vector3(1f, 0f, -4f));
                EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_Laborers",
                    CCS_PopulationPresenceContentIds.IronRidgeLaborersAnchorId, settlementId,
                    CCS_SettlementPopulationCategory.Laborers, "Laborers", 1, 4, 2.5f, new Vector3(-3f, 0f, -2f));
                return;
            }

            EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_LumberWorkers",
                CCS_PopulationPresenceContentIds.PineRidgeLumberWorkersAnchorId, settlementId,
                CCS_SettlementPopulationCategory.LumberWorkers, "Lumber Workers", 1, 4, 2.5f,
                new Vector3(2f, 0f, -4f));
            EnsurePopulationAnchor(campRoot, "CCS_PopulationPresence_Laborers",
                CCS_PopulationPresenceContentIds.PineRidgeLaborersAnchorId, settlementId,
                CCS_SettlementPopulationCategory.Laborers, "Laborers", 1, 4, 2.5f, new Vector3(-2f, 0f, -3f));
        }

        private static void EnsurePopulationAnchor(
            Transform parent,
            string objectName,
            string anchorId,
            string settlementId,
            CCS_SettlementPopulationCategory category,
            string displayName,
            int minimumCount,
            int maxVisible,
            float spawnRadius,
            Vector3 localPosition)
        {
            Transform existing = parent.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = localPosition;

            CCS_PopulationPresenceAnchor anchor = root.GetComponent<CCS_PopulationPresenceAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_PopulationPresenceAnchor>();
            }

            Transform labelTransform = root.transform.Find("CCS_PopulationPresence_LabelRoot");
            GameObject labelRoot = labelTransform != null
                ? labelTransform.gameObject
                : new GameObject("CCS_PopulationPresence_LabelRoot");
            if (labelTransform == null)
            {
                labelRoot.transform.SetParent(root.transform, false);
            }

            labelRoot.transform.localPosition = Vector3.zero;
            CCS_PopulationPresenceLabel label = labelRoot.GetComponent<CCS_PopulationPresenceLabel>();
            if (label == null)
            {
                label = labelRoot.AddComponent<CCS_PopulationPresenceLabel>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("workforceCategory").enumValueIndex = (int)category;
            serializedAnchor.FindProperty("displayName").stringValue = displayName;
            serializedAnchor.FindProperty("minimumPopulationCount").intValue = minimumCount;
            serializedAnchor.FindProperty("maxVisibleActors").intValue = maxVisible;
            serializedAnchor.FindProperty("spawnRadius").floatValue = spawnRadius;
            serializedAnchor.FindProperty("presenceLabel").objectReferenceValue = label;
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static void EnsurePlaytestPopulationPresenceSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.populationpresence.discover",
                "Discover settlement for population presence",
                CCS_PlaytestStepType.DiscoverSettlementForPopulationPresence);
            InsertStep(profile, "ccs.survival.playtest.populationpresence.verify.low",
                "Verify low placeholder actor count at low population",
                CCS_PlaytestStepType.VerifyLowPopulationPlaceholderActors);
            InsertStep(profile, "ccs.survival.playtest.populationpresence.grow",
                "Trigger population growth for placeholders",
                CCS_PlaytestStepType.TriggerPopulationGrowthForPlaceholders);
            InsertStep(profile, "ccs.survival.playtest.populationpresence.verify.visible",
                "Verify placeholder actors appear",
                CCS_PlaytestStepType.VerifyPopulationPlaceholderActorsVisible);
            InsertStep(profile, "ccs.survival.playtest.populationpresence.save",
                "Save population presence state",
                CCS_PlaytestStepType.SavePopulationPresenceState);
            InsertStep(profile, "ccs.survival.playtest.populationpresence.load",
                "Verify population presence restored after load",
                CCS_PlaytestStepType.VerifyPopulationPresenceAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType)
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
            step.FindPropertyRelative("instructionText").stringValue =
                $"{displayName}. Ctrl+Shift+X shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindSceneRoot()
        {
            GameObject sceneRoot = GameObject.Find("CCS_BuildVerificationScene");
            if (sceneRoot != null)
            {
                return sceneRoot.transform;
            }

            sceneRoot = GameObject.Find("CCS_SurvivalBootstrapScene");
            return sceneRoot != null ? sceneRoot.transform : null;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
