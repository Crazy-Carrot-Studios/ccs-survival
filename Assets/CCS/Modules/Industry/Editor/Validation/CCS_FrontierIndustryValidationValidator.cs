using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Playtesting;
using CCS.Modules.Shelter;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Industry.Editor
{
    public sealed class CCS_FrontierIndustryValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.industry";
        private const string IndustryProfilePath = "Assets/CCS/Survival/Profiles/Industry/CCS_DefaultIndustryProfile.asset";
        private const string CampTierProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampTierProfile.asset";
        private const string CampDefinitionPath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateIndustryProfile(report);
            ValidateCampTierIndustrial(report);
            ValidateWorkbenchCatalog(report);
            ValidateComposition(report);
            ValidateSaveIntegration(report);
            ValidatePlaytestSteps(report);
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            bool ok = File.Exists(projectSettingsPath)
                && File.ReadAllText(projectSettingsPath).Contains("bundleVersion: 1.5.0");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "bundleVersion is 1.5.0." : "Expected bundleVersion 1.5.0. Run CCS_FrontierIndustryBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateIndustryProfile(CCS_SurvivalValidationReport report)
        {
            CCS_IndustryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_IndustryProfile>(IndustryProfilePath);
            CCS_SurvivalValidationResult result = CCS_IndustryValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateCampTierIndustrial(CCS_SurvivalValidationReport report)
        {
            CCS_CampTierProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CampTierProfile>(CampTierProfilePath);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing camp tier profile.");
                return;
            }

            bool hasIndustrial = false;
            CCS_CampTierDefinition[] tiers = profile.GetTiersOrderedAscending();
            for (int index = 0; index < tiers.Length; index++)
            {
                if (tiers[index]?.CampTier == CCS_CampTier.IndustrialHomestead)
                {
                    hasIndustrial = true;
                    break;
                }
            }

            report.AddIssue(
                hasIndustrial ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasIndustrial
                    ? "Camp tier profile includes IndustrialHomestead."
                    : "Camp tier profile missing IndustrialHomestead tier.");
        }

        private static void ValidateWorkbenchCatalog(CCS_SurvivalValidationReport report)
        {
            CCS_CampDefinition campDefinition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(CampDefinitionPath);
            CCS_SurvivalValidationResult result =
                CCS_CampValidationUtility.ValidateWorkbenchCatalog(campDefinition?.WorkbenchDefinitions);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateComposition(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
            string source = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            bool ok = source.Contains("CreateIndustryService")
                && source.Contains("BindIndustryService");
            string saveSource = File.Exists("Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs")
                ? File.ReadAllText("Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs")
                : string.Empty;
            ok = ok && saveSource.Contains("CaptureIndustry");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Industry services registered in gameplay composition." : "Industry composition wiring missing.");
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
            string source = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            bool ok = source.Contains("CCS_SaveIndustryWorldData")
                && source.Contains("hasPrimitiveForge");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Save payloads include industry and forge camp flags." : "Industry save integration incomplete.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(path);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing playtest profile.");
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyIndustrialHomesteadTier);
            ValidateStep(report, profile, CCS_PlaytestStepType.ProduceLumberAtSawTable);
            ValidateStep(report, profile, CCS_PlaytestStepType.ProduceCharcoalAtKiln);
        }

        private static void ValidateStep(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool found = false;
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                if (profile.StepDefinitions[index].StepType == stepType)
                {
                    found = true;
                    break;
                }
            }

            report.AddIssue(
                found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                found
                    ? $"Playtest step {stepType} present."
                    : $"Missing playtest step {stepType}.");
        }
    }
}
