// =============================================================================
// SCRIPT: CCS_BusinessServicePointMapping
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Maps settlement service point types to business simulation types.
// PLACEMENT: Used by CCS_BusinessRuntimeBridge and service point access checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessServicePointMapping
    {
        public static CCS_BusinessType MapServicePointType(CCS_SettlementServicePointType servicePointType)
        {
            return servicePointType switch
            {
                CCS_SettlementServicePointType.GeneralStore => CCS_BusinessType.GeneralStore,
                CCS_SettlementServicePointType.Stable => CCS_BusinessType.Stable,
                CCS_SettlementServicePointType.Gunsmith => CCS_BusinessType.Gunsmith,
                CCS_SettlementServicePointType.Blacksmith => CCS_BusinessType.Blacksmith,
                CCS_SettlementServicePointType.Bank => CCS_BusinessType.Bank,
                CCS_SettlementServicePointType.ContractBoard => CCS_BusinessType.ContractOffice,
                _ => CCS_BusinessType.Unknown
            };
        }
    }
}
