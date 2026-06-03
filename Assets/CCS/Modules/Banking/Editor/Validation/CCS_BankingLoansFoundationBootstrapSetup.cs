using System.IO;
using CCS.Modules.Banking;
using CCS.Modules.Playtesting;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankingLoansFoundationBootstrapSetup
// CATEGORY: Modules / Banking / Editor / Validation
// PURPOSE: Batch-creates loan content, profile wiring, playtest steps, and version bump.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 — loans and debt foundation.
// =============================================================================

namespace CCS.Modules.Banking.Editor
{
    public static class CCS_BankingLoansFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_BankingLoansFoundationBootstrapSetup]";
        private const string LoansMilestoneVersion = "2.6.0";
        private const string LoansContentRoot = "Assets/CCS/Survival/Content/Banking/Loans";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_LoanDefinition smallLoan = EnsureFrontierSmallLoanDefinition();
            CCS_LoanProfile loanProfile = EnsureLoanProfile(smallLoan);
            WireLoanProfileIntoBankProfile(loanProfile);
            EnsurePlaytestLoanSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Banking loans foundation bootstrap setup complete ({LoansMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Banking");
            EnsureFolder(LoansContentRoot);
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

        private static CCS_LoanDefinition EnsureFrontierSmallLoanDefinition()
        {
            CCS_LoanDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_LoanDefinition>(
                CCS_BankingContentIds.FrontierSmallLoanDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_LoanDefinition>();
                AssetDatabase.CreateAsset(definition, CCS_BankingContentIds.FrontierSmallLoanDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("loanDefinitionId").stringValue =
                CCS_BankingContentIds.FrontierSmallLoanDefinitionId;
            serialized.FindProperty("displayName").stringValue = "Frontier Small Loan";
            serialized.FindProperty("currencyId").stringValue = CCS_BankingContentIds.TradeDollarsCurrencyId;
            serialized.FindProperty("principalAmount").intValue = 500;
            serialized.FindProperty("repaymentAmount").intValue = 550;
            serialized.FindProperty("repaymentIntervalDaysPlaceholder").intValue = 7;
            serialized.FindProperty("maxActiveLoans").intValue = 1;
            serialized.FindProperty("enabled").boolValue = true;
            serialized.FindProperty("autoRepayFromBank").boolValue = true;
            serialized.FindProperty("autoRepayFromWallet").boolValue = true;
            serialized.FindProperty("futureCollateralTypePlaceholder").stringValue = "none";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_LoanProfile EnsureLoanProfile(CCS_LoanDefinition smallLoan)
        {
            CCS_LoanProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_LoanProfile>(CCS_BankingContentIds.DefaultLoanProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_LoanProfile>();
                AssetDatabase.CreateAsset(profile, CCS_BankingContentIds.DefaultLoanProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = "ccs.survival.profile.banking.loans.default";
            serialized.FindProperty("profileDisplayName").stringValue = "Default Loan Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier bank loan products for bootstrap trading post services.";
            serialized.FindProperty("profileVersion").stringValue = LoansMilestoneVersion;
            serialized.FindProperty("defaultLoanDefinitionId").stringValue =
                CCS_BankingContentIds.FrontierSmallLoanDefinitionId;
            SerializedProperty definitions = serialized.FindProperty("loanDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = smallLoan;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void WireLoanProfileIntoBankProfile(CCS_LoanProfile loanProfile)
        {
            CCS_BankAccountProfile bankProfile = AssetDatabase.LoadAssetAtPath<CCS_BankAccountProfile>(
                CCS_BankingContentIds.DefaultBankProfilePath);
            if (bankProfile == null)
            {
                Debug.LogError($"{LogPrefix} Missing bank account profile at {CCS_BankingContentIds.DefaultBankProfilePath}.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(bankProfile);
            serialized.FindProperty("loanProfile").objectReferenceValue = loanProfile;
            serialized.FindProperty("profileVersion").stringValue = LoansMilestoneVersion;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bankProfile);
        }

        private static void EnsurePlaytestLoanSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.borrow",
                "Borrow frontier small loan",
                CCS_PlaytestStepType.BorrowSmallLoan,
                CCS_BankingContentIds.FrontierSmallLoanDefinitionId);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.verify.wallet",
                "Verify wallet increased after loan",
                CCS_PlaytestStepType.VerifyWalletIncreasedAfterLoan,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.deposit.part",
                "Deposit part of loan proceeds",
                CCS_PlaytestStepType.DepositPartOfLoan,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.repay",
                "Repay frontier small loan",
                CCS_PlaytestStepType.RepayBankLoan,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.verify.paid",
                "Verify loan state paid",
                CCS_PlaytestStepType.VerifyLoanPaid,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.save",
                "Save loan state",
                CCS_PlaytestStepType.SaveLoanState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.banking.loan.verify.load",
                "Verify loan state restored after load",
                CCS_PlaytestStepType.VerifyLoanAfterLoad,
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
                $"Loan playtest: {displayName}. Ctrl+Shift+O shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(LoansMilestoneVersion);
        }
    }
}
