// =============================================================================
// SCRIPT: CCS_GameTimeSnapshot
// CATEGORY: Modules / TimeOfDay / Runtime / Data
// PURPOSE: Read-only game clock snapshot for HUD and debug display.
// PLACEMENT: Produced by CCS_TimeOfDayService.CreateSnapshot().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Immutable-friendly value type. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public readonly struct CCS_GameTimeSnapshot
    {
        #region Public Methods

        public CCS_GameTimeSnapshot(
            int dayNumber,
            int hour,
            int minute,
            float normalizedDayProgress,
            CCS_TimeOfDayPhase currentPhase,
            bool isPaused,
            float timeScale)
        {
            DayNumber = dayNumber < 1 ? 1 : dayNumber;
            Hour = ClampHour(hour);
            Minute = ClampMinute(minute);
            NormalizedDayProgress = normalizedDayProgress < 0f ? 0f : normalizedDayProgress > 1f ? 1f : normalizedDayProgress;
            CurrentPhase = currentPhase;
            IsPaused = isPaused;
            TimeScale = timeScale < 0f ? 0f : timeScale;
        }

        public static CCS_GameTimeSnapshot Empty =>
            new CCS_GameTimeSnapshot(1, 0, 0, 0f, CCS_TimeOfDayPhase.Dawn, false, 1f);

        #endregion

        #region Properties

        public int DayNumber { get; }

        public int Hour { get; }

        public int Minute { get; }

        public float NormalizedDayProgress { get; }

        public CCS_TimeOfDayPhase CurrentPhase { get; }

        public bool IsPaused { get; }

        public float TimeScale { get; }

        #endregion

        #region Private Methods

        private static int ClampHour(int hour)
        {
            if (hour < 0)
            {
                return 0;
            }

            return hour > 23 ? 23 : hour;
        }

        private static int ClampMinute(int minute)
        {
            if (minute < 0)
            {
                return 0;
            }

            return minute > 59 ? 59 : minute;
        }

        #endregion
    }
}
