using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionSpecializationFoundationBootstrapSetup
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Batch-updates region economy content, world simulation, contracts, and playtest steps.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    public static class CCS_RegionSpecializationFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_RegionSpecializationFoundationBootstrapSetup]";
        private const string RegionalEconomyMilestoneVersion = "3.1.0";
        private const string ContentRoot = "Assets/CCS/Survival/Content/Regions";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureRegionDefinitions();
            EnsureWorldSimulationProfile();
            EnsureContractRegionalSpecializations();
            EnsurePlaytestRegionalEconomySteps();
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Regional specialization foundation bootstrap complete ({RegionalEconomyMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureRegionDefinitions()
        {
            ApplyRegionEconomy(
                ContentRoot + "/CCS_Region_PineRidgeForest.asset",
                CCS_RegionSpecializationType.Timber,
                1.25f,
                1.1f,
                CCS_RegionSpecializationType.Timber,
                CCS_RegionSpecializationType.FrontierMixed);
            ApplyRegionEconomy(
                ContentRoot + "/CCS_Region_BrokenCreek.asset",
                CCS_RegionSpecializationType.Agriculture,
                1.3f,
                1.15f,
                CCS_RegionSpecializationType.Agriculture,
                CCS_RegionSpecializationType.Ranching);
            ApplyRegionEconomy(
                ContentRoot + "/CCS_Region_IronRidgeMine.asset",
                CCS_RegionSpecializationType.Mining,
                1.35f,
                1.2f,
                CCS_RegionSpecializationType.Mining,
                CCS_RegionSpecializationType.FrontierMixed);
            ApplyRegionEconomy(
                ContentRoot + "/CCS_Region_FrontierTradingPost.asset",
                CCS_RegionSpecializationType.FrontierMixed,
                1.1f,
                1.25f,
                CCS_RegionSpecializationType.FrontierMixed,
                CCS_RegionSpecializationType.Agriculture,
                CCS_RegionSpecializationType.Ranching,
                CCS_RegionSpecializationType.Mining,
                CCS_RegionSpecializationType.Timber);
        }

        private static void ApplyRegionEconomy(
            string assetPath,
            CCS_RegionSpecializationType specializationType,
            float productionBonus,
            float prosperityModifier,
            params CCS_RegionSpecializationType[] preferredCategories)
        {
            CCS_RegionDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RegionDefinition>(assetPath);
            if (definition == null)
            {
                Debug.LogError($"{LogPrefix} Missing region definition: {assetPath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("specializationType").enumValueIndex = (int)specializationType;
            SerializedProperty modifier = serialized.FindProperty("productionModifier");
            modifier.FindPropertyRelative("productionBonus").floatValue = productionBonus;
            modifier.FindPropertyRelative("prosperityModifier").floatValue = prosperityModifier;
            SerializedProperty categories = modifier.FindPropertyRelative("preferredContractCategories");
            categories.arraySize = preferredCategories.Length;
            for (int index = 0; index < preferredCategories.Length; index++)
            {
                categories.GetArrayElementAtIndex(index).enumValueIndex = (int)preferredCategories[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureWorldSimulationProfile()
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
            serialized.FindProperty("profileVersion").stringValue = RegionalEconomyMilestoneVersion;

            SerializedProperty settlementEntries = serialized.FindProperty("settlementEntries");
            if (settlementEntries.arraySize > 0)
            {
                settlementEntries.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("regionId")
                    .stringValue = CCS_RegionContentIds.FrontierTradingPostRegionId;
            }

            SerializedProperty regionEntries = serialized.FindProperty("regionEntries");
            regionEntries.arraySize = 4;
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(0),
                CCS_RegionContentIds.PineRidgeForestRegionId,
                CCS_RegionSpecializationType.Timber,
                0.85f,
                0.75f,
                0.15f,
                0.20f,
                1.25f,
                1.1f,
                CCS_RegionSpecializationType.Timber,
                CCS_RegionSpecializationType.FrontierMixed);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(1),
                CCS_RegionContentIds.BrokenCreekRegionId,
                CCS_RegionSpecializationType.Agriculture,
                0.70f,
                0.65f,
                0.10f,
                0.15f,
                1.3f,
                1.15f,
                CCS_RegionSpecializationType.Agriculture,
                CCS_RegionSpecializationType.Ranching);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(2),
                CCS_RegionContentIds.IronRidgeMineRegionId,
                CCS_RegionSpecializationType.Mining,
                0.20f,
                0.15f,
                0.90f,
                0.75f,
                1.35f,
                1.2f,
                CCS_RegionSpecializationType.Mining,
                CCS_RegionSpecializationType.FrontierMixed);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(3),
                CCS_RegionContentIds.FrontierTradingPostRegionId,
                CCS_RegionSpecializationType.FrontierMixed,
                0.35f,
                0.25f,
                0.30f,
                0.80f,
                1.1f,
                1.25f,
                CCS_RegionSpecializationType.FrontierMixed,
                CCS_RegionSpecializationType.Agriculture,
                CCS_RegionSpecializationType.Ranching,
                CCS_RegionSpecializationType.Mining,
                CCS_RegionSpecializationType.Timber);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void SetRegionEntry(
            SerializedProperty entry,
            string regionId,
            CCS_RegionSpecializationType specializationType,
            float foodPotential,
            float wildlifePotential,
            float miningPotential,
            float industryPotential,
            float productionBonus,
            float prosperityModifier,
            params CCS_RegionSpecializationType[] preferredCategories)
        {
            entry.FindPropertyRelative("regionId").stringValue = regionId;
            entry.FindPropertyRelative("specializationType").intValue = (int)specializationType;
            entry.FindPropertyRelative("foodPotential").floatValue = foodPotential;
            entry.FindPropertyRelative("wildlifePotential").floatValue = wildlifePotential;
            entry.FindPropertyRelative("miningPotential").floatValue = miningPotential;
            entry.FindPropertyRelative("industryPotential").floatValue = industryPotential;
            entry.FindPropertyRelative("productionBonus").floatValue = productionBonus;
            entry.FindPropertyRelative("prosperityModifier").floatValue = prosperityModifier;
            SerializedProperty categories = entry.FindPropertyRelative("preferredContractCategories");
            categories.arraySize = preferredCategories.Length;
            for (int index = 0; index < preferredCategories.Length; index++)
            {
                categories.GetArrayElementAtIndex(index).intValue = (int)preferredCategories[index];
            }
        }

        private static void EnsureContractRegionalSpecializations()
        {
            SetContractSpecialization(CCS_ContractContentIds.LumberDeliveryContractPath, CCS_RegionSpecializationType.Timber);
            SetContractSpecialization(CCS_ContractContentIds.CornDeliveryContractPath, CCS_RegionSpecializationType.Agriculture);
            SetContractSpecialization(CCS_ContractContentIds.PotatoDeliveryContractPath, CCS_RegionSpecializationType.Agriculture);
            SetContractSpecialization(CCS_ContractContentIds.FeedDeliveryContractPath, CCS_RegionSpecializationType.Ranching);
            SetContractSpecialization(CCS_ContractContentIds.MilkDeliveryContractPath, CCS_RegionSpecializationType.Ranching);
            SetContractSpecialization(CCS_ContractContentIds.IronOreDeliveryContractPath, CCS_RegionSpecializationType.Mining);
            SetContractSpecialization(CCS_ContractContentIds.RefinedIronDeliveryContractPath, CCS_RegionSpecializationType.Mining);
            SetContractSpecialization(CCS_ContractContentIds.CharcoalDeliveryContractPath, CCS_RegionSpecializationType.Timber);
            SetContractSpecialization(CCS_ContractContentIds.MixedFrontierSupplyContractPath, CCS_RegionSpecializationType.FrontierMixed);
        }

        private static void SetContractSpecialization(string assetPath, CCS_RegionSpecializationType specializationType)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                Debug.LogError($"{LogPrefix} Missing contract definition: {assetPath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("regionSpecialization").enumValueIndex = (int)specializationType;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsurePlaytestRegionalEconomySteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing playtest profile: {DefaultPlaytestProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.discover", "Discover regions", CCS_PlaytestStepType.DiscoverRegionsForRegionalEconomy);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.verify.specialization", "Verify region specialization", CCS_PlaytestStepType.VerifyRegionSpecialization);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.accept", "Accept region contract", CCS_PlaytestStepType.AcceptRegionalSpecialtyContract);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.gather", "Gather regional contract goods", CCS_PlaytestStepType.GatherRegionalContractGoods);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.complete", "Complete specialized contract", CCS_PlaytestStepType.CompleteRegionalSpecialtyContract);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.verify.prosperity", "Verify prosperity increase", CCS_PlaytestStepType.VerifyRegionalProsperityIncrease);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.save", "Save region state", CCS_PlaytestStepType.SaveRegionalEconomyState);
            InsertStep(profile, "ccs.survival.playtest.regionaleconomy.verify.load", "Verify after load", CCS_PlaytestStepType.VerifyRegionalEconomyAfterLoad);
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
                $"Regional economy playtest: {displayName}. Ctrl+Shift+E shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
