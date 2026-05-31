using System.IO;
using CCS.Modules.Building;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingValidationValidator
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Validates building module folders, asmdefs, profile, definitions, and wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Construction cost and inventory integration checks for 0.8.2.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    public sealed class CCS_BuildingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Building";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Building/CCS_DefaultBuildingProfile.asset";
        private const string TestFoundationPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestFoundation.asset";
        private const string TestWallPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestWall.asset";
        private const string TestRoofPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestRoof.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Building_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string GameplayServiceHostPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceHost.cs";
        private const string SaveableIdsPath =
            "Assets/CCS/Modules/SaveLoad/Runtime/Data/CCS_SaveLoadSaveableIds.cs";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string EnvironmentHudPresenterPath =
            "Assets/CCS/Modules/EnvironmentEffects/Runtime/Presentation/CCS_EnvironmentEffectsHudPresenter.cs";
        private const string BuildingValidationUtilityPath =
            RuntimeRoot + "/Validation/CCS_BuildingValidationUtility.cs";
        private const string PlacementValidationUtilityPath =
            RuntimeRoot + "/Validation/CCS_BuildingPlacementValidationUtility.cs";
        private const string HudPresentationServicePath =
            "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudPresentationService.cs";
        private const string HudWiringPath =
            "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudGameplayServiceWiring.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.building";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Building", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Placement", RuntimeRoot + "/Placement");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Building.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Building.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_BuildingPieceType", RuntimeRoot + "/Definitions/CCS_BuildingPieceType.cs");
            ValidateRequiredScript(report, "CCS_BuildingPieceDefinition", RuntimeRoot + "/Definitions/CCS_BuildingPieceDefinition.cs");
            ValidateRequiredScript(report, "CCS_BuildingCostEntry", RuntimeRoot + "/Definitions/CCS_BuildingCostEntry.cs");
            ValidateRequiredScript(report, "CCS_BuildingPieceSnapshot", RuntimeRoot + "/Data/CCS_BuildingPieceSnapshot.cs");
            ValidateRequiredScript(report, "CCS_BuildingState", RuntimeRoot + "/Data/CCS_BuildingState.cs");
            ValidateRequiredScript(report, "CCS_BuildingSaveData", RuntimeRoot + "/Data/CCS_BuildingSaveData.cs");
            ValidateRequiredScript(report, "CCS_BuildingProfile", RuntimeRoot + "/Profiles/CCS_BuildingProfile.cs");
            ValidateRequiredScript(report, "CCS_BuildingService", RuntimeRoot + "/Services/CCS_BuildingService.cs");
            ValidateRequiredScript(report, "CCS_BuildingRuntimeBridge", RuntimeRoot + "/Services/CCS_BuildingRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_BuildingEvents", RuntimeRoot + "/Events/CCS_BuildingEvents.cs");
            ValidateRequiredScript(report, "CCS_BuildingEventArgs", RuntimeRoot + "/Events/CCS_BuildingEventArgs.cs");
            ValidateRequiredScript(report, "CCS_BuildingValidationUtility", BuildingValidationUtilityPath);
            ValidateRequiredScript(report, "CCS_BuildingPlacementValidationResult", RuntimeRoot + "/Validation/CCS_BuildingPlacementValidationResult.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementValidationUtility", PlacementValidationUtilityPath);
            ValidateRequiredScript(report, "CCS_BuildingInstance", RuntimeRoot + "/Data/CCS_BuildingInstance.cs");
            ValidateRequiredScript(report, "CCS_BuildingInstanceSaveRecord", RuntimeRoot + "/Data/CCS_BuildingInstanceSaveRecord.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementState", RuntimeRoot + "/Data/CCS_BuildingPlacementState.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementSnapshot", RuntimeRoot + "/Data/CCS_BuildingPlacementSnapshot.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementService", RuntimeRoot + "/Services/CCS_BuildingPlacementService.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementPreview", RuntimeRoot + "/Placement/CCS_BuildingPlacementPreview.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementTestHarness", RuntimeRoot + "/Testing/CCS_BuildingPlacementTestHarness.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementEventArgs", RuntimeRoot + "/Events/CCS_BuildingPlacementEventArgs.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementFailedEventArgs", RuntimeRoot + "/Events/CCS_BuildingPlacementFailedEventArgs.cs");

            ValidateDocumentationAsset(report, "Building Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateTestDefinitions(report);
            ValidateBuildCostIntegration(report);
            ValidateSaveIntegration(report);
            ValidatePlacementIntegration(report);
            ValidateInventoryIntegration(report);
            ValidateServiceRegistration(report);
            ValidateRestoreOrder(report);
            ValidateHudDisplay(report);
            ValidateBuildingNotifications(report);
            ValidateBootstrapTestArea(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Building validator completed (0.8.2 construction costs and placement validation).");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Building Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Building Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_BuildingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BuildingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Building Profile",
                    "Could not load default building profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_BuildingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Building Profile",
                profileValidation.Message);
        }

        private static void ValidateTestDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateTestDefinitionAsset(report, "CCS_TestFoundation", TestFoundationPath);
            ValidateTestDefinitionAsset(report, "CCS_TestWall", TestWallPath);
            ValidateTestDefinitionAsset(report, "CCS_TestRoof", TestRoofPath);
        }

        private static void ValidateTestDefinitionAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Missing required definition asset: {assetPath}");
                return;
            }

            CCS_BuildingPieceDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(assetPath);

            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Could not load definition asset: {assetPath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_BuildingValidationUtility.ValidateDefinition(definition);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                context,
                validation.Message);

            if (definition.BuildCostEntries == null || definition.BuildCostEntries.Count == 0)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Definition '{definition.PieceId}' is missing build cost entries.");
            }
        }

        private static void ValidateBuildCostIntegration(CCS_SurvivalValidationReport report)
        {
            CCS_BuildingPieceDefinition foundation =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestFoundationPath);
            CCS_BuildingPieceDefinition wall =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestWallPath);
            CCS_BuildingPieceDefinition roof =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestRoofPath);

            ValidateDefinitionCostCount(report, "Foundation Costs", foundation, 2);
            ValidateDefinitionCostCount(report, "Wall Costs", wall, 1);
            ValidateDefinitionCostCount(report, "Roof Costs", roof, 2);
        }

        private static void ValidateDefinitionCostCount(
            CCS_SurvivalValidationReport report,
            string context,
            CCS_BuildingPieceDefinition definition,
            int expectedCount)
        {
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Definition asset could not be loaded.");
                return;
            }

            int count = definition.BuildCostEntries?.Count ?? 0;
            if (count == expectedCount)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Definition '{definition.PieceId}' has {count} build cost entries.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Definition '{definition.PieceId}' expected {expectedCount} build cost entries, found {count}.");
        }

        private static void ValidateInventoryIntegration(CCS_SurvivalValidationReport report)
        {
            const string placementServicePath = RuntimeRoot + "/Services/CCS_BuildingPlacementService.cs";
            if (!File.Exists(placementServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Inventory Integration",
                    $"Missing placement service script: {placementServicePath}");
                return;
            }

            string placementSource = File.ReadAllText(placementServicePath);
            if (placementSource.Contains("BindInventoryService")
                && placementSource.Contains("TryPlaceCurrentPiece")
                && placementSource.Contains("CCS_BuildingPlacementValidationResult"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Inventory Integration",
                    "CCS_BuildingPlacementService validates inventory costs through TryPlaceCurrentPiece.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Inventory Integration",
                    "CCS_BuildingPlacementService is missing inventory-aware TryPlaceCurrentPiece flow.");
            }

            if (!File.Exists(PlacementValidationUtilityPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Inventory Integration",
                    $"Missing placement validation utility: {PlacementValidationUtilityPath}");
                return;
            }

            string utilitySource = File.ReadAllText(PlacementValidationUtilityPath);
            if (utilitySource.Contains("ValidateInventoryCosts")
                && utilitySource.Contains("TryConsumeBuildCosts")
                && utilitySource.Contains("RestoreConsumedEntries")
                && utilitySource.Contains("RestoreBuildCosts"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Cost Rollback",
                    "Placement validation utility restores consumed items on partial failure.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Cost Rollback",
                    "Placement validation utility is missing rollback protection.");
            }

            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("BindInventoryService(inventoryService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Inventory Wiring",
                    "Gameplay composition binds inventory service to building placement.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Inventory Wiring",
                    "Gameplay composition is missing inventory binding for building placement.");
            }
        }

        private static void ValidateBuildingNotifications(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(HudPresentationServicePath) || !File.Exists(HudWiringPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building HUD Notifications",
                    "HUD presentation or wiring scripts are missing.");
                return;
            }

            string presentationSource = File.ReadAllText(HudPresentationServicePath);
            string wiringSource = File.ReadAllText(HudWiringPath);
            if (presentationSource.Contains("BindBuildingPlacementService")
                && presentationSource.Contains("HandleBuildingPlaced")
                && presentationSource.Contains("HandleBuildingPlacementFailed")
                && wiringSource.Contains("BindBuildingPlacementService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building HUD Notifications",
                    "HUD presentation subscribes to building placement success and failure events.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Building HUD Notifications",
                "HUD presentation is missing building placement notification wiring.");
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_BuildingService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Save Integration",
                    $"Missing service script: {servicePath}");
                return;
            }

            string serviceSource = File.ReadAllText(servicePath);
            if (serviceSource.Contains("CCS_ISaveable")
                && serviceSource.Contains("CaptureState")
                && serviceSource.Contains("RestoreState")
                && serviceSource.Contains("saveDataVersion")
                && serviceSource.Contains("placedInstanceRecords"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Save Integration",
                    "CCS_BuildingService implements CCS_ISaveable with definition and placed instance save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Save Integration",
                    "CCS_BuildingService is missing CCS_ISaveable persistence implementation.");
            }

            const string saveDataPath = RuntimeRoot + "/Data/CCS_BuildingSaveData.cs";
            if (File.Exists(saveDataPath))
            {
                string saveDataSource = File.ReadAllText(saveDataPath);
                if (saveDataSource.Contains("placedInstanceRecords")
                    && saveDataSource.Contains("CCS_BuildingInstanceSaveRecord"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Save Model",
                        "CCS_BuildingSaveData includes placed instance records.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Save Model",
                        "CCS_BuildingSaveData is missing placed instance records.");
                }
            }
        }

        private static void ValidatePlacementIntegration(CCS_SurvivalValidationReport report)
        {
            const string placementServicePath = RuntimeRoot + "/Services/CCS_BuildingPlacementService.cs";
            const string previewPath = RuntimeRoot + "/Placement/CCS_BuildingPlacementPreview.cs";
            const string harnessPath = RuntimeRoot + "/Testing/CCS_BuildingPlacementTestHarness.cs";
            const string instancePath = RuntimeRoot + "/Data/CCS_BuildingInstance.cs";
            const string buildingServicePath = RuntimeRoot + "/Services/CCS_BuildingService.cs";

            ValidateRequiredScript(report, "CCS_BuildingPlacementService", placementServicePath);
            ValidateRequiredScript(report, "CCS_BuildingPlacementPreview", previewPath);
            ValidateRequiredScript(report, "CCS_BuildingPlacementTestHarness", harnessPath);
            ValidateRequiredScript(report, "CCS_BuildingInstance", instancePath);

            if (File.Exists(buildingServicePath))
            {
                string serviceSource = File.ReadAllText(buildingServicePath);
                if (serviceSource.Contains("GetPlacedInstances")
                    && serviceSource.Contains("GetPlacementSnapshot")
                    && serviceSource.Contains("TryAddPlacedInstance"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Placement Integration",
                        "CCS_BuildingService tracks placed instances and exposes placement snapshots.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Placement Integration",
                        "CCS_BuildingService is missing placed instance integration.");
                }
            }

            if (File.Exists(harnessPath))
            {
                string harnessSource = File.ReadAllText(harnessPath);
                if (harnessSource.Contains("TryPlaceCurrentPiece")
                    && harnessSource.Contains("TrySeedTestResources"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Placement Test Harness",
                        "Placement harness uses TryPlaceCurrentPiece and seeds test resources.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Placement Test Harness",
                        "Placement harness is missing inventory-aware TryPlaceCurrentPiece flow.");
                }
            }

            if (File.Exists(RuntimeRoot + "/Events/CCS_BuildingEvents.cs"))
            {
                string eventsSource = File.ReadAllText(RuntimeRoot + "/Events/CCS_BuildingEvents.cs");
                if (eventsSource.Contains("PlacementStarted")
                    && eventsSource.Contains("PlacementCancelled")
                    && eventsSource.Contains("BuildingPlaced")
                    && eventsSource.Contains("PlacementFailed"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Placement Events",
                        "Building events include placement lifecycle and failure hooks.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Placement Events",
                        "Building events are missing placement lifecycle hooks.");
                }
            }
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateBuildingService")
                && registrationSource.Contains("CreateBuildingPlacementService")
                && registrationSource.Contains("RegisterSaveable(buildingService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Service Registration",
                    "Gameplay composition registers, save-registers, and binds building and placement services.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Service Registration",
                    "Gameplay composition is missing building service registration wiring.");
            }

            if (File.Exists(GameplayServiceHostPath))
            {
                string hostSource = File.ReadAllText(GameplayServiceHostPath);
                if (hostSource.Contains("buildingProfile"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost references buildingProfile.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost is missing buildingProfile reference.");
                }
            }
        }

        private static void ValidateRestoreOrder(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveableIdsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Restore Order",
                    $"Missing saveable ids script: {SaveableIdsPath}");
                return;
            }

            string saveableIdsSource = File.ReadAllText(SaveableIdsPath);
            if (saveableIdsSource.Contains("GlobalEnvironment")
                && saveableIdsSource.Contains("GlobalBuilding")
                && saveableIdsSource.Contains("ModuleRestoreOrder"))
            {
                int environmentIndex = saveableIdsSource.IndexOf("GlobalEnvironment", System.StringComparison.Ordinal);
                int buildingIndex = saveableIdsSource.IndexOf("GlobalBuilding", System.StringComparison.Ordinal);
                if (environmentIndex >= 0 && buildingIndex > environmentIndex)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Restore Order",
                        "Saveable restore order includes building after environment.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Building Restore Order",
                "Saveable restore order is missing building after environment.");
        }

        private static void ValidateHudDisplay(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(EnvironmentHudPresenterPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building HUD Display",
                    $"Missing environment HUD presenter: {EnvironmentHudPresenterPath}");
                return;
            }

            string presenterSource = File.ReadAllText(EnvironmentHudPresenterPath);
            if (presenterSource.Contains("FormatBuildingDefinitionCountLine")
                && presenterSource.Contains("FormatPlacementHudLines")
                && presenterSource.Contains("CCS_BuildingRuntimeBridge"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building HUD Display",
                    "Environment HUD presenter displays building definition and placement debug values.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Building HUD Display",
                "Environment HUD presenter is missing building placement display.");
        }

        private static void ValidateBootstrapTestArea(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Building Test Area",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains("CCS_BuildingTestArea")
                && sceneText.Contains("CCS_BuildingPlacementTestHarness")
                && sceneText.Contains("CCS_BuildingPlacementPreview"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Building Test Area",
                    "Bootstrap scene includes CCS_BuildingTestArea with preview and placement harness.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Building Test Area",
                "Bootstrap scene is missing CCS_BuildingTestArea test setup.");
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

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            bool foundEditorReference = false;
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script", new[] { runtimeRoot });
            for (int index = 0; index < scriptGuids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scriptGuids[index]);
                if (ScriptContainsUnityEditorReference(File.ReadAllText(path)))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Script Editor Reference",
                        $"Runtime script references UnityEditor: {path}");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Editor Reference",
                    "Runtime building scripts do not reference UnityEditor.");
            }
        }

        private static bool ScriptContainsUnityEditorReference(string contents)
        {
            string[] lines = contents.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string trimmedLine = lines[lineIndex].Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                int commentIndex = trimmedLine.IndexOf("//", System.StringComparison.Ordinal);
                if (commentIndex >= 0)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                if (trimmedLine.StartsWith("using UnityEditor", System.StringComparison.Ordinal) ||
                    trimmedLine.Contains("UnityEditor.", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
