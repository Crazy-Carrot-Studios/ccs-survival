using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Modules.Settlements;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ServiceAccessFoundationValidationValidator
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Validates service access rules, vendor price modifiers, and playtest wiring for 2.8.0.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 service access and price modifier foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public sealed class CCS_ServiceAccessFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.serviceaccess";
        private const string ServiceAccessMilestoneVersion = "2.8.0";
        private const string RuntimeRoot = "Assets/CCS/Modules/Reputation/Runtime";
        private const string EditorRoot = "Assets/CCS/Modules/Reputation/Editor";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string VendorServicePath = "Assets/CCS/Modules/Economy/Runtime/Services/CCS_VendorService.cs";
        private const string VendorTransactionResultPath =
            "Assets/CCS/Modules/Economy/Runtime/Vendors/CCS_VendorTransactionResult.cs";
        private const string RouteResolverPath =
            "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementServiceRouteResolver.cs";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScript(report, "CCS_ServiceAccessRule", RuntimeRoot + "/Definitions/CCS_ServiceAccessRule.cs");
            ValidateRequiredScript(report, "CCS_ServiceAccessProfile", RuntimeRoot + "/Profiles/CCS_ServiceAccessProfile.cs");
            ValidateRequiredScript(
                report,
                "CCS_ServiceAccessEvaluationUtility",
                RuntimeRoot + "/Validation/CCS_ServiceAccessEvaluationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_ReputationPriceModifierUtility",
                RuntimeRoot + "/Validation/CCS_ReputationPriceModifierUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_ServiceAccessFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_ServiceAccessFoundationBootstrapSetup.cs");

            ValidateServiceAccessProfile(report);
            ValidateAccessRule(report);
            ValidateReputationProfileModifiers(report);
            ValidateCompositionVendorBind(report);
            ValidateVendorTransactionResult(report);
            ValidateSettlementRouting(report);
            ValidateMissingReputationFallback(report);
            ValidatePlaytestSteps(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                ServiceAccessMilestoneVersion,
                "Run CCS_ServiceAccessFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Service Access Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Service Access Obsolete API Scan");
        }

        private static void ValidateServiceAccessProfile(CCS_SurvivalValidationReport report)
        {
            CCS_ServiceAccessProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ServiceAccessProfile>(
                CCS_ReputationContentIds.DefaultServiceAccessProfilePath);
            CCS_SurvivalValidationResult result = CCS_ReputationValidationUtility.ValidateServiceAccessProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateAccessRule(CCS_SurvivalValidationReport report)
        {
            CCS_ServiceAccessRule rule = AssetDatabase.LoadAssetAtPath<CCS_ServiceAccessRule>(
                CCS_ReputationContentIds.BlacksmithAdvancedAccessRulePath);
            CCS_SurvivalValidationResult result = CCS_ReputationValidationUtility.ValidateServiceAccessRule(rule);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateReputationProfileModifiers(CCS_SurvivalValidationReport report)
        {
            CCS_ReputationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ReputationProfile>(CCS_ReputationContentIds.DefaultReputationProfilePath);
            CCS_SurvivalValidationResult result = CCS_ReputationValidationUtility.ValidatePriceModifiers(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateCompositionVendorBind(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing composition registration for vendor reputation bind.");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool hasBind = source.Contains("BindReputationService", System.StringComparison.Ordinal)
                && source.Contains("vendorService.BindReputationService", System.StringComparison.Ordinal);
            report.AddIssue(
                hasBind
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasBind
                    ? "Vendor service binds reputation service in composition."
                    : "Composition missing vendorService.BindReputationService wiring.");
        }

        private static void ValidateVendorTransactionResult(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(VendorTransactionResultPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing CCS_VendorTransactionResult.cs.");
                return;
            }

            string source = File.ReadAllText(VendorTransactionResultPath);
            bool ok = source.Contains("BaseUnitPrice", System.StringComparison.Ordinal)
                && source.Contains("FinalUnitPrice", System.StringComparison.Ordinal)
                && source.Contains("ReputationPriceModifier", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Vendor transaction result includes reputation price fields."
                    : "Vendor transaction result missing base/final/modifier price fields.");
        }

        private static void ValidateSettlementRouting(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(RouteResolverPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing settlement service route resolver.");
                return;
            }

            string source = File.ReadAllText(RouteResolverPath);
            bool ok = source.Contains("EvaluateServiceAccess", System.StringComparison.Ordinal)
                && source.Contains("SetActiveVendor(vendorDefinition, servicePoint.ResolveSettlementId())", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement routing evaluates service access and passes settlement id to vendor."
                    : "Settlement routing missing service access evaluation or vendor settlement binding.");
        }

        private static void ValidateMissingReputationFallback(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(VendorServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing CCS_VendorService.cs.");
                return;
            }

            string vendorSource = File.ReadAllText(VendorServicePath);
            string modifierUtilitySource = File.ReadAllText(RuntimeRoot + "/Validation/CCS_ReputationPriceModifierUtility.cs");
            bool vendorUsesUtility = vendorSource.Contains("CCS_ReputationPriceModifierUtility", System.StringComparison.Ordinal);
            bool fallbackSafe = modifierUtilitySource.Contains("return 1f", System.StringComparison.Ordinal);
            report.AddIssue(
                vendorUsesUtility && fallbackSafe
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                vendorUsesUtility && fallbackSafe
                    ? "Vendor price modifiers fall back to 1.0 when reputation service is missing."
                    : "Missing safe reputation price modifier fallback.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing playtest profile at {DefaultPlaytestProfilePath}.");
                return;
            }

            bool hasAccessLoad = false;
            System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyServiceAccessAfterLoad)
                {
                    hasAccessLoad = true;
                    break;
                }
            }

            report.AddIssue(
                hasAccessLoad
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasAccessLoad
                    ? "Service access playtest includes save/load stability step."
                    : "Run CCS_ServiceAccessFoundationBootstrapSetup.ExecuteBatch for service access playtest steps.");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            bool exists = File.Exists(scriptPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} present." : $"Missing script: {scriptPath}");
        }
    }
}
