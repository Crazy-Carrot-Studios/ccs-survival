// =============================================================================
// SCRIPT: CCS_SmokeTestDependentModuleInstaller
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Installer for dependency validation smoke module.
// PLACEMENT: Used only by diagnostics smoke bridge. Not for production bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No auto-discovery. Manual invocation from smoke tests only.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_SmokeTestDependentModuleInstaller : CCS_ModuleInstallerBase
    {
        #region Public Methods

        public CCS_SmokeTestDependentModuleInstaller(bool enableDebugLogs)
            : base(new CCS_SmokeTestDependentModule(enableDebugLogs), enableDebugLogs)
        {
        }

        protected override string GetLogCategory()
        {
            return "SmokeTest";
        }

        #endregion
    }
}
