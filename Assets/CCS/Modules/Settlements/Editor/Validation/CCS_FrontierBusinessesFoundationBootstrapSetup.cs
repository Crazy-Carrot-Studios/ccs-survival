using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierBusinessesFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates business profile, world simulation wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_FrontierBusinessesFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierBusinessesFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.7.0";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_BusinessContentIds.BusinessProfilesRoot);

            CCS_BusinessProfile businessProfile = EnsureBusinessProfile();
            EnsureWorldSimulationBusinessProfile(businessProfile);
            EnsurePlaytestBusinessSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier businesses bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_BusinessProfile EnsureBusinessProfile()
        {
            CCS_BusinessProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BusinessProfile>(
                CCS_BusinessContentIds.DefaultBusinessProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_BusinessProfile>();
                AssetDatabase.CreateAsset(profile, CCS_BusinessContentIds.DefaultBusinessProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_BusinessContentIds.DefaultBusinessProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Frontier Business Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Settlement business activation thresholds and per-settlement catalogs.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;

            SerializedProperty definitions = serialized.FindProperty("businessDefinitions");
            definitions.arraySize = 9;
            SetDefinition(definitions, 0, "ccs.survival.business.generalstore", "General Store", CCS_BusinessType.GeneralStore, 8, 10f, CCS_SettlementGrowthStage.Outpost, -1);
            SetDefinition(definitions, 1, "ccs.survival.business.blacksmith", "Blacksmith", CCS_BusinessType.Blacksmith, 20, 25f, CCS_SettlementGrowthStage.TradingPost, -1);
            SetDefinition(definitions, 2, "ccs.survival.business.stable", "Stable", CCS_BusinessType.Stable, 12, 15f, CCS_SettlementGrowthStage.Outpost, -1);
            SetDefinition(definitions, 3, "ccs.survival.business.gunsmith", "Gunsmith", CCS_BusinessType.Gunsmith, 18, 22f, CCS_SettlementGrowthStage.TradingPost, -1);
            SetDefinition(definitions, 4, "ccs.survival.business.bank", "Bank", CCS_BusinessType.Bank, 22, 30f, CCS_SettlementGrowthStage.TradingPost, -1);
            SetDefinition(definitions, 5, "ccs.survival.business.farmsupply", "Farm Supply", CCS_BusinessType.FarmSupply, 6, 15f, CCS_SettlementGrowthStage.Outpost, -1);
            SetDefinition(definitions, 6, "ccs.survival.business.miningsupplier", "Mining Supplier", CCS_BusinessType.MiningSupplier, 6, 15f, CCS_SettlementGrowthStage.Outpost, -1);
            SetDefinition(definitions, 7, "ccs.survival.business.lumberyard", "Lumber Yard", CCS_BusinessType.LumberYard, 6, 15f, CCS_SettlementGrowthStage.Outpost, -1);
            SetDefinition(definitions, 8, "ccs.survival.business.contractoffice", "Contract Office", CCS_BusinessType.ContractOffice, 10, 12f, CCS_SettlementGrowthStage.Outpost, (int)CCS_ReputationTier.Neutral);

            SerializedProperty catalog = serialized.FindProperty("settlementCatalog");
            catalog.arraySize = 4;
            SetCatalog(catalog, 0, CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_BusinessType.GeneralStore,
                CCS_BusinessType.Stable,
                CCS_BusinessType.Gunsmith,
                CCS_BusinessType.Bank,
                CCS_BusinessType.ContractOffice);
            SetCatalog(catalog, 1, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId, CCS_BusinessType.FarmSupply);
            SetCatalog(catalog, 2, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId, CCS_BusinessType.MiningSupplier);
            SetCatalog(catalog, 3, CCS_MultiSettlementContentIds.PineRidgeCampSettlementId, CCS_BusinessType.LumberYard);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void SetDefinition(
            SerializedProperty definitions,
            int index,
            string businessId,
            string displayName,
            CCS_BusinessType businessType,
            int minimumPopulation,
            float minimumProsperity,
            CCS_SettlementGrowthStage minimumGrowthStage,
            int minimumReputationTier)
        {
            SerializedProperty entry = definitions.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("businessId").stringValue = businessId;
            entry.FindPropertyRelative("displayName").stringValue = displayName;
            entry.FindPropertyRelative("businessType").enumValueIndex = (int)businessType;
            entry.FindPropertyRelative("minimumPopulation").intValue = minimumPopulation;
            entry.FindPropertyRelative("minimumProsperity").floatValue = minimumProsperity;
            entry.FindPropertyRelative("minimumGrowthStage").intValue = (int)minimumGrowthStage;
            entry.FindPropertyRelative("minimumReputationTier").intValue = minimumReputationTier;
        }

        private static void SetCatalog(
            SerializedProperty catalog,
            int index,
            string settlementId,
            params CCS_BusinessType[] businessTypes)
        {
            SerializedProperty entry = catalog.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            SerializedProperty types = entry.FindPropertyRelative("businessTypes");
            types.arraySize = businessTypes.Length;
            for (int typeIndex = 0; typeIndex < businessTypes.Length; typeIndex++)
            {
                types.GetArrayElementAtIndex(typeIndex).enumValueIndex = (int)businessTypes[typeIndex];
            }
        }

        private static void EnsureWorldSimulationBusinessProfile(CCS_BusinessProfile businessProfile)
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
            serialized.FindProperty("settlementBusinessProfile").objectReferenceValue = businessProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestBusinessSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.business.discover", "Discover settlement for businesses", CCS_PlaytestStepType.DiscoverSettlementForBusinesses);
            InsertStep(profile, "ccs.survival.playtest.business.contract", "Complete contract for business activation", CCS_PlaytestStepType.CompleteContractForBusinessActivation);
            InsertStep(profile, "ccs.survival.playtest.business.verify.active", "Verify business activated", CCS_PlaytestStepType.VerifyBusinessActivated);
            InsertStep(profile, "ccs.survival.playtest.business.verify.catalog", "Verify settlement business catalog", CCS_PlaytestStepType.VerifySettlementBusinessCatalog);
            InsertStep(profile, "ccs.survival.playtest.business.save", "Save business state", CCS_PlaytestStepType.SaveBusinessState);
            InsertStep(profile, "ccs.survival.playtest.business.load", "Verify business state after load", CCS_PlaytestStepType.VerifyBusinessStateAfterLoad);
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
                $"{displayName}. Ctrl+Shift+J shortcut available.";
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
