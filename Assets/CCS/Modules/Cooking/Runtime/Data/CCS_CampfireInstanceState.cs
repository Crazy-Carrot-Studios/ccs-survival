// =============================================================================
// SCRIPT: CCS_CampfireInstanceState
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Mutable runtime state for one campfire instance tracked by CCS_CampfireService.
// PLACEMENT: Owned by CCS_CampfireService keyed by instance ID.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: BurnedOut is reserved for future fuel systems.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CampfireInstanceState
    {
        #region Public Methods

        public CCS_CampfireInstanceState(
            CCS_CampfireDefinition campfireDefinition,
            string instanceKey,
            CCS_CampfireState initialState = CCS_CampfireState.Unlit)
        {
            CampfireDefinition = campfireDefinition;
            InstanceKey = instanceKey ?? string.Empty;
            CampfireState = initialState;
        }

        public void SetState(CCS_CampfireState campfireState)
        {
            CampfireState = campfireState;
        }

        #endregion

        #region Properties

        public CCS_CampfireDefinition CampfireDefinition { get; }

        public string InstanceKey { get; }

        public CCS_CampfireState CampfireState { get; private set; }

        #endregion
    }
}
