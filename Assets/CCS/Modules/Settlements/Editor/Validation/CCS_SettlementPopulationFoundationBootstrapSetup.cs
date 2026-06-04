using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates population profile, growth thresholds, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementPopulationFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_SettlementPopulationFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.6.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_SettlementPopulationContentIds.PopulationProfilesRoot);

            CCS_SettlementPopulationProfile populationProfile = EnsurePopulationProfile();
            ApplyGrowthPopulationThresholds();
            EnsureWorldSimulationPopulationProfile(populationProfile);
            EnsurePlaytestPopulationSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Population foundation bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void ApplyGrowthPopulationThresholds()
        {
            ApplyGrowthPopulation(
                CCS_SettlementGrowthContentIds.OutpostGrowthDefinitionPath,
                0);
            ApplyGrowthPopulation(
                CCS_SettlementGrowthContentIds.TradingPostGrowthDefinitionPath,
                50);
        }

        private static void ApplyGrowthPopulation(string assetPath, int minimumPopulation)
        {
            CCS_SettlementGrowthDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthDefinition>(assetPath);
            if (definition == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing growth definition: {assetPath}");
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("minimumPopulation").intValue = minimumPopulation;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static CCS_SettlementPopulationProfile EnsurePopulationProfile()
        {
            CCS_SettlementPopulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementPopulationProfile>(
                CCS_SettlementPopulationContentIds.DefaultPopulationProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SettlementPopulationProfile>();
                AssetDatabase.CreateAsset(profile, CCS_SettlementPopulationContentIds.DefaultPopulationProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue =
                CCS_SettlementPopulationContentIds.DefaultPopulationProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Settlement Population Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement population growth, capacity, and workforce distribution defaults.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("basePassiveGrowthRate").floatValue = 0.25f;
            serialized.FindProperty("prosperityGrowthFactor").floatValue = 0.015f;
            serialized.FindProperty("contractCompletionGrowthBonus").floatValue = 2f;
            serialized.FindProperty("poorSupplyThresholdPercent").floatValue = 25f;
            serialized.FindProperty("poorSupplyGrowthMultiplier").floatValue = 0.5f;
            serialized.FindProperty("trustedGrowthMultiplier").floatValue = 1.1f;
            serialized.FindProperty("honoredGrowthMultiplier").floatValue = 1.15f;
            serialized.FindProperty("distrustedGrowthMultiplier").floatValue = 0.9f;
            serialized.FindProperty("hostileGrowthMultiplier").floatValue = 0.75f;
            serialized.FindProperty("basePopulationCapacity").intValue = 40;
            serialized.FindProperty("capacityPerProsperityPoint").floatValue = 1.25f;

            SerializedProperty entries = serialized.FindProperty("settlementEntries");
            entries.arraySize = 4;
            SetPopulationEntry(entries, 0, CCS_SettlementGrowthContentIds.TradingPostSettlementId, 12, 80);
            SetPopulationEntry(entries, 1, CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, 8, 60);
            SetPopulationEntry(entries, 2, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, 10, 65);
            SetPopulationEntry(entries, 3, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, 8, 60);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetPopulationEntry(
            SerializedProperty entries,
            int index,
            string settlementId,
            int startingPopulation,
            int startingCapacity)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("startingPopulation").intValue = startingPopulation;
            entry.FindPropertyRelative("startingCapacity").intValue = startingCapacity;
        }

        private static void EnsureWorldSimulationPopulationProfile(CCS_SettlementPopulationProfile populationProfile)
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
            serialized.FindProperty("settlementPopulationProfile").objectReferenceValue = populationProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestPopulationSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.population.discover", "Discover settlement for population", CCS_PlaytestStepType.DiscoverSettlementForPopulation);
            InsertStep(profile, "ccs.survival.playtest.population.contract", "Complete contract for population growth", CCS_PlaytestStepType.CompleteContractForPopulationGrowth);
            InsertStep(profile, "ccs.survival.playtest.population.verify.increase", "Verify population increased", CCS_PlaytestStepType.VerifyPopulationIncreased);
            InsertStep(profile, "ccs.survival.playtest.population.verify.nonnegative", "Verify population non-negative", CCS_PlaytestStepType.VerifyPopulationNonNegative);
            InsertStep(profile, "ccs.survival.playtest.population.verify.growthrate", "Verify population growth rate valid", CCS_PlaytestStepType.VerifyPopulationGrowthRateValid);
            InsertStep(profile, "ccs.survival.playtest.population.save", "Save population state", CCS_PlaytestStepType.SavePopulationState);
            InsertStep(profile, "ccs.survival.playtest.population.load", "Verify population after load", CCS_PlaytestStepType.VerifyPopulationAfterLoad);
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
                $"Population playtest: {displayName}. Ctrl+Shift+K shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
