using System;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;

// =============================================================================
// SCRIPT: CCS_DynamicContractRuntimeBridge
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Runtime bridge for dynamic contract queries, playtest hooks, and debug HUD.
// PLACEMENT: Wired by CCS_DynamicContractService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — used by contract boards, playtest harness, and save restore.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_DynamicContractRuntimeBridge
    {
        public static Func<string, CCS_DynamicContractSnapshot[]> ResolveSettlementSnapshots;

        public static Func<CCS_DynamicContractGenerationRequest, CCS_DynamicContractGenerationResult> TryGenerateForRequest;

        public static Action<string> TryEvaluateSettlementSupply;

        public static Action<string> TryEvaluateRegionalSpecialization;

        public static Action<string, CCS_SettlementEventSnapshot> TryHandleEventActivated;

        public static Func<string, CCS_SettlementSupplyType, float, bool> TrySetSupplyFillPercentForPlaytest;

        public static CCS_DynamicContractGenerationResult LastGenerationResult { get; set; }

        public static bool TryGetSettlementSnapshots(string settlementId, out CCS_DynamicContractSnapshot[] snapshots)
        {
            snapshots = ResolveSettlementSnapshots?.Invoke(settlementId) ?? Array.Empty<CCS_DynamicContractSnapshot>();
            return snapshots != null && snapshots.Length > 0;
        }
    }
}
