// =============================================================================
// SCRIPT: CCS_CampfireSnapshot
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Immutable snapshot of campfire instance state for diagnostics.
// PLACEMENT: Returned by CCS_CampfireInteractable.GetSnapshot().
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Save integration deferred beyond 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CampfireSnapshot
    {
        #region Public Methods

        public CCS_CampfireSnapshot(
            CCS_CampfireDefinition campfireDefinition,
            CCS_CampfireState campfireState,
            string instanceKey = "")
        {
            CampfireDefinition = campfireDefinition;
            CampfireState = campfireState;
            InstanceKey = instanceKey ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_CampfireDefinition CampfireDefinition { get; }

        public CCS_CampfireState CampfireState { get; }

        public string InstanceKey { get; }

        #endregion
    }
}
