// =============================================================================
// SCRIPT: CCS_SettlementServiceRouteType
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Identifies how a settlement service point routes when activated.
// PLACEMENT: Used by service point activation results and playtest validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.1 settlement service routing polish.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementServiceRouteType
    {
        Unknown = 0,
        Vendor = 1,
        Industry = 2,
        Placeholder = 3,
        Disabled = 4,
        Unavailable = 5
    }
}
