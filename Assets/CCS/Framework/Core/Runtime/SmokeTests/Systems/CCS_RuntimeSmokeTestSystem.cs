// =============================================================================
// SCRIPT: CCS_RuntimeSmokeTestSystem
// CATEGORY: Framework / Tests / Runtime / SmokeTests
// PURPOSE: Validates CCS runtime system and update loop execution in Play Mode.
// PLACEMENT: Registered by CCS_RuntimeSmokeTestInstaller. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Architecture validation only. No gameplay logic. No services or events.
// =============================================================================

namespace CCS.Core.Tests
{
    public sealed class CCS_RuntimeSmokeTestSystem : CCS_ISystem, CCS_IUpdatable
    {
        private const string LogCategory = "SmokeTest";

        #region Variables

        private bool isInitialized;
        private bool hasLoggedTick;
        private readonly bool enableDebugLogs;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        #endregion

        #region Public Methods

        public CCS_RuntimeSmokeTestSystem(bool enableDebugLogs)
        {
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Initialize()
        {
            isInitialized = true;
            CCS_Logger.Log(LogCategory, "Runtime smoke test initialized", enableDebugLogs);
        }

        public void Shutdown()
        {
            isInitialized = false;
            CCS_Logger.Log(LogCategory, "Runtime smoke test shutdown", enableDebugLogs);
        }

        public void Tick(float deltaTime)
        {
            if (hasLoggedTick)
            {
                return;
            }

            hasLoggedTick = true;
            CCS_Logger.Log(LogCategory, "Runtime update loop tick confirmed", enableDebugLogs);
        }

        #endregion
    }
}
