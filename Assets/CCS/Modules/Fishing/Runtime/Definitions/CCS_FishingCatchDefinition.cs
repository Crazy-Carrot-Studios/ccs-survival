using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingCatchDefinition
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Weighted catch table entry for fishing spot or profile rolls.
// PLACEMENT: Arrays on CCS_FishingSpotDefinition and CCS_FishingProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: itemDefinitionId must resolve through profile item catalog when granted.
// =============================================================================

namespace CCS.Modules.Fishing
{
    [Serializable]
    public sealed class CCS_FishingCatchDefinition
    {
        [Tooltip("Catch category used for result typing and messaging.")]
        public CCS_FishingCatchKind catchKind = CCS_FishingCatchKind.Fish;

        [Tooltip("Inventory item id granted on success. Ignored for Nothing.")]
        public string itemDefinitionId;

        [Tooltip("Quantity granted when this entry wins the roll.")]
        [Min(1)]
        public int quantity = 1;

        [Tooltip("Relative weight for random selection. Must be positive.")]
        [Min(1)]
        public int weight = 10;
    }
}
