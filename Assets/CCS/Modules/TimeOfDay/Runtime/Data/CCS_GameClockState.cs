// =============================================================================
// SCRIPT: CCS_GameClockState
// CATEGORY: Modules / TimeOfDay / Runtime / Data
// PURPOSE: Mutable runtime clock state owned by CCS_TimeOfDayService.
// PLACEMENT: Internal service state. Not exposed to gameplay modules directly.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: MinutesIntoDay wraps at 1440. DayNumber increments on wrap.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public sealed class CCS_GameClockState
    {
        #region Variables

        public const float MinutesPerDay = 1440f;

        #endregion

        #region Properties

        public int DayNumber { get; set; } = 1;

        public float MinutesIntoDay { get; set; }

        public float TimeScale { get; set; } = 1f;

        public bool IsPaused { get; set; }

        public CCS_TimeOfDayPhase CurrentPhase { get; set; } = CCS_TimeOfDayPhase.Day;

        #endregion
    }
}
