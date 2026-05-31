using System.IO;
using CCS.Modules.Inventory;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InventoryValidationValidator
// CATEGORY: Modules / Inventory / Editor / Validation
// PURPOSE: Validates inventory module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require scene objects, UI, or gameplay module coupling.
// =============================================================================

namespace CCS.Modules.Inventory.Editor
{
    public sealed class CCS_InventoryValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Inventory";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Inventory_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.inventory";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Inventory", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Containers", RuntimeRoot + "/Containers");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Inventory.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Inventory.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_ItemCategory", RuntimeRoot + "/Definitions/CCS_ItemCategory.cs");
            ValidateRequiredScript(report, "CCS_ItemDefinition", RuntimeRoot + "/Definitions/CCS_ItemDefinition.cs");
            ValidateRequiredScript(report, "CCS_ItemStack", RuntimeRoot + "/Data/CCS_ItemStack.cs");
            ValidateRequiredScript(report, "CCS_InventorySlot", RuntimeRoot + "/Data/CCS_InventorySlot.cs");
            ValidateRequiredScript(report, "CCS_InventorySnapshot", RuntimeRoot + "/Data/CCS_InventorySnapshot.cs");
            ValidateRequiredScript(report, "CCS_InventoryCapacityModifierSnapshot", RuntimeRoot + "/Data/CCS_InventoryCapacityModifierSnapshot.cs");
            ValidateRequiredScript(report, "CCS_IInventoryContainer", RuntimeRoot + "/Containers/CCS_IInventoryContainer.cs");
            ValidateRequiredScript(report, "CCS_InventoryContainer", RuntimeRoot + "/Containers/CCS_InventoryContainer.cs");
            ValidateRequiredScript(report, "CCS_PlayerInventoryService", RuntimeRoot + "/Services/CCS_PlayerInventoryService.cs");
            ValidateRequiredScript(report, "CCS_InventoryEventArgs", RuntimeRoot + "/Events/CCS_InventoryEventArgs.cs");
            ValidateRequiredScript(report, "CCS_InventoryEvents", RuntimeRoot + "/Events/CCS_InventoryEvents.cs");
            ValidateRequiredScript(report, "CCS_InventoryProfile", RuntimeRoot + "/Profiles/CCS_InventoryProfile.cs");
            ValidateRequiredScript(report, "CCS_InventoryValidationUtility", RuntimeRoot + "/Validation/CCS_InventoryValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_InventorySaveData", RuntimeRoot + "/Data/CCS_InventorySaveData.cs");
            ValidateRequiredScript(report, "CCS_InventorySaveSlotEntry", RuntimeRoot + "/Data/CCS_InventorySaveSlotEntry.cs");
            ValidateRequiredScript(report, "CCS_ItemDefinitionLookup", RuntimeRoot + "/Definitions/CCS_ItemDefinitionLookup.cs");

            ValidateDocumentationAsset(report, "Inventory Module Doc", ModuleDocPath);
            ValidateInventoryPersistenceIntegration(report);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Inventory Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_InventoryProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_InventoryValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Inventory Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Inventory Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            ValidateCraftOutputCapacity(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Inventory validator completed (0.6.2 inventory persistence integrated with save/load).");
        }

        #endregion

        #region Private Methods

        private static void ValidateInventoryPersistenceIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_PlayerInventoryService.cs";
            const string registrationPath =
                SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Inventory Save Integration",
                    $"Missing inventory service script: {servicePath}");
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
                    "Inventory Save Integration",
                    "CCS_PlayerInventoryService implements CCS_ISaveable with versioned save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Inventory Save Integration",
                    "CCS_PlayerInventoryService is missing CCS_ISaveable persistence implementation.");
            }

            if (!File.Exists(registrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Inventory Save Registration",
                    $"Missing gameplay service registration script: {registrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(registrationPath);
            if (registrationSource.Contains("RegisterGameplaySaveables")
                && registrationSource.Contains("RegisterSaveable(inventoryService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Inventory Save Registration",
                    "Inventory service registers with CCS_SaveLoadService during gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Inventory Save Registration",
                    "Gameplay composition is missing inventory saveable registration.");
            }
        }

        private static void ValidateCraftOutputCapacity(CCS_SurvivalValidationReport report)
        {
            const string campfireKitPath =
                SurvivalRoot + "/Profiles/Crafting/TestItems/CCS_TestItem_CampfireKit.asset";
            const string bandagePath =
                SurvivalRoot + "/Profiles/Crafting/TestItems/CCS_TestItem_Bandage.asset";

            if (!File.Exists(DefaultProfilePath))
            {
                return;
            }

            CCS_InventoryProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(DefaultProfilePath);

            if (profile == null)
            {
                return;
            }

            CCS_InventoryContainer container = new CCS_InventoryContainer(profile.InventorySlotCount);
            ValidateCraftOutputItem(report, container, campfireKitPath, "Campfire Kit Craft Output");
            ValidateCraftOutputItem(report, container, bandagePath, "Bandage Craft Output");
        }

        private static void ValidateCraftOutputItem(
            CCS_SurvivalValidationReport report,
            CCS_InventoryContainer container,
            string itemAssetPath,
            string context)
        {
            if (!File.Exists(itemAssetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    context,
                    $"Craft output item asset missing: {itemAssetPath}");
                return;
            }

            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(itemAssetPath);
            if (itemDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Could not load craft output item: {itemAssetPath}");
                return;
            }

            if (container.CanAdd(itemDefinition, 1))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    "Default inventory profile can hold test craft output.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                "Default inventory profile cannot hold test craft output.");
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
