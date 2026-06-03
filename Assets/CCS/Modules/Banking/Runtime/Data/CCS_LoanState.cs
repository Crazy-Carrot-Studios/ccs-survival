// =============================================================================
// SCRIPT: CCS_LoanState
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Loan lifecycle states for borrow, due, paid, and future default handling.
// PLACEMENT: Used by CCS_BankingService and CCS_LoanSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 — Defaulted and Disabled are placeholders without punishment.
// =============================================================================

namespace CCS.Modules.Banking
{
    public enum CCS_LoanState
    {
        None = 0,
        Active = 1,
        Due = 2,
        Paid = 3,
        Defaulted = 4,
        Disabled = 5
    }
}
