using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationFoundationBootstrapSetup
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Batch-creates reputation content, profile wiring, playtest steps, and version bump.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 — reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public static class CCS_ReputationFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_ReputationFoundationBootstrapSetup]";
        private const string ReputationMilestoneVersion = "2.7.0";
        private const string ReputationContentRoot = "Assets/CCS/Survival/Content/Reputation";
        private const string ReputationProfileRoot = "Assets/CCS/Survival/Profiles/Reputation";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ReputationDefinition tradingPostReputation = EnsureFrontierTradingPostReputationDefinition();
            CCS_ReputationProfile reputationProfile = EnsureReputationProfile(tradingPostReputation);
            AssignReputationProfileToBootstrapHost(reputationProfile);
            EnsurePlaytestReputationSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Reputation foundation bootstrap setup complete ({ReputationMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ReputationContentRoot);
            EnsureFolder(ReputationProfileRoot);
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

        private static CCS_ReputationDefinition EnsureFrontierTradingPostReputationDefinition()
        {
            CCS_ReputationDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ReputationDefinition>(
                CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ReputationDefinition>();
                AssetDatabase.CreateAsset(definition, CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("reputationDefinitionId").stringValue =
                CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Frontier Trading Post Trust";
            serialized.FindProperty("scopeType").enumValueIndex = (int)CCS_ReputationScopeType.Settlement;
            serialized.FindProperty("targetId").stringValue = CCS_ReputationContentIds.DefaultTradingPostSettlementId;
            serialized.FindProperty("minValue").intValue = -100;
            serialized.FindProperty("maxValue").intValue = 100;
            serialized.FindProperty("defaultValue").intValue = 0;
            serialized.FindProperty("enabled").boolValue = true;
            serialized.FindProperty("futureHookPlaceholder").stringValue = "law.discount.access.placeholder";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ReputationProfile EnsureReputationProfile(CCS_ReputationDefinition tradingPostReputation)
        {
            CCS_ReputationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ReputationProfile>(CCS_ReputationContentIds.DefaultReputationProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_ReputationProfile>();
                AssetDatabase.CreateAsset(profile, CCS_ReputationContentIds.DefaultReputationProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.reputation.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Reputation Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier settlement trust for trade, obligations, and future service access hooks.";
            serialized.FindProperty("profileVersion").stringValue = ReputationMilestoneVersion;
            serialized.FindProperty("defaultSettlementReputationDefinitionId").stringValue =
                CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionId;
            serialized.FindProperty("defaultTradingPostSettlementId").stringValue =
                CCS_ReputationContentIds.DefaultTradingPostSettlementId;
            SerializedProperty definitions = serialized.FindProperty("reputationDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = tradingPostReputation;
            serialized.FindProperty("enableGoodsSoldEvents").boolValue = true;
            serialized.FindProperty("enableLoanRepaidEvents").boolValue = true;
            serialized.FindProperty("enableUpkeepPaidEvents").boolValue = true;
            serialized.FindProperty("enableFailedUpkeepEvents").boolValue = true;
            serialized.FindProperty("enableSettlementDiscoveredEvents").boolValue = true;
            serialized.FindProperty("goodsSoldDelta").intValue = 2;
            serialized.FindProperty("loanRepaidDelta").intValue = 3;
            serialized.FindProperty("upkeepPaidDelta").intValue = 2;
            serialized.FindProperty("failedUpkeepDelta").intValue = -1;
            serialized.FindProperty("settlementDiscoveredDelta").intValue = 1;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignReputationProfileToBootstrapHost(CCS_ReputationProfile profile)
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
            serialized.FindProperty("reputationProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestReputationSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.sell",
                "Verify trading post reputation increased after sell",
                CCS_PlaytestStepType.VerifyTradingPostReputationAfterSell,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.obligation",
                "Verify reputation changed after loan or upkeep obligation",
                CCS_PlaytestStepType.VerifyReputationChangedAfterObligation,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.save",
                "Save reputation state",
                CCS_PlaytestStepType.SaveReputationState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.load",
                "Verify reputation restored after load",
                CCS_PlaytestStepType.VerifyReputationAfterLoad,
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
                $"Reputation playtest: {displayName}. Ctrl+Shift+T shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(ReputationMilestoneVersion);
        }
    }
}
