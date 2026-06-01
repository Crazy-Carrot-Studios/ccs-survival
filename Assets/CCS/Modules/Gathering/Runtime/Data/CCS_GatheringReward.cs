using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringReward
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Serializable reward entry mapping resource type to inventory item grants.
// PLACEMENT: Embedded on CCS_GatheringProfile node reward tables.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: itemDefinitionId resolves through profile reward item catalog at gather time.
// =============================================================================

namespace CCS.Modules.Gathering
{
    [Serializable]
    public struct CCS_GatheringReward
    {
        [Tooltip("Logical resource category for validation and HUD messaging.")]
        public CCS_GatheringResourceType resourceType;

        [Tooltip("Stable inventory item ID used to resolve CCS_ItemDefinition grants.")]
        public string itemDefinitionId;

        [Tooltip("Quantity granted when this reward entry is applied.")]
        public int amount;
    }
}
