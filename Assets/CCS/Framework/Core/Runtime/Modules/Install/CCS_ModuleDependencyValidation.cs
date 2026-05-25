using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ModuleDependencyValidation
// CATEGORY: Core / Runtime / Modules / Install
// PURPOSE: Validates declared module dependencies against current runtime host state.
// PLACEMENT: Static runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No auto-install. Required dependencies must already be satisfied.
// =============================================================================

namespace CCS.Core
{
    public static class CCS_ModuleDependencyValidation
    {
        #region Public Methods

        public static CCS_Result ValidateInstallDependencies(
            CCS_RuntimeHost runtimeHost,
            CCS_IModule module,
            int installPlanIndex = -1)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                return hostValidation;
            }

            CCS_Result moduleValidation = CCS_CoreValidation.ValidateModule(module);
            if (!moduleValidation.IsSuccess)
            {
                return moduleValidation;
            }

            IReadOnlyCollection<CCS_ModuleDependency> dependencies = module.Dependencies;
            if (dependencies == null || dependencies.Count == 0)
            {
                return CCS_Result.Success();
            }

            string planContext = installPlanIndex >= 0
                ? $" (install plan index {installPlanIndex})"
                : string.Empty;

            foreach (CCS_ModuleDependency dependency in dependencies)
            {
                CCS_Result dependencyResult = ValidateDependency(runtimeHost, dependency, planContext);
                if (!dependencyResult.IsSuccess)
                {
                    return dependencyResult;
                }
            }

            return CCS_Result.Success();
        }

        #endregion

        #region Private Methods

        private static CCS_Result ValidateDependency(
            CCS_RuntimeHost runtimeHost,
            CCS_ModuleDependency dependency,
            string planContext)
        {
            switch (dependency.DependencyType)
            {
                case CCS_ModuleDependencyType.RequiredModuleId:
                    if (!runtimeHost.ModuleHost.IsModuleInstalled(dependency.ModuleId))
                    {
                        return CCS_Result.Failure(
                            CCS_CoreErrorCode.MissingRequiredModuleDependency,
                            $"Required module dependency not installed: {dependency.ModuleId}{planContext}");
                    }

                    return CCS_Result.Success();

                case CCS_ModuleDependencyType.OptionalModuleId:
                    return CCS_Result.Success();

                case CCS_ModuleDependencyType.RequiredServiceType:
                    if (dependency.ServiceType == null)
                    {
                        return CCS_Result.Failure(
                            CCS_CoreErrorCode.InvalidServiceType,
                            $"Required service dependency has no service type{planContext}");
                    }

                    if (!runtimeHost.ServiceRegistry.HasServiceType(dependency.ServiceType))
                    {
                        return CCS_Result.Failure(
                            CCS_CoreErrorCode.MissingRequiredServiceDependency,
                            $"Required service dependency not registered: {dependency.ServiceType.Name}{planContext}");
                    }

                    return CCS_Result.Success();

                case CCS_ModuleDependencyType.OptionalServiceType:
                    return CCS_Result.Success();

                default:
                    return CCS_Result.Failure(
                        CCS_CoreErrorCode.ValidationFailed,
                        $"Unknown module dependency type: {dependency.DependencyType}{planContext}");
            }
        }

        #endregion
    }
}
