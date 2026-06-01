using System.IO;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.SaveSystem;
using CCS.Modules.Storage;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageValidationValidator
// CATEGORY: Modules / Storage / Editor / Validation
// PURPOSE: Validates storage module layout, assets, composition wiring, and save integration.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation.
// =============================================================================

namespace CCS.Modules.Storage.Editor
{
    public sealed class CCS_StorageValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Storage";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Storage/CCS_DefaultStorageProfile.asset";
        private const string DefinitionPath =
            SurvivalRoot + "/Content/Storage/Primitive/CCS_PrimitiveStorageCrateDefinition.asset";
        private const string PrefabPath =
            SurvivalRoot + "/Content/Storage/Primitive/Prefabs/PF_CCS_PrimitiveStorageCrate.prefab";
        private const string StorageCrateItemPath =
            SurvivalRoot + "/Content/Items/Progression/CCS_Item_StorageCrate.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Storage_Module.md";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string CompositionHostPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceHost.cs";
        private const string DefaultPlaytestProfilePath =
            SurvivalRoot + "/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.storage";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Storage", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Runtime/Interactables", RuntimeRoot + "/Interactables");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Storage.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Storage.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_StorageContainerDefinition", RuntimeRoot + "/Definitions/CCS_StorageContainerDefinition.cs");
            ValidateRequiredScript(report, "CCS_StorageContainer", RuntimeRoot + "/Components/CCS_StorageContainer.cs");
            ValidateRequiredScript(report, "CCS_StorageContainerInteractable", RuntimeRoot + "/Interactables/CCS_StorageContainerInteractable.cs");
            ValidateRequiredScript(report, "CCS_StorageService", RuntimeRoot + "/Services/CCS_StorageService.cs");
            ValidateRequiredScript(report, "CCS_StorageRuntimeBridge", RuntimeRoot + "/Services/CCS_StorageRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_StorageEventArgs", RuntimeRoot + "/Events/CCS_StorageEventArgs.cs");
            ValidateRequiredScript(report, "CCS_StorageProfile", RuntimeRoot + "/Profiles/CCS_StorageProfile.cs");

            ValidateDefaultProfile(report);
            ValidatePrimitiveContent(report);
            ValidateStorageCrateItem(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveDataSupport(report);
            ValidatePlaytestStep(report);
            ValidateDocumentation(report, ModuleDocPath, "Storage module documentation");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            CCS_StorageProfile profile = AssetDatabase.LoadAssetAtPath<CCS_StorageProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Profile",
                    $"Missing default profile: {DefaultProfilePath}. Run CCS_StorageBootstrapSetup.ExecuteBatch.");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_StorageValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, "Storage Profile", validation.Message);
                return;
            }

            if (profile.ProfileVersion != "1.1.2")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Storage Profile",
                    $"Expected profileVersion 1.1.2 but found '{profile.ProfileVersion}'.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Profile",
                "Default storage profile validated.");
        }

        private static void ValidatePrimitiveContent(CCS_SurvivalValidationReport report)
        {
            CCS_StorageContainerDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_StorageContainerDefinition>(DefinitionPath);
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Definition",
                    $"Missing definition asset: {DefinitionPath}");
            }
            else if (definition.PrefabReference == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Definition",
                    "Primitive storage crate definition is missing prefabReference.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Storage Definition",
                    $"Definition present: {definition.ContainerId}");
            }

            if (!File.Exists(PrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Prefab",
                    $"Missing prefab: {PrefabPath}");
                return;
            }

            string prefabText = File.ReadAllText(PrefabPath);
            bool hasContainerComponent = prefabText.Contains("CCS.Modules.Storage.CCS_StorageContainer");
            bool hasInteractableComponent = prefabText.Contains("CCS.Modules.Storage.CCS_StorageContainerInteractable");
            if (!hasContainerComponent || !hasInteractableComponent)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Prefab",
                    "PF_CCS_PrimitiveStorageCrate must include storage container and interactable components. "
                    + "Run CCS_StorageBootstrapSetup.ExecuteBatch if missing.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Prefab",
                "Primitive storage crate prefab validated.");
        }

        private static void ValidateStorageCrateItem(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(StorageCrateItemPath);
            if (item == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Crate Item",
                    $"Missing progression item: {StorageCrateItemPath}");
                return;
            }

            if (item.ItemId != "ccs.survival.item.progression.storagecrate")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Storage Crate Item",
                    $"Unexpected item id '{item.ItemId}'.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Crate Item",
                "Progression storage crate item present.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Composition",
                    $"Missing composition registration file: {CompositionRegistrationPath}");
                return;
            }

            string registrationText = File.ReadAllText(CompositionRegistrationPath);
            if (!registrationText.Contains("CreateStorageService")
                || !registrationText.Contains("CCS_StorageService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Composition",
                    "CCS_SurvivalGameplayServiceRegistration is missing storage service registration.");
                return;
            }

            if (!File.Exists(CompositionHostPath)
                || !File.ReadAllText(CompositionHostPath).Contains("storageProfile"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Composition",
                    "CCS_SurvivalGameplayServiceHost is missing storageProfile field.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Composition",
                "Storage service composition wiring present.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject bootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (bootstrapPrefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Bootstrap",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = bootstrapPrefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Bootstrap",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("storageProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Bootstrap",
                    "Bootstrap gameplay service host storageProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Storage Bootstrap",
                    "Bootstrap storage profile wired.");
            }

            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Bootstrap",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (!sceneText.Contains("CCS_TestStorageCrate"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Bootstrap",
                    "Bootstrap scene is missing CCS_TestStorageCrate.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Bootstrap",
                "CCS_TestStorageCrate present in bootstrap scene.");
        }

        private static void ValidateSaveDataSupport(CCS_SurvivalValidationReport report)
        {
            string saveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
            if (!File.Exists(saveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Save Data",
                    "CCS_SaveData.cs was not found.");
                return;
            }

            string saveDataText = File.ReadAllText(saveDataPath);
            if (!saveDataText.Contains("CCS_SaveStorageWorldData")
                || !saveDataText.Contains("CCS_SaveStorageContainerData"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Save Data",
                    "CCS_SaveData is missing storage container persistence fields.");
                return;
            }

            CCS_SaveData sample = new CCS_SaveData
            {
                saveVersion = CCS_SaveData.CurrentSaveVersion,
                storage = new CCS_SaveStorageWorldData
                {
                    containers = new[]
                    {
                        new CCS_SaveStorageContainerData
                        {
                            containerDefinitionId = "ccs.survival.storage.primitive.crate",
                            instanceId = "ccs.survival.storage.instance.sample",
                            displayName = "Storage Crate",
                            slots = new[]
                            {
                                new CCS_SaveStorageContainerSlotData
                                {
                                    itemId = "ccs.survival.item.resource.wood",
                                    quantity = 2
                                }
                            }
                        }
                    }
                }
            };

            string json = JsonUtility.ToJson(sample);
            if (string.IsNullOrWhiteSpace(json) || !json.Contains("storage"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Save Data",
                    "Storage save payload failed JsonUtility serialization smoke test.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Save Data",
                "Unified save payload supports storage containers.");
        }

        private static void ValidatePlaytestStep(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Playtest",
                    $"Missing playtest profile: {DefaultPlaytestProfilePath}");
                return;
            }

            bool hasStorageStep = false;
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                CCS_PlaytestStepDefinition step = profile.StepDefinitions[index];
                if (step != null && step.StepType == CCS_PlaytestStepType.UseStorageCrate)
                {
                    hasStorageStep = true;
                    break;
                }
            }

            if (!hasStorageStep)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Storage Playtest",
                    "Default playtest profile is missing UseStorageCrate step.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Storage Playtest",
                "Use storage crate playtest step configured.");
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

        private static void ValidateDocumentation(
            CCS_SurvivalValidationReport report,
            string docPath,
            string context)
        {
            if (File.Exists(docPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Documentation present: {docPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                context,
                $"Documentation missing: {docPath}");
        }

        #endregion
    }
}
