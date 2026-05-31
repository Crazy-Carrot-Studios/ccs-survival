using System.IO;
using CCS.Modules.Crafting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingValidationValidator
// CATEGORY: Modules / Crafting / Editor / Validation
// PURPOSE: Validates crafting module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require scene objects, UI, world stations, or save systems.
// =============================================================================

namespace CCS.Modules.Crafting.Editor
{
    public sealed class CCS_CraftingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Crafting";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Crafting/CCS_DefaultCraftingProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Crafting_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.crafting";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Crafting", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Stations", RuntimeRoot + "/Stations");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Crafting.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Crafting.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_CraftingStationType", RuntimeRoot + "/Definitions/CCS_CraftingStationType.cs");
            ValidateRequiredScript(report, "CCS_CraftingRecipeDefinition", RuntimeRoot + "/Definitions/CCS_CraftingRecipeDefinition.cs");
            ValidateRequiredScript(report, "CCS_CraftingIngredientDefinition", RuntimeRoot + "/Definitions/CCS_CraftingIngredientDefinition.cs");
            ValidateRequiredScript(report, "CCS_CraftingResultDefinition", RuntimeRoot + "/Definitions/CCS_CraftingResultDefinition.cs");
            ValidateRequiredScript(report, "CCS_CraftingRequest", RuntimeRoot + "/Data/CCS_CraftingRequest.cs");
            ValidateRequiredScript(report, "CCS_CraftingResult", RuntimeRoot + "/Data/CCS_CraftingResult.cs");
            ValidateRequiredScript(report, "CCS_CraftingQueueEntry", RuntimeRoot + "/Data/CCS_CraftingQueueEntry.cs");
            ValidateRequiredScript(report, "CCS_CraftingSnapshot", RuntimeRoot + "/Data/CCS_CraftingSnapshot.cs");
            ValidateRequiredScript(report, "CCS_CraftingStationContext", RuntimeRoot + "/Stations/CCS_CraftingStationContext.cs");
            ValidateRequiredScript(report, "CCS_CraftingService", RuntimeRoot + "/Services/CCS_CraftingService.cs");
            ValidateRequiredScript(report, "CCS_CraftingEventArgs", RuntimeRoot + "/Events/CCS_CraftingEventArgs.cs");
            ValidateRequiredScript(report, "CCS_CraftingEvents", RuntimeRoot + "/Events/CCS_CraftingEvents.cs");
            ValidateRequiredScript(report, "CCS_CraftingProfile", RuntimeRoot + "/Profiles/CCS_CraftingProfile.cs");
            ValidateRequiredScript(report, "CCS_CraftingValidationUtility", RuntimeRoot + "/Validation/CCS_CraftingValidationUtility.cs");

            ValidateDocumentationAsset(report, "Crafting Module Doc", ModuleDocPath);

            CCS_SurvivalValidationResult stationValidation =
                CCS_CraftingValidationUtility.ValidateRequiredStationTypes();

            report.AddIssue(
                stationValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Crafting Station Types",
                stationValidation.Message);

            ValidateRecipeDefinitionRules(report);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Crafting Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_CraftingProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_CraftingProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_CraftingValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Crafting Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Crafting Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Crafting validator completed (runtime architecture foundation; no UI/world station/save coupling).");
        }

        #endregion

        #region Private Methods

        private static void ValidateRecipeDefinitionRules(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalValidationResult nullRecipeValidation =
                CCS_CraftingValidationUtility.ValidateRecipeDefinition(null);

            if (nullRecipeValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Recipe Definition Validation",
                    "ValidateRecipeDefinition(null) should fail.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Recipe Definition Validation",
                    "Recipe validation rejects null recipe definitions.");
            }

            CCS_SurvivalValidationResult negativeTimeValidation =
                ValidateSyntheticRecipeCraftTime(-1f);

            if (negativeTimeValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Recipe Craft Time Validation",
                    "Negative craft time should fail validation.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Recipe Craft Time Validation",
                    negativeTimeValidation.Message);
            }
        }

        private static CCS_SurvivalValidationResult ValidateSyntheticRecipeCraftTime(float craftTimeSeconds)
        {
            CCS_CraftingRecipeDefinition recipe = ScriptableObject.CreateInstance<CCS_CraftingRecipeDefinition>();
            try
            {
                SerializedObject serializedRecipe = new SerializedObject(recipe);
                serializedRecipe.FindProperty("recipeId").stringValue = "ccs.survival.recipe.validation.test";
                serializedRecipe.FindProperty("craftTimeSeconds").floatValue = craftTimeSeconds;
                serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
                return CCS_CraftingValidationUtility.ValidateRecipeDefinition(recipe);
            }
            finally
            {
                Object.DestroyImmediate(recipe);
            }
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string context,
            string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"File present: {filePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                context,
                $"Documentation missing: {assetPath}");
        }

        #endregion
    }
}
