// =============================================================================
// SCRIPT: CCS_SleepResult
// CATEGORY: Modules / Sleep / Runtime / Data
// PURPOSE: Outcome payload for sleep attempts.
// PLACEMENT: Returned by CCS_SleepService.TrySleep.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe failures only. No health damage in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepResult
    {
        #region Constructors

        private CCS_SleepResult(
            bool isSuccess,
            string message,
            CCS_SleepFailureReason failureReason,
            float hoursSlept,
            float fatigueRestored,
            bool usedPoorShelterPenalty)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            FailureReason = failureReason;
            HoursSlept = hoursSlept;
            FatigueRestored = fatigueRestored;
            UsedPoorShelterPenalty = usedPoorShelterPenalty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public string Message { get; }

        public CCS_SleepFailureReason FailureReason { get; }

        public float HoursSlept { get; }

        public float FatigueRestored { get; }

        public bool UsedPoorShelterPenalty { get; }

        #endregion

        #region Public Methods

        public static CCS_SleepResult Success(
            float hoursSlept,
            float fatigueRestored,
            bool usedPoorShelterPenalty,
            string message)
        {
            return new CCS_SleepResult(
                true,
                message,
                CCS_SleepFailureReason.None,
                hoursSlept,
                fatigueRestored,
                usedPoorShelterPenalty);
        }

        public static CCS_SleepResult Failure(CCS_SleepFailureReason failureReason, string message)
        {
            return new CCS_SleepResult(
                false,
                message,
                failureReason,
                0f,
                0f,
                false);
        }

        #endregion
    }
}
