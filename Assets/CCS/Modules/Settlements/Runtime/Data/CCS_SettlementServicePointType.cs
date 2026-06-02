// =============================================================================
// SCRIPT: CCS_SettlementServicePointType
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Service point archetypes for settlement interaction routing.
// PLACEMENT: Used by CCS_SettlementServicePoint and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Vendor-backed types route to economy; Blacksmith routes to industry in 1.8.1.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementServicePointType
    {
        GeneralStore = 0,
        Stable = 1,
        Gunsmith = 2,
        Blacksmith = 3,
        Doctor = 4,
        Telegraph = 5,
        Sheriff = 6,
        Bank = 7,
        FreightDepot = 8,
        Other = 9
    }
}
