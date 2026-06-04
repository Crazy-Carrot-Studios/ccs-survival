using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionSnapshot
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Runtime discovery record for a frontier world region.
// PLACEMENT: Stored by CCS_RegionService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: No final map UI yet; supports future region tracking.
// =============================================================================

namespace CCS.Modules.Regions
{
    public sealed class CCS_RegionSnapshot
    {
        public static readonly CCS_RegionSnapshot Empty = new CCS_RegionSnapshot();

        public string RegionId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public CCS_RegionType RegionType { get; set; }

        public bool Discovered { get; set; }

        public Vector3 Position { get; set; }

        public CCS_RegionSpecializationType SpecializationType { get; set; }

        public CCS_RegionSpecializationType DominantIndustry { get; set; }

        public float FoodSupplyStrength { get; set; }

        public float IndustrialSupplyStrength { get; set; }

        public float BuildingSupplyStrength { get; set; }

        public float TradeSupplyStrength { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(RegionId)
            && !string.IsNullOrWhiteSpace(DisplayName);
    }
}
