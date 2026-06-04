using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRoutesFreightFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates outbound trade routes, freight contracts, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 trade routes and freight contracts.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_TradeRoutesFreightFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_TradeRoutesFreightFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.4.0";
        private const string DefaultContractProfilePath = CCS_ContractContentIds.DefaultContractProfilePath;
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradingPostSettlementId = CCS_SettlementContentIds.TestTradingPostSettlementId;

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolder(CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot);

            CCS_TradeRouteProfile tradeRouteProfile = EnsureOutboundAndMixedTradeRoutes();
            EnsureFreightContracts(tradeRouteProfile);
            EnsurePlaytestFreightSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Trade routes and freight bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
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

        private static CCS_TradeRouteProfile EnsureOutboundAndMixedTradeRoutes()
        {
            CCS_TradeRouteDefinition routePineOutbound = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_PineRidge_TradingPost.asset",
                CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                "Pine Ridge to Trading Post",
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                TradingPostSettlementId,
                new[] { CCS_RegionEconomyUtility.LumberItemId, CCS_ContractContentIds.CharcoalItemId },
                18f,
                CCS_TradeRouteDifficulty.Moderate);
            CCS_TradeRouteDefinition routeBrokenOutbound = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_BrokenCreek_TradingPost.asset",
                CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                "Broken Creek to Trading Post",
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                TradingPostSettlementId,
                new[] { CCS_RegionEconomyUtility.CornItemId, CCS_RegionEconomyUtility.WheatItemId },
                22f,
                CCS_TradeRouteDifficulty.Moderate);
            CCS_TradeRouteDefinition routeIronOutbound = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_IronRidge_TradingPost.asset",
                CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                "Iron Ridge to Trading Post",
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                TradingPostSettlementId,
                new[] { CCS_RegionEconomyUtility.IronOreItemId, CCS_RegionEconomyUtility.CoalItemId },
                26f,
                CCS_TradeRouteDifficulty.Hard);

            CCS_TradeRouteDefinition routeToPine = LoadRoute(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_PineRidge.asset");
            CCS_TradeRouteDefinition routeToBroken = LoadRoute(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_BrokenCreek.asset");
            CCS_TradeRouteDefinition routeToIron = LoadRoute(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_IronRidge.asset");

            UpdateRouteMetadata(routeToPine, CCS_TradeRouteDifficulty.Easy);
            UpdateRouteMetadata(routeToBroken, CCS_TradeRouteDifficulty.Easy);
            UpdateRouteMetadata(routeToIron, CCS_TradeRouteDifficulty.Moderate);

            CCS_TradeRouteProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteProfile>(
                CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing trade route profile.");
                EditorApplication.Exit(1);
                return null;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty routes = serialized.FindProperty("tradeRouteDefinitions");
            routes.arraySize = 6;
            routes.GetArrayElementAtIndex(0).objectReferenceValue = routeToPine;
            routes.GetArrayElementAtIndex(1).objectReferenceValue = routeToBroken;
            routes.GetArrayElementAtIndex(2).objectReferenceValue = routeToIron;
            routes.GetArrayElementAtIndex(3).objectReferenceValue = routePineOutbound;
            routes.GetArrayElementAtIndex(4).objectReferenceValue = routeBrokenOutbound;
            routes.GetArrayElementAtIndex(5).objectReferenceValue = routeIronOutbound;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_TradeRouteDefinition LoadRoute(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_TradeRouteDefinition>(assetPath);
        }

        private static void UpdateRouteMetadata(CCS_TradeRouteDefinition definition, CCS_TradeRouteDifficulty difficulty)
        {
            if (definition == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("routeDifficulty").enumValueIndex = (int)difficulty;
            serialized.FindProperty("startsDiscovered").boolValue = false;
            serialized.FindProperty("startsActive").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static CCS_TradeRouteDefinition EnsureTradeRouteDefinition(
            string assetPath,
            string routeId,
            string displayName,
            string originSettlementId,
            string destinationSettlementId,
            string[] preferredGoods,
            float distance,
            CCS_TradeRouteDifficulty difficulty)
        {
            CCS_TradeRouteDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_TradeRouteDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("routeId").stringValue = routeId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("originSettlementId").stringValue = originSettlementId;
            serialized.FindProperty("destinationSettlementId").stringValue = destinationSettlementId;
            SerializedProperty goods = serialized.FindProperty("preferredGoods");
            goods.arraySize = preferredGoods.Length;
            for (int index = 0; index < preferredGoods.Length; index++)
            {
                goods.GetArrayElementAtIndex(index).stringValue = preferredGoods[index];
            }

            serialized.FindProperty("distance").floatValue = distance;
            serialized.FindProperty("routeDifficulty").enumValueIndex = (int)difficulty;
            serialized.FindProperty("startsDiscovered").boolValue = false;
            serialized.FindProperty("startsActive").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureFreightContracts(CCS_TradeRouteProfile tradeRouteProfile)
        {
            CCS_ContractDefinition lumberFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_PineRidge_Lumber.asset",
                CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                "Pine Ridge Lumber Freight",
                CCS_ContractContentIds.LumberItemId,
                5,
                24,
                3,
                1,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                CCS_RegionSpecializationType.Timber);
            CCS_ContractDefinition charcoalFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_PineRidge_Charcoal.asset",
                CCS_TradeRoutesFreightContentIds.PineRidgeCharcoalFreightContractId,
                "Pine Ridge Charcoal Freight",
                CCS_ContractContentIds.CharcoalItemId,
                4,
                20,
                3,
                1,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.PineRidgeToTradingPostRouteId,
                CCS_RegionSpecializationType.Timber);
            CCS_ContractDefinition cornFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_BrokenCreek_Corn.asset",
                CCS_TradeRoutesFreightContentIds.BrokenCreekCornFreightContractId,
                "Broken Creek Corn Freight",
                CCS_RegionEconomyUtility.CornItemId,
                6,
                18,
                3,
                1,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                CCS_RegionSpecializationType.Agriculture);
            CCS_ContractDefinition wheatFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_BrokenCreek_Wheat.asset",
                CCS_TradeRoutesFreightContentIds.BrokenCreekWheatFreightContractId,
                "Broken Creek Wheat Freight",
                CCS_RegionEconomyUtility.WheatItemId,
                5,
                16,
                3,
                1,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.BrokenCreekToTradingPostRouteId,
                CCS_RegionSpecializationType.Agriculture);
            CCS_ContractDefinition ironFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_IronRidge_IronOre.asset",
                CCS_TradeRoutesFreightContentIds.IronRidgeIronOreFreightContractId,
                "Iron Ridge Iron Ore Freight",
                CCS_RegionEconomyUtility.IronOreItemId,
                4,
                28,
                4,
                1,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                CCS_RegionSpecializationType.Mining);
            CCS_ContractDefinition coalFreight = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_IronRidge_Coal.asset",
                CCS_TradeRoutesFreightContentIds.IronRidgeCoalFreightContractId,
                "Iron Ridge Coal Freight",
                CCS_RegionEconomyUtility.CoalItemId,
                4,
                24,
                4,
                1,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                TradingPostSettlementId,
                CCS_TradeRoutesFreightContentIds.IronRidgeToTradingPostRouteId,
                CCS_RegionSpecializationType.Mining);
            CCS_ContractDefinition mixedPine = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_TradingPost_PineRidge_Mixed.asset",
                CCS_TradeRoutesFreightContentIds.TradingPostPineMixedFreightContractId,
                "Trading Post Mixed Supplies (Pine Ridge)",
                CCS_ContractContentIds.HideItemId,
                2,
                12,
                2,
                0,
                TradingPostSettlementId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_TradeRoutesFreightContentIds.TradingPostToPineRidgeMixedRouteId,
                CCS_RegionSpecializationType.FrontierMixed);
            CCS_ContractDefinition mixedBroken = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_TradingPost_BrokenCreek_Mixed.asset",
                CCS_TradeRoutesFreightContentIds.TradingPostBrokenCreekMixedFreightContractId,
                "Trading Post Mixed Supplies (Broken Creek)",
                CCS_ContractContentIds.CordageItemId,
                2,
                12,
                2,
                0,
                TradingPostSettlementId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_TradeRoutesFreightContentIds.TradingPostToBrokenCreekMixedRouteId,
                CCS_RegionSpecializationType.FrontierMixed);
            CCS_ContractDefinition mixedIron = EnsureFreightContract(
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_TradingPost_IronRidge_Mixed.asset",
                CCS_TradeRoutesFreightContentIds.TradingPostIronRidgeMixedFreightContractId,
                "Trading Post Mixed Supplies (Iron Ridge)",
                CCS_ContractContentIds.HideItemId,
                2,
                12,
                2,
                0,
                TradingPostSettlementId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_TradeRoutesFreightContentIds.TradingPostToIronRidgeMixedRouteId,
                CCS_RegionSpecializationType.FrontierMixed);

            CCS_ContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(DefaultContractProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty definitions = serialized.FindProperty("contractDefinitions");
            int nextIndex = definitions.arraySize;
            AddContractDefinition(definitions, ref nextIndex, lumberFreight);
            AddContractDefinition(definitions, ref nextIndex, charcoalFreight);
            AddContractDefinition(definitions, ref nextIndex, cornFreight);
            AddContractDefinition(definitions, ref nextIndex, wheatFreight);
            AddContractDefinition(definitions, ref nextIndex, ironFreight);
            AddContractDefinition(definitions, ref nextIndex, coalFreight);
            AddContractDefinition(definitions, ref nextIndex, mixedPine);
            AddContractDefinition(definitions, ref nextIndex, mixedBroken);
            AddContractDefinition(definitions, ref nextIndex, mixedIron);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static CCS_ContractDefinition EnsureFreightContract(
            string assetPath,
            string contractId,
            string displayName,
            string itemId,
            int quantity,
            int tradeDollars,
            int destinationReputation,
            int originReputation,
            string sourceSettlementId,
            string destinationSettlementId,
            string linkedTradeRouteId,
            CCS_RegionSpecializationType specialization)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ContractDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("contractId").stringValue = contractId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("contractType").enumValueIndex = (int)CCS_ContractType.FreightDelivery;
            serialized.FindProperty("settlementId").stringValue = string.Empty;
            serialized.FindProperty("regionSpecialization").enumValueIndex = (int)specialization;
            serialized.FindProperty("enabled").boolValue = true;
            serialized.FindProperty("freightSourceSettlementId").stringValue = sourceSettlementId;
            serialized.FindProperty("freightDestinationSettlementId").stringValue = destinationSettlementId;
            serialized.FindProperty("linkedTradeRouteId").stringValue = linkedTradeRouteId;
            serialized.FindProperty("preferWagonCargo").boolValue = true;
            serialized.FindProperty("allowPlayerInventoryFallback").boolValue = false;
            SerializedProperty requirements = serialized.FindProperty("requirements");
            requirements.arraySize = 1;
            SerializedProperty requirement = requirements.GetArrayElementAtIndex(0);
            requirement.FindPropertyRelative("itemId").stringValue = itemId;
            requirement.FindPropertyRelative("quantity").intValue = quantity;
            requirement.FindPropertyRelative("settlementIdRestriction").stringValue = string.Empty;
            SerializedProperty reward = serialized.FindProperty("reward");
            reward.FindPropertyRelative("tradeDollars").intValue = tradeDollars;
            reward.FindPropertyRelative("reputationGain").intValue = destinationReputation;
            reward.FindPropertyRelative("originReputationGain").intValue = originReputation;
            reward.FindPropertyRelative("prosperityGain").floatValue = 1.5f;
            reward.FindPropertyRelative("supplyType").enumValueIndex = (int)CCS_SettlementSupplyType.TradeGoods;
            reward.FindPropertyRelative("supplyAmount").floatValue = 2f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void AddContractDefinition(
            SerializedProperty definitions,
            ref int nextIndex,
            CCS_ContractDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            for (int index = 0; index < definitions.arraySize; index++)
            {
                if (definitions.GetArrayElementAtIndex(index).objectReferenceValue == definition)
                {
                    return;
                }
            }

            definitions.InsertArrayElementAtIndex(nextIndex);
            definitions.GetArrayElementAtIndex(nextIndex).objectReferenceValue = definition;
            nextIndex++;
        }

        private static void EnsurePlaytestFreightSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.freight.discover", "Discover Pine Ridge and Trading Post", CCS_PlaytestStepType.DiscoverFreightRouteSettlements);
            InsertStep(profile, "ccs.survival.playtest.freight.accept.lumber", "Accept Pine Ridge lumber freight contract", CCS_PlaytestStepType.AcceptPineRidgeLumberFreightContract);
            InsertStep(profile, "ccs.survival.playtest.freight.wagon", "Place or summon wagon", CCS_PlaytestStepType.SummonWagonForFreight);
            InsertStep(profile, "ccs.survival.playtest.freight.load", "Put lumber in wagon cargo", CCS_PlaytestStepType.LoadLumberIntoWagonCargoForFreight);
            InsertStep(profile, "ccs.survival.playtest.freight.travel", "Travel to Trading Post contract board", CCS_PlaytestStepType.TravelToTradingPostFreightBoard);
            InsertStep(profile, "ccs.survival.playtest.freight.complete", "Complete freight delivery", CCS_PlaytestStepType.CompletePineRidgeLumberFreightDelivery);
            InsertStep(profile, "ccs.survival.playtest.freight.verify.sim", "Verify destination prosperity and supply", CCS_PlaytestStepType.VerifyFreightDestinationProsperitySupply);
            InsertStep(profile, "ccs.survival.playtest.freight.verify.route", "Verify route usage count", CCS_PlaytestStepType.VerifyTradeRouteUsageCount);
            InsertStep(profile, "ccs.survival.playtest.freight.save", "Save freight and route state", CCS_PlaytestStepType.SaveFreightRouteState);
            InsertStep(profile, "ccs.survival.playtest.freight.load.verify", "Verify route and freight state after load", CCS_PlaytestStepType.VerifyFreightRouteStateAfterLoad);
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
                $"Trade routes / freight playtest: {displayName}. Ctrl+Shift+F shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
