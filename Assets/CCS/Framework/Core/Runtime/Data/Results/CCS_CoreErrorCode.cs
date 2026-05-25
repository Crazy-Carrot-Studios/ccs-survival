// =============================================================================
// SCRIPT: CCS_CoreErrorCode
// CATEGORY: Core / Runtime / Data
// PURPOSE: Stable error classification codes for CCS Core operation results.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Used with CCS_Result. None indicates success or unspecified failure.
// =============================================================================

namespace CCS.Core
{
    public enum CCS_CoreErrorCode
    {
        None = 0,
        Unknown = 1,
        NullRuntimeHost = 2,
        NullModule = 3,
        InvalidModuleId = 4,
        DuplicateModuleId = 5,
        ModuleNotRegistered = 6,
        ModuleAlreadyInstalled = 7,
        ModuleNotInstalled = 8,
        ModuleInstallFailed = 9,
        ModuleUninstallFailed = 10,
        NullService = 11,
        InvalidServiceType = 12,
        DuplicateService = 13,
        ServiceNotRegistered = 14,
        NullBootstrapInstaller = 15,
        DuplicateBootstrapInstaller = 16,
        NullUpdatable = 17,
        DuplicateUpdatable = 18,
        ValidationFailed = 19,
        MissingRequiredModuleDependency = 20,
        MissingRequiredServiceDependency = 21,
        DuplicateInstallPlanEntry = 22,
        InstallPlanDependencyOrderInvalid = 23,
        InstallPlanFailed = 24
    }
}
