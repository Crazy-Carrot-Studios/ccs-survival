using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRecipe
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Serializable primitive building recipe for progression placement.
// PLACEMENT: Stored on CCS_BuildingProgressionProfile recipe lists.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Links recipe metadata to a CCS_BuildingPieceDefinition by pieceDefinitionId.
// =============================================================================

namespace CCS.Modules.Building
{
    [Serializable]
    public sealed class CCS_BuildingRecipe
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS recipe id.")]
        [SerializeField] private string recipeId = string.Empty;

        [Tooltip("Player-facing recipe label.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Primitive building category granted by this recipe.")]
        [SerializeField] private CCS_BuildingPieceCategory pieceCategory = CCS_BuildingPieceCategory.Foundation;

        [Tooltip("Building piece definition id placed when the recipe succeeds.")]
        [SerializeField] private string pieceDefinitionId = string.Empty;

        [Header("Costs")]
        [Tooltip("Inventory items consumed when this recipe is placed.")]
        [SerializeField] private List<CCS_BuildingRecipeRequiredItem> requiredItems =
            new List<CCS_BuildingRecipeRequiredItem>();

        [Header("Placement")]
        [Tooltip("Placement restrictions enforced before resources are consumed.")]
        [SerializeField] private CCS_BuildingRecipePlacementRules placementRules = new CCS_BuildingRecipePlacementRules();

        #endregion

        #region Properties

        public string RecipeId => recipeId;

        public string DisplayName => displayName;

        public CCS_BuildingPieceCategory PieceCategory => pieceCategory;

        public string PieceDefinitionId => pieceDefinitionId;

        public IReadOnlyList<CCS_BuildingRecipeRequiredItem> RequiredItems => requiredItems;

        public CCS_BuildingRecipePlacementRules PlacementRules => placementRules;

        #endregion
    }
}
