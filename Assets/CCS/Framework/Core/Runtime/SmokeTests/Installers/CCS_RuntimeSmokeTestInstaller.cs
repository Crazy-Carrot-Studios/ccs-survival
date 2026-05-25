// =============================================================================
// SCRIPT: CCS_RuntimeSmokeTestInstaller
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Installs runtime smoke test system into CCS_RuntimeHost for validation.
// PLACEMENT: Registered with CCS_BootstrapRunner during smoke test bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No global references. No services or events. Framework validation only.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_RuntimeSmokeTestInstaller : CCS_IBootstrapInstaller
    {
        private const string LogCategory = "SmokeTest";

        #region Variables

        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_RuntimeSmokeTestInstaller(bool enableDebugLogs)
        {
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Install(CCS_RuntimeHost runtimeHost)
        {
            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            CCS_RuntimeSmokeTestSystem smokeTestSystem = new CCS_RuntimeSmokeTestSystem(enableDebugLogs);
            smokeTestSystem.Initialize();
            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(smokeTestSystem);

            CCS_Logger.Log(LogCategory, "Smoke test installer completed", enableDebugLogs);
        }

        #endregion
    }
}
