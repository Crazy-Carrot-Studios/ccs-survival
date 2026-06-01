using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_FishingRequest
// CATEGORY: Modules / Fishing / Runtime / Data
// PURPOSE: Payload for a fishing attempt from active item routing or interactables.
// PLACEMENT: Built by CCS_FishingService and CCS_ActiveItemService fishing routes.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Service-driven request object for future multiplayer authority extensions.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingRequest
    {
        public CCS_FishingRequest(
            CCS_FishingSpot fishingSpot,
            CCS_ItemDefinition fishingPoleItem,
            string baitItemDefinitionId = null)
        {
            FishingSpot = fishingSpot;
            FishingPoleItem = fishingPoleItem;
            BaitItemDefinitionId = baitItemDefinitionId;
        }

        public CCS_FishingSpot FishingSpot { get; }

        public CCS_ItemDefinition FishingPoleItem { get; }

        public string BaitItemDefinitionId { get; }
    }
}
