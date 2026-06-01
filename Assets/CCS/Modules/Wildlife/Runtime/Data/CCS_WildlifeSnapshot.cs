// =============================================================================
// SCRIPT: CCS_WildlifeSnapshot
// CATEGORY: Modules / Wildlife / Runtime / Data
// PURPOSE: Immutable snapshot of wildlife instance state for diagnostics and future save.
// PLACEMENT: Returned by CCS_HarvestableWildlife.GetSnapshot().
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Save integration deferred beyond 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeSnapshot
    {
        #region Public Methods

        public CCS_WildlifeSnapshot(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey = "")
        {
            WildlifeDefinition = wildlifeDefinition;
            WildlifeState = wildlifeState;
            InstanceKey = instanceKey ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_WildlifeDefinition WildlifeDefinition { get; }

        public CCS_WildlifeState WildlifeState { get; }

        public string InstanceKey { get; }

        #endregion
    }
}
