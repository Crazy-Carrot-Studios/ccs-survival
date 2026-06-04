using System.Collections.Generic;
using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MultiSettlementFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates multi-settlement network, trade routes, contracts, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.3.0 multi-settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_MultiSettlementFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.multisettlement";
        private const string MultiSettlementMilestoneVersion = "3.3.0";
        private const string DefaultSettlementProfilePath =
            "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultSettlementProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScripts(report);
            ValidateSettlementProfile(report);
            ValidateTradeRouteProfile(report);
            ValidateWorldSimulationSettlements(report);
            ValidateRegionalContracts(report);
            ValidateSavePath(report);
            ValidatePlaytestSteps(report);
            ValidateSceneSettlementRoots(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MultiSettlementMilestoneVersion,
                "Run CCS_MultiSettlementFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Multi-Settlement Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Multi-Settlement Obsolete API Scan");
        }

        private static void ValidateRequiredScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScript(report, "CCS_TradeRouteDefinition", "Assets/CCS/Modules/Settlements/Runtime/TradeRoutes/CCS_TradeRouteDefinition.cs");
            ValidateScript(report, "CCS_TradeRouteProfile", "Assets/CCS/Modules/Settlements/Runtime/Profiles/CCS_TradeRouteProfile.cs");
            ValidateScript(report, "CCS_TradeRouteUtility", "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_TradeRouteUtility.cs");
            ValidateScript(
                report,
                "CCS_MultiSettlementFoundationBootstrapSetup",
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_MultiSettlementFoundationBootstrapSetup.cs");
        }

        private static void ValidateSettlementProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing settlement profile: {DefaultSettlementProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SettlementValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            if (profile.SettlementDefinitions.Length < CCS_MultiSettlementContentIds.BootstrapSettlementCount)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Settlement profile requires {CCS_MultiSettlementContentIds.BootstrapSettlementCount} settlement definitions.");
            }

            HashSet<string> settlementIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < profile.SettlementDefinitions.Length; index++)
            {
                CCS_SettlementDefinition definition = profile.SettlementDefinitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    continue;
                }

                if (!settlementIds.Add(definition.SettlementId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        ValidatorContext,
                        $"Duplicate settlement id '{definition.SettlementId}'.");
                }
            }
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
        }

        private static void ValidateWorldSimulationSettlements(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(WorldSimulationProfilePath);
            if (profile == null)
            {
                return;
            }

            if (profile.SettlementEntries.Length < CCS_MultiSettlementContentIds.BootstrapSettlementCount)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "World simulation profile is missing multi-settlement entries.");
            }
        }

        private static void ValidateRegionalContracts(CCS_SurvivalValidationReport report)
        {
            ValidateContractSettlement(
                report,
                CCS_ContractContentIds.LumberDeliveryContractPath,
                CCS_MultiSettlementContentIds.PineRidgeCampSettlementId);
            ValidateContractSettlement(
                report,
                CCS_ContractContentIds.MixedFrontierSupplyContractPath,
                CCS_SettlementContentIds.TestTradingPostSettlementId);
        }

        private static void ValidateContractSettlement(
            CCS_SurvivalValidationReport report,
            string contractPath,
            string expectedSettlementId)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(contractPath);
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing contract definition: {contractPath}");
                return;
            }

            if (!string.Equals(definition.SettlementId, expectedSettlementId, System.StringComparison.OrdinalIgnoreCase))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Contract '{definition.ContractId}' must target settlement '{expectedSettlementId}'.");
            }
        }

        private static void ValidateSavePath(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveDataPath))
            {
                return;
            }

            string source = File.ReadAllText(SaveDataPath);
            if (!source.Contains("tradeRoutes"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Save data is missing tradeRoutes persistence.");
            }
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            ValidateStep(report, steps, CCS_PlaytestStepType.DiscoverPineRidgeCampSettlement);
            ValidateStep(report, steps, CCS_PlaytestStepType.DiscoverBrokenCreekFarmsteadSettlement);
            ValidateStep(report, steps, CCS_PlaytestStepType.DiscoverIronRidgeMiningCampSettlement);
            ValidateStep(report, steps, CCS_PlaytestStepType.AcceptMultiSettlementRegionalContract);
            ValidateStep(report, steps, CCS_PlaytestStepType.CompleteMultiSettlementRegionalContract);
            ValidateStep(report, steps, CCS_PlaytestStepType.VerifyMultiSettlementProsperityChanged);
            ValidateStep(report, steps, CCS_PlaytestStepType.VerifyMultiSettlementReputationChanged);
            ValidateStep(report, steps, CCS_PlaytestStepType.SaveMultiSettlementState);
            ValidateStep(report, steps, CCS_PlaytestStepType.VerifyMultiSettlementAfterLoad);
        }

        private static void ValidateSceneSettlementRoots(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            string sceneSource = File.ReadAllText(BootstrapScenePath);
            if (!sceneSource.Contains(CCS_MultiSettlementContentIds.PineRidgeCampObjectName)
                || !sceneSource.Contains(CCS_MultiSettlementContentIds.BrokenCreekFarmsteadObjectName)
                || !sceneSource.Contains(CCS_MultiSettlementContentIds.IronRidgeMiningCampObjectName))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Bootstrap scene is missing multi-settlement camp roots.");
            }
        }

        private static void ValidateStep(
            CCS_SurvivalValidationReport report,
            IReadOnlyList<CCS_PlaytestStepDefinition> steps,
            CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index] != null && steps[index].StepType == stepType)
                {
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                $"Missing playtest step type {stepType}. Run multi-settlement bootstrap setup.");
        }

        private static void ValidateScript(CCS_SurvivalValidationReport report, string label, string path)
        {
            if (File.Exists(path))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, ValidatorContext, $"{label} present.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                $"Missing required script: {path}");
        }
    }
}
