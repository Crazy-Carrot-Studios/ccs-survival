// =============================================================================
// SCRIPT: CCS_SleepSnapshot
// CATEGORY: Modules / Sleep / Runtime / Data
// PURPOSE: Read-only sleep readiness and last-result snapshot for HUD and tests.
// PLACEMENT: Produced by CCS_SleepService.CreateSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Sleep Ready reflects bedroll, rest need, and required service availability.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepSnapshot
    {
        public static readonly CCS_SleepSnapshot Empty = new CCS_SleepSnapshot(
            false,
            false,
            false,
            false,
            0f,
            0f,
            CCS_SleepFailureReason.None,
            string.Empty);

        #region Constructors

        public CCS_SleepSnapshot(
            bool hasBedroll,
            bool isSheltered,
            bool canSleep,
            bool sleepReady,
            float lastHoursSlept,
            float lastFatigueRestored,
            CCS_SleepFailureReason lastFailureReason,
            string lastMessage)
        {
            HasBedroll = hasBedroll;
            IsSheltered = isSheltered;
            CanSleep = canSleep;
            SleepReady = sleepReady;
            LastHoursSlept = lastHoursSlept;
            LastFatigueRestored = lastFatigueRestored;
            LastFailureReason = lastFailureReason;
            LastMessage = lastMessage ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool HasBedroll { get; }

        public bool IsSheltered { get; }

        public bool CanSleep { get; }

        public bool SleepReady { get; }

        public float LastHoursSlept { get; }

        public float LastFatigueRestored { get; }

        public CCS_SleepFailureReason LastFailureReason { get; }

        public string LastMessage { get; }

        #endregion
    }
}
