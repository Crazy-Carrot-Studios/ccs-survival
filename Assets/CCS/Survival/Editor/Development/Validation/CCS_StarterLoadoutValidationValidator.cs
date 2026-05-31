using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Validates starter loadout profile, items, recipes, and composition wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Confirms primitive progression foundation for milestone 0.9.1.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_StarterLoadoutValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string StarterItemsRoot = SurvivalRoot + "/Content/Items/Starter";
        private const string StarterProfilePath = SurvivalRoot + "/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string PrimitiveRecipesRoot = SurvivalRoot + "/Profiles/Crafting/PrimitiveRecipes";
        private const string TreeResourcePath = SurvivalRoot + "/Profiles/WorldResources/TestResources/CCS_TestResource_Tree.asset";
        private const string RegistrationPath = SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.starterloadout";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Starter Items", StarterItemsRoot);
            ValidateStarterItem(report, "CCS_Item_Knife", "Knife", true, CCS_ItemToolType.Knife);
            ValidateStarterItem(report, "CCS_Item_BasicFood", "Basic Food", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_Coin", "Coin", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_Branch", "Branch", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_Spear", "Spear", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_BowStave", "Bow Stave", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_ArrowShaft", "Arrow Shaft", false, CCS_ItemToolType.None);
            ValidateStarterItem(report, "CCS_Item_CampfireKit", "Campfire Kit", false, CCS_ItemToolType.None);

            ValidateRequiredScript(report, "CCS_StarterLoadoutProfile", SurvivalRoot + "/Runtime/Player/Loadout/CCS_StarterLoadoutProfile.cs");
            ValidateRequiredScript(report, "CCS_StarterLoadoutService", SurvivalRoot + "/Runtime/Player/Loadout/CCS_StarterLoadoutService.cs");
            ValidateRequiredScript(report, "CCS_InventoryToolUtility", "Assets/CCS/Modules/Inventory/Runtime/Utilities/CCS_InventoryToolUtility.cs");

            ValidateStarterLoadoutProfileAsset(report);
            ValidatePrimitiveRecipes(report);
            ValidateTreeKnifeHarvest(report);
            ValidateCompositionWiring(report);
            ValidateBootstrapHostWiring(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Starter loadout validator completed.");
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

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required script: {scriptPath}");
        }

        private static void ValidateStarterItem(
            CCS_SurvivalValidationReport report,
            string assetName,
            string displayName,
            bool expectsToolIdentity,
            CCS_ItemToolType expectedToolType)
        {
            string assetPath = $"{StarterItemsRoot}/{assetName}.asset";
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Item Definitions",
                    $"Missing starter item asset: {assetPath}");
                return;
            }

            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Item Definitions",
                    $"Could not load starter item asset: {assetPath}");
                return;
            }

            if (itemDefinition.DisplayName != displayName)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Starter Item Definitions",
                    $"{assetName} display name is '{itemDefinition.DisplayName}', expected '{displayName}'.");
            }

            if (itemDefinition.HasToolIdentity != expectsToolIdentity
                || itemDefinition.ToolType != expectedToolType)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Item Definitions",
                    $"{assetName} tool identity does not match expected harvest tool mapping.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Starter Item Definitions",
                $"{assetName} validated.");
        }

        private static void ValidateStarterLoadoutProfileAsset(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(StarterProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Profile",
                    $"Missing profile asset: {StarterProfilePath}");
                return;
            }

            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Profile",
                    $"Could not load profile asset: {StarterProfilePath}");
                return;
            }

            if (!profile.ApplyWhenInventoryEmpty)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Profile",
                    "Starter loadout profile must apply only when inventory is empty.");
            }

            if (profile.StartingCurrencyAmount <= 0 || profile.CurrencyItemDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Profile",
                    "Starter loadout profile must define currency item and starting amount.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Starter Loadout Profile",
                $"Profile present: {StarterProfilePath}");
        }

        private static void ValidatePrimitiveRecipes(CCS_SurvivalValidationReport report)
        {
            string[] recipeNames =
            {
                "CCS_PrimitiveSpearRecipe",
                "CCS_PrimitiveBowStaveRecipe",
                "CCS_PrimitiveArrowShaftRecipe",
                "CCS_PrimitiveCampfireKitRecipe"
            };

            for (int index = 0; index < recipeNames.Length; index++)
            {
                string assetPath = $"{PrimitiveRecipesRoot}/{recipeNames[index]}.asset";
                if (!File.Exists(assetPath))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Primitive Crafting Recipes",
                        $"Missing recipe asset: {assetPath}");
                    continue;
                }

                CCS_CraftingRecipeDefinition recipe =
                    AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(assetPath);
                if (recipe == null || recipe.RequiredStationType != CCS_CraftingStationType.Hand)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Primitive Crafting Recipes",
                        $"{recipeNames[index]} must be a hand crafting recipe.");
                    continue;
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Primitive Crafting Recipes",
                    $"{recipeNames[index]} validated.");
            }
        }

        private static void ValidateTreeKnifeHarvest(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(TreeResourcePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Tree Knife Harvest",
                    $"Missing tree resource definition: {TreeResourcePath}");
                return;
            }

            CCS_ResourceDefinition treeDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(TreeResourcePath);
            if (treeDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Tree Knife Harvest",
                    "Could not load test tree resource definition.");
                return;
            }

            if (treeDefinition.RequiredToolType != CCS_RequiredToolType.Knife)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Tree Knife Harvest",
                    "Test tree resource must require Knife for early-game harvesting.");
                return;
            }

            bool dropsBranch = false;
            for (int index = 0; index < treeDefinition.DropDefinitions.Count; index++)
            {
                CCS_ResourceDropDefinition drop = treeDefinition.DropDefinitions[index];
                if (drop?.ItemDefinition != null
                    && drop.ItemDefinition.ItemId.Contains("branch", System.StringComparison.OrdinalIgnoreCase))
                {
                    dropsBranch = true;
                    break;
                }
            }

            if (!dropsBranch)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Tree Knife Harvest",
                    "Test tree resource must drop Branch items for primitive progression.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Tree Knife Harvest",
                "Test tree resource uses Knife harvest and Branch drops.");
        }

        private static void ValidateCompositionWiring(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(RegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Composition",
                    $"Missing registration file: {RegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(RegistrationPath);
            if (registrationSource.Contains("CCS_StarterLoadoutService")
                && registrationSource.Contains("TryApplyStarterLoadout")
                && registrationSource.Contains("RegisterPrimitiveRecipes"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Starter Loadout Composition",
                    "Gameplay composition registers and applies starter loadout safely.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Starter Loadout Composition",
                "Gameplay composition is missing starter loadout service wiring.");
        }

        private static void ValidateBootstrapHostWiring(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Starter Loadout Bootstrap Host",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            GameObject bootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = bootstrapPrefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Bootstrap Host",
                    "Bootstrap root is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            SerializedProperty profileProperty = serializedHost.FindProperty("starterLoadoutProfile");
            if (profileProperty != null && profileProperty.objectReferenceValue != null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Starter Loadout Bootstrap Host",
                    "Bootstrap gameplay service host assigns starter loadout profile.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Starter Loadout Bootstrap Host",
                "Bootstrap gameplay service host is missing starter loadout profile assignment.");
        }

        #endregion
    }
}
