// =============================================================================
// SCRIPT: CCS_SettlementServiceActivationStatus
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Result status for settlement service point activation attempts.
// PLACEMENT: Used by CCS_SettlementServiceActivationResult and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.1 settlement service routing polish.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementServiceActivationStatus
    {
        Succeeded = 0,
        Unavailable = 1,
        Disabled = 2,
        ServiceMissing = 3,
        UnknownRoute = 4,
        Failed = 5
    }
}
