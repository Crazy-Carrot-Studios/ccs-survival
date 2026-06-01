using CCS.Modules.Gathering;
using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_ActiveItemTargetContext
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Resolved interaction target payload for active tool use routing.
// PLACEMENT: Built by CCS_ActiveItemTargetResolver; consumed by CCS_ActiveItemService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses interaction service current target; no duplicate targeting ray.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public readonly struct CCS_ActiveItemTargetContext
    {
        public CCS_ActiveItemTargetContext(
            CCS_ActiveItemTargetKind targetKind,
            string displayName,
            string targetTypeLabel,
            bool isOutOfRange,
            CCS_GatheringNode gatheringNode,
            CCS_HarvestableResource harvestableResource)
        {
            TargetKind = targetKind;
            DisplayName = displayName ?? string.Empty;
            TargetTypeLabel = targetTypeLabel ?? string.Empty;
            IsOutOfRange = isOutOfRange;
            GatheringNode = gatheringNode;
            HarvestableResource = harvestableResource;
        }

        public CCS_ActiveItemTargetKind TargetKind { get; }

        public string DisplayName { get; }

        public string TargetTypeLabel { get; }

        public bool IsOutOfRange { get; }

        public CCS_GatheringNode GatheringNode { get; }

        public CCS_HarvestableResource HarvestableResource { get; }

        public bool HasHarvestTarget =>
            TargetKind == CCS_ActiveItemTargetKind.GatheringNode
            || TargetKind == CCS_ActiveItemTargetKind.HarvestableResource;

        public static CCS_ActiveItemTargetContext None { get; } = new CCS_ActiveItemTargetContext(
            CCS_ActiveItemTargetKind.None,
            string.Empty,
            string.Empty,
            false,
            null,
            null);
    }
}
