using System.Collections.Generic;
using System.IO;
using CCS.Modules.Banking;
using CCS.Modules.Playtesting;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankingFoundationBootstrapSetup
// CATEGORY: Modules / Banking / Editor / Validation
// PURPOSE: Batch-creates bank account content, profile wiring, playtest steps, and settlement hooks.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 — banking and land office foundation.
// =============================================================================

namespace CCS.Modules.Banking.Editor
{
    public static class CCS_BankingFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_BankingFoundationBootstrapSetup]";
        private const string BankingMilestoneVersion = "2.4.0";
        private const string BankingProfilePath = CCS_BankingContentIds.DefaultBankProfilePath;
        private const string BankingContentRoot = "Assets/CCS/Survival/Content/Banking";
        private const string AccountsContentRoot = BankingContentRoot + "/Accounts";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_BankAccountDefinition savingsAccount = EnsureFrontierSavingsAccountDefinition();
            CCS_BankAccountProfile bankProfile = EnsureBankAccountProfile(savingsAccount);
            AssignBankAccountProfileToBootstrapHost(bankProfile);
            EnsurePlaytestBankingSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Banking foundation bootstrap setup complete ({BankingMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Banking");
            EnsureFolder(BankingContentRoot);
            EnsureFolder(AccountsContentRoot);
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

        private static CCS_BankAccountDefinition EnsureFrontierSavingsAccountDefinition()
        {
            CCS_BankAccountDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BankAccountDefinition>(
                    CCS_BankingContentIds.FrontierSavingsAccountDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_BankAccountDefinition>();
                AssetDatabase.CreateAsset(definition, CCS_BankingContentIds.FrontierSavingsAccountDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("accountDefinitionId").stringValue =
                CCS_BankingContentIds.FrontierSavingsAccountDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Frontier Savings Account";
            serialized.FindProperty("currencyId").stringValue = CCS_BankingContentIds.TradeDollarsCurrencyId;
            serialized.FindProperty("minimumDepositAmount").intValue = 1;
            serialized.FindProperty("minimumWithdrawAmount").intValue = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_BankAccountProfile EnsureBankAccountProfile(CCS_BankAccountDefinition savingsAccount)
        {
            CCS_BankAccountProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BankAccountProfile>(BankingProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_BankAccountProfile>();
                AssetDatabase.CreateAsset(profile, BankingProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.banking.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Bank Account Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier stored-currency accounts for bootstrap trading post bank services.";
            serialized.FindProperty("profileVersion").stringValue = BankingMilestoneVersion;
            serialized.FindProperty("defaultAccountDefinitionId").stringValue =
                CCS_BankingContentIds.FrontierSavingsAccountDefinitionId;
            SerializedProperty definitions = serialized.FindProperty("accountDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = savingsAccount;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AssignBankAccountProfileToBootstrapHost(CCS_BankAccountProfile profile)
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
            serialized.FindProperty("bankAccountProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestBankingSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.banking.interact.bank",
                "Interact with Bank service point",
                CCS_PlaytestStepType.InteractBankServicePoint,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.interact.landoffice",
                "Interact with Land Office service point",
                CCS_PlaytestStepType.InteractLandOfficeServicePoint,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.deposit",
                "Deposit trade dollars to bank",
                CCS_PlaytestStepType.DepositBankCurrency,
                CCS_BankingContentIds.TradeDollarsCurrencyId);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.verify.deposit",
                "Verify wallet decreases and bank balance increases",
                CCS_PlaytestStepType.VerifyBankDepositBalances,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.withdraw",
                "Withdraw trade dollars from bank",
                CCS_PlaytestStepType.WithdrawBankCurrency,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.verify.withdraw",
                "Verify wallet increases and bank balance decreases",
                CCS_PlaytestStepType.VerifyBankWithdrawBalances,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.verify.landoffice.claims",
                "Verify land office sees owned claims",
                CCS_PlaytestStepType.VerifyLandOfficeOwnedClaims,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.save",
                "Save bank balance",
                CCS_PlaytestStepType.SaveBankState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.verify.load",
                "Verify bank balance restored after load",
                CCS_PlaytestStepType.VerifyBankBalanceAfterLoad,
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
                $"Banking playtest: {displayName}. Ctrl+Shift+B shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(BankingMilestoneVersion);
        }
    }
}
