// =============================================================================
// SCRIPT: CCS_ActiveItemUseResult
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Outcome of an active item use attempt with a player-facing message.
// PLACEMENT: Returned by CCS_ActiveItemService.TryUseActiveItem.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: IsSuccess is true for combat hits and benign no-target outcomes.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public readonly struct CCS_ActiveItemUseResult
    {
        public CCS_ActiveItemUseResult(
            CCS_ActiveItemUseResultType resultType,
            string message,
            bool isSuccess,
            string activeItemId = "",
            string targetDisplayName = "",
            string targetTypeLabel = "")
        {
            ResultType = resultType;
            Message = message ?? string.Empty;
            IsSuccess = isSuccess;
            ActiveItemId = activeItemId ?? string.Empty;
            TargetDisplayName = targetDisplayName ?? string.Empty;
            TargetTypeLabel = targetTypeLabel ?? string.Empty;
        }

        public CCS_ActiveItemUseResultType ResultType { get; }

        public string Message { get; }

        public bool IsSuccess { get; }

        public string ActiveItemId { get; }

        public string TargetDisplayName { get; }

        public string TargetTypeLabel { get; }

        public static CCS_ActiveItemUseResult NoActiveItem()
        {
            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.NoActiveItem,
                "No active item selected.",
                true);
        }

        public static CCS_ActiveItemUseResult NoBehavior(string itemId)
        {
            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                "No active use behavior registered for this item.",
                true,
                itemId);
        }
    }
}
