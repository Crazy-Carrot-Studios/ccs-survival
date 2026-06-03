// =============================================================================
// SCRIPT: CCS_CropGrowthStage
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Timer-based crop growth stages for farm plots.
// PLACEMENT: Used by CCS_CropInstance and CCS_FarmService growth logic.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — no seasons or soil quality yet.
// =============================================================================

namespace CCS.Modules.Farming
{
    public enum CCS_CropGrowthStage
    {
        Empty = 0,
        Planted = 1,
        Sprouting = 2,
        Growing = 3,
        Mature = 4,
        Harvested = 5
    }
}
