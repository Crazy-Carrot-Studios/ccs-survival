// =============================================================================
// SCRIPT: CCS_SmokeTestModule
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Minimal test-only module validating CCS_ModuleBase lifecycle hooks.
// PLACEMENT: Installed by CCS_SmokeTestModuleInstaller during diagnostics Play Mode.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Architecture validation only. No gameplay logic or services.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_SmokeTestModule : CCS_ModuleBase
    {
        public const string ModuleId = "ccs.smoketest.module";

        private const string LogCategory = "SmokeTest";

        #region Variables

        private readonly bool smokeTestDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SmokeTestModule(bool enableDebugLogs)
            : base(
                new CCS_ModuleMetadata(ModuleId, "CCS Smoke Test Module", "0.4.0", "Core platform Phase One validation."),
                enableDebugLogs)
        {
            smokeTestDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnInitialize()
        {
            CCS_Logger.Log(LogCategory, "Smoke test module initialize hook confirmed.", smokeTestDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnInstall(CCS_RuntimeHost runtimeHost)
        {
            if (LifecycleState == CCS_ModuleLifecycleState.Installing)
            {
                CCS_Logger.Log(LogCategory, "Smoke test module Installing lifecycle confirmed.", smokeTestDebugLogs);
            }

            CCS_Logger.Log(LogCategory, "Smoke test module install hook confirmed.", smokeTestDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnUninstall(CCS_RuntimeHost runtimeHost)
        {
            if (LifecycleState == CCS_ModuleLifecycleState.Uninstalling)
            {
                CCS_Logger.Log(LogCategory, "Smoke test module Uninstalling lifecycle confirmed.", smokeTestDebugLogs);
            }

            CCS_Logger.Log(LogCategory, "Smoke test module uninstall hook confirmed.", smokeTestDebugLogs);
            return CCS_Result.Success();
        }

        #endregion
    }
}
