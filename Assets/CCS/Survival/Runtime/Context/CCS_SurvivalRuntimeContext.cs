using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalRuntimeContext
// CATEGORY: Survival / Runtime
// PURPOSE: Instance-owned survival layer state bound to a single CCS_RuntimeHost.
// PLACEMENT: Created by CCS_SurvivalBootstrap on the survival bootstrap root.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No static singleton. Core must never reference this type. FUTURE: multiplayer role mapping extends SimulationRole.
// =============================================================================

namespace CCS.Survival
{
    public enum CCS_SurvivalSimulationRole
    {
        Unassigned = 0,
        LocalAuthority = 1
    }

    public sealed class CCS_SurvivalRuntimeContext
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.SurvivalContextLogCategory;

        #region Variables

        private readonly bool enableDebugLogs;
        private bool isSurvivalLayerInitialized;

        #endregion

        #region Public Methods

        public CCS_SurvivalRuntimeContext(CCS_RuntimeHost runtimeHost, bool enableDebugLogs = false)
        {
            RuntimeHost = runtimeHost;
            this.enableDebugLogs = enableDebugLogs;
            SimulationRole = CCS_SurvivalSimulationRole.Unassigned;
        }

        public CCS_Result Initialize()
        {
            CCS_Result hostValidation = CCS_CoreValidation.ValidateRuntimeHost(RuntimeHost);
            if (!hostValidation.IsSuccess)
            {
                return hostValidation;
            }

            if (!RuntimeHost.IsRuntimeInitialized)
            {
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Runtime host must finish Awake before survival context initialization.");
            }

            SimulationRole = CCS_SurvivalSimulationRole.LocalAuthority;
            isSurvivalLayerInitialized = true;

            CCS_Logger.Log(LogCategory, "Survival runtime context initialized.", enableDebugLogs);
            return CCS_Result.Success();
        }

        public void Shutdown()
        {
            isSurvivalLayerInitialized = false;
            SimulationRole = CCS_SurvivalSimulationRole.Unassigned;
            CCS_Logger.Log(LogCategory, "Survival runtime context shutdown.", enableDebugLogs);
        }

        #endregion

        #region Properties

        public CCS_RuntimeHost RuntimeHost { get; }

        public bool IsSurvivalLayerInitialized => isSurvivalLayerInitialized;

        public CCS_SurvivalSimulationRole SimulationRole { get; private set; }

        #endregion
    }
}
