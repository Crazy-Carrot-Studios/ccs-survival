using System.IO;
using CCS.Modules.Sleep;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepValidationValidator
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Validates sleep module folders, 1.1.3 bedroll foundation, save, and respawn wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 sleep and bedroll foundation.
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
        private const string BedrollPrefabPath =
            SurvivalRoot + "/Content/Sleep/Primitive/Prefabs/PF_CCS_PrimitiveBedroll.prefab";
        private const string BedrollDefinitionPath =
            SurvivalRoot + "/Content/Sleep/Primitive/CCS_PrimitiveBedrollSleepSpotDefinition.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Sleep_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
        private const string PlayerDeathServicePath =
            "Assets/CCS/Modules/PlayerDeath/Runtime/Services/CCS_PlayerDeathService.cs";
        private const string PlaytestServicePath =
            "Assets/CCS/Modules/Playtesting/Runtime/Services/CCS_PlaytestService.cs";

        private static readonly string[] RequiredTestObjectNames =
        {
            "CCS_SleepTestArea",
            "CCS_TestBedroll"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.sleep";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Sleep", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Interactables", RuntimeRoot + "/Interactables");
            ValidateRequiredFolder(report, "Runtime/Interaction", RuntimeRoot + "/Interaction");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Sleep.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Sleep.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_SleepProfile", RuntimeRoot + "/Profiles/CCS_SleepProfile.cs");
            ValidateRequiredScript(report, "CCS_SleepSpotDefinition", RuntimeRoot + "/Definitions/CCS_SleepSpotDefinition.cs");
            ValidateRequiredScript(report, "CCS_SleepSpot", RuntimeRoot + "/Components/CCS_SleepSpot.cs");
            ValidateRequiredScript(report, "CCS_SleepSpotInteractable", RuntimeRoot + "/Interactables/CCS_SleepSpotInteractable.cs");
            ValidateRequiredScript(report, "CCS_SleepService", RuntimeRoot + "/Services/CCS_SleepService.cs");
            ValidateRequiredScript(report, "CCS_SleepRuntimeBridge", RuntimeRoot + "/Services/CCS_SleepRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_SleepEventArgs", RuntimeRoot + "/Events/CCS_SleepEventArgs.cs");
            ValidateRequiredScript(report, "CCS_SleepEvents", RuntimeRoot + "/Events/CCS_SleepEvents.cs");
            ValidateRequiredScript(report, "CCS_SleepSpotSaveState", RuntimeRoot + "/Data/CCS_SleepSpotSaveState.cs");
            ValidateRequiredScript(report, "CCS_SleepValidationUtility", RuntimeRoot + "/Validation/CCS_SleepValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_BedrollSleepInteractable", RuntimeRoot + "/Interaction/CCS_BedrollSleepInteractable.cs");

            ValidateDocumentationAsset(report, "Sleep Module Doc", ModuleDocPath);
            ValidateRequiredAsset(report, "Default Sleep Profile", DefaultProfilePath);
            ValidateRequiredAsset(report, "Bedroll Item", BedrollItemPath);
            ValidateRequiredAsset(report, "Primitive Bedroll Prefab", BedrollPrefabPath);
            ValidateRequiredAsset(report, "Primitive Bedroll Definition", BedrollDefinitionPath);

            ValidateSleepProfileAsset(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidateBootstrapTestObjects(report);
            ValidateSaveIntegration(report);
            ValidatePlayerDeathIntegration(report);
            ValidatePlaytestIntegration(report);
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

            if (profile.ProfileVersion != "1.1.3")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Sleep Profile Version",
                    $"Expected profileVersion 1.1.3 but found '{profile.ProfileVersion}'.");
            }

            if (profile.DefaultSleepSpotDefinition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Sleep Spot Definition",
                    "Run CCS_SleepBedrollFoundationBootstrapSetup.ExecuteBatch.");
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
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
            SerializedObject serializedHost = host != null ? new SerializedObject(host) : null;
            Object sleepProfile = serializedHost != null
                ? serializedHost.FindProperty("sleepProfile").objectReferenceValue
                : null;
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

            bool hasTestBedroll = sceneText.Contains("CCS_TestBedroll");
            report.AddIssue(
                hasTestBedroll
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Test Bedroll",
                hasTestBedroll
                    ? "Bootstrap scene contains CCS_TestBedroll with placeable sleep spot."
                    : "Bootstrap scene is missing CCS_TestBedroll. Run CCS_SleepBedrollFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Sleep Save Data",
                    $"Missing {SaveDataPath}");
                return;
            }

            string saveDataSource = File.ReadAllText(SaveDataPath);
            report.AddIssue(
                saveDataSource.Contains("CCS_SaveSleepWorldData")
                    && saveDataSource.Contains("CCS_SaveSleepSpotData")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Sleep Save Data",
                "CCS_SaveData includes sleep world snapshots.");

            if (!File.Exists(SaveServicePath))
            {
                return;
            }

            string saveServiceSource = File.ReadAllText(SaveServicePath);
            report.AddIssue(
                saveServiceSource.Contains("CaptureSleep")
                    && saveServiceSource.Contains("ApplySleep")
                    && saveServiceSource.Contains("CCS_SleepService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Sleep Save Service Wiring",
                "CCS_SaveService captures and restores sleep spot world state.");
        }

        private static void ValidatePlayerDeathIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(PlayerDeathServicePath))
            {
                return;
            }

            string source = File.ReadAllText(PlayerDeathServicePath);
            report.AddIssue(
                source.Contains("BindAssignedRespawnSpawnIdProvider")
                    && source.Contains("ResolveRespawnSpawnId")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bedroll Respawn Override",
                "CCS_PlayerDeathService supports assigned bedroll respawn before bootstrap fallback.");
        }

        private static void ValidatePlaytestIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(PlaytestServicePath))
            {
                return;
            }

            string source = File.ReadAllText(PlaytestServicePath);
            report.AddIssue(
                source.Contains("PlaceAndSleepAtBedroll")
                    && source.Contains("TryPlaceOrSleepBedrollNearPlayer")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Playtest Sleep Step",
                "Playtest service includes place and sleep at bedroll step and dev helper.");
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
                    : $"Missing asset: {assetPath}. Run CCS_SleepBedrollFoundationBootstrapSetup.ExecuteBatch.");
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
