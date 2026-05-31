using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_HarvestResult
// CATEGORY: Modules / WorldResources / Runtime / Data
// PURPOSE: Represents the outcome of a harvest attempt.
// PLACEMENT: Returned by CCS_ResourceHarvestService harvest methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Safe failure results instead of exceptions. Not a drop definition type.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_HarvestResult
    {
        #region Public Methods

        public static CCS_HarvestResult Success(
            IReadOnlyList<CCS_HarvestedItemDrop> drops,
            int itemsAddedToInventory = 0,
            string message = "Harvest completed.")
        {
            return new CCS_HarvestResult(true, drops, itemsAddedToInventory, message ?? string.Empty);
        }

        public static CCS_HarvestResult Failure(string message)
        {
            return new CCS_HarvestResult(false, System.Array.Empty<CCS_HarvestedItemDrop>(), 0, message ?? string.Empty);
        }

        private CCS_HarvestResult(
            bool isSuccess,
            IReadOnlyList<CCS_HarvestedItemDrop> drops,
            int itemsAddedToInventory,
            string message)
        {
            IsSuccess = isSuccess;
            Drops = drops ?? System.Array.Empty<CCS_HarvestedItemDrop>();
            ItemsAddedToInventory = itemsAddedToInventory < 0 ? 0 : itemsAddedToInventory;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public IReadOnlyList<CCS_HarvestedItemDrop> Drops { get; }

        public int ItemsAddedToInventory { get; }

        public string Message { get; }

        #endregion
    }
}
