using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Modules.Settlements;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ServiceAccessFoundationBootstrapSetup
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Batch-creates service access content, price modifier wiring, playtest steps, version bump.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public static class CCS_ServiceAccessFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_ServiceAccessFoundationBootstrapSetup]";
        private const string ServiceAccessMilestoneVersion = "2.8.0";
        private const string AccessContentRoot = "Assets/CCS/Survival/Content/Reputation/Access";
        private const string ReputationProfileRoot = "Assets/CCS/Survival/Profiles/Reputation";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ServiceAccessRule blacksmithRule = EnsureBlacksmithAdvancedAccessRule();
            CCS_ServiceAccessProfile accessProfile = EnsureServiceAccessProfile(blacksmithRule);
            CCS_ReputationProfile reputationProfile = EnsureReputationProfile(accessProfile);
            AssignReputationProfileToBootstrapHost(reputationProfile);
            EnsurePlaytestServiceAccessSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Service access foundation bootstrap setup complete ({ServiceAccessMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(AccessContentRoot);
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

        private static CCS_ServiceAccessRule EnsureBlacksmithAdvancedAccessRule()
        {
            CCS_ServiceAccessRule rule = AssetDatabase.LoadAssetAtPath<CCS_ServiceAccessRule>(
                CCS_ReputationContentIds.BlacksmithAdvancedAccessRulePath);
            if (rule == null)
            {
                rule = ScriptableObject.CreateInstance<CCS_ServiceAccessRule>();
                AssetDatabase.CreateAsset(rule, CCS_ReputationContentIds.BlacksmithAdvancedAccessRulePath);
            }

            SerializedObject serialized = new SerializedObject(rule);
            serialized.FindProperty("ruleId").stringValue = CCS_ReputationContentIds.BlacksmithAdvancedAccessRuleId;
            serialized.FindProperty("settlementId").stringValue =
                CCS_ReputationContentIds.DefaultTradingPostSettlementId;
            serialized.FindProperty("servicePointId").stringValue = string.Empty;
            serialized.FindProperty("servicePointTypeFilter").intValue =
                (int)CCS_SettlementServicePointType.Blacksmith;
            SerializedProperty requirement = serialized.FindProperty("requirement");
            requirement.FindPropertyRelative("enabled").boolValue = true;
            requirement.FindPropertyRelative("minimumReputationTier").enumValueIndex =
                (int)CCS_ReputationTier.Trusted;
            requirement.FindPropertyRelative("minimumReputationValue").intValue = 0;
            requirement.FindPropertyRelative("requireDiscoveredSettlement").boolValue = false;
            requirement.FindPropertyRelative("requiredCampTier").intValue = -1;
            requirement.FindPropertyRelative("requireLandClaim").boolValue = false;
            requirement.FindPropertyRelative("futureHookPlaceholder").stringValue =
                "service.access.blacksmith.advanced";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(rule);
            return rule;
        }

        private static CCS_ServiceAccessProfile EnsureServiceAccessProfile(CCS_ServiceAccessRule blacksmithRule)
        {
            CCS_ServiceAccessProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ServiceAccessProfile>(
                CCS_ReputationContentIds.DefaultServiceAccessProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_ServiceAccessProfile>();
                AssetDatabase.CreateAsset(profile, CCS_ReputationContentIds.DefaultServiceAccessProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_ReputationContentIds.DefaultServiceAccessProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Service Access Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Settlement service access rules driven by frontier trust standing.";
            serialized.FindProperty("profileVersion").stringValue = ServiceAccessMilestoneVersion;
            SerializedProperty rules = serialized.FindProperty("serviceAccessRules");
            rules.arraySize = 1;
            rules.GetArrayElementAtIndex(0).objectReferenceValue = blacksmithRule;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_ReputationProfile EnsureReputationProfile(CCS_ServiceAccessProfile accessProfile)
        {
            CCS_ReputationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ReputationProfile>(CCS_ReputationContentIds.DefaultReputationProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Default reputation profile missing. Run reputation foundation bootstrap first.");
                EditorApplication.Exit(1);
                return null;
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = ServiceAccessMilestoneVersion;
            serialized.FindProperty("serviceAccessProfile").objectReferenceValue = accessProfile;
            serialized.FindProperty("enableBuyPriceModifiers").boolValue = true;
            serialized.FindProperty("enableSellPriceModifiers").boolValue = false;
            serialized.FindProperty("neutralBuyPriceModifier").floatValue = 1f;
            serialized.FindProperty("trustedBuyPriceModifier").floatValue = 0.95f;
            serialized.FindProperty("honoredBuyPriceModifier").floatValue = 0.9f;
            serialized.FindProperty("distrustedBuyPriceModifier").floatValue = 1.1f;
            serialized.FindProperty("hostileBuyPriceModifier").floatValue = 1.25f;
            serialized.FindProperty("neutralSellPriceModifier").floatValue = 1f;
            serialized.FindProperty("trustedSellPriceModifier").floatValue = 1f;
            serialized.FindProperty("honoredSellPriceModifier").floatValue = 1f;
            serialized.FindProperty("distrustedSellPriceModifier").floatValue = 1f;
            serialized.FindProperty("hostileSellPriceModifier").floatValue = 1f;
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

        private static void EnsurePlaytestServiceAccessSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.standing",
                "Verify settlement reputation tier and value",
                CCS_PlaytestStepType.VerifySettlementReputationStanding,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.buy.modifier",
                "Verify vendor buy price reputation modifier",
                CCS_PlaytestStepType.VerifyVendorBuyPriceModifier,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.service.access",
                "Verify settlement service access check",
                CCS_PlaytestStepType.VerifySettlementServiceAccess,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.save.access",
                "Save service access and price modifier state",
                CCS_PlaytestStepType.SaveServiceAccessState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.reputation.verify.access.load",
                "Verify service access and modifier after load",
                CCS_PlaytestStepType.VerifyServiceAccessAfterLoad,
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
                $"Service access playtest: {displayName}. Ctrl+Shift+Y shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(ServiceAccessMilestoneVersion);
        }
    }
}
