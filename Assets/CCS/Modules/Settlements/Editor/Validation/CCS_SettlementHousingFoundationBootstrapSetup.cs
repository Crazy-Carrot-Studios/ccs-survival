using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SettlementHousingFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates housing profile, bootstrap scene anchors, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — settlement housing foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementHousingFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementHousingFoundationBootstrapSetup]";
        private const string MilestoneVersion = "4.4.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_SettlementHousingContentIds.HousingProfilesRoot);

            CCS_SettlementHousingProfile housingProfile = EnsureHousingProfile();
            EnsureWorldSimulationHousingProfile(housingProfile);
            EnsureBootstrapHousingAnchors();
            EnsurePlaytestHousingSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Settlement housing bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_SettlementHousingProfile EnsureHousingProfile()
        {
            CCS_SettlementHousingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementHousingProfile>(
                CCS_SettlementHousingContentIds.DefaultHousingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementHousingProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementHousingContentIds.DefaultHousingProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_SettlementHousingContentIds.DefaultHousingProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Housing Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Bootstrap settlement-owned housing contributing to population capacity.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty definitions = serialized.FindProperty("housingDefinitions");
            definitions.arraySize = 4;
            SetDefinition(
                definitions,
                0,
                CCS_SettlementHousingContentIds.TradingPostBoardingHouseId,
                CCS_SettlementHousingContentIds.TradingPostBoardingHouseAnchorId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "Boarding House",
                CCS_SettlementHousingType.BoardingHouse,
                20,
                CCS_SettlementGrowthStage.TradingPost,
                CCS_SettlementPopulationCategory.Merchants);
            SetDefinition(
                definitions,
                1,
                CCS_SettlementHousingContentIds.BrokenCreekFarmhouseId,
                CCS_SettlementHousingContentIds.BrokenCreekFarmhouseAnchorId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                "Farmhouse",
                CCS_SettlementHousingType.Farmhouse,
                12,
                CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementPopulationCategory.Farmers);
            SetDefinition(
                definitions,
                2,
                CCS_SettlementHousingContentIds.PineRidgeWorkerCabinId,
                CCS_SettlementHousingContentIds.PineRidgeWorkerCabinAnchorId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                "Worker Cabin",
                CCS_SettlementHousingType.WorkerCabin,
                10,
                CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementPopulationCategory.Laborers);
            SetDefinition(
                definitions,
                3,
                CCS_SettlementHousingContentIds.IronRidgeMiningBarracksId,
                CCS_SettlementHousingContentIds.IronRidgeMiningBarracksAnchorId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                "Mining Barracks",
                CCS_SettlementHousingType.MiningBarracks,
                25,
                CCS_SettlementGrowthStage.Outpost,
                CCS_SettlementPopulationCategory.Miners);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetDefinition(
            SerializedProperty definitions,
            int index,
            string housingId,
            string anchorId,
            string settlementId,
            string displayName,
            CCS_SettlementHousingType housingType,
            int capacityContribution,
            CCS_SettlementGrowthStage requiredGrowthStage,
            CCS_SettlementPopulationCategory workforceAffinity)
        {
            SerializedProperty entry = definitions.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("housingId").stringValue = housingId;
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("anchorId").stringValue = anchorId;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
            entry.FindPropertyRelative("housingType").enumValueIndex = (int)housingType;
            entry.FindPropertyRelative("capacityContribution").intValue = capacityContribution;
            entry.FindPropertyRelative("requiredGrowthStage").enumValueIndex = (int)requiredGrowthStage;
            entry.FindPropertyRelative("workforceAffinity").enumValueIndex = (int)workforceAffinity;
        }

        private static void EnsureWorldSimulationHousingProfile(CCS_SettlementHousingProfile housingProfile)
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
            serialized.FindProperty("settlementHousingProfile").objectReferenceValue = housingProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapHousingAnchors()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            EnsureTradingPostAnchor(sceneRoot);
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                "CCS_SettlementHousing_Farmhouse",
                CCS_SettlementHousingContentIds.BrokenCreekFarmhouseAnchorId,
                CCS_SettlementHousingContentIds.BrokenCreekFarmhouseId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                "Farmhouse",
                12,
                new Vector3(-2f, 1.2f, 3f),
                new Color(0.75f, 0.7f, 0.35f));
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                "CCS_SettlementHousing_WorkerCabin",
                CCS_SettlementHousingContentIds.PineRidgeWorkerCabinAnchorId,
                CCS_SettlementHousingContentIds.PineRidgeWorkerCabinId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                "Worker Cabin",
                10,
                new Vector3(-2f, 1.2f, 3f),
                new Color(0.55f, 0.45f, 0.3f));
            EnsureCampAnchor(
                sceneRoot,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                "CCS_SettlementHousing_MiningBarracks",
                CCS_SettlementHousingContentIds.IronRidgeMiningBarracksAnchorId,
                CCS_SettlementHousingContentIds.IronRidgeMiningBarracksId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                "Mining Barracks",
                25,
                new Vector3(-2f, 1.2f, 3f),
                new Color(0.5f, 0.5f, 0.55f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureTradingPostAnchor(Transform sceneRoot)
        {
            Transform tradingPost = sceneRoot.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPost == null)
            {
                Debug.LogError($"{LogPrefix} Missing trading post root.");
                return;
            }

            EnsureAnchorObject(
                tradingPost,
                "CCS_SettlementHousing_BoardingHouse",
                CCS_SettlementHousingContentIds.TradingPostBoardingHouseAnchorId,
                CCS_SettlementHousingContentIds.TradingPostBoardingHouseId,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                "Boarding House",
                20,
                new Vector3(-4f, 1.2f, 4f),
                PrimitiveType.Cube,
                new Color(0.4f, 0.55f, 0.85f));
        }

        private static void EnsureCampAnchor(
            Transform sceneRoot,
            string settlementObjectName,
            string objectName,
            string anchorId,
            string housingId,
            string settlementId,
            string displayName,
            int capacityContribution,
            Vector3 localPosition,
            Color markerColor)
        {
            Transform settlementRoot = sceneRoot.Find(settlementObjectName);
            if (settlementRoot == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing settlement root {settlementObjectName}.");
                return;
            }

            EnsureAnchorObject(
                settlementRoot,
                objectName,
                anchorId,
                housingId,
                settlementId,
                displayName,
                capacityContribution,
                localPosition,
                PrimitiveType.Cube,
                markerColor);
        }

        private static void EnsureAnchorObject(
            Transform parent,
            string objectName,
            string anchorId,
            string housingId,
            string settlementId,
            string displayName,
            int capacityContribution,
            Vector3 localPosition,
            PrimitiveType primitiveType,
            Color markerColor)
        {
            Transform existing = parent.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.localPosition = localPosition;

            CCS_SettlementHousingAnchor anchor = root.GetComponent<CCS_SettlementHousingAnchor>();
            if (anchor == null)
            {
                anchor = root.AddComponent<CCS_SettlementHousingAnchor>();
            }

            Transform markerTransform = root.transform.Find("CCS_SettlementHousing_Marker");
            GameObject markerObject = markerTransform != null
                ? markerTransform.gameObject
                : GameObject.CreatePrimitive(primitiveType);
            if (markerTransform == null)
            {
                markerObject.name = "CCS_SettlementHousing_Marker";
                markerObject.transform.SetParent(root.transform, false);
            }

            markerObject.transform.localPosition = Vector3.zero;
            markerObject.transform.localScale = new Vector3(1.4f, 1.1f, 1.4f);
            Collider markerCollider = markerObject.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Object.DestroyImmediate(markerCollider);
            }

            Renderer markerRenderer = markerObject.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.sharedMaterial.color = markerColor;
            }

            CCS_SettlementHousingMarker marker = markerObject.GetComponent<CCS_SettlementHousingMarker>();
            if (marker == null)
            {
                marker = markerObject.AddComponent<CCS_SettlementHousingMarker>();
            }

            Transform labelTransform = root.transform.Find("CCS_SettlementHousing_LabelRoot");
            GameObject labelRoot = labelTransform != null
                ? labelTransform.gameObject
                : new GameObject("CCS_SettlementHousing_LabelRoot");
            if (labelTransform == null)
            {
                labelRoot.transform.SetParent(root.transform, false);
            }

            labelRoot.transform.localPosition = Vector3.zero;
            CCS_SettlementHousingLabel label = labelRoot.GetComponent<CCS_SettlementHousingLabel>();
            if (label == null)
            {
                label = labelRoot.AddComponent<CCS_SettlementHousingLabel>();
            }

            SerializedObject serializedAnchor = new SerializedObject(anchor);
            serializedAnchor.FindProperty("anchorId").stringValue = anchorId;
            serializedAnchor.FindProperty("housingId").stringValue = housingId;
            serializedAnchor.FindProperty("settlementId").stringValue = settlementId;
            serializedAnchor.FindProperty("displayName").stringValue = displayName;
            serializedAnchor.FindProperty("capacityContribution").intValue = capacityContribution;
            serializedAnchor.FindProperty("housingMarker").objectReferenceValue = marker;
            serializedAnchor.FindProperty("housingLabel").objectReferenceValue = label;
            serializedAnchor.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static void EnsurePlaytestHousingSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.settlementhousing.discover", "Discover settlement for housing",
                CCS_PlaytestStepType.DiscoverSettlementForSettlementHousing);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.verify.marker", "Verify housing marker exists",
                CCS_PlaytestStepType.VerifyHousingMarkerExists);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.verify.capacity", "Verify housing capacity contribution",
                CCS_PlaytestStepType.VerifyHousingCapacityContribution);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.increase.population", "Increase population toward capacity",
                CCS_PlaytestStepType.IncreasePopulationForHousingCapacity);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.verify.respects.capacity",
                "Verify population respects total capacity",
                CCS_PlaytestStepType.VerifyPopulationRespectsTotalCapacity);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.save", "Save settlement housing state",
                CCS_PlaytestStepType.SaveSettlementHousingState);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.load", "Load settlement housing state",
                CCS_PlaytestStepType.LoadSettlementHousingState);
            InsertStep(profile, "ccs.survival.playtest.settlementhousing.verify.load", "Verify housing restored after load",
                CCS_PlaytestStepType.VerifySettlementHousingAfterLoad);
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
                $"{displayName}. Ctrl+Alt+H shortcut available.";
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
