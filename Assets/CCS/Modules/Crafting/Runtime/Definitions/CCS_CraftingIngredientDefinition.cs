using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingIngredientDefinition
// CATEGORY: Modules / Crafting / Runtime / Definitions
// PURPOSE: Serializable ingredient entry referencing inventory item definitions.
// PLACEMENT: Serialized on CCS_CraftingRecipeDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI or world references. Quantity validated by crafting validation utility.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [Serializable]
    public sealed class CCS_CraftingIngredientDefinition
    {
        #region Variables

        [Tooltip("Inventory item consumed by the recipe.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Quantity consumed per craft.")]
        [SerializeField] private int quantity = 1;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int Quantity => quantity;

        #endregion
    }
}
