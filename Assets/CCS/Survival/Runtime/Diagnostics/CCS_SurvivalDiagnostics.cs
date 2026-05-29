using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalDiagnostics
// CATEGORY: Survival / Diagnostics
// PURPOSE: Survival-owned diagnostics that verify Core health and survival validation rules.
// PLACEMENT: Invoked by CCS_SurvivalBootstrap when survival diagnostics are enabled.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Bootstrap-time only. See CCS_SurvivalFrameworkFutureMarkers.FeatureDiagnosticsExtension.
// =============================================================================

using CCS.Survival.Inventory;

namespace CCS.Survival
{
    public static class CCS_SurvivalDiagnostics
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.SurvivalDiagnosticsLogCategory;

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

            CCS_Result survivalRulesResult = RunSurvivalValidationRules(runtimeHost, enableDebugLogs);
            if (!survivalRulesResult.IsSuccess)
            {
                return survivalRulesResult;
            }

            CCS_SurvivalBootstrap survivalBootstrap = runtimeHost.GetComponent<CCS_SurvivalBootstrap>();
            CCS_SurvivalValidationResult sceneBootstrapValidation =
                CCS_SurvivalSceneBootstrapValidationUtility.ValidateSceneBootstrap(survivalContext, survivalBootstrap);
            LogValidationResult(sceneBootstrapValidation, enableDebugLogs);
            if (!sceneBootstrapValidation.IsSuccess)
            {
                return sceneBootstrapValidation.ToCoreResult();
            }

            CCS_Logger.Log(
                LogCategory,
                $"Core health OK. Modules={report.RegisteredModuleCount}, Services={report.Services.RegisteredServiceCount}, BootstrapInstallers={report.BootstrapInstallerCount}",
                enableDebugLogs);

            return CCS_Result.Success();
        }

        public static CCS_Result RunSurvivalValidationRules(CCS_RuntimeHost runtimeHost, bool enableDebugLogs)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, hostValidation.Message);
                return hostValidation;
            }

            LogValidationResult(
                CCS_SurvivalModuleValidationUtility.ValidateExpectedSkeletonModuleCount(runtimeHost),
                enableDebugLogs);

            LogValidationResult(
                CCS_SurvivalModuleValidationUtility.ValidateNoDuplicateModuleIds(runtimeHost),
                enableDebugLogs);

            LogValidationResult(
                CCS_SurvivalModuleValidationUtility.ValidateSkeletonServiceCount(runtimeHost),
                enableDebugLogs);

            LogValidationResult(
                CCS_SurvivalModuleValidationUtility.ValidateSkeletonUpdateSystemCount(runtimeHost),
                enableDebugLogs);

            CCS_SurvivalValidationResult characterModuleValidation =
                CCS_SurvivalModuleValidationUtility.ValidateModuleIdPrefix(CCS_SurvivalRuntimeConstants.CharacterModuleId);
            LogValidationResult(characterModuleValidation, enableDebugLogs);

            if (!runtimeHost.ModuleHost.IsModuleInstalled(CCS_SurvivalRuntimeConstants.CharacterModuleId))
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    $"Expected installed module: {CCS_SurvivalRuntimeConstants.CharacterModuleId}");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    $"Expected installed module: {CCS_SurvivalRuntimeConstants.CharacterModuleId}");
            }

            if (!characterModuleValidation.IsSuccess)
            {
                return characterModuleValidation.ToCoreResult();
            }

            CCS_SurvivalValidationResult inventoryModuleValidation =
                CCS_SurvivalModuleValidationUtility.ValidateModuleIdPrefix(CCS_SurvivalRuntimeConstants.InventoryModuleId);
            LogValidationResult(inventoryModuleValidation, enableDebugLogs);

            if (!runtimeHost.ModuleHost.IsModuleInstalled(CCS_SurvivalRuntimeConstants.InventoryModuleId))
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    $"Expected installed module: {CCS_SurvivalRuntimeConstants.InventoryModuleId}");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    $"Expected installed module: {CCS_SurvivalRuntimeConstants.InventoryModuleId}");
            }

            if (!inventoryModuleValidation.IsSuccess)
            {
                return inventoryModuleValidation.ToCoreResult();
            }

            if (!runtimeHost.ServiceRegistry.TryGetService(out CCS_ISurvivalInventoryService inventoryService))
            {
                CCS_Logger.LogWarning(LogCategory, "Expected CCS_ISurvivalInventoryService to be registered.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Expected CCS_ISurvivalInventoryService to be registered.");
            }

            if (inventoryService.SlotCount < 1)
            {
                CCS_Logger.LogWarning(LogCategory, "Inventory service slot count must be at least 1.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Inventory service slot count must be at least 1.");
            }

            CCS_CoreDiagnosticsReport report = runtimeHost.BuildDiagnosticsReport();
            if (report != null
                && report.BootstrapInstallerCount != 1)
            {
                CCS_Logger.LogWarning(
                    LogCategory,
                    $"Expected 1 bootstrap installer on runner during skeleton phase, got {report.BootstrapInstallerCount}.");
            }

            LogSceneBootstrapStandardsNotes(enableDebugLogs);

            LogAuthorityAvatarBoundaryNotes(enableDebugLogs);

            CCS_Logger.Log(LogCategory, "Survival validation rules passed.", enableDebugLogs);
            return CCS_Result.Success();
        }

        #endregion

        #region Private Methods

        private static void LogSceneBootstrapStandardsNotes(bool enableDebugLogs)
        {
            CCS_Logger.Log(
                CCS_SurvivalRuntimeConstants.BootstrapRulesLogCategory,
                CCS_SurvivalSceneBootstrapRules.CompositionRootRule,
                enableDebugLogs);

            CCS_Logger.Log(
                CCS_SurvivalRuntimeConstants.SceneValidationLogCategory,
                CCS_SurvivalRuntimeConstants.SceneIdentityGuidanceMessage,
                enableDebugLogs);
        }

        private static void LogAuthorityAvatarBoundaryNotes(bool enableDebugLogs)
        {
            CCS_Logger.Log(
                LogCategory,
                "Authority/avatar boundary contracts are defined (CCS_ISurvivalAuthority, CCS_ISurvivalAvatar). Skeleton bootstrap does not require runtime authority or avatar instances.",
                enableDebugLogs);

            CCS_Logger.Log(
                CCS_SurvivalRuntimeConstants.IdentityValidationLogCategory,
                CCS_SurvivalRuntimeConstants.StableRuntimeIdentityGuidanceMessage,
                enableDebugLogs);
        }

        private static void LogValidationResult(CCS_SurvivalValidationResult validationResult, bool enableDebugLogs)
        {
            string logMessage = ResolveValidationLogMessage(validationResult);

            if (!validationResult.IsSuccess)
            {
                CCS_Logger.LogWarning(
                    CCS_SurvivalRuntimeConstants.ValidationLogCategory,
                    logMessage);
                return;
            }

            if (validationResult.IsWarning)
            {
                CCS_Logger.LogWarning(
                    CCS_SurvivalRuntimeConstants.ValidationLogCategory,
                    logMessage);
                return;
            }

            CCS_Logger.Log(
                CCS_SurvivalRuntimeConstants.ValidationLogCategory,
                logMessage,
                enableDebugLogs);
        }

        private static string ResolveValidationLogMessage(CCS_SurvivalValidationResult validationResult)
        {
            if (!string.IsNullOrWhiteSpace(validationResult.Message))
            {
                return validationResult.Message;
            }

            return CCS_SurvivalRuntimeConstants.ValidationPassedNoDetailMessage;
        }

        #endregion
    }
}
