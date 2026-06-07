using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractsFoundationBootstrapSetup
// CATEGORY: Modules / Contracts / Editor / Validation
// PURPOSE: Batch-creates dynamic contract profile, contract wiring, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts.Editor
{
    public static class CCS_DynamicContractsFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_DynamicContractsFoundationBootstrapSetup]";
        private const string MilestoneVersion = "5.3.0";
        private const string ContractProfilePath = CCS_ContractContentIds.DefaultContractProfilePath;
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_DynamicContractContentIds.DynamicProfilesRoot);

            CCS_DynamicContractProfile dynamicProfile = EnsureDynamicContractProfile();
            EnsureContractProfileLink(dynamicProfile);
            EnsurePlaytestDynamicContractSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(MilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Dynamic contracts bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static CCS_DynamicContractProfile EnsureDynamicContractProfile()
        {
            CCS_DynamicContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_DynamicContractProfile>(
                CCS_DynamicContractContentIds.DefaultDynamicContractProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_DynamicContractProfile>();
                AssetDatabase.CreateAsset(profile, CCS_DynamicContractContentIds.DefaultDynamicContractProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_DynamicContractContentIds.DefaultDynamicContractProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Dynamic Contract Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Profile-driven settlement contract generation from supply, events, and regional specialization.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("maxActiveGeneratedContractsPerSettlement").intValue = 3;
            serialized.FindProperty("evaluationIntervalHours").intValue = 6;

            SerializedProperty rules = serialized.FindProperty("rules");
            rules.arraySize = 8;
            WriteRule(
                rules.GetArrayElementAtIndex(0),
                CCS_DynamicContractContentIds.LowFoodSupplyRuleId,
                CCS_DynamicContractGenerationSource.LowSettlementSupply,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.Food,
                25f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Unknown,
                "Low Food Supply Run",
                CCS_ContractType.GeneralStoreSupply,
                new[]
                {
                    CCS_ContractContentIds.CornItemId,
                    CCS_ContractContentIds.PotatoItemId,
                    CCS_ContractContentIds.MilkItemId,
                    CCS_DynamicContractContentIds.WheatProxyItemId
                },
                new[] { 4, 4, 2, 2 },
                12,
                2,
                1f,
                CCS_SettlementSupplyType.Food,
                2f,
                3,
                7,
                string.Empty,
                false);
            WriteRule(
                rules.GetArrayElementAtIndex(1),
                CCS_DynamicContractContentIds.MarketDayMixedGoodsRuleId,
                CCS_DynamicContractGenerationSource.ActiveSettlementEvent,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.TradeGoods,
                0f,
                CCS_SettlementEventType.MarketDay,
                CCS_RegionSpecializationType.Unknown,
                "Market Day Mixed Goods",
                CCS_ContractType.TradingPostSupply,
                new[]
                {
                    CCS_ContractContentIds.CornItemId,
                    CCS_ContractContentIds.HideItemId,
                    CCS_ContractContentIds.CordageItemId
                },
                new[] { 3, 2, 2 },
                15,
                2,
                1f,
                CCS_SettlementSupplyType.TradeGoods,
                2f,
                2,
                5,
                string.Empty,
                false);
            WriteRule(
                rules.GetArrayElementAtIndex(2),
                CCS_DynamicContractContentIds.HarvestFestivalFreightRuleId,
                CCS_DynamicContractGenerationSource.ActiveSettlementEvent,
                CCS_DynamicContractKind.FreightDelivery,
                CCS_SettlementSupplyType.Food,
                0f,
                CCS_SettlementEventType.HarvestFestival,
                CCS_RegionSpecializationType.Unknown,
                "Harvest Festival Food Freight",
                CCS_ContractType.FreightDelivery,
                new[] { CCS_ContractContentIds.CornItemId, CCS_ContractContentIds.PotatoItemId },
                new[] { 6, 6 },
                18,
                2,
                1f,
                CCS_SettlementSupplyType.Food,
                3f,
                2,
                5,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                false);
            WriteRule(
                rules.GetArrayElementAtIndex(3),
                CCS_DynamicContractContentIds.MiningRegionalSupplyRuleId,
                CCS_DynamicContractGenerationSource.RegionalSpecialization,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.IndustrialMaterials,
                0f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Mining,
                "Mining Camp Supply Run",
                CCS_ContractType.GunsmithSupply,
                new[]
                {
                    CCS_DynamicContractContentIds.BoneHatchetItemId,
                    CCS_ContractContentIds.LumberItemId,
                    CCS_ContractContentIds.CornItemId
                },
                new[] { 1, 3, 4 },
                16,
                2,
                1f,
                CCS_SettlementSupplyType.IndustrialMaterials,
                2f,
                4,
                7,
                string.Empty,
                false);
            WriteRule(
                rules.GetArrayElementAtIndex(4),
                CCS_DynamicContractContentIds.TimberRegionalSupplyRuleId,
                CCS_DynamicContractGenerationSource.RegionalSpecialization,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.BuildingMaterials,
                0f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Timber,
                "Timber Camp Supply Run",
                CCS_ContractType.GeneralStoreSupply,
                new[]
                {
                    CCS_ContractContentIds.CharcoalItemId,
                    CCS_ContractContentIds.LumberItemId,
                    CCS_DynamicContractContentIds.PolesItemId
                },
                new[] { 3, 4, 3 },
                14,
                2,
                1f,
                CCS_SettlementSupplyType.BuildingMaterials,
                2f,
                4,
                7,
                string.Empty,
                false);
            WriteRule(
                rules.GetArrayElementAtIndex(5),
                CCS_DynamicContractContentIds.WorkforceDemandPlaceholderRuleId,
                CCS_DynamicContractGenerationSource.WorkforceDemand,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.Food,
                0f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Unknown,
                "Workforce Demand Placeholder",
                CCS_ContractType.TradingPostSupply,
                new[] { CCS_ContractContentIds.CornItemId },
                new[] { 1 },
                0,
                0,
                0f,
                CCS_SettlementSupplyType.Food,
                0f,
                1,
                1,
                string.Empty,
                true);
            WriteRule(
                rules.GetArrayElementAtIndex(6),
                CCS_DynamicContractContentIds.BusinessDemandPlaceholderRuleId,
                CCS_DynamicContractGenerationSource.BusinessDemand,
                CCS_DynamicContractKind.LocalSupply,
                CCS_SettlementSupplyType.TradeGoods,
                0f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Unknown,
                "Business Demand Placeholder",
                CCS_ContractType.TradingPostSupply,
                new[] { CCS_ContractContentIds.LumberItemId },
                new[] { 1 },
                0,
                0,
                0f,
                CCS_SettlementSupplyType.TradeGoods,
                0f,
                1,
                1,
                string.Empty,
                true);
            WriteRule(
                rules.GetArrayElementAtIndex(7),
                CCS_DynamicContractContentIds.TradeRouteDemandPlaceholderRuleId,
                CCS_DynamicContractGenerationSource.TradeRouteDemand,
                CCS_DynamicContractKind.FreightDelivery,
                CCS_SettlementSupplyType.TradeGoods,
                0f,
                CCS_SettlementEventType.Unknown,
                CCS_RegionSpecializationType.Unknown,
                "Trade Route Demand Placeholder",
                CCS_ContractType.FreightDelivery,
                new[] { CCS_ContractContentIds.LumberItemId },
                new[] { 1 },
                0,
                0,
                0f,
                CCS_SettlementSupplyType.TradeGoods,
                0f,
                1,
                1,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                true);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureContractProfileLink(CCS_DynamicContractProfile dynamicProfile)
        {
            CCS_ContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(ContractProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing contract profile at {ContractProfilePath}.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("dynamicContractProfile").objectReferenceValue = dynamicProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlaytestDynamicContractSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.discover", "Discover settlement",
                CCS_PlaytestStepType.DiscoverSettlementsForDynamicContracts,
                "Discover Trading Post for dynamic contract playtest. Ctrl+Alt+C shortcut available.");
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.lowsupply", "Simulate low supply",
                CCS_PlaytestStepType.SimulateLowSupplyForDynamicContracts);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.generate.need", "Generate need-based contract",
                CCS_PlaytestStepType.GenerateNeedBasedDynamicContract);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.force.event", "Force settlement event",
                CCS_PlaytestStepType.ForceEventForDynamicContracts);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.generate.event", "Generate event-based contract",
                CCS_PlaytestStepType.GenerateEventBasedDynamicContract);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.verify.board", "Verify generated contract on board",
                CCS_PlaytestStepType.VerifyGeneratedDynamicContractOnBoard);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.complete", "Complete generated contract",
                CCS_PlaytestStepType.CompleteGeneratedDynamicContract);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.verify.rewards", "Verify rewards and supply",
                CCS_PlaytestStepType.VerifyDynamicContractRewardsApplied);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.save", "Save dynamic contract state",
                CCS_PlaytestStepType.SaveDynamicContractState);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.load", "Load dynamic contract state",
                CCS_PlaytestStepType.LoadDynamicContractState);
            InsertStep(profile, "ccs.survival.playtest.dynamiccontracts.verify.load", "Verify dynamic contract after load",
                CCS_PlaytestStepType.VerifyDynamicContractStateAfterLoad);
            EditorUtility.SetDirty(profile);
        }

        private static void WriteRule(
            SerializedProperty ruleProperty,
            string ruleId,
            CCS_DynamicContractGenerationSource generationSource,
            CCS_DynamicContractKind contractKind,
            CCS_SettlementSupplyType supplyType,
            float supplyThresholdPercent,
            CCS_SettlementEventType eventType,
            CCS_RegionSpecializationType regionSpecialization,
            string displayName,
            CCS_ContractType contractType,
            string[] itemIds,
            int[] quantities,
            int tradeDollars,
            int reputationGain,
            float prosperityGain,
            CCS_SettlementSupplyType rewardSupplyType,
            float rewardSupplyAmount,
            int cooldownDays,
            int expirationDays,
            string freightDestinationSettlementId,
            bool placeholderOnly)
        {
            ruleProperty.FindPropertyRelative("ruleId").stringValue = ruleId;
            ruleProperty.FindPropertyRelative("generationSource").intValue = (int)generationSource;
            ruleProperty.FindPropertyRelative("contractKind").intValue = (int)contractKind;
            ruleProperty.FindPropertyRelative("supplyType").intValue = (int)supplyType;
            ruleProperty.FindPropertyRelative("supplyThresholdPercent").floatValue = supplyThresholdPercent;
            ruleProperty.FindPropertyRelative("eventType").intValue = (int)eventType;
            ruleProperty.FindPropertyRelative("regionSpecialization").intValue = (int)regionSpecialization;
            ruleProperty.FindPropertyRelative("displayName").stringValue = displayName;
            ruleProperty.FindPropertyRelative("contractType").intValue = (int)contractType;
            WriteStringArray(ruleProperty.FindPropertyRelative("requiredItemIds"), itemIds);
            WriteIntArray(ruleProperty.FindPropertyRelative("requiredQuantities"), quantities);
            ruleProperty.FindPropertyRelative("tradeDollars").intValue = tradeDollars;
            ruleProperty.FindPropertyRelative("reputationGain").intValue = reputationGain;
            ruleProperty.FindPropertyRelative("prosperityGain").floatValue = prosperityGain;
            ruleProperty.FindPropertyRelative("rewardSupplyType").intValue = (int)rewardSupplyType;
            ruleProperty.FindPropertyRelative("rewardSupplyAmount").floatValue = rewardSupplyAmount;
            ruleProperty.FindPropertyRelative("cooldownDays").intValue = cooldownDays;
            ruleProperty.FindPropertyRelative("expirationDays").intValue = expirationDays;
            ruleProperty.FindPropertyRelative("freightDestinationSettlementId").stringValue =
                freightDestinationSettlementId ?? string.Empty;
            ruleProperty.FindPropertyRelative("enabled").boolValue = true;
            ruleProperty.FindPropertyRelative("placeholderOnly").boolValue = placeholderOnly;
        }

        private static void WriteStringArray(SerializedProperty arrayProperty, string[] values)
        {
            arrayProperty.arraySize = values?.Length ?? 0;
            for (int index = 0; index < arrayProperty.arraySize; index++)
            {
                arrayProperty.GetArrayElementAtIndex(index).stringValue = values[index] ?? string.Empty;
            }
        }

        private static void WriteIntArray(SerializedProperty arrayProperty, int[] values)
        {
            arrayProperty.arraySize = values?.Length ?? 0;
            for (int index = 0; index < arrayProperty.arraySize; index++)
            {
                arrayProperty.GetArrayElementAtIndex(index).intValue = values[index];
            }
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructions = "")
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("steps");
            for (int index = 0; index < steps.arraySize; index++)
            {
                SerializedProperty step = steps.GetArrayElementAtIndex(index);
                if (step.FindPropertyRelative("stepType").intValue == (int)stepType)
                {
                    step.FindPropertyRelative("stepId").stringValue = stepId;
                    step.FindPropertyRelative("displayName").stringValue = displayName;
                    if (!string.IsNullOrWhiteSpace(instructions))
                    {
                        step.FindPropertyRelative("instructions").stringValue = instructions;
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }

            int insertIndex = steps.arraySize;
            steps.InsertArrayElementAtIndex(insertIndex);
            SerializedProperty newStep = steps.GetArrayElementAtIndex(insertIndex);
            newStep.FindPropertyRelative("stepId").stringValue = stepId;
            newStep.FindPropertyRelative("displayName").stringValue = displayName;
            newStep.FindPropertyRelative("stepType").intValue = (int)stepType;
            newStep.FindPropertyRelative("instructions").stringValue = instructions ?? string.Empty;
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
            if (!string.IsNullOrWhiteSpace(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
