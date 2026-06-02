using System.Collections.Generic;
using System.IO;
using CCS.Modules.Cooking;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

namespace CCS.Modules.Cooking.Editor
{
    public sealed class CCS_FrontierCookingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string CookingProfilePath = "Assets/CCS/Survival/Profiles/Cooking/CCS_DefaultCookingProfile.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string HardtackPath = "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset";

        private static readonly string[] RequiredFoodItemPaths =
        {
            "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_RawFish.asset",
            "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_SmallFish.asset",
            "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset",
            "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawRabbitMeat.asset",
            "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawVenison.asset",
            "Assets/CCS/Survival/Content/Items/Resources/Wildlife/CCS_Item_RawTurkeyMeat.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedFish.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedMeat.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedRabbitMeat.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedVenison.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_CookedTurkey.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_Jerky.asset",
            "Assets/CCS/Survival/Content/Items/Food/CCS_Item_DriedFish.asset",
            "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset"
        };

        private static readonly string[] RequiredRecipeIds =
        {
            "ccs.survival.cooking.recipe.cookfish",
            "ccs.survival.cooking.recipe.cooksmallfish",
            "ccs.survival.cooking.recipe.cookmeat",
            "ccs.survival.cooking.recipe.cookrabbit",
            "ccs.survival.cooking.recipe.cookvenison",
            "ccs.survival.cooking.recipe.cookturkey",
            "ccs.survival.cooking.recipe.smokejerky",
            "ccs.survival.cooking.recipe.smokedriedfish"
        };

        private static readonly CCS_PlaytestStepType[] RequiredCookingPlaytestSteps =
        {
            CCS_PlaytestStepType.ObtainRawFoodForCooking,
            CCS_PlaytestStepType.CookFood,
            CCS_PlaytestStepType.VerifyCookedFoodInInventory,
            CCS_PlaytestStepType.EatFood,
            CCS_PlaytestStepType.PreserveFoodAtCampfire,
            CCS_PlaytestStepType.SellPreservedFoodAtVendor,
            CCS_PlaytestStepType.VerifyCookingCurrencyIncreased
        };

        public string ValidatorId => "ccs.survival.validation.cooking.frontier";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateFoodItems(report);
            ValidateCookingProfile(report);
            ValidateHardtack(report);
            ValidateVendorFoodCatalog(report);
            ValidatePlaytestCookingSteps(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Frontier cooking validator completed (milestone 1.3.4).");
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            string path = "ProjectSettings/ProjectSettings.asset";
            bool ok = File.Exists(path) && File.ReadAllText(path).Contains("bundleVersion: 1.4.1");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Project Version",
                ok ? "bundleVersion is 1.4.1." : "Expected bundleVersion 1.4.1. Run CCS_FrontierHomesteadBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateFoodItems(CCS_SurvivalValidationReport report)
        {
            HashSet<string> itemIds = new HashSet<string>();
            for (int index = 0; index < RequiredFoodItemPaths.Length; index++)
            {
                string path = RequiredFoodItemPaths[index];
                CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
                if (item == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Frontier Food Item",
                        $"Missing item asset: {path}");
                    continue;
                }

                if (!itemIds.Add(item.ItemId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Frontier Food Item IDs",
                        $"Duplicate item ID detected: {item.ItemId}");
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Frontier Food Item",
                    $"{item.DisplayName} ({item.ItemId}) present.");
            }
        }

        private static void ValidateCookingProfile(CCS_SurvivalValidationReport report)
        {
            CCS_CookingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CookingProfile>(CookingProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Cooking Profile",
                    $"Missing profile: {CookingProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult baseResult = CCS_CookingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                baseResult.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Cooking Profile",
                baseResult.Message);

            CCS_SurvivalValidationResult frontierResult =
                CCS_CookingValidationUtility.ValidateFrontierCookingExpansion(profile);
            report.AddIssue(
                frontierResult.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                "Frontier Cooking Expansion",
                frontierResult.Message);

            for (int index = 0; index < RequiredRecipeIds.Length; index++)
            {
                string recipeId = RequiredRecipeIds[index];
                bool found = profile.TryGetRecipe(recipeId, out CCS_CookingRecipe recipe) && recipe != null;
                report.AddIssue(
                    found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                    "Cooking Recipe",
                    found ? $"Recipe {recipeId} configured." : $"Missing recipe {recipeId}.");
            }
        }

        private static void ValidateHardtack(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition hardtack = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(HardtackPath);
            if (hardtack == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Hardtack Item",
                    $"Missing hardtack at {HardtackPath}");
                return;
            }

            report.AddIssue(
                hardtack.ItemId == "ccs.survival.item.starter.hardtack"
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Hardtack Item",
                "Starter hardtack item remains valid.");
        }

        private static void ValidateVendorFoodCatalog(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor?.VendorInventory?.Items == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Vendor Food Catalog",
                    "General store vendor missing.");
                return;
            }

            bool sellsJerky = VendorHasItem(vendor, "ccs.survival.item.food.jerky", allowBuy: true);
            bool buysCookedFish = VendorHasItem(vendor, "ccs.survival.item.food.cookedfish", allowSell: true);
            bool buysJerky = VendorHasItem(vendor, "ccs.survival.item.food.jerky", allowSell: true);

            report.AddIssue(
                sellsJerky && buysCookedFish && buysJerky
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Vendor Food Catalog",
                sellsJerky && buysCookedFish && buysJerky
                    ? "General store sells/buys frontier cooked and preserved foods."
                    : "Vendor catalog missing jerky sell/buy or cooked fish buy.");
        }

        private static bool VendorHasItem(CCS_VendorDefinition vendor, string itemId, bool allowBuy = false, bool allowSell = false)
        {
            CCS_VendorItemEntry[] entries = vendor.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry?.ItemDefinition == null || entry.ItemDefinition.ItemId != itemId)
                {
                    continue;
                }

                if (allowBuy && entry.AllowBuy)
                {
                    return true;
                }

                if (allowSell && entry.AllowSell)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidatePlaytestCookingSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                    "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile?.StepDefinitions == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Cooking Playtest",
                    "Playtest profile missing.");
                return;
            }

            for (int index = 0; index < RequiredCookingPlaytestSteps.Length; index++)
            {
                CCS_PlaytestStepType stepType = RequiredCookingPlaytestSteps[index];
                bool found = false;
                for (int stepIndex = 0; stepIndex < profile.StepDefinitions.Count; stepIndex++)
                {
                    if (profile.StepDefinitions[stepIndex]?.StepType == stepType)
                    {
                        found = true;
                        break;
                    }
                }

                report.AddIssue(
                    found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                    "Cooking Playtest",
                    found ? $"Step {stepType} present." : $"Missing playtest step {stepType}.");
            }
        }
    }
}
