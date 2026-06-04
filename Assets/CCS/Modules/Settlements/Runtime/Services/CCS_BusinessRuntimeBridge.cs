// =============================================================================
// SCRIPT: CCS_BusinessRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Static bridge for settlement service points to query business activation.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration after business service starts.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — simulation-level gating without scene scanning.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessRuntimeBridge
    {
        public static System.Func<string, CCS_BusinessType, bool> IsBusinessActiveAtSettlement;

        public static bool TryIsBusinessActive(string settlementId, CCS_BusinessType businessType)
        {
            if (IsBusinessActiveAtSettlement == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return true;
            }

            return IsBusinessActiveAtSettlement.Invoke(settlementId, businessType);
        }

        public static bool TryIsServicePointActive(string settlementId, CCS_SettlementServicePointType servicePointType)
        {
            CCS_BusinessType businessType = CCS_BusinessServicePointMapping.MapServicePointType(servicePointType);
            if (businessType == CCS_BusinessType.Unknown)
            {
                return true;
            }

            return TryIsBusinessActive(settlementId, businessType);
        }
    }
}
