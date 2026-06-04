using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_ContractsFoundationBootstrapSetup
// CATEGORY: Modules / Contracts / Editor / Validation
// PURPOSE: Batch-creates contract content, profile wiring, contract board, and playtest steps.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts.Editor
{
    public static class CCS_ContractsFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_ContractsFoundationBootstrapSetup]";
        private const string ContractsMilestoneVersion = "3.0.0";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string SceneRootName = "CCS_BuildVerificationScene";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ContractDefinition lumber = EnsureSingleItemContract(
                CCS_ContractContentIds.LumberDeliveryContractPath,
                CCS_ContractContentIds.LumberDeliveryContractId,
                "Lumber Delivery",
                CCS_ContractType.GeneralStoreSupply,
                CCS_ContractContentIds.LumberItemId,
                3,
                15,
                2,
                1f,
                CCS_SettlementSupplyType.TradeGoods,
                1f);
            CCS_ContractDefinition corn = EnsureSingleItemContract(
                CCS_ContractContentIds.CornDeliveryContractPath,
                CCS_ContractContentIds.CornDeliveryContractId,
                "Corn Delivery",
                CCS_ContractType.GeneralStoreSupply,
                CCS_ContractContentIds.CornItemId,
                5,
                12,
                2,
                1f,
                CCS_SettlementSupplyType.Food,
                1f);
            CCS_ContractDefinition potato = EnsureSingleItemContract(
                CCS_ContractContentIds.PotatoDeliveryContractPath,
                CCS_ContractContentIds.PotatoDeliveryContractId,
                "Potato Delivery",
                CCS_ContractType.GeneralStoreSupply,
                CCS_ContractContentIds.PotatoItemId,
                5,
                10,
                2,
                1f,
                CCS_SettlementSupplyType.Food,
                1f);
            CCS_ContractDefinition feed = EnsureSingleItemContract(
                CCS_ContractContentIds.FeedDeliveryContractPath,
                CCS_ContractContentIds.FeedDeliveryContractId,
                "Feed Delivery",
                CCS_ContractType.StableSupply,
                CCS_ContractContentIds.FeedItemId,
                3,
                8,
                1,
                1f,
                CCS_SettlementSupplyType.Food,
                0.5f);
            CCS_ContractDefinition milk = EnsureSingleItemContract(
                CCS_ContractContentIds.MilkDeliveryContractPath,
                CCS_ContractContentIds.MilkDeliveryContractId,
                "Milk Delivery",
                CCS_ContractType.StableSupply,
                CCS_ContractContentIds.MilkItemId,
                2,
                10,
                1,
                1f,
                CCS_SettlementSupplyType.Food,
                0.5f);
            CCS_ContractDefinition ironOre = EnsureSingleItemContract(
                CCS_ContractContentIds.IronOreDeliveryContractPath,
                CCS_ContractContentIds.IronOreDeliveryContractId,
                "Iron Ore Delivery",
                CCS_ContractType.GunsmithSupply,
                CCS_ContractContentIds.IronOreItemId,
                3,
                18,
                2,
                1f,
                CCS_SettlementSupplyType.IndustrialMaterials,
                1f);
            CCS_ContractDefinition refinedIron = EnsureSingleItemContract(
                CCS_ContractContentIds.RefinedIronDeliveryContractPath,
                CCS_ContractContentIds.RefinedIronDeliveryContractId,
                "Refined Iron Delivery",
                CCS_ContractType.GunsmithSupply,
                CCS_ContractContentIds.RefinedIronItemId,
                2,
                20,
                2,
                1f,
                CCS_SettlementSupplyType.IndustrialMaterials,
                1f);
            CCS_ContractDefinition charcoal = EnsureSingleItemContract(
                CCS_ContractContentIds.CharcoalDeliveryContractPath,
                CCS_ContractContentIds.CharcoalDeliveryContractId,
                "Charcoal Delivery",
                CCS_ContractType.GunsmithSupply,
                CCS_ContractContentIds.CharcoalItemId,
                3,
                15,
                2,
                1f,
                CCS_SettlementSupplyType.IndustrialMaterials,
                1f);
            CCS_ContractDefinition mixed = EnsureMixedFrontierContract();

            CCS_ContractProfile profile = EnsureContractProfile(
                lumber,
                corn,
                potato,
                feed,
                milk,
                ironOre,
                refinedIron,
                charcoal,
                mixed);
            AssignContractsProfileToBootstrapHost(profile);
            EnsureContractBoardInScene();
            EnsurePlaytestContractSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Contracts foundation bootstrap setup complete ({ContractsMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(CCS_ContractContentIds.ContractsContentRoot);
            EnsureFolder(CCS_ContractContentIds.ContractsProfileRoot);
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

        private static CCS_ContractDefinition EnsureSingleItemContract(
            string assetPath,
            string contractId,
            string displayName,
            CCS_ContractType contractType,
            string itemId,
            int quantity,
            int tradeDollars,
            int reputationGain,
            float prosperityGain,
            CCS_SettlementSupplyType supplyType,
            float supplyAmount)
        {
            CCS_ContractDefinition definition = LoadOrCreateDefinition(assetPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("contractId").stringValue = contractId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("contractType").enumValueIndex = (int)contractType;
            serialized.FindProperty("settlementId").stringValue =
                CCS_ContractContentIds.DefaultTradingPostSettlementId;
            serialized.FindProperty("enabled").boolValue = true;

            SerializedProperty requirements = serialized.FindProperty("requirements");
            requirements.arraySize = 1;
            SerializedProperty requirement = requirements.GetArrayElementAtIndex(0);
            requirement.FindPropertyRelative("itemId").stringValue = itemId;
            requirement.FindPropertyRelative("quantity").intValue = quantity;
            requirement.FindPropertyRelative("settlementIdRestriction").stringValue = string.Empty;

            ApplyReward(serialized.FindProperty("reward"), tradeDollars, reputationGain, prosperityGain, supplyType, supplyAmount);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ContractDefinition EnsureMixedFrontierContract()
        {
            CCS_ContractDefinition definition = LoadOrCreateDefinition(
                CCS_ContractContentIds.MixedFrontierSupplyContractPath);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("contractId").stringValue =
                CCS_ContractContentIds.MixedFrontierSupplyContractId;
            serialized.FindProperty("displayName").stringValue = "Mixed Frontier Supply";
            serialized.FindProperty("contractType").enumValueIndex = (int)CCS_ContractType.TradingPostSupply;
            serialized.FindProperty("settlementId").stringValue =
                CCS_ContractContentIds.DefaultTradingPostSettlementId;
            serialized.FindProperty("enabled").boolValue = true;

            SerializedProperty requirements = serialized.FindProperty("requirements");
            requirements.arraySize = 2;
            SerializedProperty hideRequirement = requirements.GetArrayElementAtIndex(0);
            hideRequirement.FindPropertyRelative("itemId").stringValue = CCS_ContractContentIds.HideItemId;
            hideRequirement.FindPropertyRelative("quantity").intValue = 3;
            hideRequirement.FindPropertyRelative("settlementIdRestriction").stringValue = string.Empty;
            SerializedProperty cordageRequirement = requirements.GetArrayElementAtIndex(1);
            cordageRequirement.FindPropertyRelative("itemId").stringValue = CCS_ContractContentIds.CordageItemId;
            cordageRequirement.FindPropertyRelative("quantity").intValue = 2;
            cordageRequirement.FindPropertyRelative("settlementIdRestriction").stringValue = string.Empty;

            ApplyReward(
                serialized.FindProperty("reward"),
                25,
                3,
                1.5f,
                CCS_SettlementSupplyType.TradeGoods,
                2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ContractDefinition LoadOrCreateDefinition(string assetPath)
        {
            CCS_ContractDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ContractDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            return definition;
        }

        private static void ApplyReward(
            SerializedProperty reward,
            int tradeDollars,
            int reputationGain,
            float prosperityGain,
            CCS_SettlementSupplyType supplyType,
            float supplyAmount)
        {
            reward.FindPropertyRelative("tradeDollars").intValue = tradeDollars;
            reward.FindPropertyRelative("reputationGain").intValue = reputationGain;
            reward.FindPropertyRelative("prosperityGain").floatValue = prosperityGain;
            reward.FindPropertyRelative("supplyType").enumValueIndex = (int)supplyType;
            reward.FindPropertyRelative("supplyAmount").floatValue = supplyAmount;
        }

        private static CCS_ContractProfile EnsureContractProfile(params CCS_ContractDefinition[] definitions)
        {
            CCS_ContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(
                CCS_ContractContentIds.DefaultContractProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_ContractProfile>();
                AssetDatabase.CreateAsset(profile, CCS_ContractContentIds.DefaultContractProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_ContractContentIds.DefaultContractProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Contract Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement delivery contracts for general store, stable, gunsmith, and trading post boards.";
            serialized.FindProperty("profileVersion").stringValue = ContractsMilestoneVersion;
            serialized.FindProperty("defaultSettlementId").stringValue =
                CCS_ContractContentIds.DefaultTradingPostSettlementId;
            serialized.FindProperty("defaultCurrencyId").stringValue =
                CCS_ContractContentIds.TradeDollarsCurrencyId;
            serialized.FindProperty("enableDebugLogging").boolValue = true;

            SerializedProperty contractDefinitions = serialized.FindProperty("contractDefinitions");
            contractDefinitions.arraySize = definitions.Length;
            for (int index = 0; index < definitions.Length; index++)
            {
                contractDefinitions.GetArrayElementAtIndex(index).objectReferenceValue = definitions[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignContractsProfileToBootstrapHost(CCS_ContractProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapRootPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabRoot != null
                ? prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>()
                : null;
            if (host == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("contractsProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureContractBoardInScene()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject sceneRoot = GameObject.Find(SceneRootName);
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing scene root {SceneRootName}.");
                EditorApplication.Exit(1);
                return;
            }

            Transform tradingPostRoot = sceneRoot.transform.Find(CCS_SettlementContentIds.TestTradingPostObjectName);
            if (tradingPostRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing trading post root. Run settlement bootstrap first.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SettlementLocation location = tradingPostRoot.GetComponent<CCS_SettlementLocation>();
            if (location == null)
            {
                Debug.LogError($"{LogPrefix} Trading post missing CCS_SettlementLocation.");
                EditorApplication.Exit(1);
                return;
            }

            EnsureContractBoardServicePoint(
                tradingPostRoot,
                location,
                CCS_SettlementContentIds.TestTradingPostContractBoardObjectName,
                CCS_SettlementContentIds.ContractBoardServicePointId,
                new Vector3(12f, 0.5f, 0f));

            EditorUtility.SetDirty(tradingPostRoot.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureContractBoardServicePoint(
            Transform parent,
            CCS_SettlementLocation settlementLocation,
            string objectName,
            string servicePointId,
            Vector3 localPosition)
        {
            Transform existing = parent.Find(objectName);
            GameObject serviceObject;
            if (existing != null)
            {
                serviceObject = existing.gameObject;
            }
            else
            {
                serviceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                serviceObject.name = objectName;
                serviceObject.transform.SetParent(parent, false);
            }

            serviceObject.transform.localPosition = localPosition;
            serviceObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);

            Rigidbody rigidbody = serviceObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = serviceObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            CCS_SettlementServicePoint servicePoint = serviceObject.GetComponent<CCS_SettlementServicePoint>();
            if (servicePoint == null)
            {
                servicePoint = serviceObject.AddComponent<CCS_SettlementServicePoint>();
            }

            SerializedObject serialized = new SerializedObject(servicePoint);
            serialized.FindProperty("servicePointId").stringValue = servicePointId;
            serialized.FindProperty("servicePointType").enumValueIndex =
                (int)CCS_SettlementServicePointType.ContractBoard;
            serialized.FindProperty("settlementLocation").objectReferenceValue = settlementLocation;
            serialized.FindProperty("vendorDefinition").objectReferenceValue = null;
            serialized.FindProperty("placeholderMessage").stringValue = string.Empty;
            serialized.FindProperty("isAvailable").boolValue = true;
            serialized.FindProperty("unavailableReason").stringValue = string.Empty;
            serialized.FindProperty("requiredSettlementDiscovered").boolValue = false;
            serialized.FindProperty("requiredCampTier").intValue = -1;
            serialized.FindProperty("routeOverride").enumValueIndex =
                (int)CCS_SettlementServiceRouteType.ContractBoard;
            serialized.FindProperty("interactionDistance").floatValue = 3f;
            serialized.FindProperty("interactionDisplayNameOverride").stringValue = "Contract Board";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serviceObject);
        }

        private static void EnsurePlaytestContractSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.discover",
                "Discover trading post for contracts",
                CCS_PlaytestStepType.DiscoverTradingPostForContracts,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.interact.board",
                "Interact with contract board",
                CCS_PlaytestStepType.InteractContractBoard,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.accept",
                "Accept frontier contract",
                CCS_PlaytestStepType.AcceptFrontierContract,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.gather",
                "Gather contract delivery goods",
                CCS_PlaytestStepType.GatherContractGoods,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.complete",
                "Complete frontier contract",
                CCS_PlaytestStepType.CompleteFrontierContract,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.verify.money",
                "Verify contract money reward",
                CCS_PlaytestStepType.VerifyContractMoneyReward,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.verify.reputation",
                "Verify contract reputation reward",
                CCS_PlaytestStepType.VerifyContractReputationReward,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.verify.prosperity",
                "Verify contract prosperity reward",
                CCS_PlaytestStepType.VerifyContractProsperityReward,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.save",
                "Save contract state",
                CCS_PlaytestStepType.SaveContractState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.contracts.verify.load",
                "Verify contract state after load",
                CCS_PlaytestStepType.VerifyContractStateAfterLoad,
                string.Empty);
            EditorUtility.SetDirty(profile);
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string targetItemId)
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
                $"Contracts playtest: {displayName}. Ctrl+Shift+C shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(ContractsMilestoneVersion);
        }
    }
}
