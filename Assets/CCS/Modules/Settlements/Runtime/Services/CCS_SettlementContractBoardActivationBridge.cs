using System;

// =============================================================================
// SCRIPT: CCS_SettlementContractBoardActivationBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Composition-registered hook for contract board service point activation.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration after contract service starts.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Avoids Settlements -> Contracts asmdef cycle (WorldSimulation -> Settlements).
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementContractBoardActivationBridge
    {
        public static Func<CCS_SettlementServicePoint, CCS_SettlementServiceActivationResult> ActivateHandler;

        public static bool TryActivate(CCS_SettlementServicePoint servicePoint, out CCS_SettlementServiceActivationResult result)
        {
            result = null;
            if (servicePoint == null || ActivateHandler == null)
            {
                return false;
            }

            result = ActivateHandler.Invoke(servicePoint);
            return result != null;
        }
    }
}
