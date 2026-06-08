using System.Collections.Generic;
using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalModuleValidationUtility
// CATEGORY: Survival / Runtime / Foundation / Validation
// PURPOSE: Static survival module and skeleton-phase host validation helpers.
// PLACEMENT: Runtime utility. Not attached to GameObjects. No gameplay rules.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Uses CCS_Result via CCS_SurvivalValidationResult. No service or updatable registration.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_SurvivalModuleValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateModuleNotNull(CCS_IModule module)
        {
            if (module == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival module reference is null.");
            }

            return CCS_SurvivalValidationResult.Pass("Survival module reference is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateModuleId(string moduleId)
        {
            if (string.IsNullOrWhiteSpace(moduleId))
            {
                return CCS_SurvivalValidationResult.Fail("Survival module ID is null or empty.");
            }

            return CCS_SurvivalValidationResult.Pass("Survival module ID is present.");
        }

        public static CCS_SurvivalValidationResult ValidateModuleIdPrefix(string moduleId)
        {
            CCS_SurvivalValidationResult idValidation = ValidateModuleId(moduleId);
            if (!idValidation.IsSuccess)
            {
                return idValidation;
            }

            if (!moduleId.StartsWith(CCS_SurvivalRuntimeConstants.ModuleIdPrefix))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Survival module ID must start with '{CCS_SurvivalRuntimeConstants.ModuleIdPrefix}'. Got: {moduleId}");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Survival module ID prefix validated for '{moduleId}'.");
        }

        public static CCS_SurvivalValidationResult ValidateModule(CCS_IModule module)
        {
            CCS_SurvivalValidationResult notNullValidation = ValidateModuleNotNull(module);
            if (!notNullValidation.IsSuccess)
            {
                return notNullValidation;
            }

            return ValidateModuleIdPrefix(module.Metadata.ModuleId);
        }

        public static CCS_SurvivalValidationResult ValidateNoDuplicateModuleIds(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            IReadOnlyCollection<CCS_IModule> registeredModules = runtimeHost.ModuleHost.GetRegisteredModules();
            HashSet<string> seenModuleIds = new HashSet<string>();

            foreach (CCS_IModule module in registeredModules)
            {
                if (module == null)
                {
                    continue;
                }

                string moduleId = module.Metadata.ModuleId;
                if (!seenModuleIds.Add(moduleId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate survival module ID detected: {moduleId}");
                }
            }

            return CCS_SurvivalValidationResult.Pass("No duplicate survival module IDs detected.");
        }

        public static CCS_SurvivalValidationResult ValidateExpectedSkeletonModuleCount(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            int registeredModuleCount = runtimeHost.ModuleHost.GetRegisteredModules().Count;
            if (registeredModuleCount != CCS_SurvivalRuntimeConstants.ExpectedSkeletonModuleCount)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected {CCS_SurvivalRuntimeConstants.ExpectedSkeletonModuleCount} survival module(s), got {registeredModuleCount}.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Skeleton module count validated ({CCS_SurvivalRuntimeConstants.ExpectedSkeletonModuleCount}).");
        }

        public static CCS_SurvivalValidationResult ValidateSkeletonServiceCount(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            CCS_ServiceDiagnosticsInfo serviceDiagnostics = runtimeHost.ServiceRegistry.BuildDiagnosticsSnapshot();
            if (serviceDiagnostics.RegisteredServiceCount != CCS_SurvivalRuntimeConstants.SkeletonExpectedServicesCount)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected {CCS_SurvivalRuntimeConstants.SkeletonExpectedServicesCount} survival services during skeleton phase, got {serviceDiagnostics.RegisteredServiceCount}.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Skeleton service count validated ({CCS_SurvivalRuntimeConstants.SkeletonExpectedServicesCount}).");
        }

        public static CCS_SurvivalValidationResult ValidateSkeletonUpdateSystemCount(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail(hostValidation.Message);
            }

            CCS_UpdateLoopDiagnosticsInfo updateDiagnostics = runtimeHost.RuntimeUpdateLoop.BuildDiagnosticsSnapshot();
            int totalUpdateSystems =
                updateDiagnostics.UpdatableSystemCount
                + updateDiagnostics.FixedUpdatableSystemCount
                + updateDiagnostics.LateUpdatableSystemCount;

            if (totalUpdateSystems != CCS_SurvivalRuntimeConstants.SkeletonExpectedUpdateSystemsCount)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"Expected {CCS_SurvivalRuntimeConstants.SkeletonExpectedUpdateSystemsCount} update systems during skeleton phase, got {totalUpdateSystems}.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Skeleton update system count validated ({CCS_SurvivalRuntimeConstants.SkeletonExpectedUpdateSystemsCount}).");
        }

        public static CCS_SurvivalValidationResult ValidateSkeletonHostState(CCS_RuntimeHost runtimeHost)
        {
            CCS_SurvivalValidationResult moduleCountValidation = ValidateExpectedSkeletonModuleCount(runtimeHost);
            if (!moduleCountValidation.IsSuccess)
            {
                return moduleCountValidation;
            }

            CCS_SurvivalValidationResult duplicateValidation = ValidateNoDuplicateModuleIds(runtimeHost);
            if (!duplicateValidation.IsSuccess)
            {
                return duplicateValidation;
            }

            CCS_SurvivalValidationResult serviceValidation = ValidateSkeletonServiceCount(runtimeHost);
            if (!serviceValidation.IsSuccess)
            {
                return serviceValidation;
            }

            CCS_SurvivalValidationResult updateValidation = ValidateSkeletonUpdateSystemCount(runtimeHost);
            if (!updateValidation.IsSuccess)
            {
                return updateValidation;
            }

            if (!runtimeHost.ModuleHost.IsModuleInstalled(CCS_SurvivalRuntimeConstants.CharacterModuleId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Expected installed module: {CCS_SurvivalRuntimeConstants.CharacterModuleId}");
            }

            if (moduleCountValidation.IsWarning)
            {
                return moduleCountValidation;
            }

            if (serviceValidation.IsWarning)
            {
                return serviceValidation;
            }

            if (updateValidation.IsWarning)
            {
                return updateValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Skeleton host state validated.");
        }

        #endregion
    }
}
