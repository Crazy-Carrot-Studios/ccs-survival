using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalDiagnostics
// CATEGORY: Survival / Diagnostics
// PURPOSE: Survival-owned diagnostics that verify Core health without running Core smoke tests.
// PLACEMENT: Invoked by CCS_SurvivalBootstrap when survival diagnostics are enabled.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Read-only Core report usage. Does not modify SCN_CCS_Bootstrap or Core smoke installers.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalDiagnostics
    {
        private const string LogCategory = "Survival Diagnostics";

        #region Public Methods

        public static CCS_Result RunCoreHealthValidation(CCS_SurvivalRuntimeContext survivalContext, bool enableDebugLogs)
        {
            if (!CCS_Validation.IsObjectValid(survivalContext))
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Survival context is null.");
            }

            CCS_RuntimeHost runtimeHost = survivalContext.RuntimeHost;
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, hostValidation.Message);
                return hostValidation;
            }

            if (!runtimeHost.IsRuntimeInitialized)
            {
                CCS_Logger.LogWarning(LogCategory, "Runtime host is not initialized.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Runtime host must be initialized before survival diagnostics.");
            }

            if (runtimeHost.EnableRuntimeDiagnostics)
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    "Core runtime diagnostics are enabled on this host. Survival scenes should keep Core diagnostics disabled and use survival diagnostics ownership instead.");
            }

            CCS_CoreDiagnosticsReport report = runtimeHost.BuildDiagnosticsReport();
            if (report == null)
            {
                CCS_Logger.LogWarning(LogCategory, "Core diagnostics report was null.");
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Core diagnostics report was null.");
            }

            if (!report.IsRuntimeHostInitialized)
            {
                CCS_Logger.LogWarning(LogCategory, "Core diagnostics report expected initialized runtime host.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Core diagnostics report expected initialized runtime host.");
            }

            if (report.RegisteredModuleCount != 1)
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    $"Expected one survival module at 0.3.0, got {report.RegisteredModuleCount}.");
            }

            if (!runtimeHost.ModuleHost.IsModuleInstalled(CCS_SurvivalCharacterDiagnostics.ModuleId))
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    $"Expected installed module: {CCS_SurvivalCharacterDiagnostics.ModuleId}");
            }

            CCS_Logger.Log(
                LogCategory,
                $"Core health OK. Modules={report.RegisteredModuleCount}, Services={report.Services.RegisteredServiceCount}, BootstrapInstallers={report.BootstrapInstallerCount}",
                enableDebugLogs);

            return CCS_Result.Success();
        }

        #endregion
    }
}
