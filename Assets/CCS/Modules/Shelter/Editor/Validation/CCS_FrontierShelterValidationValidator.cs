using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Shelter;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Shelter.Editor
{
    public sealed class CCS_FrontierShelterValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string CampDefinitionPath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";
        private const string FrontierStructuresRoot = "Assets/CCS/Survival/Content/Structures/Frontier";
        private const string LeanToDefinitionPath = FrontierStructuresRoot + "/CCS_ShelterDefinition_LeanTo.asset";
        private const string LeanToRecipePath =
            "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/CCS_FrontierLeanToRecipe.asset";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";

        public string ValidatorId => "ccs.survival.validation.shelter.frontier";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateCampDefinition(report);
            ValidateLeanToDefinition(report);
            ValidateLeanToRecipe(report);
            ValidateServiceRegistration(report);
            ValidateSaveIntegration(report);
            ValidatePlaytestSteps(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Frontier shelter validator completed (milestone 1.4.0).");
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            string path = "ProjectSettings/ProjectSettings.asset";
            bool ok = File.Exists(path) && File.ReadAllText(path).Contains("bundleVersion: 1.4.0");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Project Version",
                ok ? "bundleVersion is 1.4.0." : "Expected bundleVersion 1.4.0. Run CCS_FrontierShelterBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateCampDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_CampDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(CampDefinitionPath);
            if (definition == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Camp Definition", $"Missing {CampDefinitionPath}");
                return;
            }

            CCS_SurvivalValidationResult result = CCS_CampValidationUtility.ValidateCampDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Camp Definition",
                result.Message);
        }

        private static void ValidateLeanToDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_ShelterDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ShelterDefinition>(LeanToDefinitionPath);
            if (definition == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Lean-To Definition", $"Missing {LeanToDefinitionPath}");
                return;
            }

            CCS_SurvivalValidationResult result = CCS_CampValidationUtility.ValidateShelterDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Lean-To Definition",
                result.Message);
        }

        private static void ValidateLeanToRecipe(CCS_SurvivalValidationReport report)
        {
            bool ok = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(LeanToRecipePath) != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Lean-To Recipe",
                ok ? "Frontier Lean-To recipe exists." : $"Missing recipe at {LeanToRecipePath}");
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(RegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Camp Service Registration",
                    $"Missing registration script: {RegistrationPath}");
                return;
            }

            string source = File.ReadAllText(RegistrationPath);
            bool ok = source.Contains("CreateCampService")
                && source.Contains("CreateFrontierShelterService")
                && source.Contains("BindFrontierShelterService");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Camp Service Registration",
                ok
                    ? "Gameplay composition registers frontier shelter and camp services."
                    : "Gameplay composition is missing frontier shelter/camp service registration.");
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Camp Save Integration",
                    $"Missing save service script: {SaveServicePath}");
                return;
            }

            string source = File.ReadAllText(SaveServicePath);
            bool ok = source.Contains("CaptureCamp") && source.Contains("ApplyCamp") && source.Contains("CCS_SaveCampWorldData");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Camp Save Integration",
                ok
                    ? "Unified save captures and restores camp and frontier shelter world data."
                    : "Save service is missing camp persistence methods.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile?.StepDefinitions == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Shelter Playtest", "Playtest profile missing.");
                return;
            }

            ValidateStepPresent(report, profile, CCS_PlaytestStepType.PlaceLeanToShelter);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.VerifyTemporaryCampTier);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.SleepInFrontierCamp);
        }

        private static void ValidateStepPresent(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool found = false;
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                if (profile.StepDefinitions[index]?.StepType == stepType)
                {
                    found = true;
                    break;
                }
            }

            report.AddIssue(
                found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Shelter Playtest",
                found
                    ? $"Playtest includes {stepType}."
                    : $"Missing playtest step {stepType}. Run CCS_FrontierShelterBootstrapSetup.ExecuteBatch.");
        }
    }
}
