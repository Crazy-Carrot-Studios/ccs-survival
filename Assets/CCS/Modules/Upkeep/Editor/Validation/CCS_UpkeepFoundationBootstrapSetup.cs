using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Upkeep;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UpkeepFoundationBootstrapSetup
// CATEGORY: Modules / Upkeep / Editor / Validation
// PURPOSE: Batch-creates upkeep content, profile wiring, playtest steps, and bootstrap hooks.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 — tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep.Editor
{
    public static class CCS_UpkeepFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_UpkeepFoundationBootstrapSetup]";
        private const string UpkeepMilestoneVersion = "2.5.0";
        private const string UpkeepContentRoot = "Assets/CCS/Survival/Content/Upkeep";
        private const string UpkeepProfileRoot = "Assets/CCS/Survival/Profiles/Upkeep";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_UpkeepDefinition homesteadTax = EnsureFrontierHomesteadClaimTaxDefinition();
            CCS_UpkeepProfile upkeepProfile = EnsureUpkeepProfile(homesteadTax);
            AssignUpkeepProfileToBootstrapHost(upkeepProfile);
            EnsurePlaytestUpkeepSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Upkeep foundation bootstrap setup complete ({UpkeepMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder(UpkeepContentRoot);
            EnsureFolder(UpkeepProfileRoot);
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

        private static CCS_UpkeepDefinition EnsureFrontierHomesteadClaimTaxDefinition()
        {
            CCS_UpkeepDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_UpkeepDefinition>(
                CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_UpkeepDefinition>();
                AssetDatabase.CreateAsset(definition, CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("upkeepDefinitionId").stringValue =
                CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Frontier Homestead Claim Tax";
            serialized.FindProperty("targetType").enumValueIndex = (int)CCS_UpkeepTargetType.LandClaim;
            serialized.FindProperty("currencyId").stringValue = CCS_UpkeepContentIds.TradeDollarsCurrencyId;
            serialized.FindProperty("amount").intValue = 25;
            serialized.FindProperty("intervalDaysPlaceholder").intValue = 7;
            serialized.FindProperty("gracePeriodDaysPlaceholder").intValue = 3;
            serialized.FindProperty("autoPayFromBank").boolValue = true;
            serialized.FindProperty("autoPayFromWallet").boolValue = true;
            serialized.FindProperty("enabled").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_UpkeepProfile EnsureUpkeepProfile(CCS_UpkeepDefinition homesteadTax)
        {
            CCS_UpkeepProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_UpkeepProfile>(CCS_UpkeepContentIds.DefaultUpkeepProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_UpkeepProfile>();
                AssetDatabase.CreateAsset(profile, CCS_UpkeepContentIds.DefaultUpkeepProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.upkeep.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Upkeep Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier recurring costs for land claims and future owned assets.";
            serialized.FindProperty("profileVersion").stringValue = UpkeepMilestoneVersion;
            serialized.FindProperty("defaultLandClaimUpkeepDefinitionId").stringValue =
                CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionId;
            SerializedProperty definitions = serialized.FindProperty("upkeepDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = homesteadTax;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignUpkeepProfileToBootstrapHost(CCS_UpkeepProfile profile)
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
            serialized.FindProperty("upkeepProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestUpkeepSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.register",
                "Register upkeep for land claim",
                CCS_PlaytestStepType.RegisterUpkeepForLandClaim,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.force.due",
                "Force upkeep due",
                CCS_PlaytestStepType.ForceUpkeepDue,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.pay.bank",
                "Pay upkeep from bank",
                CCS_PlaytestStepType.PayUpkeepFromBank,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.verify.bank",
                "Verify bank balance decreased after upkeep",
                CCS_PlaytestStepType.VerifyUpkeepBankPayment,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.force.due.again",
                "Force upkeep due again",
                CCS_PlaytestStepType.ForceUpkeepDueAgain,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.prepare.wallet",
                "Empty bank or prepare wallet for upkeep",
                CCS_PlaytestStepType.PrepareWalletUpkeepPayment,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.pay.wallet",
                "Pay upkeep from wallet",
                CCS_PlaytestStepType.PayUpkeepFromWallet,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.save",
                "Save upkeep state",
                CCS_PlaytestStepType.SaveUpkeepState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.upkeep.verify.load",
                "Verify upkeep restored after load",
                CCS_PlaytestStepType.VerifyUpkeepAfterLoad,
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
                $"Upkeep playtest: {displayName}. Ctrl+Shift+U shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(UpkeepMilestoneVersion);
        }
    }
}
