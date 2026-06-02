using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldSimulationBootstrapSetup
// CATEGORY: Modules / WorldSimulation / Editor / Validation
// PURPOSE: Creates world simulation profile, bootstrap wiring, and playtest steps.
// PLACEMENT: Batch entry for milestone 2.0.0 frontier world simulation foundation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific content under Assets/CCS/Survival/.
// =============================================================================

namespace CCS.Modules.WorldSimulation.Editor
{
    public static class CCS_WorldSimulationBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/WorldSimulation";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultWorldSimulationProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string LogPrefix = "[CCS_WorldSimulationBootstrapSetup]";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_WorldSimulationProfile profile = EnsureWorldSimulationProfile();
            EnsureBootstrapGameplayServiceHost(profile);
            EnsurePlaytestWorldSimulationSteps();

            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier world simulation bootstrap complete (2.0.0).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            if (AssetDatabase.IsValidFolder(ProfilesRoot))
            {
                return;
            }

            const string survivalProfilesRoot = "Assets/CCS/Survival/Profiles";
            if (!AssetDatabase.IsValidFolder(survivalProfilesRoot))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival", "Profiles");
            }

            AssetDatabase.CreateFolder(survivalProfilesRoot, "WorldSimulation");
        }

        private static CCS_WorldSimulationProfile EnsureWorldSimulationProfile()
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WorldSimulationProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_WorldSimulationContentIds.DefaultProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default World Simulation Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement and region simulation defaults for bootstrap trading post.";
            serialized.FindProperty("profileVersion").stringValue = "2.0.0";

            SerializedProperty settlementEntries = serialized.FindProperty("settlementEntries");
            settlementEntries.arraySize = 1;
            SerializedProperty settlementEntry = settlementEntries.GetArrayElementAtIndex(0);
            settlementEntry.FindPropertyRelative("settlementId").stringValue =
                CCS_WorldSimulationContentIds.TradingPostSettlementId;
            settlementEntry.FindPropertyRelative("population").intValue = 48;
            SetSupplyEntries(
                settlementEntry.FindPropertyRelative("supplies"),
                new[]
                {
                    CreateSupply((int)CCS_SettlementSupplyType.Food, 12f, 40f),
                    CreateSupply((int)CCS_SettlementSupplyType.Water, 20f, 40f),
                    CreateSupply((int)CCS_SettlementSupplyType.Fuel, 8f, 30f),
                    CreateSupply((int)CCS_SettlementSupplyType.BuildingMaterials, 10f, 35f),
                    CreateSupply((int)CCS_SettlementSupplyType.IndustrialMaterials, 6f, 30f),
                    CreateSupply((int)CCS_SettlementSupplyType.Tools, 4f, 20f),
                    CreateSupply((int)CCS_SettlementSupplyType.TradeGoods, 15f, 45f)
                });
            SetDemandEntries(
                settlementEntry.FindPropertyRelative("demands"),
                new[]
                {
                    CreateDemand((int)CCS_SettlementSupplyType.Food, 18f),
                    CreateDemand((int)CCS_SettlementSupplyType.IndustrialMaterials, 12f),
                    CreateDemand((int)CCS_SettlementSupplyType.TradeGoods, 10f)
                });
            SetProductionEntries(
                settlementEntry.FindPropertyRelative("productions"),
                new[]
                {
                    CreateProduction((int)CCS_SettlementSupplyType.Food, 8f),
                    CreateProduction((int)CCS_SettlementSupplyType.IndustrialMaterials, 5f),
                    CreateProduction((int)CCS_SettlementSupplyType.TradeGoods, 12f)
                });

            SerializedProperty regionEntries = serialized.FindProperty("regionEntries");
            regionEntries.arraySize = 4;
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(0),
                CCS_RegionContentIds.PineRidgeForestRegionId,
                0.85f,
                0.75f,
                0.15f,
                0.20f);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(1),
                CCS_RegionContentIds.BrokenCreekRegionId,
                0.70f,
                0.65f,
                0.10f,
                0.15f);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(2),
                CCS_RegionContentIds.IronRidgeMineRegionId,
                0.20f,
                0.15f,
                0.90f,
                0.75f);
            SetRegionEntry(
                regionEntries.GetArrayElementAtIndex(3),
                CCS_RegionContentIds.FrontierTradingPostRegionId,
                0.35f,
                0.25f,
                0.30f,
                0.80f);

            SerializedProperty vendorRoutes = serialized.FindProperty("vendorRoutes");
            vendorRoutes.arraySize = 1;
            SerializedProperty vendorRoute = vendorRoutes.GetArrayElementAtIndex(0);
            vendorRoute.FindPropertyRelative("vendorId").stringValue = CCS_WorldSimulationContentIds.GeneralStoreVendorId;
            vendorRoute.FindPropertyRelative("settlementId").stringValue =
                CCS_WorldSimulationContentIds.TradingPostSettlementId;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost(CCS_WorldSimulationProfile profile)
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
            serializedHost.FindProperty("worldSimulationProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestWorldSimulationSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.discover.settlement",
                "Discover settlement for world simulation",
                CCS_PlaytestStepType.DiscoverSettlementForWorldSimulation,
                "Discover the frontier trading post settlement.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.sell.food",
                "Sell food for world simulation",
                CCS_PlaytestStepType.SellFoodForWorldSimulation,
                "Sell fish, meat, or preserved food at the general store.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.verify.food",
                "Verify food supply increased",
                CCS_PlaytestStepType.VerifyFoodSupplyIncreased,
                "Confirm trading post food supply increased after selling food.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.sell.industry",
                "Sell industry goods for world simulation",
                CCS_PlaytestStepType.SellIndustryGoodsForWorldSimulation,
                "Sell ore, nails, or refined iron at the general store.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.verify.industry",
                "Verify industry supply increased",
                CCS_PlaytestStepType.VerifyIndustrySupplyIncreased,
                "Confirm trading post industrial supply increased after selling goods.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.verify.prosperity",
                "Verify prosperity increased",
                CCS_PlaytestStepType.VerifyProsperityIncreased,
                "Confirm trading post prosperity increased after supply changes.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.save",
                "Save world simulation state",
                CCS_PlaytestStepType.SaveWorldSimulationState,
                "Press F5 after world simulation supply changes.");
            InsertStep(
                profile,
                "ccs.survival.playtest.worldsim.load.verify",
                "Verify world simulation restored after load",
                CCS_PlaytestStepType.VerifyWorldSimulationRestoredAfterLoad,
                "Press F9 and confirm settlement simulation supply and prosperity persist.");
            EditorUtility.SetDirty(profile);
        }

        private static void SetRegionEntry(
            SerializedProperty entry,
            string regionId,
            float foodPotential,
            float wildlifePotential,
            float miningPotential,
            float industryPotential)
        {
            entry.FindPropertyRelative("regionId").stringValue = regionId;
            entry.FindPropertyRelative("foodPotential").floatValue = foodPotential;
            entry.FindPropertyRelative("wildlifePotential").floatValue = wildlifePotential;
            entry.FindPropertyRelative("miningPotential").floatValue = miningPotential;
            entry.FindPropertyRelative("industryPotential").floatValue = industryPotential;
        }

        private static (int supplyType, float currentAmount, float desiredAmount) CreateSupply(
            int supplyType,
            float currentAmount,
            float desiredAmount)
        {
            return (supplyType, currentAmount, desiredAmount);
        }

        private static (int supplyType, float currentDemand) CreateDemand(int supplyType, float currentDemand)
        {
            return (supplyType, currentDemand);
        }

        private static (int supplyType, float currentProduction) CreateProduction(
            int supplyType,
            float currentProduction)
        {
            return (supplyType, currentProduction);
        }

        private static void SetSupplyEntries(
            SerializedProperty property,
            (int supplyType, float currentAmount, float desiredAmount)[] values)
        {
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("supplyType").intValue = values[index].supplyType;
                entry.FindPropertyRelative("currentAmount").floatValue = values[index].currentAmount;
                entry.FindPropertyRelative("desiredAmount").floatValue = values[index].desiredAmount;
            }
        }

        private static void SetDemandEntries(
            SerializedProperty property,
            (int supplyType, float currentDemand)[] values)
        {
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("supplyType").intValue = values[index].supplyType;
                entry.FindPropertyRelative("currentDemand").floatValue = values[index].currentDemand;
            }
        }

        private static void SetProductionEntries(
            SerializedProperty property,
            (int supplyType, float currentProduction)[] values)
        {
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("supplyType").intValue = values[index].supplyType;
                entry.FindPropertyRelative("currentProduction").floatValue = values[index].currentProduction;
            }
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
    }
}
