using System;

// =============================================================================
// SCRIPT: CCS_BusinessState
// CATEGORY: Modules / Settlements / Runtime / Businesses
// PURPOSE: Persisted per-business activation state on settlement simulation.
// PLACEMENT: Stored on CCS_SettlementSimulationState.businessStates.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — save/load via world simulation snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_BusinessState
    {
        public int businessType;

        public bool isActive;

        public CCS_BusinessType ResolvedBusinessType =>
            Enum.IsDefined(typeof(CCS_BusinessType), businessType)
                ? (CCS_BusinessType)businessType
                : CCS_BusinessType.Unknown;
    }
}
