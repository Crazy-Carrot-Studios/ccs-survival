// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthSnapshot
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Query snapshot of visual growth marker states for a settlement.
// PLACEMENT: Built by CCS_SettlementVisualGrowthValidationUtility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — derived from CCS_SettlementGrowthSnapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthSnapshot
    {
        public static readonly CCS_SettlementVisualGrowthSnapshot Empty = new CCS_SettlementVisualGrowthSnapshot();

        public string SettlementId { get; set; } = string.Empty;

        public CCS_SettlementGrowthStage CurrentGrowthStage { get; set; }

        public CCS_SettlementVisualGrowthEntry[] Entries { get; set; } =
            System.Array.Empty<CCS_SettlementVisualGrowthEntry>();

        public bool IsValid => !string.IsNullOrWhiteSpace(SettlementId);
    }

    public sealed class CCS_SettlementVisualGrowthEntry
    {
        public string AnchorId { get; set; } = string.Empty;

        public CCS_SettlementVisualGrowthMarkerType MarkerType { get; set; }

        public CCS_SettlementGrowthStage RequiredGrowthStage { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public CCS_SettlementVisualGrowthStatus Status { get; set; }
    }
}
