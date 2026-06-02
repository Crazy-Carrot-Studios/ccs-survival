using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Storage;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Shelter.Editor
{
    public sealed class CCS_FrontierHomesteadValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.homestead";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateCampTierProfile(report);
            ValidateCampDefinition(report);
            ValidateRegistration(report);
            ValidateSaveIntegration(report);
            ValidatePlaytestSteps(report);
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            bool ok = File.Exists(projectSettingsPath)
                && File.ReadAllText(projectSettingsPath).Contains("bundleVersion: 1.7.0");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "bundleVersion is 1.7.0." : "Expected bundleVersion 1.7.0. Run CCS_WagonFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateCampTierProfile(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampTierProfile.asset";
            CCS_CampTierProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CampTierProfile>(path);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing CCS_DefaultCampTierProfile.asset.");
                return;
            }

            CCS_CampTierDefinition[] tiers = profile.GetTiersOrderedAscending();
            if (tiers.Length < 3)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Camp tier profile must define TemporaryCamp, FrontierCamp, and FrontierHomestead.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    ValidatorContext,
                    "Camp tier profile defines homestead progression ladder.");
            }
        }

        private static void ValidateCampDefinition(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";
            CCS_CampDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(path);
            CCS_SurvivalValidationResult result = CCS_CampValidationUtility.ValidateCampDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);

            string storageProfilePath =
                "Assets/CCS/Survival/Profiles/Storage/CCS_FrontierStorageCampProfile.asset";
            CCS_FrontierStorageCampProfile storageProfile =
                AssetDatabase.LoadAssetAtPath<CCS_FrontierStorageCampProfile>(storageProfilePath);
            CCS_SurvivalValidationResult storageResult =
                CCS_StorageCampValidationUtility.ValidateFrontierStorageCatalog(storageProfile?.FrontierStorageDefinitions);
            report.AddIssue(
                storageResult.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                storageResult.Message);

            CCS_SurvivalValidationResult workbenchResult =
                CCS_CampValidationUtility.ValidateWorkbenchCatalog(definition?.WorkbenchDefinitions);
            report.AddIssue(
                workbenchResult.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                workbenchResult.Message);
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
            string source = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            bool ok = source.Contains("CreateFrontierHomesteadStructureService")
                && source.Contains("CreateFrontierStoragePlacementService")
                && source.Contains("BindHomesteadStructureService")
                && source.Contains("BindStorageProximityQuery")
                && source.Contains("BindFrontierHomesteadStructureService")
                && source.Contains("BindFrontierStoragePlacementHandler");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Homestead services registered in gameplay composition." : "Homestead service registration missing.");
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
            string source = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            bool ok = source.Contains("workbenchInstances")
                && source.Contains("hasStorage")
                && source.Contains("RebuildPlacedStorageTracking");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Save service persists homestead camp state." : "Homestead save integration incomplete.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.VerifyFrontierCampTier);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.VerifyFrontierHomesteadTier);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.VerifyHomesteadCampPersistenceAfterLoad);
        }

        private static void ValidateStepPresent(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool found = false;
            if (profile != null)
            {
                for (int index = 0; index < profile.StepDefinitions.Count; index++)
                {
                    if (profile.StepDefinitions[index]?.StepType == stepType)
                    {
                        found = true;
                        break;
                    }
                }
            }

            report.AddIssue(
                found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                found
                    ? $"Playtest step {stepType} present."
                    : $"Missing playtest step {stepType}. Run CCS_FrontierHomesteadBootstrapSetup.ExecuteBatch.");
        }
    }
}
