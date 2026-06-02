using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierStarterProgressionValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Validates 1.2.6 frontier starter loadout, items, recipes, and spear legacy status.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_FrontierStarterProgressionValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string StarterProfilePath =
            "Assets/CCS/Survival/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string KnifePath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Knife.asset";
        private const string SpearPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Spear.asset";
        private const string BedrollPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Bedroll.asset";
        private const string FishingPolePath = "Assets/CCS/Survival/Content/Items/Tools/Fishing/CCS_Item_FishingPole.asset";
        private const string BowPath = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Bow.asset";
        private const string ArrowPath = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Arrow.asset";
        private const string FishingPoleRecipePath =
            "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/CCS_FrontierFishingPoleRecipe.asset";
        private const string BowRecipePath =
            "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes/CCS_FrontierBowRecipe.asset";
        private const string FrontierRecipesRoot = "Assets/CCS/Survival/Profiles/Crafting/FrontierPrimitiveRecipes";
        private const string PocketKnifeItemId = "ccs.survival.item.starter.knife";
        private const string SpearItemId = "ccs.survival.item.starter.spear";

        public string ValidatorId => "ccs.survival.validation.frontier.starter.progression";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateStarterLoadoutNotSpearCentric(report);
            ValidateFrontierStarterItems(report);
            ValidatePocketKnifeActiveItem(report);
            ValidateBowWithoutRangedBehavior(report);
            ValidateFrontierRecipes(report);
            ValidateSpearLegacyContent(report);
            ValidateActiveItemServiceFrontierRouting(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Frontier starter progression validator completed (1.2.6).");
        }

        private static void ValidateStarterLoadoutNotSpearCentric(CCS_SurvivalValidationReport report)
        {
            CCS_StarterLoadoutProfile profile = AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Starter Loadout",
                    $"Missing profile: {StarterProfilePath}. Run CCS_FrontierStarterProgressionBootstrapSetup.ExecuteBatch.");
                return;
            }

            if (profile.ProfileVersion != "1.2.6")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Starter Loadout Version",
                    $"Expected profileVersion 1.2.6 but found '{profile.ProfileVersion}'.");
            }

            bool hasSpear = false;
            bool hasKnife = false;
            bool hasBedroll = false;
            for (int index = 0; index < profile.StartingItems.Length; index++)
            {
                CCS_StarterLoadoutEntry entry = profile.StartingItems[index];
                if (entry?.ItemDefinition == null)
                {
                    continue;
                }

                if (entry.ItemDefinition.ItemId == SpearItemId)
                {
                    hasSpear = true;
                }

                if (entry.ItemDefinition.ItemId == PocketKnifeItemId)
                {
                    hasKnife = true;
                }

                if (entry.ItemDefinition.ItemId == "ccs.survival.item.starter.bedroll")
                {
                    hasBedroll = true;
                }
            }

            if (hasSpear)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Starter Loadout",
                    "Default starter loadout must not include spear (regression-only content).");
            }

            if (!hasKnife || !hasBedroll)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Starter Loadout",
                    "Starter loadout must include pocket knife and bedroll.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Frontier Starter Loadout",
                hasSpear
                    ? "Starter loadout check failed (spear present)."
                    : "Starter loadout is frontier-themed without spear dependency.");
        }

        private static void ValidateFrontierStarterItems(CCS_SurvivalValidationReport report)
        {
            string[] requiredPaths =
            {
                KnifePath,
                BedrollPath,
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Canteen.asset",
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset",
                "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Coin.asset",
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Tinderbox.asset",
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Cordage.asset",
                BowPath,
                ArrowPath,
                FishingPolePath
            };

            for (int index = 0; index < requiredPaths.Length; index++)
            {
                if (!File.Exists(requiredPaths[index]))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Frontier Starter Items",
                        $"Missing item asset: {requiredPaths[index]}");
                }
            }
        }

        private static void ValidatePocketKnifeActiveItem(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition knife = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(KnifePath);
            if (knife == null)
            {
                return;
            }

            if (knife.DisplayName != "Pocket Knife")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Pocket Knife Display",
                    $"Expected display name 'Pocket Knife' but found '{knife.DisplayName}'.");
            }

            bool validTool = CCS_ItemGameplayUtility.IsToolItem(knife)
                && CCS_ItemGameplayUtility.ResolveHarvestToolType(knife) == CCS_ItemToolType.Knife;
            bool validWeapon = CCS_ItemGameplayUtility.IsWeaponItem(knife);

            if (!validTool || !validWeapon)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Pocket Knife Classification",
                    "Pocket knife must be classified as tool and weapon for active item routing.");
            }
        }

        private static void ValidateBowWithoutRangedBehavior(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition bow = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(BowPath);
            if (bow == null)
            {
                return;
            }

            if (!CCS_ItemGameplayUtility.IsBowWeaponItem(bow))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bow Active Item",
                    "Bow must register ranged weapon identity for frontier hunting (1.3.2).");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Bow Active Item",
                "Bow has ranged weapon identity for hunting foundation.");
        }

        private static void ValidateFrontierRecipes(CCS_SurvivalValidationReport report)
        {
            if (!Directory.Exists(FrontierRecipesRoot))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Recipes",
                    $"Missing folder: {FrontierRecipesRoot}");
                return;
            }

            ValidateRecipeAsset(report, FishingPoleRecipePath, "ccs.survival.recipe.frontier.fishingpole");
            ValidateRecipeAsset(report, BowRecipePath, "ccs.survival.recipe.frontier.bow");

            CCS_StarterLoadoutProfile profile = AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                return;
            }

            bool hasFishingPoleRecipe = false;
            bool hasBowRecipe = false;
            for (int index = 0; index < profile.PrimitiveRecipes.Length; index++)
            {
                CCS_CraftingRecipeDefinition recipe = profile.PrimitiveRecipes[index];
                if (recipe == null)
                {
                    continue;
                }

                if (recipe.RecipeId == "ccs.survival.recipe.frontier.fishingpole")
                {
                    hasFishingPoleRecipe = true;
                }

                if (recipe.RecipeId == "ccs.survival.recipe.frontier.bow")
                {
                    hasBowRecipe = true;
                }
            }

            if (!hasFishingPoleRecipe || !hasBowRecipe)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Starter Recipes",
                    "Starter loadout primitiveRecipes must include fishing pole and bow frontier recipes.");
            }
        }

        private static void ValidateRecipeAsset(CCS_SurvivalValidationReport report, string path, string recipeId)
        {
            CCS_CraftingRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(path);
            if (recipe == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Recipes",
                    $"Missing recipe: {path}");
                return;
            }

            if (recipe.RecipeId != recipeId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Recipes",
                    $"Recipe at {path} expected id {recipeId} but found {recipe.RecipeId}.");
            }

            if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Recipes",
                    $"Recipe {recipeId} has no ingredients.");
                return;
            }

            for (int index = 0; index < recipe.Ingredients.Count; index++)
            {
                CCS_CraftingIngredientDefinition ingredient = recipe.Ingredients[index];
                if (ingredient?.ItemDefinition == null || ingredient.Quantity <= 0)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Frontier Recipes",
                        $"Recipe {recipeId} has invalid ingredient at index {index}.");
                }
            }
        }

        private static void ValidateSpearLegacyContent(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SpearPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Spear Legacy",
                    "Legacy spear item asset missing (optional regression content).");
                return;
            }

            CCS_ItemDefinition spear = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(SpearPath);
            if (spear != null && spear.ItemId == SpearItemId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Spear Legacy",
                    "Spear remains available as optional regression content.");
            }
        }

        private static void ValidateActiveItemServiceFrontierRouting(CCS_SurvivalValidationReport report)
        {
            string path = "Assets/CCS/Modules/Hotbar/Runtime/ActiveItem/CCS_ActiveItemService.cs";
            if (!File.Exists(path))
            {
                return;
            }

            string source = File.ReadAllText(path);
            bool fishing = source.Contains("TryUseFishingPole");
            bool gathering = source.Contains("TryUseToolOnGatheringNode");
            if (fishing && gathering)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Active Item Routing",
                    "Active item service retains fishing and gathering routes alongside frontier tools.");
            }
        }
    }
}
