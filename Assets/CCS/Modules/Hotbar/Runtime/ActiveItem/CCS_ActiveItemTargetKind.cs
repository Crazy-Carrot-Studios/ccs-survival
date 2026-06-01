// =============================================================================
// SCRIPT: CCS_ActiveItemTargetKind
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Classifies the focused interactable for active tool use routing.
// PLACEMENT: Set by CCS_ActiveItemTargetResolver during use attempts.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: None means no harvestable target was resolved from interaction scan.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public enum CCS_ActiveItemTargetKind
    {
        None = 0,
        GatheringNode = 1,
        HarvestableResource = 2,
        UnsupportedInteractable = 3
    }
}
