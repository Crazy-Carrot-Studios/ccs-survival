using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_WildlifeEventArgs
// CATEGORY: Modules / Wildlife / Runtime / Events
// PURPOSE: Event payload for wildlife harvest notifications.
// PLACEMENT: Passed to wildlife harvest service event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No UI references in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeEventArgs
    {
        #region Public Methods

        public CCS_WildlifeEventArgs(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey = "",
            IReadOnlyList<CCS_WildlifeHarvestedItemDrop> drops = null,
            string message = "")
        {
            WildlifeDefinition = wildlifeDefinition;
            WildlifeState = wildlifeState;
            InstanceKey = instanceKey ?? string.Empty;
            Drops = drops ?? System.Array.Empty<CCS_WildlifeHarvestedItemDrop>();
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_WildlifeDefinition WildlifeDefinition { get; }

        public CCS_WildlifeState WildlifeState { get; }

        public string InstanceKey { get; }

        public IReadOnlyList<CCS_WildlifeHarvestedItemDrop> Drops { get; }

        public string Message { get; }

        #endregion
    }
}
