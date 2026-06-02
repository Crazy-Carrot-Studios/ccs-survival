using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterResourceCost
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Serializable resource cost entry for frontier shelter crafting.
// PLACEMENT: Embedded on CCS_ShelterDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [Serializable]
    public sealed class CCS_ShelterResourceCost
    {
        [SerializeField] private string itemDefinitionId = string.Empty;
        [SerializeField] private int quantity = 1;

        public string ItemDefinitionId => itemDefinitionId ?? string.Empty;

        public int Quantity => quantity < 1 ? 1 : quantity;
    }
}
