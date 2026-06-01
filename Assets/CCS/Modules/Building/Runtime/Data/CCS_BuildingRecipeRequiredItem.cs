using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRecipeRequiredItem
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Serializable inventory cost entry for a building recipe.
// PLACEMENT: Embedded on CCS_BuildingRecipe required item lists.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: itemDefinitionId is authoritative; itemDefinition aids editor authoring.
// =============================================================================

namespace CCS.Modules.Building
{
    [Serializable]
    public struct CCS_BuildingRecipeRequiredItem
    {
        [Tooltip("Stable inventory item ID consumed when placing this recipe.")]
        public string itemDefinitionId;

        [Tooltip("Optional direct item reference for editor validation and bootstrap.")]
        public CCS_ItemDefinition itemDefinition;

        [Tooltip("Quantity consumed per placement.")]
        public int quantity;
    }
}
