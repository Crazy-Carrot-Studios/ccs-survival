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
// NOTES: Shelter integration and persistence restore checks for 0.8.5.
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
            ValidateRequiredFolder(report, "Runtime/Snap", RuntimeRoot + "/Snap");
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
            ValidateRequiredScript(report, "CCS_BuildingPersistenceTestHarness", RuntimeRoot + "/Testing/CCS_BuildingPersistenceTestHarness.cs");
            ValidateRequiredScript(report, "CCS_BuildingDefinitionLookup", RuntimeRoot + "/Services/CCS_BuildingDefinitionLookup.cs");
            ValidateRequiredScript(report, "CCS_BuildingInstanceVisualFactory", RuntimeRoot + "/Placement/CCS_BuildingInstanceVisualFactory.cs");
            ValidateRequiredScript(report, "CCS_BuildingShelterContribution", RuntimeRoot + "/Data/CCS_BuildingShelterContribution.cs");
            ValidateRequiredScript(report, "CCS_BuildingShelterRuntimeBridge", RuntimeRoot + "/Services/CCS_BuildingShelterRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementEventArgs", RuntimeRoot + "/Events/CCS_BuildingPlacementEventArgs.cs");
            ValidateRequiredScript(report, "CCS_BuildingPlacementFailedEventArgs", RuntimeRoot + "/Events/CCS_BuildingPlacementFailedEventArgs.cs");
            ValidateRequiredScript(report, "CCS_BuildingSnapPointType", RuntimeRoot + "/Snap/CCS_BuildingSnapPointType.cs");
            ValidateRequiredScript(report, "CCS_BuildingSnapPoint", RuntimeRoot + "/Snap/CCS_BuildingSnapPoint.cs");
            ValidateRequiredScript(report, "CCS_BuildingSnapMatch", RuntimeRoot + "/Snap/CCS_BuildingSnapMatch.cs");
            ValidateRequiredScript(report, "CCS_BuildingRuntimeSnapPoint", RuntimeRoot + "/Snap/CCS_BuildingRuntimeSnapPoint.cs");
            ValidateRequiredScript(report, "CCS_BuildingSnapCompatibilityUtility", RuntimeRoot + "/Snap/CCS_BuildingSnapCompatibilityUtility.cs");

            ValidateDocumentationAsset(report, "Building Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateTestDefinitions(report);
            ValidateBuildCostIntegration(report);
            ValidateSnapIntegration(report);
            ValidatePersistenceRestore(report);
            ValidateShelterIntegration(report);
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
                "Building validator completed (0.8.5 shelter integration).");
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

        private static void ValidateSnapIntegration(CCS_SurvivalValidationReport report)
        {
            const string compatibilityUtilityPath = RuntimeRoot + "/Snap/CCS_BuildingSnapCompatibilityUtility.cs";
            const string placementServicePath = RuntimeRoot + "/Services/CCS_BuildingPlacementService.cs";
            const string instancePath = RuntimeRoot + "/Data/CCS_BuildingInstance.cs";

            if (File.Exists(compatibilityUtilityPath))
            {
                string utilitySource = File.ReadAllText(compatibilityUtilityPath);
                if (utilitySource.Contains("FoundationEdge")
                    && utilitySource.Contains("WallBottom")
                    && utilitySource.Contains("WallTop")
                    && utilitySource.Contains("RoofEdge"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Snap Compatibility",
                        "Snap compatibility utility includes foundation, wall, and roof rules.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Snap Compatibility",
                        "Snap compatibility utility is missing required snap rules.");
                }
            }

            CCS_BuildingPieceDefinition foundation =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestFoundationPath);
            CCS_BuildingPieceDefinition wall =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestWallPath);
            CCS_BuildingPieceDefinition roof =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestRoofPath);

            ValidateDefinitionSnapRules(
                report,
                "Foundation Snap Rules",
                foundation,
                allowsFreePlacement: true,
                requiresSnapPoint: false,
                minimumSnapPoints: 1);
            ValidateDefinitionSnapRules(
                report,
                "Wall Snap Rules",
                wall,
                allowsFreePlacement: false,
                requiresSnapPoint: true,
                minimumSnapPoints: 2);
            ValidateDefinitionSnapRules(
                report,
                "Roof Snap Rules",
                roof,
                allowsFreePlacement: false,
                requiresSnapPoint: true,
                minimumSnapPoints: 1);

            if (File.Exists(placementServicePath))
            {
                string placementSource = File.ReadAllText(placementServicePath);
                if (placementSource.Contains("FindBestSnapMatch")
                    && placementSource.Contains("UpdatePreviewWithSnap")
                    && placementSource.Contains("PlaceCurrentPieceUsingSnap"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Snap Placement Service",
                        "Placement service exposes snap matching and snap-aware placement methods.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Snap Placement Service",
                        "Placement service is missing snap matching methods.");
                }
            }

            if (File.Exists(instancePath))
            {
                string instanceSource = File.ReadAllText(instancePath);
                if (instanceSource.Contains("InitializeRuntimeSnapPoints")
                    && instanceSource.Contains("RuntimeSnapPoints")
                    && instanceSource.Contains("TrySetSnapPointOccupied"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Runtime Snap Points",
                        "Placed instances initialize runtime snap points with occupancy support.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Runtime Snap Points",
                        "CCS_BuildingInstance is missing runtime snap point support.");
                }
            }
        }

        private static void ValidateDefinitionSnapRules(
            CCS_SurvivalValidationReport report,
            string context,
            CCS_BuildingPieceDefinition definition,
            bool allowsFreePlacement,
            bool requiresSnapPoint,
            int minimumSnapPoints)
        {
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Definition asset could not be loaded.");
                return;
            }

            int snapPointCount = definition.SnapPoints?.Count ?? 0;
            if (definition.AllowsFreePlacement != allowsFreePlacement
                || definition.RequiresSnapPoint != requiresSnapPoint
                || snapPointCount < minimumSnapPoints)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Definition '{definition.PieceId}' snap rules or snap points are incorrect.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                context,
                $"Definition '{definition.PieceId}' snap rules validated with {snapPointCount} snap points.");
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

        private static void ValidatePersistenceRestore(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_BuildingService.cs";
            const string instanceRecordPath = RuntimeRoot + "/Data/CCS_BuildingInstanceSaveRecord.cs";
            const string saveDataPath = RuntimeRoot + "/Data/CCS_BuildingSaveData.cs";
            const string lookupPath = RuntimeRoot + "/Services/CCS_BuildingDefinitionLookup.cs";
            const string visualFactoryPath = RuntimeRoot + "/Placement/CCS_BuildingInstanceVisualFactory.cs";
            const string harnessPath = RuntimeRoot + "/Testing/CCS_BuildingPersistenceTestHarness.cs";
            const string instancePath = RuntimeRoot + "/Data/CCS_BuildingInstance.cs";

            if (File.Exists(saveDataPath))
            {
                string saveDataSource = File.ReadAllText(saveDataPath);
                if (saveDataSource.Contains("CurrentSaveDataVersion = 3"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Save Data Version",
                        "CCS_BuildingSaveData uses version 3 for persistence restore.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Save Data Version",
                        "CCS_BuildingSaveData is not at the expected save data version.");
                }
            }

            if (File.Exists(instanceRecordPath))
            {
                string recordSource = File.ReadAllText(instanceRecordPath);
                if (recordSource.Contains("placedOrderIndex")
                    && recordSource.Contains("occupiedSnapPointIds")
                    && recordSource.Contains("targetSnapInstanceId")
                    && recordSource.Contains("targetSnapPointId"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Instance Save Record",
                        "Placed instance records include order, occupancy, and target snap metadata.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Instance Save Record",
                        "CCS_BuildingInstanceSaveRecord is missing persistence restore fields.");
                }
            }

            if (File.Exists(lookupPath))
            {
                string lookupSource = File.ReadAllText(lookupPath);
                if (lookupSource.Contains("TryResolveDefinition"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Definition Lookup",
                        "Definition lookup resolves piece IDs for restore.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Definition Lookup",
                        "CCS_BuildingDefinitionLookup is missing TryResolveDefinition.");
                }
            }

            if (File.Exists(visualFactoryPath))
            {
                string factorySource = File.ReadAllText(visualFactoryPath);
                if (factorySource.Contains("SpawnInstanceVisual")
                    && factorySource.Contains("DestroyAllVisuals"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Visual Factory",
                        "Visual factory spawns primitive visuals for restored instances.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Visual Factory",
                        "CCS_BuildingInstanceVisualFactory is missing required restore methods.");
                }
            }

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("RestorePlacedInstances")
                    && serviceSource.Contains("ClearPlacedInstances")
                    && serviceSource.Contains("CCS_BuildingDefinitionLookup")
                    && serviceSource.Contains("CCS_BuildingInstanceVisualFactory")
                    && !serviceSource.Contains("Full restore deferred"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Restore State",
                        "CCS_BuildingService.RestoreState recreates placed instances and visuals.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Restore State",
                        "CCS_BuildingService.RestoreState does not fully restore placed instances.");
                }
            }

            if (File.Exists(instancePath))
            {
                string instanceSource = File.ReadAllText(instancePath);
                if (instanceSource.Contains("ApplyOccupiedSnapPoints")
                    && instanceSource.Contains("CollectOccupiedSnapPointIds"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Snap Occupancy Restore",
                        "Placed instances support snap occupancy capture and restore.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Snap Occupancy Restore",
                        "CCS_BuildingInstance is missing snap occupancy persistence helpers.");
                }
            }

            if (File.Exists(harnessPath))
            {
                string harnessSource = File.ReadAllText(harnessPath);
                if (harnessSource.Contains("building_persistence_test")
                    && harnessSource.Contains("ClearPlacedInstances")
                    && harnessSource.Contains("PASS")
                    && harnessSource.Contains("FAIL"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Persistence Test Harness",
                        "Persistence harness saves, clears, loads, and verifies building restore.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Persistence Test Harness",
                        "CCS_BuildingPersistenceTestHarness is missing required verification flow.");
                }
            }
        }

        private static void ValidateShelterIntegration(CCS_SurvivalValidationReport report)
        {
            const string definitionPath = RuntimeRoot + "/Definitions/CCS_BuildingPieceDefinition.cs";
            const string servicePath = RuntimeRoot + "/Services/CCS_BuildingService.cs";
            const string harnessPath =
                "Assets/CCS/Modules/Shelter/Runtime/Testing/CCS_BuildingShelterIntegrationTestHarness.cs";

            if (File.Exists(definitionPath))
            {
                string definitionSource = File.ReadAllText(definitionPath);
                if (definitionSource.Contains("contributesToShelter")
                    && definitionSource.Contains("wetnessProtectionContribution")
                    && definitionSource.Contains("shelterCoverageRadius"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Shelter Definition Fields",
                        "Building piece definitions include shelter contribution fields.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Shelter Definition Fields",
                        "CCS_BuildingPieceDefinition is missing shelter contribution fields.");
                }
            }

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("GetShelterContributions")
                    && serviceSource.Contains("RecalculateShelterContributions")
                    && serviceSource.Contains("BuildingShelterContributionsChanged"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Shelter Contribution API",
                        "CCS_BuildingService exposes shelter contribution recalculation and events.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Shelter Contribution API",
                        "CCS_BuildingService is missing shelter contribution API.");
                }
            }

            CCS_BuildingPieceDefinition foundation =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestFoundationPath);
            CCS_BuildingPieceDefinition wall =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestWallPath);
            CCS_BuildingPieceDefinition roof =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(TestRoofPath);

            ValidateDefinitionShelterValues(report, "Foundation Shelter Values", foundation, false, 0f, 0f);
            ValidateDefinitionShelterValues(report, "Wall Shelter Values", wall, true, 0f, 0.2f);
            ValidateDefinitionShelterValues(report, "Roof Shelter Values", roof, true, 1f, 0.4f);

            if (File.Exists(harnessPath))
            {
                string harnessSource = File.ReadAllText(harnessPath);
                if (harnessSource.Contains("RecalculateShelterContributions")
                    && harnessSource.Contains("SetSubjectPosition")
                    && harnessSource.Contains("PASS"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Shelter Integration Harness",
                        "Shelter integration harness verifies building contribution protection.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Shelter Integration Harness",
                        "CCS_BuildingShelterIntegrationTestHarness is missing verification flow.");
                }
            }
        }

        private static void ValidateDefinitionShelterValues(
            CCS_SurvivalValidationReport report,
            string context,
            CCS_BuildingPieceDefinition definition,
            bool expectedContributes,
            float expectedWetness,
            float expectedExposure)
        {
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Definition asset could not be loaded.");
                return;
            }

            if (definition.ContributesToShelter != expectedContributes
                || Mathf.Abs(definition.WetnessProtectionContribution - expectedWetness) > 0.001f
                || Mathf.Abs(definition.ExposureProtectionContribution - expectedExposure) > 0.001f)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Definition '{definition.PieceId}' shelter values are incorrect.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                context,
                $"Definition '{definition.PieceId}' shelter values validated.");
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
                if (harnessSource.Contains("PlaceCurrentPieceUsingSnap")
                    && harnessSource.Contains("UpdatePreviewWithSnap")
                    && harnessSource.Contains("FindBestSnapMatch")
                    && harnessSource.Contains("TryAdvanceSnapSequence"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Placement Test Harness",
                        "Placement harness uses snap sequence for foundation, wall, and roof.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Placement Test Harness",
                        "Placement harness is missing snap sequence flow.");
                }
            }

            if (File.Exists(placementServicePath))
            {
                string placementSource = File.ReadAllText(placementServicePath);
                if (placementSource.Contains("FindBestSnapMatch")
                    && placementSource.Contains("UpdatePreviewWithSnap")
                    && placementSource.Contains("PlaceCurrentPieceUsingSnap"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Snap Placement Methods",
                        "Placement service includes snap matching and snap-aware placement.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Snap Placement Methods",
                        "Placement service is missing required snapping methods.");
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
                && registrationSource.Contains("RegisterSaveable(buildingService)")
                && registrationSource.Contains("BindBuildingShelterIntegration"))
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
                string utilitySource = File.ReadAllText(BuildingValidationUtilityPath);
                if (utilitySource.Contains("FormatSnapTargetLabel")
                    && utilitySource.Contains("Placement Valid:")
                    && utilitySource.Contains("Saved Building Records:")
                    && utilitySource.Contains("Restored Buildings:")
                    && utilitySource.Contains("Building Shelter Contributions:")
                    && utilitySource.Contains("Building Shelter Active:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building HUD Display",
                        "Environment HUD presenter displays building placement, restore, and shelter lines.");
                    return;
                }

                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building HUD Display",
                    "Building validation utility is missing shelter integration HUD formatting.");
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
                && sceneText.Contains("CCS_BuildingPersistenceTestHarness")
                && sceneText.Contains("CCS_BuildingShelterIntegrationTestHarness")
                && sceneText.Contains("CCS_BuildingPlacementPreview"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Building Test Area",
                    "Bootstrap scene includes building test area with placement, persistence, and shelter harnesses.");
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
