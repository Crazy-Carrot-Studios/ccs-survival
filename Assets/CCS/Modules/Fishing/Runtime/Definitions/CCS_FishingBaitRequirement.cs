using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingBaitRequirement
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Optional bait gate for fishing spots (disabled by default in 1.2.5).
// PLACEMENT: Serialized on CCS_FishingSpotDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Future extension for consumable bait and crafting hooks.
// =============================================================================

namespace CCS.Modules.Fishing
{
    [Serializable]
    public sealed class CCS_FishingBaitRequirement
    {
        [Tooltip("When enabled, TryFish requires bait in player inventory.")]
        public bool requireBait;

        [Tooltip("Inventory item id consumed or validated when requireBait is true.")]
        public string baitItemDefinitionId = "ccs.survival.item.consumable.bait";

        [Tooltip("Bait quantity required per attempt.")]
        [Min(1)]
        public int requiredQuantity = 1;

        [Tooltip("When enabled, bait is removed from inventory on successful attempts.")]
        public bool consumeBaitOnAttempt;
    }
}
