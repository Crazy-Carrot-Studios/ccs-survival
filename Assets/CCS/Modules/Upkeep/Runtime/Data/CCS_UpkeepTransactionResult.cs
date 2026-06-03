// =============================================================================
// SCRIPT: CCS_UpkeepTransactionResult
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Result payload for upkeep registration, due forcing, and payment operations.
// PLACEMENT: Returned by CCS_UpkeepService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public enum CCS_UpkeepPaymentSource
    {
        None = 0,
        Bank = 1,
        Wallet = 2
    }

    public enum CCS_UpkeepTransactionResultType
    {
        Success = 0,
        InvalidTarget = 1,
        InvalidDefinition = 2,
        NotDue = 3,
        InsufficientFunds = 4,
        Disabled = 5,
        ServiceNotReady = 6,
        UnknownFailure = 7
    }

    public sealed class CCS_UpkeepTransactionResult
    {
        public CCS_UpkeepTransactionResult(
            CCS_UpkeepTransactionResultType resultType,
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            int amount,
            CCS_UpkeepPaymentSource paymentSource,
            CCS_UpkeepState entryState,
            string message)
        {
            ResultType = resultType;
            EntryId = entryId ?? string.Empty;
            TargetId = targetId ?? string.Empty;
            UpkeepDefinitionId = upkeepDefinitionId ?? string.Empty;
            Amount = amount;
            PaymentSource = paymentSource;
            EntryState = entryState;
            Message = message ?? string.Empty;
        }

        public CCS_UpkeepTransactionResultType ResultType { get; }

        public string EntryId { get; }

        public string TargetId { get; }

        public string UpkeepDefinitionId { get; }

        public int Amount { get; }

        public CCS_UpkeepPaymentSource PaymentSource { get; }

        public CCS_UpkeepState EntryState { get; }

        public string Message { get; }

        public bool IsSuccess => ResultType == CCS_UpkeepTransactionResultType.Success;

        public static CCS_UpkeepTransactionResult Success(
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            int amount,
            CCS_UpkeepPaymentSource paymentSource,
            CCS_UpkeepState entryState,
            string message)
        {
            return new CCS_UpkeepTransactionResult(
                CCS_UpkeepTransactionResultType.Success,
                entryId,
                targetId,
                upkeepDefinitionId,
                amount,
                paymentSource,
                entryState,
                message);
        }

        public static CCS_UpkeepTransactionResult Failure(
            CCS_UpkeepTransactionResultType resultType,
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            CCS_UpkeepState entryState,
            string message)
        {
            return new CCS_UpkeepTransactionResult(
                resultType,
                entryId,
                targetId,
                upkeepDefinitionId,
                0,
                CCS_UpkeepPaymentSource.None,
                entryState,
                message);
        }
    }
}
