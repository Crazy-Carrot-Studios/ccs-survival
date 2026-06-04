// =============================================================================
// SCRIPT: CCS_ReputationEventType
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Simple reputation event kinds for trust adjustments.
// PLACEMENT: Used by CCS_ReputationService event hooks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 conservative trust deltas via profile flags.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public enum CCS_ReputationEventType
    {
        None = 0,
        GoodsSold = 1,
        LoanRepaid = 2,
        UpkeepPaid = 3,
        SettlementDiscovered = 4,
        FailedUpkeep = 5,
        PlaceholderFuture = 6
    }
}
