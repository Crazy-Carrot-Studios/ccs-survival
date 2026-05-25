// =============================================================================
// SCRIPT: CCS_SmokeTestModuleInstaller
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Validates CCS_ModuleInstallerBase and module host registry integration.
// PLACEMENT: Registered with CCS_BootstrapRunner during diagnostics Play Mode.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Test-only installer. No gameplay logic or auto-discovery.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_SmokeTestModuleInstaller : CCS_ModuleInstallerBase
    {
        private const string LogCategory = "SmokeTest";

        #region Variables

        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SmokeTestModuleInstaller(bool enableDebugLogs)
            : base(new CCS_SmokeTestModule(enableDebugLogs), enableDebugLogs)
        {
            this.enableDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnBeforeInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(LogCategory, "Smoke test module installer before install hook confirmed.", enableDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnAfterInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(LogCategory, "Smoke test module installer after install hook confirmed.", enableDebugLogs);
            return CCS_Result.Success();
        }

        protected override string GetLogCategory()
        {
            return LogCategory;
        }

        #endregion
    }
}
