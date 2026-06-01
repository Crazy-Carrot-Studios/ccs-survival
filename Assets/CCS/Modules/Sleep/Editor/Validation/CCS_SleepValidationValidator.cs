using System.IO;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Sleep;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepValidationValidator
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Validates sleep module folders, asmdefs, profile assets, and bootstrap content.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Validates bootstrap sleep test area in SCN_CCS_Survival_Bootstrap.
// =============================================================================

namespace CCS.Modules.Sleep.Editor
{
    public sealed class CCS_SleepValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Sleep";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Sleep/CCS_DefaultSleepProfile.asset";
        private const string BedrollItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Bedroll.asset";
        private const string FiberItemPath = SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Fiber.asset";
        private const string BedrollEquipmentPath =
            SurvivalRoot + "/Content/Equipment/Primitive/CCS_Equipment_Bedroll.asset";
        private const string BedrollRecipePath =
            SurvivalRoot + "/Profiles/Crafting/PrimitiveRecipes/CCS_Recipe_Bedroll.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Sleep_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string HudPresentationPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudPresentationService.cs";
        private const string HudWiringPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudGameplayServiceWiring.cs";

        private static readonly string[] RequiredTestObjectNames =
        {
            "CCS_SleepTestArea",
            "CCS_TestBedrollRestPoint"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.sleep";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Sleep", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Interaction", RuntimeRoot + "/Interaction");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Sleep.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Sleep.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_SleepProfile", RuntimeRoot + "/Profiles/CCS_SleepProfile.cs");
            ValidateRequiredScript(report, "CCS_SleepService", RuntimeRoot + "/Services/CCS_SleepService.cs");
            ValidateRequiredScript(report, "CCS_SleepRuntimeBridge", RuntimeRoot + "/Services/CCS_SleepRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_SleepRequest", RuntimeRoot + "/Data/CCS_SleepRequest.cs");
            ValidateRequiredScript(report, "CCS_SleepResult", RuntimeRoot + "/Data/CCS_SleepResult.cs");
            ValidateRequiredScript(report, "CCS_SleepSnapshot", RuntimeRoot + "/Data/CCS_SleepSnapshot.cs");
            ValidateRequiredScript(report, "CCS_SleepFailureReason", RuntimeRoot + "/Data/CCS_SleepFailureReason.cs");
            ValidateRequiredScript(report, "CCS_SleepValidationUtility", RuntimeRoot + "/Validation/CCS_SleepValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_BedrollSleepInteractable", RuntimeRoot + "/Interaction/CCS_BedrollSleepInteractable.cs");
            ValidateRequiredScript(report, "CCS_SleepTestHarness", RuntimeRoot + "/Testing/CCS_SleepTestHarness.cs");

            ValidateDocumentationAsset(report, "Sleep Module Doc", ModuleDocPath);
            ValidateRequiredAsset(report, "Default Sleep Profile", DefaultProfilePath);
            ValidateRequiredAsset(report, "Bedroll Item", BedrollItemPath);
            ValidateRequiredAsset(report, "Fiber Item", FiberItemPath);
            ValidateRequiredAsset(report, "Bedroll Equipment", BedrollEquipmentPath);
            ValidateRequiredAsset(report, "Bedroll Recipe", BedrollRecipePath);

            ValidateSleepProfileAsset(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidateBootstrapTestObjects(report);
            ValidateHudIntegration(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
        }

        #endregion

        #region Private Methods

        private static void ValidateSleepProfileAsset(CCS_SurvivalValidationReport report)
        {
            CCS_SleepProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SleepProfile>(DefaultProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SleepValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Sleep Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "0.9.6")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Sleep Profile Version",
                    $"Expected profileVersion 0.9.6 but found '{profile.ProfileVersion}'.");
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Sleep Service Registration",
                    $"Missing script: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            report.AddIssue(
                source.Contains("CreateSleepService")
                    && source.Contains("CCS_SleepService")
                    && source.Contains("sleepProfile")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Sleep Service Registration",
                "Gameplay composition registers CCS_SleepService with profile and dependencies.");
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Sleep Profile Wiring",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Sleep Profile Wiring",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object sleepProfile = serializedHost.FindProperty("sleepProfile").objectReferenceValue;
            report.AddIssue(
                sleepProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Sleep Profile Wiring",
                sleepProfile != null
                    ? "Bootstrap gameplay host references CCS_DefaultSleepProfile."
                    : "Bootstrap gameplay host is missing sleepProfile assignment.");
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
                    "Bootstrap Sleep Test Objects",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_BedrollSleepInteractable")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Sleep Interactable",
                "Bootstrap scene includes CCS_BedrollSleepInteractable for sleep verification.");
        }

        private static void ValidateHudIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(HudPresentationPath) || !File.Exists(HudWiringPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Sleep Integration",
                    "HUD presentation or wiring scripts are missing.");
                return;
            }

            string presentationSource = File.ReadAllText(HudPresentationPath);
            string wiringSource = File.ReadAllText(HudWiringPath);

            report.AddIssue(
                presentationSource.Contains("BindSleepService")
                    && presentationSource.Contains("Sleep failed: Missing Bedroll")
                    && presentationSource.Contains("Sleep failed: Unsafe Conditions")
                    && presentationSource.Contains("Slept ")
                    && presentationSource.Contains("Sleep Ready:")
                    && wiringSource.Contains("BindSleepService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "HUD Sleep Notifications",
                "HUD binds sleep service and queues sleep success/failure notifications.");
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
