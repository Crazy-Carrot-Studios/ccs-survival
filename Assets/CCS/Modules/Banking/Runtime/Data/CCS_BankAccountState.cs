// =============================================================================
// SCRIPT: CCS_BankAccountState
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Lifecycle state for stored-currency bank accounts.
// PLACEMENT: Used by CCS_BankingService and save snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 — loans, taxes, and suspension rules deferred.
// =============================================================================

namespace CCS.Modules.Banking
{
    public enum CCS_BankAccountState
    {
        Closed = 0,
        Open = 1,
        Suspended = 2
    }
}
