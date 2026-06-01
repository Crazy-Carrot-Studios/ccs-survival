using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PrimitiveToolWeaponValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Validates tool/weapon classifications, bone resources, recipes, and equipment wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Confirms 0.9.2 primitive tool and weapon foundation.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_PrimitiveToolWeaponValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string ResourcesRoot = SurvivalRoot + "/Content/Items/Resources/Primitive";
        private const string ToolsPrimitiveRoot = SurvivalRoot + "/Content/Items/Tools/Primitive";
        private const string ToolsBoneRoot = SurvivalRoot + "/Content/Items/Tools/Bone";
        private const string EquipmentRoot = SurvivalRoot + "/Content/Equipment/Primitive";
        private const string BoneRecipesRoot = SurvivalRoot + "/Profiles/Crafting/BoneRecipes";
        private const string StarterProfilePath = SurvivalRoot + "/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string StarterKnifePath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Knife.asset";
        private const string StarterSpearPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Spear.asset";
        private const string ServicePath = SurvivalRoot + "/Runtime/Player/Loadout/CCS_StarterLoadoutService.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.primitivetoolweapon";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScript(report, "CCS_ItemGameplayKind", "Assets/CCS/Modules/Inventory/Runtime/Definitions/CCS_ItemGameplayKind.cs");
            ValidateRequiredScript(report, "CCS_ToolArchetype", "Assets/CCS/Modules/Inventory/Runtime/Definitions/CCS_ToolArchetype.cs");
            ValidateRequiredScript(report, "CCS_ToolTier", "Assets/CCS/Modules/Inventory/Runtime/Definitions/CCS_ToolTier.cs");
            ValidateRequiredScript(report, "CCS_WeaponArchetype", "Assets/CCS/Modules/Inventory/Runtime/Definitions/CCS_WeaponArchetype.cs");
            ValidateRequiredScript(report, "CCS_ItemGameplayUtility", "Assets/CCS/Modules/Inventory/Runtime/Utilities/CCS_ItemGameplayUtility.cs");
            ValidateRequiredScript(report, "CCS_PrimitiveToolEquipTestHarness", "Assets/CCS/Modules/Equipment/Runtime/Testing/CCS_PrimitiveToolEquipTestHarness.cs");

            ValidateRequiredFolder(report, "Primitive Resources", ResourcesRoot);
            ValidateResourceItem(report, "CCS_Item_Bone", "Bone");
            ValidateResourceItem(report, "CCS_Item_Sinew", "Sinew");
            ValidateResourceItem(report, "CCS_Item_Hide", "Hide");

            ValidateRequiredFolder(report, "Primitive Tools", ToolsPrimitiveRoot);
            ValidateToolItem(report, $"{ToolsPrimitiveRoot}/CCS_Item_Hatchet.asset", "Hatchet", CCS_ToolArchetype.Hatchet, CCS_ToolTier.Primitive);
            ValidateToolItem(report, $"{ToolsPrimitiveRoot}/CCS_Item_Pick.asset", "Pick", CCS_ToolArchetype.Pick, CCS_ToolTier.Primitive);
            ValidateToolItem(report, $"{ToolsPrimitiveRoot}/CCS_Item_Shovel.asset", "Shovel", CCS_ToolArchetype.Shovel, CCS_ToolTier.Primitive);

            ValidateRequiredFolder(report, "Bone Tools", ToolsBoneRoot);
            ValidateToolItem(report, $"{ToolsBoneRoot}/CCS_Item_BoneKnife.asset", "Bone Knife", CCS_ToolArchetype.Knife, CCS_ToolTier.Bone);
            ValidateToolItem(report, $"{ToolsBoneRoot}/CCS_Item_BoneHatchet.asset", "Bone Hatchet", CCS_ToolArchetype.Hatchet, CCS_ToolTier.Bone);
            ValidateToolItem(report, $"{ToolsBoneRoot}/CCS_Item_BonePick.asset", "Bone Pick", CCS_ToolArchetype.Pick, CCS_ToolTier.Bone);
            ValidateToolItem(report, $"{ToolsBoneRoot}/CCS_Item_BoneShovel.asset", "Bone Shovel", CCS_ToolArchetype.Shovel, CCS_ToolTier.Bone);

            ValidateStarterKnife(report);
            ValidateStarterSpear(report);

            ValidateBoneRecipes(report);
            ValidateEquipmentDefinitions(report);
            ValidateStarterProfileBoneRecipes(report);
            ValidateServiceBoneRecipeRegistration(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Primitive tool and weapon validator completed.");
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, context, $"Missing required folder: {folderPath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, context, $"Missing required script: {scriptPath}");
        }

        private static void ValidateResourceItem(
            CCS_SurvivalValidationReport report,
            string assetName,
            string displayName)
        {
            string assetPath = $"{ResourcesRoot}/{assetName}.asset";
            if (!File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Primitive Resources", $"Missing resource asset: {assetPath}");
                return;
            }

            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null || itemDefinition.DisplayName != displayName)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Primitive Resources", $"{assetName} is invalid.");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Primitive Resources", $"{assetName} validated.");
        }

        private static void ValidateToolItem(
            CCS_SurvivalValidationReport report,
            string assetPath,
            string displayName,
            CCS_ToolArchetype expectedArchetype,
            CCS_ToolTier expectedTier)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Tool Archetypes", $"Missing tool asset: {assetPath}");
                return;
            }

            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null
                || itemDefinition.DisplayName != displayName
                || !itemDefinition.HasToolIdentity
                || itemDefinition.ToolArchetype != expectedArchetype
                || itemDefinition.ToolTier != expectedTier)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Tool Archetypes",
                    $"{displayName} tool metadata is invalid.");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Tool Archetypes", $"{displayName} validated.");
        }

        private static void ValidateStarterKnife(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition knife = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(StarterKnifePath);
            if (knife == null
                || knife.ToolArchetype != CCS_ToolArchetype.Knife
                || knife.ToolTier != CCS_ToolTier.Primitive
                || !knife.HasWeaponIdentity
                || knife.WeaponArchetype != CCS_WeaponArchetype.Knife)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Knife",
                    "Starter knife must be a primitive tool/weapon foundation item.");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Starter Knife", "Starter knife tool/weapon metadata validated.");
        }

        private static void ValidateStarterSpear(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition spear = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(StarterSpearPath);
            if (spear == null
                || !spear.HasWeaponIdentity
                || spear.WeaponArchetype != CCS_WeaponArchetype.Spear
                || spear.WeaponType == CCS_WeaponType.None
                || spear.DamageType == CCS_DamageType.None
                || spear.RangeType == CCS_RangeType.None)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Spear",
                    "Starter spear must define weapon placeholder metadata.");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Starter Spear", "Starter spear weapon metadata validated.");
        }

        private static void ValidateBoneRecipes(CCS_SurvivalValidationReport report)
        {
            string[] recipeNames =
            {
                "CCS_BoneKnifeRecipe",
                "CCS_BoneHatchetRecipe",
                "CCS_BonePickRecipe",
                "CCS_BoneShovelRecipe"
            };

            for (int index = 0; index < recipeNames.Length; index++)
            {
                string assetPath = $"{BoneRecipesRoot}/{recipeNames[index]}.asset";
                if (!File.Exists(assetPath))
                {
                    report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Bone Tool Recipes", $"Missing recipe: {assetPath}");
                    continue;
                }

                CCS_CraftingRecipeDefinition recipe =
                    AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(assetPath);
                if (recipe == null
                    || recipe.RequiredStationType != CCS_CraftingStationType.Hand
                    || recipe.Ingredients.Count < 2)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bone Tool Recipes",
                        $"{recipeNames[index]} must be a hand recipe using bone and branch.");
                    continue;
                }

                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Bone Tool Recipes", $"{recipeNames[index]} validated.");
            }
        }

        private static void ValidateEquipmentDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateEquipmentAsset(report, "CCS_Equipment_Knife", CCS_EquipmentSlotType.MainHand);
            ValidateEquipmentAsset(report, "CCS_Equipment_BoneHatchet", CCS_EquipmentSlotType.Tool);
            ValidateEquipmentAsset(report, "CCS_Equipment_BonePick", CCS_EquipmentSlotType.Tool);
            ValidateEquipmentAsset(report, "CCS_Equipment_BoneKnife", CCS_EquipmentSlotType.Tool);
            ValidateEquipmentAsset(report, "CCS_Equipment_BoneShovel", CCS_EquipmentSlotType.Tool);
        }

        private static void ValidateEquipmentAsset(
            CCS_SurvivalValidationReport report,
            string assetName,
            CCS_EquipmentSlotType expectedSlot)
        {
            string assetPath = $"{EquipmentRoot}/{assetName}.asset";
            if (!File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Primitive Equipment", $"Missing equipment asset: {assetPath}");
                return;
            }

            CCS_EquipmentItemDefinition equipmentDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(assetPath);
            if (equipmentDefinition == null
                || equipmentDefinition.AllowedSlot != expectedSlot
                || equipmentDefinition.ItemDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Primitive Equipment",
                    $"{assetName} equipment definition is invalid.");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "Primitive Equipment", $"{assetName} validated.");
        }

        private static void ValidateStarterProfileBoneRecipes(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(StarterProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Profile Bone Recipes",
                    $"Missing starter profile: {StarterProfilePath}");
                return;
            }

            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null || profile.BoneToolRecipes == null || profile.BoneToolRecipes.Length < 4)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Profile Bone Recipes",
                    "Starter loadout profile must register bone tool recipes.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Starter Profile Bone Recipes",
                "Starter profile bone tool recipes validated.");
        }

        private static void ValidateServiceBoneRecipeRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Service",
                    $"Missing service file: {ServicePath}");
                return;
            }

            string serviceSource = File.ReadAllText(ServicePath);
            if (serviceSource.Contains("BoneToolRecipes") && serviceSource.Contains("RegisterRecipeList"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Starter Loadout Service",
                    "Starter loadout service registers bone tool recipes.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Starter Loadout Service",
                "Starter loadout service is missing bone tool recipe registration.");
        }

        #endregion
    }
}
