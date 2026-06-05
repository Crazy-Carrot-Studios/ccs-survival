using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthDefinition
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Maps settlement growth stage requirements to bootstrap visual anchors.
// PLACEMENT: Serialized on CCS_SettlementVisualGrowthProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — optional business and service point references.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementVisualGrowthDefinition
    {
        public string anchorId = string.Empty;

        public string settlementId = string.Empty;

        public CCS_SettlementGrowthStage requiredGrowthStage = CCS_SettlementGrowthStage.Outpost;

        public CCS_SettlementVisualGrowthMarkerType markerType = CCS_SettlementVisualGrowthMarkerType.Unknown;

        public string displayName = string.Empty;

        public string businessId = string.Empty;

        public string servicePointId = string.Empty;

        public string AnchorId => anchorId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName) ? markerType.ToString() : displayName;
    }
}
