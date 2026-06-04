using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Reputation;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MultiSettlementFoundationBootstrapSetup
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Batch-creates multi-settlement network, trade routes, contracts, and playtest steps.
// PLACEMENT: Invoked by milestone bootstrap / validation pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.3.0 multi-settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_MultiSettlementFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_MultiSettlementFoundationBootstrapSetup]";
        private const string MilestoneVersion = "3.3.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TradingPostDefinitionPath = "Assets/CCS/Survival/Content/Settlements/CCS_Settlement_TestTradingPost.asset";
        private const string DefaultSettlementProfilePath = "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultSettlementProfile.asset";
        private const string DefaultGrowthProfilePath = CCS_SettlementGrowthContentIds.DefaultGrowthProfilePath;
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string DefaultContractProfilePath = CCS_ContractContentIds.DefaultContractProfilePath;
        private const string DefaultReputationProfilePath =
            "Assets/CCS/Survival/Profiles/Reputation/CCS_DefaultReputationProfile.asset";

        private const string PolesContractPath = CCS_ContractContentIds.ContractsContentRoot + "/CCS_Contract_PolesDelivery.asset";
        private const string WheatContractPath = CCS_ContractContentIds.ContractsContentRoot + "/CCS_Contract_WheatDelivery.asset";
        private const string CoalContractPath = CCS_ContractContentIds.ContractsContentRoot + "/CCS_Contract_CoalDelivery.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_SettlementDefinition tradingPost =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementDefinition>(TradingPostDefinitionPath);
            if (tradingPost == null)
            {
                Debug.LogError($"{LogPrefix} Missing trading post definition. Run frontier settlement bootstrap first.");
                EditorApplication.Exit(1);
                return;
            }

            UpdateTradingPostDefinition(tradingPost);
            CCS_SettlementDefinition pineRidge = EnsureSettlementDefinition(
                CCS_MultiSettlementContentIds.PineRidgeCampDefinitionPath,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                "Pine Ridge Camp",
                "Timber camp serving frontier lumber, poles, and charcoal contracts.",
                CCS_SettlementType.Other,
                new Vector3(-8f, 0f, -6f));
            CCS_SettlementDefinition brokenCreek = EnsureSettlementDefinition(
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadDefinitionPath,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                "Broken Creek Farmstead",
                "Agriculture farmstead for crop and dairy frontier contracts.",
                CCS_SettlementType.Homestead,
                new Vector3(-22f, 0f, 10f));
            CCS_SettlementDefinition ironRidge = EnsureSettlementDefinition(
                CCS_MultiSettlementContentIds.IronRidgeMiningCampDefinitionPath,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                "Iron Ridge Mining Camp",
                "Mining camp for ore, coal, and refined iron contracts.",
                CCS_SettlementType.MiningCamp,
                new Vector3(40f, 0f, 28f));

            CCS_TradeRouteProfile tradeRouteProfile = EnsureTradeRouteProfile(tradingPost, pineRidge, brokenCreek, ironRidge);
            CCS_SettlementProfile settlementProfile = EnsureSettlementProfile(
                tradeRouteProfile,
                tradingPost,
                pineRidge,
                brokenCreek,
                ironRidge);
            EnsureBootstrapHostProfiles(settlementProfile);
            EnsureRegionSettlementOwnership();
            EnsureWorldSimulationSettlements();
            EnsureGrowthStartingEntries();
            EnsureRegionalContractsAndProfile();
            EnsureReputationDefinitions(tradingPost, pineRidge, brokenCreek, ironRidge);
            EnsureBootstrapSettlementScene(tradingPost, pineRidge, brokenCreek, ironRidge);
            EnsurePlaytestMultiSettlementSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Multi-settlement foundation bootstrap complete ({MilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(CCS_MultiSettlementContentIds.TradeRoutesContentRoot);
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

        private static void UpdateTradingPostDefinition(CCS_SettlementDefinition tradingPost)
        {
            SerializedObject serialized = new SerializedObject(tradingPost);
            serialized.FindProperty("displayName").stringValue = "Frontier Trading Post";
            serialized.FindProperty("description").stringValue =
                "Frontier mixed hub with contract board and full service routing.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tradingPost);
        }

        private static CCS_SettlementDefinition EnsureSettlementDefinition(
            string assetPath,
            string settlementId,
            string displayName,
            string description,
            CCS_SettlementType settlementType,
            Vector3 defaultPosition)
        {
            CCS_SettlementDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_SettlementDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_SettlementDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("settlementId").stringValue = settlementId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("settlementType").enumValueIndex = (int)settlementType;
            serialized.FindProperty("defaultWorldPosition").vector3Value = defaultPosition;
            serialized.FindProperty("offersBankingServices").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_TradeRouteProfile EnsureTradeRouteProfile(
            CCS_SettlementDefinition tradingPost,
            CCS_SettlementDefinition pineRidge,
            CCS_SettlementDefinition brokenCreek,
            CCS_SettlementDefinition ironRidge)
        {
            CCS_TradeRouteDefinition routeToPine = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_PineRidge.asset",
                "ccs.survival.traderoute.frontier.tradingpost.pineridge",
                "Trading Post to Pine Ridge",
                tradingPost.SettlementId,
                pineRidge.SettlementId,
                new[] { CCS_RegionEconomyUtility.LumberItemId, CCS_RegionEconomyUtility.PolesItemId },
                18f);
            CCS_TradeRouteDefinition routeToBroken = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_BrokenCreek.asset",
                "ccs.survival.traderoute.frontier.tradingpost.brokencreek",
                "Trading Post to Broken Creek",
                tradingPost.SettlementId,
                brokenCreek.SettlementId,
                new[]
                {
                    CCS_RegionEconomyUtility.CornItemId,
                    CCS_RegionEconomyUtility.WheatItemId,
                    CCS_RegionEconomyUtility.PotatoItemId
                },
                22f);
            CCS_TradeRouteDefinition routeToIron = EnsureTradeRouteDefinition(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_TradingPost_IronRidge.asset",
                "ccs.survival.traderoute.frontier.tradingpost.ironridge",
                "Trading Post to Iron Ridge",
                tradingPost.SettlementId,
                ironRidge.SettlementId,
                new[]
                {
                    CCS_RegionEconomyUtility.IronOreItemId,
                    CCS_RegionEconomyUtility.CoalItemId,
                    CCS_RegionEconomyUtility.RefinedIronItemId
                },
                26f);

            CCS_TradeRouteProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteProfile>(
                CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_TradeRouteProfile>();
                AssetDatabase.CreateAsset(profile, CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.traderoutes.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Trade Route Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Metadata-only frontier trade routes between bootstrap settlements.";
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty routes = serialized.FindProperty("tradeRouteDefinitions");
            routes.arraySize = 3;
            routes.GetArrayElementAtIndex(0).objectReferenceValue = routeToPine;
            routes.GetArrayElementAtIndex(1).objectReferenceValue = routeToBroken;
            routes.GetArrayElementAtIndex(2).objectReferenceValue = routeToIron;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_TradeRouteDefinition EnsureTradeRouteDefinition(
            string assetPath,
            string routeId,
            string displayName,
            string originSettlementId,
            string destinationSettlementId,
            string[] preferredGoods,
            float distance)
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
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_SettlementProfile EnsureSettlementProfile(
            CCS_TradeRouteProfile tradeRouteProfile,
            params CCS_SettlementDefinition[] definitions)
        {
            CCS_SettlementProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing settlement profile: {DefaultSettlementProfilePath}");
                EditorApplication.Exit(1);
                return null;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            serialized.FindProperty("tradeRouteProfile").objectReferenceValue = tradeRouteProfile;
            SerializedProperty settlementDefinitions = serialized.FindProperty("settlementDefinitions");
            settlementDefinitions.arraySize = definitions.Length;
            for (int index = 0; index < definitions.Length; index++)
            {
                settlementDefinitions.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapHostProfiles(CCS_SettlementProfile settlementProfile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab");
            if (prefabRoot == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("settlementProfile").objectReferenceValue = settlementProfile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureRegionSettlementOwnership()
        {
            SetRegionSettlementIds(
                "Assets/CCS/Survival/Content/Regions/CCS_Region_PineRidgeForest.asset",
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            SetRegionSettlementIds(
                "Assets/CCS/Survival/Content/Regions/CCS_Region_BrokenCreek.asset",
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            SetRegionSettlementIds(
                "Assets/CCS/Survival/Content/Regions/CCS_Region_IronRidgeMine.asset",
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            SetRegionSettlementIds(
                "Assets/CCS/Survival/Content/Regions/CCS_Region_FrontierTradingPost.asset",
                CCS_SettlementContentIds.TestTradingPostSettlementId);
        }

        private static void SetRegionSettlementIds(string regionAssetPath, params string[] settlementIds)
        {
            CCS_RegionDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RegionDefinition>(regionAssetPath);
            if (definition == null)
            {
                Debug.LogError($"{LogPrefix} Missing region definition: {regionAssetPath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            SerializedProperty ids = serialized.FindProperty("settlementIds");
            ids.arraySize = settlementIds.Length;
            for (int index = 0; index < settlementIds.Length; index++)
            {
                ids.GetArrayElementAtIndex(index).stringValue = settlementIds[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureWorldSimulationSettlements()
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
            SerializedProperty settlementEntries = serialized.FindProperty("settlementEntries");
            settlementEntries.arraySize = CCS_MultiSettlementContentIds.BootstrapSettlementCount;
            ConfigureSettlementEntry(
                settlementEntries.GetArrayElementAtIndex(0),
                CCS_SettlementContentIds.TestTradingPostSettlementId,
                CCS_RegionContentIds.FrontierTradingPostRegionId,
                48,
                12f,
                20f,
                8f,
                10f,
                6f,
                15f);
            ConfigureSettlementEntry(
                settlementEntries.GetArrayElementAtIndex(1),
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_RegionContentIds.PineRidgeForestRegionId,
                18,
                6f,
                10f,
                14f,
                8f,
                4f,
                6f);
            ConfigureSettlementEntry(
                settlementEntries.GetArrayElementAtIndex(2),
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_RegionContentIds.BrokenCreekRegionId,
                22,
                14f,
                12f,
                4f,
                5f,
                3f,
                4f);
            ConfigureSettlementEntry(
                settlementEntries.GetArrayElementAtIndex(3),
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_RegionContentIds.IronRidgeMineRegionId,
                16,
                4f,
                4f,
                6f,
                12f,
                10f,
                5f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureSettlementEntry(
            SerializedProperty entry,
            string settlementId,
            string regionId,
            int population,
            float food,
            float water,
            float fuel,
            float building,
            float industrial,
            float tradeGoods)
        {
            entry.FindPropertyRelative("settlementId").stringValue = settlementId;
            entry.FindPropertyRelative("regionId").stringValue = regionId;
            entry.FindPropertyRelative("population").intValue = population;
            SerializedProperty supplies = entry.FindPropertyRelative("supplies");
            supplies.arraySize = 0;
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.Food, food, 40f);
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.Water, water, 40f);
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.Fuel, fuel, 30f);
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.BuildingMaterials, building, 35f);
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.IndustrialMaterials, industrial, 30f);
            SetSupplyEntry(supplies, (int)CCS_SettlementSupplyType.TradeGoods, tradeGoods, 45f);
        }

        private static void SetSupplyEntry(
            SerializedProperty supplies,
            int supplyType,
            float currentAmount,
            float desiredAmount)
        {
            int index = supplies.arraySize;
            supplies.InsertArrayElementAtIndex(index);
            SerializedProperty supply = supplies.GetArrayElementAtIndex(index);
            supply.FindPropertyRelative("supplyType").intValue = supplyType;
            supply.FindPropertyRelative("currentAmount").floatValue = currentAmount;
            supply.FindPropertyRelative("desiredAmount").floatValue = desiredAmount;
        }

        private static void EnsureGrowthStartingEntries()
        {
            CCS_SettlementGrowthProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthProfile>(DefaultGrowthProfilePath);
            if (profile == null)
            {
                return;
            }

            string[] settlementIds =
            {
                CCS_SettlementContentIds.TestTradingPostSettlementId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId
            };

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty startingEntries = serialized.FindProperty("startingEntries");
            startingEntries.arraySize = settlementIds.Length;
            for (int index = 0; index < settlementIds.Length; index++)
            {
                SerializedProperty entry = startingEntries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("settlementId").stringValue = settlementIds[index];
                entry.FindPropertyRelative("startingGrowthStage").intValue = (int)CCS_SettlementGrowthStage.Outpost;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureRegionalContractsAndProfile()
        {
            AssignContractSettlement(CCS_ContractContentIds.LumberDeliveryContractPath, CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            AssignContractSettlement(CCS_ContractContentIds.CharcoalDeliveryContractPath, CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            CCS_ContractDefinition poles = EnsureSingleItemContract(
                PolesContractPath,
                "ccs.survival.contract.pineridge.poles",
                "Poles Delivery",
                CCS_ContractType.GeneralStoreSupply,
                CCS_RegionEconomyUtility.PolesItemId,
                4,
                14,
                2,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_RegionSpecializationType.Timber);

            AssignContractSettlement(CCS_ContractContentIds.CornDeliveryContractPath, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            AssignContractSettlement(CCS_ContractContentIds.PotatoDeliveryContractPath, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            AssignContractSettlement(CCS_ContractContentIds.MilkDeliveryContractPath, CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId);
            CCS_ContractDefinition wheat = EnsureSingleItemContract(
                WheatContractPath,
                "ccs.survival.contract.brokencreek.wheat",
                "Wheat Delivery",
                CCS_ContractType.GeneralStoreSupply,
                CCS_RegionEconomyUtility.WheatItemId,
                5,
                12,
                2,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadSettlementId,
                CCS_RegionSpecializationType.Agriculture);

            AssignContractSettlement(CCS_ContractContentIds.IronOreDeliveryContractPath, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            AssignContractSettlement(CCS_ContractContentIds.RefinedIronDeliveryContractPath, CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId);
            CCS_ContractDefinition coal = EnsureSingleItemContract(
                CoalContractPath,
                "ccs.survival.contract.ironridge.coal",
                "Coal Delivery",
                CCS_ContractType.GunsmithSupply,
                CCS_RegionEconomyUtility.CoalItemId,
                4,
                16,
                2,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_RegionSpecializationType.Mining);

            AssignContractSettlement(CCS_ContractContentIds.MixedFrontierSupplyContractPath, CCS_SettlementContentIds.TestTradingPostSettlementId);
            AssignContractSettlement(CCS_ContractContentIds.FeedDeliveryContractPath, CCS_SettlementContentIds.TestTradingPostSettlementId);

            CCS_ContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(DefaultContractProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty definitions = serialized.FindProperty("contractDefinitions");
            int nextIndex = definitions.arraySize;
            AddContractDefinition(definitions, ref nextIndex, poles);
            AddContractDefinition(definitions, ref nextIndex, wheat);
            AddContractDefinition(definitions, ref nextIndex, coal);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
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

        private static void AssignContractSettlement(string assetPath, string settlementId)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("settlementId").stringValue = settlementId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static CCS_ContractDefinition EnsureSingleItemContract(
            string assetPath,
            string contractId,
            string displayName,
            CCS_ContractType contractType,
            string itemId,
            int quantity,
            int tradeDollars,
            int reputationGain,
            string settlementId,
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
            serialized.FindProperty("contractType").enumValueIndex = (int)contractType;
            serialized.FindProperty("settlementId").stringValue = settlementId;
            serialized.FindProperty("regionSpecialization").enumValueIndex = (int)specialization;
            serialized.FindProperty("enabled").boolValue = true;
            SerializedProperty requirements = serialized.FindProperty("requirements");
            requirements.arraySize = 1;
            SerializedProperty requirement = requirements.GetArrayElementAtIndex(0);
            requirement.FindPropertyRelative("itemId").stringValue = itemId;
            requirement.FindPropertyRelative("quantity").intValue = quantity;
            requirement.FindPropertyRelative("settlementIdRestriction").stringValue = string.Empty;
            SerializedProperty reward = serialized.FindProperty("reward");
            reward.FindPropertyRelative("tradeDollars").intValue = tradeDollars;
            reward.FindPropertyRelative("reputationGain").intValue = reputationGain;
            reward.FindPropertyRelative("prosperityGain").floatValue = 1f;
            reward.FindPropertyRelative("supplyType").enumValueIndex = (int)CCS_SettlementSupplyType.TradeGoods;
            reward.FindPropertyRelative("supplyAmount").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureReputationDefinitions(
            CCS_SettlementDefinition tradingPost,
            CCS_SettlementDefinition pineRidge,
            CCS_SettlementDefinition brokenCreek,
            CCS_SettlementDefinition ironRidge)
        {
            CCS_ReputationDefinition tradingPostReputation = EnsureReputationDefinition(
                "Assets/CCS/Survival/Content/Reputation/CCS_Reputation_FrontierTradingPost.asset",
                "ccs.survival.reputation.settlement.tradingpost",
                "Frontier Trading Post Trust",
                tradingPost.SettlementId);
            CCS_ReputationDefinition pineReputation = EnsureReputationDefinition(
                "Assets/CCS/Survival/Content/Reputation/CCS_Reputation_PineRidgeCamp.asset",
                "ccs.survival.reputation.settlement.pineridgecamp",
                "Pine Ridge Camp Trust",
                pineRidge.SettlementId);
            CCS_ReputationDefinition brokenReputation = EnsureReputationDefinition(
                "Assets/CCS/Survival/Content/Reputation/CCS_Reputation_BrokenCreekFarmstead.asset",
                "ccs.survival.reputation.settlement.brokencreekfarmstead",
                "Broken Creek Farmstead Trust",
                brokenCreek.SettlementId);
            CCS_ReputationDefinition ironReputation = EnsureReputationDefinition(
                "Assets/CCS/Survival/Content/Reputation/CCS_Reputation_IronRidgeMiningCamp.asset",
                "ccs.survival.reputation.settlement.ironridgeminingcamp",
                "Iron Ridge Mining Camp Trust",
                ironRidge.SettlementId);

            CCS_ReputationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ReputationProfile>(DefaultReputationProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = MilestoneVersion;
            SerializedProperty definitions = serialized.FindProperty("reputationDefinitions");
            definitions.arraySize = 4;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = tradingPostReputation;
            definitions.GetArrayElementAtIndex(1).objectReferenceValue = pineReputation;
            definitions.GetArrayElementAtIndex(2).objectReferenceValue = brokenReputation;
            definitions.GetArrayElementAtIndex(3).objectReferenceValue = ironReputation;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static CCS_ReputationDefinition EnsureReputationDefinition(
            string assetPath,
            string reputationDefinitionId,
            string displayName,
            string settlementId)
        {
            CCS_ReputationDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ReputationDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ReputationDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("reputationDefinitionId").stringValue = reputationDefinitionId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("scopeType").enumValueIndex = (int)CCS_ReputationScopeType.Settlement;
            serialized.FindProperty("targetId").stringValue = settlementId;
            serialized.FindProperty("defaultValue").intValue = 0;
            serialized.FindProperty("enabled").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void EnsureBootstrapSettlementScene(
            CCS_SettlementDefinition tradingPost,
            CCS_SettlementDefinition pineRidge,
            CCS_SettlementDefinition brokenCreek,
            CCS_SettlementDefinition ironRidge)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                EditorApplication.Exit(1);
                return;
            }

            EnsureCampSettlement(
                sceneRoot,
                CCS_MultiSettlementContentIds.PineRidgeCampObjectName,
                pineRidge,
                CCS_MultiSettlementContentIds.PineRidgeContractBoardObjectName,
                new Vector3(-8f, 0f, -6f),
                new Color(0.45f, 0.65f, 0.35f));
            EnsureCampSettlement(
                sceneRoot,
                CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName,
                brokenCreek,
                CCS_MultiSettlementContentIds.BrokenCreekContractBoardObjectName,
                new Vector3(-22f, 0f, 10f),
                new Color(0.75f, 0.7f, 0.35f));
            EnsureCampSettlement(
                sceneRoot,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName,
                ironRidge,
                CCS_MultiSettlementContentIds.IronRidgeContractBoardObjectName,
                new Vector3(40f, 0f, 28f),
                new Color(0.55f, 0.55f, 0.6f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureCampSettlement(
            Transform sceneRoot,
            string objectName,
            CCS_SettlementDefinition definition,
            string contractBoardObjectName,
            Vector3 worldPosition,
            Color markerColor)
        {
            Transform existing = sceneRoot.Find(objectName);
            GameObject root = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                root.transform.SetParent(sceneRoot, false);
            }

            root.transform.position = worldPosition;
            CCS_SettlementLocation location = root.GetComponent<CCS_SettlementLocation>();
            if (location == null)
            {
                location = root.AddComponent<CCS_SettlementLocation>();
            }

            SerializedObject serializedLocation = new SerializedObject(location);
            serializedLocation.FindProperty("settlementDefinition").objectReferenceValue = definition;
            serializedLocation.FindProperty("discoverRadius").floatValue = 12f;
            serializedLocation.FindProperty("autoDiscoverOnProximity").boolValue = true;
            serializedLocation.ApplyModifiedPropertiesWithoutUndo();

            EnsureContractBoard(root.transform, location, contractBoardObjectName, new Vector3(0f, 0.5f, 0f));
            EnsureMarkerCube(root.transform, "CCS_SettlementMarker", markerColor, new Vector3(0f, 0.5f, -2f));
            EditorUtility.SetDirty(root);
        }

        private static void EnsureContractBoard(
            Transform parent,
            CCS_SettlementLocation location,
            string objectName,
            Vector3 localPosition)
        {
            Transform existing = parent.Find(objectName);
            GameObject boardObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (existing == null)
            {
                boardObject.name = objectName;
                boardObject.transform.SetParent(parent, false);
            }

            boardObject.transform.localPosition = localPosition;
            boardObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
            Collider collider = boardObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            Rigidbody rigidbody = boardObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            CCS_SettlementServicePoint servicePoint = boardObject.GetComponent<CCS_SettlementServicePoint>();
            if (servicePoint == null)
            {
                servicePoint = boardObject.AddComponent<CCS_SettlementServicePoint>();
            }

            SerializedObject serialized = new SerializedObject(servicePoint);
            serialized.FindProperty("servicePointId").stringValue =
                CCS_SettlementContentIds.ContractBoardServicePointId + "." + parent.name;
            serialized.FindProperty("servicePointType").enumValueIndex = (int)CCS_SettlementServicePointType.ContractBoard;
            serialized.FindProperty("settlementLocation").objectReferenceValue = location;
            serialized.FindProperty("isAvailable").boolValue = true;
            serialized.FindProperty("routeOverride").enumValueIndex = (int)CCS_SettlementServiceRouteType.ContractBoard;
            serialized.FindProperty("interactionDistance").floatValue = 3f;
            serialized.FindProperty("interactionDisplayNameOverride").stringValue = "Contract Board";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(boardObject);
        }

        private static void EnsureMarkerCube(Transform parent, string objectName, Color color, Vector3 localPosition)
        {
            Transform existing = parent.Find(objectName);
            GameObject marker = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (existing == null)
            {
                marker.name = objectName;
                marker.transform.SetParent(parent, false);
            }

            marker.transform.localPosition = localPosition;
            marker.transform.localScale = new Vector3(2f, 1.2f, 2f);
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = color;
            }

            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Rigidbody rigidbody = marker.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }
        }

        private static void EnsurePlaytestMultiSettlementSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.multisettlement.discover.pine", "Discover Pine Ridge Camp", CCS_PlaytestStepType.DiscoverPineRidgeCampSettlement);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.discover.broken", "Discover Broken Creek Farmstead", CCS_PlaytestStepType.DiscoverBrokenCreekFarmsteadSettlement);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.discover.iron", "Discover Iron Ridge Mining Camp", CCS_PlaytestStepType.DiscoverIronRidgeMiningCampSettlement);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.accept", "Accept regional contract", CCS_PlaytestStepType.AcceptMultiSettlementRegionalContract);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.complete", "Complete regional contract", CCS_PlaytestStepType.CompleteMultiSettlementRegionalContract);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.prosperity", "Verify settlement prosperity changed", CCS_PlaytestStepType.VerifyMultiSettlementProsperityChanged);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.reputation", "Verify settlement reputation changed", CCS_PlaytestStepType.VerifyMultiSettlementReputationChanged);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.save", "Save multi-settlement state", CCS_PlaytestStepType.SaveMultiSettlementState);
            InsertStep(profile, "ccs.survival.playtest.multisettlement.load", "Verify multi-settlement after load", CCS_PlaytestStepType.VerifyMultiSettlementAfterLoad);
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
                $"Multi-settlement playtest: {displayName}. Ctrl+Shift+N shortcut available.";
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
