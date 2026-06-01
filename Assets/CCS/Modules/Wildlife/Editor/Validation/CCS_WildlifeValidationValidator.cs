using System.IO;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeValidationValidator
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Validates wildlife module folders, asmdefs, profile asset, and bootstrap content.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Validates carcass placeholders in SCN_CCS_Survival_Bootstrap.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public sealed class CCS_WildlifeValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Wildlife";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Wildlife/CCS_DefaultWildlifeProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Wildlife_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string RawMeatItemPath = SurvivalRoot + "/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset";
        private const string BoneItemPath = SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Bone.asset";
        private const string HideItemPath = SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Hide.asset";
        private const string SinewItemPath = SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Sinew.asset";
        private const string RabbitDefinitionPath = SurvivalRoot + "/Content/Wildlife/Definitions/CCS_TestRabbit.asset";
        private const string DeerDefinitionPath = SurvivalRoot + "/Content/Wildlife/Definitions/CCS_TestDeerCarcass.asset";

        private static readonly string[] RequiredTestObjectNames =
        {
            "CCS_TestRabbitCarcass",
            "CCS_TestDeerCarcass"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.wildlife";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Wildlife", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Harvesting", RuntimeRoot + "/Harvesting");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Wildlife.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Wildlife.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_WildlifeType", RuntimeRoot + "/Definitions/CCS_WildlifeType.cs");
            ValidateRequiredScript(report, "CCS_WildlifeDefinition", RuntimeRoot + "/Definitions/CCS_WildlifeDefinition.cs");
            ValidateRequiredScript(report, "CCS_WildlifeHarvestDropDefinition", RuntimeRoot + "/Definitions/CCS_WildlifeHarvestDropDefinition.cs");
            ValidateRequiredScript(report, "CCS_WildlifeState", RuntimeRoot + "/Data/CCS_WildlifeState.cs");
            ValidateRequiredScript(report, "CCS_WildlifeSnapshot", RuntimeRoot + "/Data/CCS_WildlifeSnapshot.cs");
            ValidateRequiredScript(report, "CCS_WildlifeHarvestRequest", RuntimeRoot + "/Data/CCS_WildlifeHarvestRequest.cs");
            ValidateRequiredScript(report, "CCS_WildlifeHarvestResult", RuntimeRoot + "/Data/CCS_WildlifeHarvestResult.cs");
            ValidateRequiredScript(report, "CCS_HarvestableWildlife", RuntimeRoot + "/Harvesting/CCS_HarvestableWildlife.cs");
            ValidateRequiredScript(report, "CCS_WildlifeHarvestService", RuntimeRoot + "/Services/CCS_WildlifeHarvestService.cs");
            ValidateRequiredScript(report, "CCS_WildlifeRuntimeBridge", RuntimeRoot + "/Services/CCS_WildlifeRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_WildlifeEventArgs", RuntimeRoot + "/Events/CCS_WildlifeEventArgs.cs");
            ValidateRequiredScript(report, "CCS_WildlifeEvents", RuntimeRoot + "/Events/CCS_WildlifeEvents.cs");
            ValidateRequiredScript(report, "CCS_WildlifeProfile", RuntimeRoot + "/Profiles/CCS_WildlifeProfile.cs");
            ValidateRequiredScript(report, "CCS_WildlifeValidationUtility", RuntimeRoot + "/Validation/CCS_WildlifeValidationUtility.cs");

            ValidateDocumentationAsset(report, "Wildlife Module Doc", ModuleDocPath);

            report.AddIssue(
                typeof(CCS_IInteractableResultProvider).IsAssignableFrom(typeof(CCS_HarvestableWildlife))
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Harvestable Interaction Contract",
                "CCS_HarvestableWildlife implements CCS_IInteractableResultProvider for harvest integration.");

            ValidateWildlifeItemDefinitions(report);
            ValidateTestWildlifeDefinitions(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidateBootstrapTestObjects(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);

            CCS_SurvivalValidationResult wildlifeTypeValidation =
                CCS_WildlifeValidationUtility.ValidateRequiredWildlifeTypes();

            report.AddIssue(
                wildlifeTypeValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Wildlife Types",
                wildlifeTypeValidation.Message);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Wildlife Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_WildlifeProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_WildlifeProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_WildlifeValidationUtility.ValidateProfile(profile);

                    report.AddIssue(
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : CCS_SurvivalValidationIssueSeverity.Error,
                        "Default Wildlife Profile",
                        profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Wildlife Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            if (File.Exists(BootstrapScenePath))
            {
                string sceneText = File.ReadAllText(BootstrapScenePath);
                report.AddIssue(
                    sceneText.Contains("CCS_WildlifeTestArea")
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Wildlife Test Area",
                    "Bootstrap scene contains CCS_WildlifeTestArea.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Wildlife validator completed (carcass resource foundation; no AI or combat).");
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

        private static void ValidateWildlifeItemDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredAsset(report, "Bone Item", BoneItemPath);
            ValidateRequiredAsset(report, "Hide Item", HideItemPath);
            ValidateRequiredAsset(report, "Sinew Item", SinewItemPath);
            ValidateRequiredAsset(report, "Raw Meat Item", RawMeatItemPath);
        }

        private static void ValidateTestWildlifeDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredAsset(report, "Test Rabbit Definition", RabbitDefinitionPath);
            ValidateRequiredAsset(report, "Test Deer Carcass Definition", DeerDefinitionPath);

            CCS_WildlifeDefinition rabbitDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(RabbitDefinitionPath);

            if (rabbitDefinition != null)
            {
                CCS_SurvivalValidationResult validation =
                    CCS_WildlifeValidationUtility.ValidateWildlifeDefinition(rabbitDefinition);

                report.AddIssue(
                    validation.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Test Rabbit Definition",
                    validation.Message);
            }

            CCS_WildlifeDefinition deerDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(DeerDefinitionPath);

            if (deerDefinition != null)
            {
                CCS_SurvivalValidationResult validation =
                    CCS_WildlifeValidationUtility.ValidateWildlifeDefinition(deerDefinition);

                report.AddIssue(
                    validation.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Test Deer Carcass Definition",
                    validation.Message);
            }
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Asset present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required asset: {assetPath}");
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Services",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            string prefabText = File.ReadAllText(BootstrapPrefabPath);
            if (prefabText.Contains("wildlifeProfile"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Gameplay Services",
                    "Bootstrap root assigns wildlife profile.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Services",
                    "Bootstrap root is missing wildlife profile assignment.");
            }

            if (prefabText.Contains("CCS_WildlifeHarvestService")
                || prefabText.Contains("CCS.Modules.Wildlife"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Wildlife Harvest Service",
                    "Bootstrap composition references wildlife module types.");
            }

            report.AddIssue(
                File.Exists(RuntimeRoot + "/Services/CCS_WildlifeHarvestService.cs")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Wildlife Harvest Service",
                "CCS_WildlifeHarvestService script exists for gameplay registration.");
        }

        private static void ValidateBootstrapTestObjects(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Wildlife Test Objects",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
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
                    "Bootstrap Wildlife Test Objects",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_HarvestableWildlife")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Wildlife Test Objects",
                "Bootstrap scene includes CCS_HarvestableWildlife components.");
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            bool foundEditorReference = false;
            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                if (ScriptContainsUnityEditorReference(File.ReadAllText(scriptPaths[index])))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Script Purity",
                        $"Runtime script references UnityEditor: {scriptPaths[index]}");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Purity",
                    $"Runtime scripts under {runtimeFolderPath} avoid UnityEditor.");
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
