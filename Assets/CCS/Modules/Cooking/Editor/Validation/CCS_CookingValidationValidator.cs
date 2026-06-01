using System.Collections.Generic;
using System.IO;
using CCS.Modules.Building;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.Cooking;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using CCS.Survival.Player;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingValidationValidator
// CATEGORY: Modules / Cooking / Editor / Validation
// PURPOSE: Validates cooking module folders, asmdefs, profile assets, and bootstrap content.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Validates campfire test area in SCN_CCS_Survival_Bootstrap.
// =============================================================================

namespace CCS.Modules.Cooking.Editor
{
    public sealed class CCS_CookingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Cooking";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Cooking/CCS_DefaultCookingProfile.asset";
        private const string DefaultCampfireDefinitionPath =
            SurvivalRoot + "/Content/Cooking/Definitions/CCS_TestCampfireDefinition.asset";
        private const string CookedMeatItemPath = SurvivalRoot + "/Content/Items/Food/CCS_Item_CookedMeat.asset";
        private const string RawMeatItemPath = SurvivalRoot + "/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset";
        private const string BasicFoodItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_BasicFood.asset";
        private const string CampfireKitItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_CampfireKit.asset";
        private const string CampfireBuildingPiecePath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestCampfire.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Cooking_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string PlayerPrefabPath = SurvivalRoot + "/Prefabs/Player/PF_CCS_Player.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string HudPresentationPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudPresentationService.cs";
        private const string HudWiringPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudGameplayServiceWiring.cs";

        private static readonly string[] RequiredTestObjectNames =
        {
            "CCS_CampfireTestArea",
            "CCS_TestCampfire"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.cooking";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Cooking", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Interactables", RuntimeRoot + "/Interactables");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Cooking.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Cooking.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_CampfireDefinition", RuntimeRoot + "/Definitions/CCS_CampfireDefinition.cs");
            ValidateRequiredScript(report, "CCS_ConsumableFoodDefinition", RuntimeRoot + "/Definitions/CCS_ConsumableFoodDefinition.cs");
            ValidateRequiredScript(report, "CCS_CampfireState", RuntimeRoot + "/Data/CCS_CampfireState.cs");
            ValidateRequiredScript(report, "CCS_CampfireInstanceState", RuntimeRoot + "/Data/CCS_CampfireInstanceState.cs");
            ValidateRequiredScript(report, "CCS_CampfireSnapshot", RuntimeRoot + "/Data/CCS_CampfireSnapshot.cs");
            ValidateRequiredScript(report, "CCS_CookingRequest", RuntimeRoot + "/Data/CCS_CookingRequest.cs");
            ValidateRequiredScript(report, "CCS_CookingResult", RuntimeRoot + "/Data/CCS_CookingResult.cs");
            ValidateRequiredScript(report, "CCS_ConsumableFoodResult", RuntimeRoot + "/Data/CCS_ConsumableFoodResult.cs");
            ValidateRequiredScript(report, "CCS_CampfireInteractable", RuntimeRoot + "/Interactables/CCS_CampfireInteractable.cs");
            ValidateRequiredScript(report, "CCS_CookingService", RuntimeRoot + "/Services/CCS_CookingService.cs");
            ValidateRequiredScript(report, "CCS_CampfireService", RuntimeRoot + "/Services/CCS_CampfireService.cs");
            ValidateRequiredScript(report, "CCS_ConsumableFoodService", RuntimeRoot + "/Services/CCS_ConsumableFoodService.cs");
            ValidateRequiredScript(report, "CCS_CookingRuntimeBridge", RuntimeRoot + "/Services/CCS_CookingRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_CookingEventArgs", RuntimeRoot + "/Events/CCS_CookingEventArgs.cs");
            ValidateRequiredScript(report, "CCS_CookingEvents", RuntimeRoot + "/Events/CCS_CookingEvents.cs");
            ValidateRequiredScript(report, "CCS_CookingProfile", RuntimeRoot + "/Profiles/CCS_CookingProfile.cs");
            ValidateRequiredScript(report, "CCS_CookingValidationUtility", RuntimeRoot + "/Validation/CCS_CookingValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_ConsumableFoodPlayerDriver", SurvivalRoot + "/Runtime/Player/CCS_ConsumableFoodPlayerDriver.cs");
            ValidateRequiredScript(report, "CCS_CampfireBuildingPlayerDriver", SurvivalRoot + "/Runtime/Player/CCS_CampfireBuildingPlayerDriver.cs");

