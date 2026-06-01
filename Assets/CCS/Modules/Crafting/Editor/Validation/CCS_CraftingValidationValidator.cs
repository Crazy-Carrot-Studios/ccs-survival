using System.Collections.Generic;
using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
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
        private const string TestRecipesRoot = SurvivalRoot + "/Profiles/Crafting/TestRecipes";
        private const string TestItemsRoot = SurvivalRoot + "/Profiles/Crafting/TestItems";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Crafting_Module.md";
        private const string ProgressionDocPath = ModuleRoot + "/Documentation/CCS_Crafting_Progression.md";
        private const string ProgressionProfilePath =
            SurvivalRoot + "/Profiles/Crafting/CCS_DefaultCraftingProgressionProfile.asset";
        private const string ProgressionItemsRoot = SurvivalRoot + "/Content/Items/Progression";
        private const string ProgressionRecipesRoot = SurvivalRoot + "/Profiles/Crafting/ProgressionRecipes";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DefaultPlaytestProfilePath =
            SurvivalRoot + "/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string SpearItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Spear.asset";
        private const string ReinforcedSpearItemPath = ProgressionItemsRoot + "/CCS_Item_ReinforcedSpear.asset";

        private const string TestCampfireRecipePath = TestRecipesRoot + "/CCS_TestCampfireRecipe.asset";
        private const string TestBandageRecipePath = TestRecipesRoot + "/CCS_TestBandageRecipe.asset";
        private const string TestCampfireKitItemPath = TestItemsRoot + "/CCS_TestItem_CampfireKit.asset";
        private const string TestBandageItemPath = TestItemsRoot + "/CCS_TestItem_Bandage.asset";

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
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
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
            ValidateRequiredScript(report, "CCS_CraftingRuntimeBridge", RuntimeRoot + "/Services/CCS_CraftingRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_CraftingTestHarness", RuntimeRoot + "/Testing/CCS_CraftingTestHarness.cs");
            ValidateRequiredScript(report, "CCS_CraftingEventArgs", RuntimeRoot + "/Events/CCS_CraftingEventArgs.cs");
            ValidateRequiredScript(report, "CCS_CraftingEvents", RuntimeRoot + "/Events/CCS_CraftingEvents.cs");
            ValidateRequiredScript(report, "CCS_CraftingProfile", RuntimeRoot + "/Profiles/CCS_CraftingProfile.cs");
            ValidateRequiredScript(report, "CCS_CraftingValidationUtility", RuntimeRoot + "/Validation/CCS_CraftingValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_CraftingProgressionRecipeEntry", RuntimeRoot + "/Data/CCS_CraftingProgressionRecipeEntry.cs");
            ValidateRequiredScript(report, "CCS_CraftingProgressionProfile", RuntimeRoot + "/Profiles/CCS_CraftingProgressionProfile.cs");
            ValidateRequiredScript(report, "CCS_CraftingRecipeService", RuntimeRoot + "/Services/CCS_CraftingRecipeService.cs");
            ValidateRequiredScript(report, "CCS_CraftingStationInteractable", RuntimeRoot + "/Interactables/CCS_CraftingStationInteractable.cs");
            ValidateRequiredScript(report, "CCS_CraftingProgressionEventArgs", RuntimeRoot + "/Events/CCS_CraftingProgressionEventArgs.cs");
            ValidateRequiredScript(report, "CCS_CraftingProgressionEvents", RuntimeRoot + "/Events/CCS_CraftingProgressionEvents.cs");

            ValidateDocumentationAsset(report, "Crafting Module Doc", ModuleDocPath);
            ValidateDocumentationAsset(report, "Crafting Progression Doc", ProgressionDocPath);

            CCS_SurvivalValidationResult stationValidation =
                CCS_CraftingValidationUtility.ValidateRequiredStationTypes();

            report.AddIssue(
                stationValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Crafting Station Types",
                stationValidation.Message);

            ValidateRecipeDefinitionRules(report);
            ValidateGameplayServiceRegistration(report);
            ValidateBootstrapCraftingProfile(report);
            ValidateTestRecipeAssets(report);

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

            ValidateCraftingProgressionFoundation(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Crafting validator completed (1.1.1 crafting progression foundation).");
        }

        #endregion

        #region Private Methods

        private static void ValidateGameplayServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Crafting Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateCraftingService")
                && registrationSource.Contains("CreateCraftingRecipeService")
                && registrationSource.Contains("CCS_CraftingProfile")
                && registrationSource.Contains("craftingProgressionProfile"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Crafting Service Registration",
                    "Gameplay composition registers crafting and progression recipe services.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Crafting Service Registration",
                "Gameplay composition is missing crafting progression service registration wiring.");
        }

        private static void ValidateBootstrapCraftingProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Crafting Profile",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            GameObject bootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            Component gameplayServiceHost = FindGameplayServiceHost(bootstrapPrefab);
            if (gameplayServiceHost == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Crafting Profile",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(gameplayServiceHost);
            SerializedProperty craftingProfileProperty = serializedHost.FindProperty("craftingProfile");
            if (craftingProfileProperty != null && craftingProfileProperty.objectReferenceValue != null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Crafting Profile",
                    "Crafting profile assigned on bootstrap prefab gameplay service host.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Crafting Profile",
                "Crafting profile is not assigned on PF_CCS_Survival_BootstrapRoot.");
        }

        private static void ValidateTestRecipeAssets(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Crafting Test Recipes", TestRecipesRoot);
            ValidateRequiredFolder(report, "Crafting Test Items", TestItemsRoot);

            ValidateTestRecipeAsset(report, "Test Bandage Recipe", TestBandageRecipePath);
            ValidateTestRecipeAsset(report, "Test Campfire Recipe", TestCampfireRecipePath);
            ValidateTestItemAsset(report, "Test Bandage Item", TestBandageItemPath);
            ValidateTestItemAsset(report, "Test Campfire Kit Item", TestCampfireKitItemPath);
        }

        private static void ValidateTestRecipeAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Missing required test recipe asset: {assetPath}");
                return;
            }

            CCS_CraftingRecipeDefinition recipe =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(assetPath);

            if (recipe == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Could not load test recipe asset: {assetPath}");
                return;
            }

            CCS_SurvivalValidationResult recipeValidation =
                CCS_CraftingValidationUtility.ValidateRecipeDefinition(recipe);

            report.AddIssue(
                recipeValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                context,
                recipeValidation.Message);

            ValidateRecipeIngredientsAndResults(report, context, recipe);
        }

        private static void ValidateRecipeIngredientsAndResults(
            CCS_SurvivalValidationReport report,
            string context,
            CCS_CraftingRecipeDefinition recipe)
        {
            bool hasValidIngredient = false;
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = recipe.Ingredients[i];
                if (ingredient?.ItemDefinition != null && ingredient.Quantity > 0)
                {
                    hasValidIngredient = true;
                    break;
                }
            }

            if (!hasValidIngredient)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Test recipe is missing non-null ingredients with positive quantities.");
            }

            bool hasValidResult = false;
            for (int i = 0; i < recipe.Results.Count; i++)
            {
                CCS_CraftingResultDefinition result = recipe.Results[i];
                if (result?.ItemDefinition != null && result.Quantity > 0)
                {
                    hasValidResult = true;
                    break;
                }
            }

            if (!hasValidResult)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Test recipe is missing non-null results with positive quantities.");
            }

            if (System.Enum.IsDefined(typeof(CCS_CraftingStationType), recipe.RequiredStationType))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Station type validated: {recipe.RequiredStationType}.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Test recipe station type is invalid.");
            }
        }

        private static void ValidateTestItemAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Missing required test item asset: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                context,
                $"Test craft output item present: {assetPath}");
        }

        private static Component FindGameplayServiceHost(GameObject bootstrapPrefab)
        {
            if (bootstrapPrefab == null)
            {
                return null;
            }

            Component[] components = bootstrapPrefab.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component != null && component.GetType().Name == "CCS_SurvivalGameplayServiceHost")
                {
                    return component;
                }
            }

            return null;
        }

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

        private static void ValidateCraftingProgressionFoundation(CCS_SurvivalValidationReport report)
        {
            ValidateProgressionProfileAsset(report);
            ValidateProgressionRecipesByStation(report);
            ValidateProgressionItems(report);
            ValidateReinforcedSpearStats(report);
            ValidateBootstrapWorkbench(report);
            ValidatePlaytestWorkbenchStep(report);
        }

        private static void ValidateProgressionProfileAsset(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ProgressionProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Crafting Progression Profile",
                    $"Missing asset: {ProgressionProfilePath}. Run CCS_CraftingProgressionBootstrapSetup.ExecuteBatch.");
                return;
            }

            CCS_CraftingProgressionProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CraftingProgressionProfile>(ProgressionProfilePath);
            if (profile == null || !profile.ProgressionEnabled)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Crafting Progression Profile",
                    "Default crafting progression profile is missing or disabled.");
                return;
            }

            if (profile.ProfileVersion != "1.1.1")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Crafting Progression Profile",
                    $"Expected profileVersion 1.1.1 but found '{profile.ProfileVersion}'.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Crafting Progression Profile",
                $"Progression profile validated ({profile.ProgressionRecipes.Count} recipes).");
        }

        private static void ValidateProgressionRecipesByStation(CCS_SurvivalValidationReport report)
        {
            int handCount = CountRecipesForStation(CCS_CraftingStationType.Hand);
            int firePitCount = CountRecipesForStation(CCS_CraftingStationType.FirePit);
            int workbenchCount = CountRecipesForStation(CCS_CraftingStationType.Workbench);

            if (handCount < 3)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Hand Crafting Recipes",
                    $"Expected at least 3 hand recipes but found {handCount}.");
            }

            if (firePitCount < 3)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Campfire Crafting Recipes",
                    $"Expected at least 3 FirePit recipes but found {firePitCount}.");
            }

            if (workbenchCount < 3)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Workbench Crafting Recipes",
                    $"Expected at least 3 workbench recipes but found {workbenchCount}.");
            }
        }

        private static int CountRecipesForStation(CCS_CraftingStationType stationType)
        {
            if (!Directory.Exists(ProgressionRecipesRoot))
            {
                return 0;
            }

            string[] recipePaths = Directory.GetFiles(ProgressionRecipesRoot, "*.asset", SearchOption.TopDirectoryOnly);
            int count = 0;
            for (int index = 0; index < recipePaths.Length; index++)
            {
                CCS_CraftingRecipeDefinition recipe =
                    AssetDatabase.LoadAssetAtPath<CCS_CraftingRecipeDefinition>(recipePaths[index]);
                if (recipe != null && recipe.RequiredStationType == stationType)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidateProgressionItems(CCS_SurvivalValidationReport report)
        {
            ValidateProgressionItemAsset(report, "Basic Bandage", ProgressionItemsRoot + "/CCS_Item_BasicBandage.asset");
            ValidateProgressionItemAsset(report, "Primitive Torch", ProgressionItemsRoot + "/CCS_Item_PrimitiveTorch.asset");
            ValidateProgressionItemAsset(report, "Charcoal", ProgressionItemsRoot + "/CCS_Item_Charcoal.asset");
            ValidateProgressionItemAsset(report, "Ash", ProgressionItemsRoot + "/CCS_Item_Ash.asset");
            ValidateProgressionItemAsset(report, "Dried Meat", ProgressionItemsRoot + "/CCS_Item_DriedMeat.asset");
            ValidateProgressionItemAsset(report, "Reinforced Spear", ReinforcedSpearItemPath);
            ValidateProgressionItemAsset(report, "Storage Crate", ProgressionItemsRoot + "/CCS_Item_StorageCrate.asset");
        }

        private static void ValidateProgressionItemAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Crafting Progression Items",
                    $"Missing {label} at {assetPath}.");
            }
        }

        private static void ValidateReinforcedSpearStats(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition spear = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(SpearItemPath);
            CCS_ItemDefinition reinforcedSpear =
                AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(ReinforcedSpearItemPath);
            if (spear == null || reinforcedSpear == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Reinforced Spear Stats",
                    "Spear or reinforced spear item definition is missing.");
                return;
            }

            if (!reinforcedSpear.HasWeaponIdentity)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Reinforced Spear Stats",
                    "Reinforced spear must have weapon identity enabled.");
            }

            if (reinforcedSpear.MeleeDamage <= spear.MeleeDamage)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Reinforced Spear Stats",
                    "Reinforced spear melee damage must exceed starter spear damage.");
            }

            if (reinforcedSpear.MeleeRange < spear.MeleeRange)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Reinforced Spear Stats",
                    "Reinforced spear melee range must be equal or higher than starter spear.");
            }
        }

        private static void ValidateBootstrapWorkbench(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Workbench",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (!sceneText.Contains("CCS_TestWorkbench"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Workbench",
                    "Bootstrap scene is missing CCS_TestWorkbench.");
                return;
            }

            if (!sceneText.Contains("CCS_CraftingStationInteractable"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Workbench",
                    "Bootstrap scene may be missing CCS_CraftingStationInteractable on workbench.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Workbench",
                    "CCS_TestWorkbench is present in bootstrap scene.");
            }
        }

        private static void ValidatePlaytestWorkbenchStep(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Playtest Workbench Step",
                    $"Missing playtest profile at {DefaultPlaytestProfilePath}.");
                return;
            }

            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                CCS_PlaytestStepDefinition step = steps[index];
                if (step != null && step.StepType == CCS_PlaytestStepType.CraftAtWorkbench)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Playtest Workbench Step",
                        "Default playtest profile includes CraftAtWorkbench step.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Playtest Workbench Step",
                "Default playtest profile is missing CraftAtWorkbench step.");
        }

        #endregion
    }
}
