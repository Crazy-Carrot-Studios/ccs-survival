// =============================================================================
// SCRIPT: CCS_FirearmUseResult
// CATEGORY: Modules / Firearms / Runtime / Data
// PURPOSE: Outcome of firearm fire or reload attempts.
// PLACEMENT: Returned by CCS_FirearmService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    public enum CCS_FirearmUseResultType
    {
        None = 0,
        Fired = 1,
        Reloaded = 2,
        Empty = 3,
        NoAmmo = 4,
        NotEquipped = 5,
        ServiceUnavailable = 6,
        CombatMiss = 7,
        CombatHit = 8
    }

    public sealed class CCS_FirearmUseResult
    {
        public CCS_FirearmUseResult(
            CCS_FirearmUseResultType resultType,
            string message,
            bool isSuccess,
            string firearmItemId = "")
        {
            ResultType = resultType;
            Message = message ?? string.Empty;
            IsSuccess = isSuccess;
            FirearmItemId = firearmItemId ?? string.Empty;
        }

        public CCS_FirearmUseResultType ResultType { get; }

        public string Message { get; }

        public bool IsSuccess { get; }

        public string FirearmItemId { get; }

        public static CCS_FirearmUseResult ServiceUnavailable() =>
            new CCS_FirearmUseResult(
                CCS_FirearmUseResultType.ServiceUnavailable,
                "Firearm service is unavailable.",
                false);
    }
}
