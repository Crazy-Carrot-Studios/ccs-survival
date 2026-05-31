using System.IO;
using CCS.Modules.Equipment;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentValidationValidator
// CATEGORY: Modules / Equipment / Editor / Validation
// PURPOSE: Validates equipment module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require scene objects, UI, combat, or visual coupling.
// =============================================================================

namespace CCS.Modules.Equipment.Editor
{
    public sealed class CCS_EquipmentValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Equipment";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Equipment_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.equipment";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Equipment", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Slots", RuntimeRoot + "/Slots");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Equipment.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Equipment.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_EquipmentSlotType", RuntimeRoot + "/Definitions/CCS_EquipmentSlotType.cs");
            ValidateRequiredScript(report, "CCS_EquipmentItemDefinition", RuntimeRoot + "/Definitions/CCS_EquipmentItemDefinition.cs");
            ValidateRequiredScript(report, "CCS_EquippedItem", RuntimeRoot + "/Data/CCS_EquippedItem.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSnapshot", RuntimeRoot + "/Data/CCS_EquipmentSnapshot.cs");
            ValidateRequiredScript(report, "CCS_DurabilityState", RuntimeRoot + "/Data/CCS_DurabilityState.cs");
            ValidateRequiredScript(report, "CCS_EquipmentCapacityModifierUtility", RuntimeRoot + "/Data/CCS_EquipmentCapacityModifierUtility.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEnvironmentalModifierSnapshot", RuntimeRoot + "/Data/CCS_EquipmentEnvironmentalModifierSnapshot.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEnvironmentalModifierUtility", RuntimeRoot + "/Data/CCS_EquipmentEnvironmentalModifierUtility.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSlot", RuntimeRoot + "/Slots/CCS_EquipmentSlot.cs");
            ValidateRequiredScript(report, "CCS_PlayerEquipmentService", RuntimeRoot + "/Services/CCS_PlayerEquipmentService.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEventArgs", RuntimeRoot + "/Events/CCS_EquipmentEventArgs.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEvents", RuntimeRoot + "/Events/CCS_EquipmentEvents.cs");
            ValidateRequiredScript(report, "CCS_EquipmentProfile", RuntimeRoot + "/Profiles/CCS_EquipmentProfile.cs");
            ValidateRequiredScript(report, "CCS_EquipmentValidationUtility", RuntimeRoot + "/Validation/CCS_EquipmentValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSaveData", RuntimeRoot + "/Data/CCS_EquipmentSaveData.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSaveSlotEntry", RuntimeRoot + "/Data/CCS_EquipmentSaveSlotEntry.cs");
            ValidateRequiredScript(report, "CCS_EquipmentItemDefinitionLookup", RuntimeRoot + "/Definitions/CCS_EquipmentItemDefinitionLookup.cs");
            ValidateRequiredScript(report, "CCS_EquipmentRuntimeBridge", RuntimeRoot + "/Services/CCS_EquipmentRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEnvironmentRuntimeBridge", RuntimeRoot + "/Services/CCS_EquipmentEnvironmentRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_InventoryEquipmentPersistenceTestHarness",
                RuntimeRoot + "/Testing/CCS_InventoryEquipmentPersistenceTestHarness.cs");

            ValidateDocumentationAsset(report, "Equipment Module Doc", ModuleDocPath);
            ValidateEquipmentPersistenceIntegration(report);
            ValidateEnvironmentalModifierIntegration(report);
            ValidateTestEnvironmentalEquipmentAssets(report);

            CCS_SurvivalValidationResult carrySlotValidation =
                CCS_EquipmentValidationUtility.ValidateCarryRelatedSlotTypes();

            report.AddIssue(
                carrySlotValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Carry-Related Equipment Slots",
                carrySlotValidation.Message);

            CCS_SurvivalValidationResult nullCapacityValidation =
                CCS_EquipmentValidationUtility.ValidateCapacityModifiers(null);

            if (nullCapacityValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Capacity Modifier Validation",
                    "ValidateCapacityModifiers(null) should fail.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Equipment Capacity Modifier Validation",
                    "Capacity modifier validation rejects null definitions and enforces non-negative values.");
            }

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Equipment Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_EquipmentProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_EquipmentProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_EquipmentValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Equipment Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Equipment Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Equipment validator completed (0.7.4 environmental modifiers).");
        }

        #endregion

        #region Private Methods

        private static void ValidateEquipmentPersistenceIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_PlayerEquipmentService.cs";
            const string registrationPath =
                SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Save Integration",
                    $"Missing equipment service script: {servicePath}");
                return;
            }

            string serviceSource = File.ReadAllText(servicePath);
            if (serviceSource.Contains("CCS_ISaveable")
                && serviceSource.Contains("CaptureState")
                && serviceSource.Contains("RestoreState")
                && serviceSource.Contains("saveDataVersion"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Equipment Save Integration",
                    "CCS_PlayerEquipmentService implements CCS_ISaveable with versioned save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Save Integration",
                    "CCS_PlayerEquipmentService is missing CCS_ISaveable persistence implementation.");
            }

            if (!File.Exists(registrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Save Registration",
                    $"Missing gameplay service registration script: {registrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(registrationPath);
            if (registrationSource.Contains("RegisterGameplaySaveables")
                && registrationSource.Contains("RegisterSaveable(equipmentService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Equipment Save Registration",
                    "Equipment service registers with CCS_SaveLoadService during gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Save Registration",
                    "Gameplay composition is missing equipment saveable registration.");
            }
        }

        private static void ValidateEnvironmentalModifierIntegration(CCS_SurvivalValidationReport report)
        {
            const string definitionPath = RuntimeRoot + "/Definitions/CCS_EquipmentItemDefinition.cs";
            const string servicePath = RuntimeRoot + "/Services/CCS_PlayerEquipmentService.cs";
            const string environmentServicePath =
                "Assets/CCS/Modules/EnvironmentEffects/Runtime/Services/CCS_EnvironmentEffectsService.cs";

            if (File.Exists(definitionPath))
            {
                string definitionSource = File.ReadAllText(definitionPath);
                if (definitionSource.Contains("temperatureResistance")
                    && definitionSource.Contains("wetnessResistance")
                    && definitionSource.Contains("exposureResistance"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Equipment Environmental Modifier Fields",
                        "CCS_EquipmentItemDefinition exposes temperature, wetness, and exposure resistance fields.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Equipment Environmental Modifier Fields",
                        "CCS_EquipmentItemDefinition is missing environmental modifier fields.");
                }
            }

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("GetEnvironmentalModifiers"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Equipment Environmental Aggregation",
                        "CCS_PlayerEquipmentService aggregates equipped environmental modifiers.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Equipment Environmental Aggregation",
                        "CCS_PlayerEquipmentService is missing GetEnvironmentalModifiers().");
                }
            }

            CCS_SurvivalValidationResult nullEnvironmentalValidation =
                CCS_EquipmentValidationUtility.ValidateEnvironmentalModifiers(null);

            if (nullEnvironmentalValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Equipment Environmental Modifier Validation",
                    "ValidateEnvironmentalModifiers(null) should fail.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Equipment Environmental Modifier Validation",
                    "Environmental modifier validation rejects null definitions and negative values.");
            }

            if (File.Exists(environmentServicePath))
            {
                string environmentServiceSource = File.ReadAllText(environmentServicePath);
                if (environmentServiceSource.Contains("BindEquipmentService")
                    && environmentServiceSource.Contains("GetEnvironmentalModifiers"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Equipment Integration",
                        "CCS_EnvironmentEffectsService reads equipment environmental modifiers.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Equipment Integration",
                        "CCS_EnvironmentEffectsService is missing equipment modifier integration.");
                }
            }
        }

        private static void ValidateTestEnvironmentalEquipmentAssets(CCS_SurvivalValidationReport report)
        {
            ValidateTestEquipmentAsset(
                report,
                "Warm Hat Test Equipment",
                SurvivalRoot + "/Profiles/Equipment/TestItems/CCS_TestEquipment_WarmHat.asset",
                1f,
                0f,
                0f);

            ValidateTestEquipmentAsset(
                report,
                "Heavy Coat Test Equipment",
                SurvivalRoot + "/Profiles/Equipment/TestItems/CCS_TestEquipment_HeavyCoat.asset",
                2f,
                0f,
                0.3f);

            ValidateTestEquipmentAsset(
                report,
                "Waterproof Boots Test Equipment",
                SurvivalRoot + "/Profiles/Equipment/TestItems/CCS_TestEquipment_WaterproofBoots.asset",
                0f,
                0.4f,
                0f);
        }

        private static void ValidateTestEquipmentAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath,
            float expectedTemperatureResistance,
            float expectedWetnessResistance,
            float expectedExposureResistance)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Missing required test equipment asset: {assetPath}");
                return;
            }

            CCS_EquipmentItemDefinition equipmentDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentItemDefinition>(assetPath);
            if (equipmentDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Could not load test equipment asset: {assetPath}");
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_EquipmentValidationUtility.ValidateEquipmentDefinition(equipmentDefinition);
            if (!validation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    validation.Message);
                return;
            }

            if (!Mathf.Approximately(equipmentDefinition.TemperatureResistance, expectedTemperatureResistance)
                || !Mathf.Approximately(equipmentDefinition.WetnessResistance, expectedWetnessResistance)
                || !Mathf.Approximately(equipmentDefinition.ExposureResistance, expectedExposureResistance))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    "Test equipment environmental modifier values do not match expected tuning.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                context,
                $"Asset present with expected environmental modifiers: {assetPath}");
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
