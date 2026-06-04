using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TradeRoutesFreightFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates trade route freight contracts, wagon cargo integration, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 trade routes and freight contracts.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_TradeRoutesFreightFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.traderoutes.freight";
        private const string MilestoneVersion = "3.4.0";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string ContractServicePath =
            "Assets/CCS/Modules/Contracts/Runtime/Services/CCS_ContractService.cs";
        private const string SaveServicePath =
            "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateTradeRouteProfile(report);
            ValidateFreightContracts(report);
            ValidateServiceIntegration(report);
            ValidatePlaytestSteps(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_TradeRoutesFreightFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Trade Routes Freight Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Trade Routes Freight Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_TradeRouteService.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Contracts/Runtime/Validation/CCS_ContractFreightUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_TradeRoutesFreightFoundationBootstrapSetup.cs");
        }

        private static void ValidateTradeRouteProfile(CCS_SurvivalValidationReport report)
        {
            CCS_TradeRouteProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteProfile>(
                CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            CCS_SurvivalValidationResult validation = CCS_TradeRouteUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            if (profile != null && profile.TradeRouteDefinitions.Length < 6)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Trade route profile requires inbound and outbound frontier routes (minimum 6).");
            }
        }

        private static void ValidateFreightContracts(CCS_SurvivalValidationReport report)
        {
            ValidateFreightContractAsset(
                report,
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_PineRidge_Lumber.asset",
                CCS_TradeRoutesFreightContentIds.PineRidgeLumberFreightContractId,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId,
                CCS_SettlementContentIds.TestTradingPostSettlementId);
            ValidateFreightContractAsset(
                report,
                CCS_TradeRoutesFreightContentIds.FreightContractsContentRoot + "/CCS_Contract_Freight_IronRidge_Coal.asset",
                CCS_TradeRoutesFreightContentIds.IronRidgeCoalFreightContractId,
                CCS_MultiSettlementContentIds.IronRidgeMiningCampSettlementId,
                CCS_SettlementContentIds.TestTradingPostSettlementId);
        }

        private static void ValidateFreightContractAsset(
            CCS_SurvivalValidationReport report,
            string assetPath,
            string contractId,
            string sourceSettlementId,
            string destinationSettlementId)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing freight contract: {assetPath}");
                return;
            }

            if (definition.ContractType != CCS_ContractType.FreightDelivery)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Contract '{contractId}' must use FreightDelivery type.");
            }

            CCS_SurvivalValidationResult validation = CCS_ContractValidationUtility.ValidateDefinition(definition);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            if (!string.Equals(definition.FreightSourceSettlementId, sourceSettlementId, System.StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    definition.FreightDestinationSettlementId,
                    destinationSettlementId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Freight contract '{contractId}' has invalid source/destination settlement ids.");
            }
        }

        private static void ValidateServiceIntegration(CCS_SurvivalValidationReport report)
        {
            string contractSource = System.IO.File.ReadAllText(ContractServicePath);
            bool hasSettlementBoard = contractSource.Contains("GetSettlementBoardContracts", System.StringComparison.Ordinal);
            bool hasFreightComplete = contractSource.Contains(
                "TryCompleteContract(string contractId, string completionSettlementId)",
                System.StringComparison.Ordinal);
            bool hasWagonUtility = contractSource.Contains("CCS_ContractFreightUtility", System.StringComparison.Ordinal);
            report.AddIssue(
                hasSettlementBoard && hasFreightComplete && hasWagonUtility
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasSettlementBoard && hasFreightComplete && hasWagonUtility
                    ? "Contract service exposes settlement board routing and wagon freight completion."
                    : "Contract service is missing freight board routing or wagon cargo completion.");

            string saveSource = System.IO.File.ReadAllText(SaveServicePath);
            bool restoresRoutes = saveSource.Contains("RestoreRouteState", System.StringComparison.Ordinal);
            report.AddIssue(
                restoresRoutes ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                restoresRoutes
                    ? "Save service restores trade route discovery/active/usage snapshots."
                    : "Save service must apply trade route runtime state on load.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverFreightRouteSettlements);
            ValidateStep(report, profile, CCS_PlaytestStepType.CompletePineRidgeLumberFreightDelivery);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyTradeRouteUsageCount);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyFreightRouteStateAfterLoad);

            string hudSource = System.IO.File.ReadAllText("Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool hasShortcut = hudSource.Contains("TryPlaytestTradeRoutesFreightShortcut", System.StringComparison.Ordinal);
            report.AddIssue(
                hasShortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasShortcut
                    ? "Playtest HUD wires Ctrl+Shift+F freight shortcut."
                    : "Playtest HUD is missing Ctrl+Shift+F freight shortcut.");
        }

        private static void ValidateStep(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool found = false;
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                CCS_PlaytestStepDefinition step = profile.StepDefinitions[index];
                if (step != null && step.StepType == stepType)
                {
                    found = true;
                    break;
                }
            }

            report.AddIssue(
                found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                found
                    ? $"Playtest profile includes step {stepType}."
                    : $"Playtest profile is missing step {stepType}. Run freight bootstrap.");
        }

        private static void ValidateScriptExists(CCS_SurvivalValidationReport report, string scriptPath)
        {
            bool exists = System.IO.File.Exists(scriptPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"Script present: {scriptPath}" : $"Missing script: {scriptPath}");
        }
    }
}
