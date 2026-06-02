using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Trapping.Editor
{
    public sealed class CCS_FrontierTrappingValidationValidator : CCS_ISurvivalValidationValidator
    {
        public string ValidatorId => "ccs.survival.validation.trapping.frontier";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateTrapProfile(report);
            ValidateTrapDefinition(report);
            ValidateSimpleTrapItem(report);
            ValidateTrapRecipe(report);
            ValidatePlaytestSteps(report);
        }

        private static void ValidateTrapProfile(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Profiles/Trapping/CCS_DefaultTrapProfile.asset";
            CCS_TrapProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TrapProfile>(path);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Trap Profile", $"Missing {path}");
                return;
            }

            CCS_SurvivalValidationResult result = CCS_TrapValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Trap Profile",
                result.Message);
        }

        private static void ValidateTrapDefinition(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Content/Trapping/CCS_TrapDefinition_Simple.asset";
            CCS_TrapDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_TrapDefinition>(path);
            if (definition == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Simple Trap Definition", $"Missing {path}");
                return;
            }

            CCS_SurvivalValidationResult result = CCS_TrapValidationUtility.ValidateTrapDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Simple Trap Definition",
                result.Message);
        }

        private static void ValidateSimpleTrapItem(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_SimpleTrap.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Simple Trap Item", $"Missing {path}");
                return;
            }

            bool ok = item.ItemId == "ccs.survival.item.frontier.simpletrap"
                && CCS_ItemGameplayUtility.IsPlaceableTrapItem(item);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Simple Trap Item",
                ok ? "Simple trap item is placeable for active item routing." : "Simple trap item missing placeable classification.");
        }

        private static void ValidateTrapRecipe(CCS_SurvivalValidationReport report)
        {
            string path =
                "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/CCS_FrontierSimpleTrapRecipe.asset";
            bool ok = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(path) != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Simple Trap Recipe",
                ok ? "Frontier simple trap recipe exists." : $"Missing recipe at {path}");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                    "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile?.StepDefinitions == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Trapping Playtest", "Playtest profile missing.");
                return;
            }

            ValidateStepPresent(report, profile, CCS_PlaytestStepType.ObtainTrapForTrapping);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.PlaceTrapForTrapping);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.HarvestTriggeredTrap);
            ValidateStepPresent(report, profile, CCS_PlaytestStepType.VerifyTrappingCurrencyIncreased);
        }

        private static void ValidateStepPresent(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                if (profile.StepDefinitions[index]?.StepType == stepType)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Trapping Playtest",
                        $"Step {stepType} present.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Trapping Playtest",
                $"Missing playtest step {stepType}. Run CCS_FrontierTrappingBootstrapSetup.ExecuteBatch.");
        }
    }
}