            ValidateDocumentationAsset(report, "Cooking Module Doc", ModuleDocPath);

            report.AddIssue(
                typeof(CCS_IInteractableResultProvider).IsAssignableFrom(typeof(CCS_CampfireInteractable))
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Campfire Interaction Contract",
                "CCS_CampfireInteractable implements CCS_IInteractableResultProvider for cooking integration.");

            ValidateRequiredAsset(report, "Default Cooking Profile", DefaultProfilePath);
            ValidateRequiredAsset(report, "Test Campfire Definition", DefaultCampfireDefinitionPath);
            ValidateRequiredAsset(report, "Cooked Meat Item", CookedMeatItemPath);
            ValidateRequiredAsset(report, "Raw Meat Item", RawMeatItemPath);
            ValidateRequiredAsset(report, "Basic Food Item", BasicFoodItemPath);
            ValidateRequiredAsset(report, "Campfire Kit Item", CampfireKitItemPath);
            ValidateRequiredAsset(report, "Campfire Building Piece", CampfireBuildingPiecePath);

            ValidateCookingProfileAsset(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidateBootstrapTestObjects(report);
            ValidatePlayerPrefabDrivers(report);
            ValidateCompositionRegistration(report);
            ValidateHudIntegration(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);

            CCS_SurvivalValidationResult stationValidation = CCS_CookingValidationUtility.ValidateCookingStationType();
            report.AddIssue(
                stationValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "FirePit Station Type",
                stationValidation.Message);
        }

        #endregion

        #region Private Methods

