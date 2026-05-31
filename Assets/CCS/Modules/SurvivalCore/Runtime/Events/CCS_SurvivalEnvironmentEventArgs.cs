// =============================================================================
// SCRIPT: CCS_SurvivalEnvironmentEventArgs
// CATEGORY: Modules / SurvivalCore / Runtime / Events
// PURPOSE: Payload for environment influence change notifications.
// PLACEMENT: Passed to CCS_SurvivalCoreService.EnvironmentInfluenceChanged subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Influence rates only. No Health or damage data.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public sealed class CCS_SurvivalEnvironmentEventArgs
    {
        #region Public Methods

        public CCS_SurvivalEnvironmentEventArgs(CCS_SurvivalEnvironmentInfluence influence)
        {
            Influence = influence;
        }

        #endregion

        #region Properties

        public CCS_SurvivalEnvironmentInfluence Influence { get; }

        #endregion
    }
}
