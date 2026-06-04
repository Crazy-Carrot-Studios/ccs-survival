using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates settlement growth content, world simulation wiring, and playtest steps.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementGrowthFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementGrowthFoundationBootstrapSetup]";
        private const string SettlementGrowthMilestoneVersion = "3.2.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_SettlementGrowthDefinition outpost = EnsureGrowthDefinition(
                CCS_SettlementGrowthContentIds.OutpostGrowthDefinitionPath,
                "ccs.survival.settlementgrowth.outpost",
                CCS_SettlementGrowthStage.Outpost,
                true,
                0f,
                0f,
                0f,
                0,
                false,
                string.Empty);
            CCS_SettlementGrowthDefinition tradingPost = EnsureGrowthDefinition(
                CCS_SettlementGrowthContentIds.TradingPostGrowthDefinitionPath,
                "ccs.survival.settlementgrowth.tradingpost",
                CCS_SettlementGrowthStage.TradingPost,
                true,
                35f,
                25f,
                0f,
                1,
                false,
                string.Empty);
            CCS_SettlementGrowthDefinition frontierTown = EnsureGrowthDefinition(
                CCS_SettlementGrowthContentIds.FrontierTownGrowthDefinitionPath,
                "ccs.survival.settlementgrowth.frontiertown",
                CCS_SettlementGrowthStage.FrontierTown,
                false,
                100f,
                100f,
                100f,
                100,
                true,
                string.Empty);
            CCS_SettlementGrowthDefinition establishedTown = EnsureGrowthDefinition(
                CCS_SettlementGrowthContentIds.EstablishedTownGrowthDefinitionPath,
                "ccs.survival.settlementgrowth.establishedtown",
                CCS_SettlementGrowthStage.EstablishedTown,
                false,
                100f,
                100f,
                100f,
                100,
                true,
                string.Empty);

            CCS_SettlementGrowthProfile growthProfile = EnsureGrowthProfile(
                outpost,
                tradingPost,
                frontierTown,
                establishedTown);
            EnsureWorldSimulationProfile(growthProfile);
            EnsurePlaytestSettlementGrowthSteps();
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Settlement growth foundation bootstrap complete ({SettlementGrowthMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(CCS_SettlementGrowthContentIds.GrowthContentRoot);
            EnsureFolder(CCS_SettlementGrowthContentIds.GrowthProfilesRoot);
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

        private static CCS_SettlementGrowthDefinition EnsureGrowthDefinition(
            string assetPath,
            string definitionId,
            CCS_SettlementGrowthStage stage,
            bool isActive,
            float prosperity,
            float foodPercent,
            float industrialPercent,
            int contracts,
            bool requiresRegion,
            string regionId)
        {
            CCS_SettlementGrowthDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_SettlementGrowthDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("growthDefinitionId").stringValue = definitionId;
            serialized.FindProperty("growthStage").enumValueIndex = (int)stage;
            serialized.FindProperty("isActive").boolValue = isActive;
            serialized.FindProperty("minimumProsperity").floatValue = prosperity;
            serialized.FindProperty("minimumFoodSupplyPercent").floatValue = foodPercent;
            serialized.FindProperty("minimumIndustrialSupplyPercent").floatValue = industrialPercent;
            serialized.FindProperty("minimumCompletedContracts").intValue = contracts;
            serialized.FindProperty("requiresRegionDiscovered").boolValue = requiresRegion;
            serialized.FindProperty("requiredRegionId").stringValue = regionId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_SettlementGrowthProfile EnsureGrowthProfile(
            params CCS_SettlementGrowthDefinition[] definitions)
        {
            CCS_SettlementGrowthProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthProfile>(
                CCS_SettlementGrowthContentIds.DefaultGrowthProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementGrowthProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementGrowthContentIds.DefaultGrowthProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_SettlementGrowthContentIds.DefaultGrowthProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Growth Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement growth thresholds for bootstrap trading post.";
            serialized.FindProperty("profileVersion").stringValue = SettlementGrowthMilestoneVersion;

            SerializedProperty growthDefinitions = serialized.FindProperty("growthDefinitions");
            growthDefinitions.arraySize = definitions.Length;
            for (int index = 0; index < definitions.Length; index++)
            {
                growthDefinitions.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            SerializedProperty startingEntries = serialized.FindProperty("startingEntries");
            startingEntries.arraySize = 1;
            SerializedProperty startingEntry = startingEntries.GetArrayElementAtIndex(0);
            startingEntry.FindPropertyRelative("settlementId").stringValue =
                CCS_SettlementGrowthContentIds.TradingPostSettlementId;
            SerializedProperty startingStageProperty =
                startingEntry.FindPropertyRelative("startingGrowthStage");
            startingStageProperty.intValue = (int)CCS_SettlementGrowthStage.Outpost;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureWorldSimulationProfile(CCS_SettlementGrowthProfile growthProfile)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(WorldSimulationProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing world simulation profile: {WorldSimulationProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = SettlementGrowthMilestoneVersion;
            serialized.FindProperty("settlementGrowthProfile").objectReferenceValue = growthProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestSettlementGrowthSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing playtest profile: {DefaultPlaytestProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.discover", "Discover trading post", CCS_PlaytestStepType.DiscoverTradingPostForSettlementGrowth);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.contract", "Complete one contract", CCS_PlaytestStepType.CompleteContractForSettlementGrowth);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.verify.supply", "Verify prosperity/supply update", CCS_PlaytestStepType.VerifySettlementGrowthSupplyProsperity);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.verify.progress", "Verify growth progress", CCS_PlaytestStepType.VerifySettlementGrowthProgress);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.reach", "Reach TradingPost stage", CCS_PlaytestStepType.ReachTradingPostGrowthStage);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.verify.stage", "Verify growth stage changed", CCS_PlaytestStepType.VerifySettlementGrowthStageChanged);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.save", "Save settlement growth", CCS_PlaytestStepType.SaveSettlementGrowthState);
            InsertStep(profile, "ccs.survival.playtest.settlementgrowth.verify.load", "Verify growth after load", CCS_PlaytestStepType.VerifySettlementGrowthAfterLoad);
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
                $"Settlement growth playtest: {displayName}. Ctrl+Shift+G shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
