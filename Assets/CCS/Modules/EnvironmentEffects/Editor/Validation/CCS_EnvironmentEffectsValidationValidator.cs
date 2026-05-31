using System.IO;
using CCS.Modules.EnvironmentEffects;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsValidationValidator
// CATEGORY: Modules / EnvironmentEffects / Editor / Validation
// PURPOSE: Validates environment effects module folders, asmdefs, profile, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation checks only. No Survival Core mutation requirements.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects.Editor
{
    public sealed class CCS_EnvironmentEffectsValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/EnvironmentEffects";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/EnvironmentEffects/CCS_DefaultEnvironmentEffectsProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Environment_Effects_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveableIdsPath =
            "Assets/CCS/Modules/SaveLoad/Runtime/Data/CCS_SaveLoadSaveableIds.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.environmenteffects";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/EnvironmentEffects", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Presentation", RuntimeRoot + "/Presentation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.EnvironmentEffects.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.EnvironmentEffects.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_EnvironmentState", RuntimeRoot + "/Data/CCS_EnvironmentState.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentSnapshot", RuntimeRoot + "/Data/CCS_EnvironmentSnapshot.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectiveValueUtility", RuntimeRoot + "/Data/CCS_EnvironmentEffectiveValueUtility.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentSaveData", RuntimeRoot + "/Data/CCS_EnvironmentSaveData.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsProfile", RuntimeRoot + "/Profiles/CCS_EnvironmentEffectsProfile.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsEvents", RuntimeRoot + "/Events/CCS_EnvironmentEffectsEvents.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsEventArgs", RuntimeRoot + "/Events/CCS_EnvironmentEffectsEventArgs.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsValidationUtility", RuntimeRoot + "/Validation/CCS_EnvironmentEffectsValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsService", RuntimeRoot + "/Services/CCS_EnvironmentEffectsService.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsRuntimeBridge", RuntimeRoot + "/Services/CCS_EnvironmentEffectsRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_EnvironmentEffectsHudPresenter", RuntimeRoot + "/Presentation/CCS_EnvironmentEffectsHudPresenter.cs");

            ValidateDocumentationAsset(report, "Environment Effects Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateSaveIntegration(report);
            ValidateServiceRegistration(report);
            ValidateRestoreOrder(report);
            ValidateBootstrapHudPresenter(report);
            ValidateSurvivalCoreIntegration(report);
            ValidateEquipmentModifierIntegration(report);
            ValidateShelterIntegration(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Environment effects validator completed (0.8.5 building shelter integration).");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Environment Effects Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Environment Effects Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_EnvironmentEffectsProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EnvironmentEffectsProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Environment Effects Profile",
                    "Could not load default environment effects profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation =
                CCS_EnvironmentEffectsValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Environment Effects Profile",
                profileValidation.Message);
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_EnvironmentEffectsService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Environment Save Integration",
                    $"Missing service script: {servicePath}");
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
                    "Environment Save Integration",
                    "CCS_EnvironmentEffectsService implements CCS_ISaveable with versioned save payloads.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Environment Save Integration",
                "CCS_EnvironmentEffectsService is missing CCS_ISaveable persistence implementation.");
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Environment Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateEnvironmentEffectsService")
                && registrationSource.Contains("RegisterSaveable(environmentEffectsService)")
                && registrationSource.Contains("BindTimeOfDayService")
                && registrationSource.Contains("BindWeatherService")
                && registrationSource.Contains("BindEquipmentService")
                && registrationSource.Contains("BindShelterService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Environment Service Registration",
                    "Gameplay composition registers, binds time/weather, and save-registers CCS_EnvironmentEffectsService.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Environment Service Registration",
                "Gameplay composition is missing environment effects service registration wiring.");
        }

        private static void ValidateRestoreOrder(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveableIdsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Environment Restore Order",
                    $"Missing saveable ids script: {SaveableIdsPath}");
                return;
            }

            string saveableIdsSource = File.ReadAllText(SaveableIdsPath);
            if (saveableIdsSource.Contains("GlobalEnvironment")
                && saveableIdsSource.Contains("GlobalWeather")
                && saveableIdsSource.Contains("GlobalShelter")
                && saveableIdsSource.Contains("GlobalTimeOfDay")
                && saveableIdsSource.Contains("ModuleRestoreOrder"))
            {
                int shelterIndex = saveableIdsSource.IndexOf("GlobalShelter", System.StringComparison.Ordinal);
                int environmentIndex = saveableIdsSource.IndexOf("GlobalEnvironment", System.StringComparison.Ordinal);
                if (shelterIndex >= 0 && environmentIndex > shelterIndex)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Restore Order",
                        "Saveable restore order includes environment after shelter.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Environment Restore Order",
                "Saveable restore order is missing environment after shelter.");
        }

        private static void ValidateSurvivalCoreIntegration(CCS_SurvivalValidationReport report)
        {
            const string survivalServicePath =
                "Assets/CCS/Modules/SurvivalCore/Runtime/Runtime/CCS_SurvivalCoreService.cs";
            const string registrationPath =
                SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

            if (File.Exists(survivalServicePath))
            {
                string serviceSource = File.ReadAllText(survivalServicePath);
                if (serviceSource.Contains("BindEnvironmentEffectsService")
                    && serviceSource.Contains("CCS_EnvironmentEffectsService"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Survival Core Environment Read",
                        "CCS_SurvivalCoreService references environment effects safely through explicit binding.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Survival Core Environment Read",
                        "CCS_SurvivalCoreService is missing environment effects integration.");
                }
            }

            if (File.Exists(registrationPath))
            {
                string registrationSource = File.ReadAllText(registrationPath);
                if (registrationSource.Contains("BindSurvivalCoreEnvironmentEffects"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Survival Core Environment Binding",
                        "Gameplay composition binds environment effects into survival core.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Survival Core Environment Binding",
                        "Gameplay composition is missing BindSurvivalCoreEnvironmentEffects wiring.");
                }
            }
        }

        private static void ValidateBootstrapHudPresenter(CCS_SurvivalValidationReport report)
        {
            const string bootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            if (!File.Exists(bootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Environment HUD",
                    $"Missing bootstrap scene: {bootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(bootstrapScenePath);
            if (sceneText.Contains("CCS_EnvironmentEffectsHudPresenter")
                && sceneText.Contains("EnvironmentHudArea"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Environment HUD",
                    "Bootstrap scene includes CCS_EnvironmentEffectsHudPresenter.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Environment HUD",
                "Bootstrap scene is missing CCS_EnvironmentEffectsHudPresenter.");
        }

        private static void ValidateEquipmentModifierIntegration(CCS_SurvivalValidationReport report)
        {
            const string snapshotPath = RuntimeRoot + "/Data/CCS_EnvironmentSnapshot.cs";
            const string validationUtilityPath = RuntimeRoot + "/Validation/CCS_EnvironmentEffectsValidationUtility.cs";
            const string bridgePath =
                "Assets/CCS/Modules/Equipment/Runtime/Services/CCS_EquipmentEnvironmentRuntimeBridge.cs";

            if (File.Exists(snapshotPath))
            {
                string snapshotSource = File.ReadAllText(snapshotPath);
                if (snapshotSource.Contains("RawTemperature")
                    && snapshotSource.Contains("EffectiveTemperature")
                    && snapshotSource.Contains("EquipmentModifierSnapshot"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Snapshot Raw/Effective Fields",
                        "CCS_EnvironmentSnapshot exposes raw, effective, and equipment modifier values.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Snapshot Raw/Effective Fields",
                        "CCS_EnvironmentSnapshot is missing raw/effective environment fields.");
                }
            }

            if (File.Exists(validationUtilityPath))
            {
                string validationSource = File.ReadAllText(validationUtilityPath);
                if (validationSource.Contains("Temp Res:")
                    && validationSource.Contains("Eff Temp:")
                    && validationSource.Contains("Eff Wet:")
                    && validationSource.Contains("Eff Exp:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment HUD Display",
                        "Environment display formatting includes resistance and effective values.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment HUD Display",
                        "Environment display formatting is missing resistance or effective values.");
                }
            }

            ValidateRequiredFile(report, "Equipment Environment Runtime Bridge", bridgePath);
        }

        private static void ValidateShelterIntegration(CCS_SurvivalValidationReport report)
        {
            const string snapshotPath = RuntimeRoot + "/Data/CCS_EnvironmentSnapshot.cs";
            const string servicePath = RuntimeRoot + "/Services/CCS_EnvironmentEffectsService.cs";
            const string displayPath = RuntimeRoot + "/Validation/CCS_EnvironmentEffectsValidationUtility.cs";

            if (File.Exists(snapshotPath))
            {
                string snapshotSource = File.ReadAllText(snapshotPath);
                if (snapshotSource.Contains("IsSheltered")
                    && snapshotSource.Contains("ShelterModifierSnapshot"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Snapshot Shelter Fields",
                        "CCS_EnvironmentSnapshot exposes sheltered state and shelter modifier snapshot.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Snapshot Shelter Fields",
                        "CCS_EnvironmentSnapshot is missing shelter snapshot fields.");
                }
            }

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("BindShelterService")
                    && serviceSource.Contains("CCS_ShelterService")
                    && !serviceSource.Contains("CCS_BuildingService"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Shelter Binding",
                        "CCS_EnvironmentEffectsService binds shelter service without direct building dependency.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Shelter Binding",
                        "CCS_EnvironmentEffectsService is missing shelter integration.");
                }
            }

            if (File.Exists(displayPath))
            {
                string displaySource = File.ReadAllText(displayPath);
                if (displaySource.Contains("Sheltered:")
                    && displaySource.Contains("Shelter Wet:")
                    && displaySource.Contains("Shelter Exp:")
                    && displaySource.Contains("Shelter Temp:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Shelter HUD Display",
                        "Environment display formatting includes shelter protection values.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Shelter HUD Display",
                        "Environment display formatting is missing shelter protection values.");
                }
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
                    "Runtime environment effects scripts do not reference UnityEditor.");
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
