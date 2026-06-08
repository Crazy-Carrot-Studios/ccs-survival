using CCS.Core;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapValidationUtility
// CATEGORY: Survival / Runtime / Foundation / Scene
// PURPOSE: Static validation for survival scene bootstrap composition during bootstrap/diagnostics only.
// PLACEMENT: Invoked from CCS_SurvivalDiagnostics. No per-frame scanning. No gameplay rules.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Optional profile slots do not fail skeleton bootstrap. Uses CCS_SurvivalValidationResult.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_SurvivalSceneBootstrapValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateSceneBootstrap(
            CCS_SurvivalRuntimeContext survivalContext,
            CCS_SurvivalBootstrap survivalBootstrap)
        {
            CCS_SurvivalValidationResult hostValidation = ValidateRuntimeHost(survivalContext);
            if (!hostValidation.IsSuccess)
            {
                return hostValidation;
            }

            CCS_RuntimeHost runtimeHost = survivalContext.RuntimeHost;

            CCS_SurvivalValidationResult bootstrapValidation = ValidateSurvivalBootstrap(survivalBootstrap);
            if (!bootstrapValidation.IsSuccess)
            {
                return bootstrapValidation;
            }

            CCS_SurvivalValidationResult contextValidation = ValidateSurvivalContext(survivalContext);
            if (!contextValidation.IsSuccess)
            {
                return contextValidation;
            }

            CCS_SurvivalValidationResult compositionValidation = ValidateCompositionRoot(runtimeHost, survivalBootstrap);
            if (!compositionValidation.IsSuccess)
            {
                return compositionValidation;
            }

            CCS_SurvivalValidationResult duplicateValidation = ValidateNoDuplicateSceneBootstrapComponents();
            if (!duplicateValidation.IsSuccess)
            {
                return duplicateValidation;
            }

            CCS_SurvivalValidationResult installerValidation = ValidateBootstrapInstallerExpectation(runtimeHost);
            if (!installerValidation.IsSuccess)
            {
                return installerValidation;
            }

            CCS_SurvivalValidationResult profileSlotsValidation = ValidateOptionalProfileSlots(survivalBootstrap);
            if (!profileSlotsValidation.IsSuccess)
            {
                return profileSlotsValidation;
            }

            if (hostValidation.IsWarning
                || duplicateValidation.IsWarning
                || contextValidation.IsWarning
                || installerValidation.IsWarning
                || profileSlotsValidation.IsWarning)
            {
                return CCS_SurvivalValidationResult.Warn(
                    BuildWarningSummary(hostValidation, duplicateValidation, contextValidation, installerValidation, profileSlotsValidation));
            }

            return CCS_SurvivalValidationResult.Pass("Survival scene bootstrap standards validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRuntimeHost(CCS_SurvivalRuntimeContext survivalContext)
        {
            if (survivalContext == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival runtime context is null.");
            }

            CCS_RuntimeHost runtimeHost = survivalContext.RuntimeHost;
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            if (!runtimeHost.IsRuntimeInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Runtime host is not initialized.");
            }

            if (runtimeHost.EnableRuntimeDiagnostics)
            {
                return CCS_SurvivalValidationResult.Warn(CCS_SurvivalSceneBootstrapRules.SurvivalDiagnosticsOwnershipRule);
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateSurvivalBootstrap(CCS_SurvivalBootstrap survivalBootstrap)
        {
            if (survivalBootstrap == null)
            {
                return CCS_SurvivalValidationResult.Fail("CCS_SurvivalBootstrap is missing on the survival composition root.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateSurvivalContext(CCS_SurvivalRuntimeContext survivalContext)
        {
            if (survivalContext == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival runtime context is null.");
            }

            if (!survivalContext.IsSurvivalLayerInitialized)
            {
                return CCS_SurvivalValidationResult.Warn("Survival runtime context is not initialized yet.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateCompositionRoot(
            CCS_RuntimeHost runtimeHost,
            CCS_SurvivalBootstrap survivalBootstrap)
        {
            if (runtimeHost == null || survivalBootstrap == null)
            {
                return CCS_SurvivalValidationResult.Fail("Runtime host and survival bootstrap are required on the composition root.");
            }

            if (runtimeHost.gameObject != survivalBootstrap.gameObject)
            {
                return CCS_SurvivalValidationResult.Warn(
                    "CCS_RuntimeHost and CCS_SurvivalBootstrap should share the same composition root GameObject.");
            }

            CCS_RuntimeHost hostOnBootstrapObject = survivalBootstrap.GetComponent<CCS_RuntimeHost>();
            if (hostOnBootstrapObject == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "CCS_RuntimeHost must be present on the same GameObject as CCS_SurvivalBootstrap.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateNoDuplicateSceneBootstrapComponents()
        {
            CCS_RuntimeHost[] runtimeHosts = Object.FindObjectsByType<CCS_RuntimeHost>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            if (runtimeHosts.Length > CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected {CCS_SurvivalRuntimeConstants.ExpectedRuntimeHostCount} runtime host in the loaded scene, found {runtimeHosts.Length}.");
            }

            CCS_SurvivalBootstrap[] survivalBootstraps = Object.FindObjectsByType<CCS_SurvivalBootstrap>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            if (survivalBootstraps.Length > CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected {CCS_SurvivalRuntimeConstants.ExpectedSurvivalBootstrapCount} survival bootstrap in the loaded scene, found {survivalBootstraps.Length}.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateBootstrapInstallerExpectation(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            CCS_CoreDiagnosticsReport report = runtimeHost.BuildDiagnosticsReport();
            if (report == null)
            {
                return CCS_SurvivalValidationResult.Warn("Core diagnostics report unavailable for bootstrap installer validation.");
            }

            if (report.BootstrapInstallerCount != 1)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected 1 bootstrap installer during skeleton phase, got {report.BootstrapInstallerCount}.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateOptionalProfileSlots(CCS_SurvivalBootstrap survivalBootstrap)
        {
            if (survivalBootstrap == null)
            {
                return CCS_SurvivalValidationResult.Pass();
            }

            System.Collections.Generic.IReadOnlyList<CCS_SurvivalBootstrapProfileSlot> profileSlots =
                survivalBootstrap.BootstrapProfileSlots;

            if (profileSlots == null || profileSlots.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass("No bootstrap profile slots configured (optional during skeleton phase).");
            }

            for (int index = 0; index < profileSlots.Count; index++)
            {
                CCS_SurvivalValidationResult slotValidation = profileSlots[index].ValidateSlot();
                if (!slotValidation.IsSuccess)
                {
                    return slotValidation;
                }
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion

        #region Private Methods

        private static string BuildWarningSummary(
            CCS_SurvivalValidationResult hostValidation,
            CCS_SurvivalValidationResult duplicateValidation,
            CCS_SurvivalValidationResult contextValidation,
            CCS_SurvivalValidationResult installerValidation,
            CCS_SurvivalValidationResult profileSlotsValidation)
        {
            if (hostValidation.IsWarning)
            {
                return hostValidation.Message;
            }

            if (duplicateValidation.IsWarning)
            {
                return duplicateValidation.Message;
            }

            if (contextValidation.IsWarning)
            {
                return contextValidation.Message;
            }

            if (installerValidation.IsWarning)
            {
                return installerValidation.Message;
            }

            if (profileSlotsValidation.IsWarning)
            {
                return profileSlotsValidation.Message;
            }

            return "Survival scene bootstrap validation completed with warnings.";
        }

        #endregion
    }
}
