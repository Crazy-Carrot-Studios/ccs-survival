using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalInstaller
// CATEGORY: Survival / Installers
// PURPOSE: Empty survival-layer bootstrap installer shell for future gameplay module sequencing.
// PLACEMENT: Registered on CCS_BootstrapRunner by CCS_SurvivalBootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No gameplay modules. No inventory/crafting/save. Downward dependency: Survival → Core only.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalInstaller : CCS_IBootstrapInstaller
    {
        private const string LogCategory = "Survival Installer";

        #region Variables

        private readonly CCS_SurvivalRuntimeContext survivalContext;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SurvivalInstaller(CCS_SurvivalRuntimeContext survivalContext, bool enableDebugLogs = false)
        {
            this.survivalContext = survivalContext;
            this.enableDebugLogs = enableDebugLogs;
        }

        public void Install(CCS_RuntimeHost runtimeHost)
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(runtimeHost);
            if (!hostValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(LogCategory, hostValidation.Message);
                return;
            }

            if (!CCS_Validation.IsObjectValid(survivalContext) || !survivalContext.IsSurvivalLayerInitialized)
            {
                CCS_Logger.LogWarning(LogCategory, "Survival context must be initialized before survival installer runs.");
                return;
            }

            // Milestone 0.2.0: empty composition root. Future module installers register here in explicit order.
            CCS_Logger.Log(LogCategory, "Survival installer completed (empty install pipeline).", enableDebugLogs);
        }

        #endregion
    }
}