        private static void ValidateCookingProfileAsset(CCS_SurvivalValidationReport report)
        {
            CCS_CookingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CookingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Cooking Profile",
                    $"Missing asset: {DefaultProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CookingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Cooking Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "0.9.5")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Cooking Profile Version",
                    $"Expected profileVersion 0.9.5 but found '{profile.ProfileVersion}'.");
            }

            ValidateConsumableFoodDefinitions(report, profile);
        }

        private static void ValidateConsumableFoodDefinitions(
            CCS_SurvivalValidationReport report,
            CCS_CookingProfile profile)
        {
            bool hasBasicFood = false;
            bool hasCookedMeat = false;

            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = profile.ConsumableFoodDefinitions;
            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = consumableDefinitions[index];
                if (consumableDefinition?.ItemDefinition == null)
                {
                    continue;
                }

                string displayName = consumableDefinition.ResolveNotificationDisplayName();
                if (displayName.IndexOf("Basic Food", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && Mathf.Approximately(consumableDefinition.HungerRestoreAmount, 15f))
                {
                    hasBasicFood = true;
                }

                if (displayName.IndexOf("Cooked Meat", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && Mathf.Approximately(consumableDefinition.HungerRestoreAmount, 40f))
                {
                    hasCookedMeat = true;
                }
            }

            report.AddIssue(
                hasBasicFood
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Basic Food Restore",
                hasBasicFood
                    ? "Basic Food restores +15 hunger."
                    : "Basic Food restore amount is missing or invalid.");

            report.AddIssue(
                hasCookedMeat
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Cooked Meat Restore",
                hasCookedMeat
                    ? "Cooked Meat restores +40 hunger."
                    : "Cooked Meat restore amount is missing or invalid.");
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Host",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Host",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object cookingProfile = serializedHost.FindProperty("cookingProfile").objectReferenceValue;
            report.AddIssue(
                cookingProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Cooking Profile Wiring",
                cookingProfile != null
                    ? "Bootstrap gameplay host references CCS_DefaultCookingProfile."
                    : "Bootstrap gameplay host is missing cookingProfile assignment.");
        }

        private static void ValidateBootstrapTestObjects(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Scene",
                    $"Missing scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int index = 0; index < RequiredTestObjectNames.Length; index++)
            {
                string objectName = RequiredTestObjectNames[index];
                report.AddIssue(
                    sceneText.Contains(objectName)
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Cooking Test Objects",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_CampfireInteractable")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Campfire Interactable",
                "Bootstrap scene includes CCS_CampfireInteractable for cooking verification.");
        }

        private static void ValidatePlayerPrefabDrivers(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(PlayerPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Prefab",
                    $"Missing prefab: {PlayerPrefabPath}");
                return;
            }

            string prefabText = File.ReadAllText(PlayerPrefabPath);
            report.AddIssue(
                prefabText.Contains(nameof(CCS_ConsumableFoodPlayerDriver))
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Player Consumable Driver",
                "Player prefab includes CCS_ConsumableFoodPlayerDriver.");

            report.AddIssue(
                prefabText.Contains(nameof(CCS_CampfireBuildingPlayerDriver))
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Player Campfire Building Driver",
                "Player prefab includes CCS_CampfireBuildingPlayerDriver.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Composition Registration",
                    $"Missing script: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            report.AddIssue(
                source.Contains("CreateCookingService")
                    && source.Contains("CreateCampfireService")
                    && source.Contains("CreateConsumableFoodService")
                    && source.Contains("RegisterCookingUpdatable")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Cooking Service Registration",
                "Gameplay composition registers cooking, campfire, and consumable food services.");
        }

        private static void ValidateHudIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(HudPresentationPath) || !File.Exists(HudWiringPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Cooking Integration",
                    "HUD presentation or wiring scripts are missing.");
                return;
            }

            string presentationSource = File.ReadAllText(HudPresentationPath);
            string wiringSource = File.ReadAllText(HudWiringPath);

            report.AddIssue(
                presentationSource.Contains("BindCookingService")
                    && presentationSource.Contains("BindCampfireService")
                    &&                 presentationSource.Contains("BindConsumableFoodService")
                    && presentationSource.Contains("FoodConsumeFailed")
                    && presentationSource.Contains("Cannot eat: Hunger Full")
                    && presentationSource.Contains("Cannot eat: No Food")
                    && presentationSource.Contains("You are hungry")
                    && presentationSource.Contains("Campfire Lit")
                    && presentationSource.Contains("Cooking Started")
                    && presentationSource.Contains("Cooking Complete")
                    && wiringSource.Contains("BindCookingService")
                    && wiringSource.Contains("BindCampfireService")
                    && wiringSource.Contains("BindConsumableFoodService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "HUD Cooking Notifications",
                "HUD binds cooking services and queues campfire/cooking/food notifications.");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string folderPath)
        {
            report.AddIssue(
                AssetDatabase.IsValidFolder(folderPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                AssetDatabase.IsValidFolder(folderPath)
                    ? $"Folder exists: {folderPath}"
                    : $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            report.AddIssue(
                File.Exists(assetPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                File.Exists(assetPath)
                    ? $"File exists: {assetPath}"
                    : $"Missing file: {assetPath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            ValidateRequiredFile(report, label, scriptPath);
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            report.AddIssue(
                asset != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                asset != null
                    ? $"Asset exists: {assetPath}"
                    : $"Missing asset: {assetPath}");
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            ValidateRequiredFile(report, label, assetPath);
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script", new[] { runtimeRoot });
            for (int index = 0; index < scriptGuids.Length; index++)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuids[index]);
                if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
                {
                    continue;
                }

                string source = File.ReadAllText(scriptPath);
                if (source.Contains("UnityEditor"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Editor Leak",
                        $"{scriptPath} references UnityEditor.");
                }
            }
        }

        #endregion
    }
}
