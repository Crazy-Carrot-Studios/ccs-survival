using System.IO;
using CCS.Modules.SurvivalCore;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationValidator
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Editor validator for survival core folders, scripts, and profile rules.
// PLACEMENT: Registered with CCS_SurvivalValidationPipeline via CCS_SurvivalCoreValidationRegistration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Does not hard-code menu logic; appends to central validation report.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public sealed class CCS_SurvivalCoreValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string SurvivalCoreRoot = "Assets/CCS/Modules/SurvivalCore";
        private const string SurvivalCoreRuntimeRoot = SurvivalCoreRoot + "/Runtime";
        private const string SurvivalCoreEditorRoot = SurvivalCoreRoot + "/Editor";
        private const string SurvivalCoreDocPath =
            SurvivalCoreRoot + "/Documentation/CCS_Survival_Core_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.survivalcore";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime", SurvivalCoreRuntimeRoot);
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Stats", $"{SurvivalCoreRuntimeRoot}/Stats");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Profiles", $"{SurvivalCoreRuntimeRoot}/Profiles");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Runtime", $"{SurvivalCoreRuntimeRoot}/Runtime");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Events", $"{SurvivalCoreRuntimeRoot}/Events");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Validation", $"{SurvivalCoreRuntimeRoot}/Validation");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Environment", $"{SurvivalCoreRuntimeRoot}/Environment");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Presentation", $"{SurvivalCoreRuntimeRoot}/Presentation");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Services", $"{SurvivalCoreRuntimeRoot}/Services");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Editor", SurvivalCoreEditorRoot);
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Editor/Validation", $"{SurvivalCoreEditorRoot}/Validation");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Documentation", $"{SurvivalCoreRoot}/Documentation");

            ValidateRequiredScript(report, "CCS_SurvivalStatType", $"{SurvivalCoreRuntimeRoot}/Stats/CCS_SurvivalStatType.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreService", $"{SurvivalCoreRuntimeRoot}/Runtime/CCS_SurvivalCoreService.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreProfile", $"{SurvivalCoreRuntimeRoot}/Profiles/CCS_SurvivalCoreProfile.cs");
            ValidateRequiredScript(report, "CCS_SurvivalEnvironmentInfluence", $"{SurvivalCoreRuntimeRoot}/Environment/CCS_SurvivalEnvironmentInfluence.cs");
            ValidateRequiredScript(report, "CCS_SurvivalEnvironmentInfluenceUtility", $"{SurvivalCoreRuntimeRoot}/Environment/CCS_SurvivalEnvironmentInfluenceUtility.cs");
            ValidateRequiredScript(report, "CCS_SurvivalEnvironmentEventArgs", $"{SurvivalCoreRuntimeRoot}/Events/CCS_SurvivalEnvironmentEventArgs.cs");
            ValidateRequiredScript(report, "CCS_SurvivalEnvironmentInfluenceHudPresenter", $"{SurvivalCoreRuntimeRoot}/Presentation/CCS_SurvivalEnvironmentInfluenceHudPresenter.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreRuntimeBridge", $"{SurvivalCoreRuntimeRoot}/Services/CCS_SurvivalCoreRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreValidationUtility", $"{SurvivalCoreRuntimeRoot}/Validation/CCS_SurvivalCoreValidationUtility.cs");

            ValidateEnvironmentIntegration(report);
            ValidateCharacterStaminaIntegration(report);

            ValidateDocumentationAsset(report, "Survival Core Module Doc", SurvivalCoreDocPath);

            string defaultProfilePath =
                $"{SurvivalRoot}/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";

            if (File.Exists(defaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Survival Core Profile",
                    $"Asset present: {defaultProfilePath}");

                CCS_SurvivalCoreProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_SurvivalCoreProfile>(defaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_SurvivalCoreValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Survival Core Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Default Survival Core Profile",
                    $"Missing recommended asset: {defaultProfilePath}. Use menu to create.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Survival core validator completed (0.7.3 environment integration).");
        }

        #endregion

        #region Private Methods

        private static void ValidateEnvironmentIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = SurvivalCoreRuntimeRoot + "/Runtime/CCS_SurvivalCoreService.cs";
            const string profilePath = SurvivalCoreRuntimeRoot + "/Profiles/CCS_SurvivalCoreProfile.cs";
            const string registrationPath = SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
            const string bootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            const string bridgePath =
                "Assets/CCS/Modules/EnvironmentEffects/Runtime/Services/CCS_EnvironmentEffectsRuntimeBridge.cs";

            if (File.Exists(bridgePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Environment Runtime Bridge",
                    "CCS_EnvironmentEffectsRuntimeBridge resolves environment service safely.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Environment Runtime Bridge",
                    $"Missing environment runtime bridge: {bridgePath}");
            }

            if (File.Exists(profilePath))
            {
                string profileSource = File.ReadAllText(profilePath);
                if (profileSource.Contains("temperatureRecoveryRate")
                    && profileSource.Contains("temperatureDecayRate")
                    && profileSource.Contains("exposureFatigueMultiplier")
                    && profileSource.Contains("wetnessThirstMultiplier")
                    && profileSource.Contains("minimumTemperatureClamp")
                    && profileSource.Contains("maximumTemperatureClamp"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Survival Core Environment Profile",
                        "CCS_SurvivalCoreProfile exposes environment tuning fields.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Survival Core Environment Profile",
                        "CCS_SurvivalCoreProfile is missing environment tuning fields.");
                }
            }

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("BindEnvironmentEffectsService")
                    && serviceSource.Contains("ApplyTemperatureInfluence")
                    && serviceSource.Contains("ApplyFatigueInfluence")
                    && serviceSource.Contains("ApplyThirstInfluence")
                    && serviceSource.Contains("EnvironmentInfluenceChanged"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Survival Core Environment Integration",
                        "CCS_SurvivalCoreService reads environment snapshots and applies temperature/fatigue/thirst influence.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Survival Core Environment Integration",
                        "CCS_SurvivalCoreService is missing safe environment integration wiring.");
                }
            }

            if (File.Exists(registrationPath))
            {
                string registrationSource = File.ReadAllText(registrationPath);
                if (registrationSource.Contains("BindSurvivalCoreEnvironmentEffects")
                    && registrationSource.Contains("RegisterSurvivalCoreUpdatable"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Survival Core Service Registration",
                        "Gameplay composition binds environment effects to survival core and registers updatable tick.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Survival Core Service Registration",
                        "Gameplay composition is missing survival core environment binding.");
                }
            }

            if (File.Exists(bootstrapScenePath))
            {
                string sceneText = File.ReadAllText(bootstrapScenePath);
                if (sceneText.Contains("CCS_SurvivalEnvironmentInfluenceHudPresenter"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Environment Influence HUD",
                        "Bootstrap scene includes CCS_SurvivalEnvironmentInfluenceHudPresenter.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Environment Influence HUD",
                        "Bootstrap scene is missing CCS_SurvivalEnvironmentInfluenceHudPresenter.");
                }
            }
        }

        private static void ValidateCharacterStaminaIntegration(CCS_SurvivalValidationReport report)
        {
            const string registrationPath = SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
            const string movementServicePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Movement/CCS_CharacterMovementService.cs";

            if (File.Exists(registrationPath))
            {
                string registrationSource = File.ReadAllText(registrationPath);
                if (registrationSource.Contains("BindCharacterStaminaIntegration")
                    && registrationSource.Contains("StaminaDrainActive")
                    && registrationSource.Contains("CCS_SurvivalStatType.Stamina"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Character Stamina Integration",
                        "Gameplay composition binds character sprint/jump to survival core stamina safely.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Character Stamina Integration",
                        "Gameplay composition is missing character stamina integration.");
                }
            }

            if (File.Exists(movementServicePath))
            {
                string movementSource = File.ReadAllText(movementServicePath);
                if (movementSource.Contains("StaminaDrainRequested")
                    && movementSource.Contains("SetSprintAllowed"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Character Stamina Hooks",
                        "Character movement service exposes stamina drain events and sprint gating.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Character Stamina Hooks",
                        "Character movement service is missing stamina hook surface.");
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

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required script: {scriptPath}");
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
