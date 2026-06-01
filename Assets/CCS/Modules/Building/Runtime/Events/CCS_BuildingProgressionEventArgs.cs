// =============================================================================
// SCRIPT: CCS_BuildingProgressionEventArgs
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event payload for building progression recipe and placement notifications.
// PLACEMENT: Passed to CCS_BuildingRecipeService event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.0 building progression foundation.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingProgressionEventArgs
    {
        #region Public Methods

        public CCS_BuildingProgressionEventArgs(
            string recipeId,
            string pieceDefinitionId,
            CCS_BuildingPieceCategory pieceCategory,
            string message)
        {
            RecipeId = recipeId ?? string.Empty;
            PieceDefinitionId = pieceDefinitionId ?? string.Empty;
            PieceCategory = pieceCategory;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string RecipeId { get; }

        public string PieceDefinitionId { get; }

        public CCS_BuildingPieceCategory PieceCategory { get; }

        public string Message { get; }

        #endregion
    }
}
