using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestResult
// CATEGORY: Modules / Wildlife / Runtime / Data
// PURPOSE: Represents the outcome of a wildlife harvest attempt.
// PLACEMENT: Returned by CCS_WildlifeHarvestService harvest methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe failure results instead of exceptions.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeHarvestResult
    {
        #region Public Methods

        public static CCS_WildlifeHarvestResult Success(
            IReadOnlyList<CCS_WildlifeHarvestedItemDrop> drops,
            int itemsAddedToInventory = 0,
            string message = "Wildlife harvest completed.")
        {
            return new CCS_WildlifeHarvestResult(
                true,
                drops,
                itemsAddedToInventory,
                message ?? string.Empty);
        }

        public static CCS_WildlifeHarvestResult Failure(string message)
        {
            return new CCS_WildlifeHarvestResult(
                false,
                System.Array.Empty<CCS_WildlifeHarvestedItemDrop>(),
                0,
                message ?? string.Empty);
        }

        private CCS_WildlifeHarvestResult(
            bool isSuccess,
            IReadOnlyList<CCS_WildlifeHarvestedItemDrop> drops,
            int itemsAddedToInventory,
            string message)
        {
            IsSuccess = isSuccess;
            Drops = drops ?? System.Array.Empty<CCS_WildlifeHarvestedItemDrop>();
            ItemsAddedToInventory = itemsAddedToInventory < 0 ? 0 : itemsAddedToInventory;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public IReadOnlyList<CCS_WildlifeHarvestedItemDrop> Drops { get; }

        public int ItemsAddedToInventory { get; }

        public string Message { get; }

        #endregion
    }
}
