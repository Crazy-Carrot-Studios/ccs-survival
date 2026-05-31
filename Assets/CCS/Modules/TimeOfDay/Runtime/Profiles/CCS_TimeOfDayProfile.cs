using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TimeOfDayProfile
// CATEGORY: Modules / TimeOfDay / Runtime / Profiles
// PURPOSE: Tuning profile for global game clock start time, scale, and phase boundaries.
// PLACEMENT: Assets/CCS/Survival/Profiles/TimeOfDay/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No lighting, weather, sleep, or AI schedule references in 0.7.0.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    [CreateAssetMenu(
        fileName = "CCS_TimeOfDayProfile",
        menuName = "CCS/Survival/Time Of Day/Time Of Day Profile")]
    public sealed class CCS_TimeOfDayProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Start Time")]
        [Tooltip("Starting day number when the clock initializes.")]
        [SerializeField] private int startDay = 1;

        [Tooltip("Starting hour when the clock initializes (0-23).")]
        [SerializeField] private int startHour = 7;

        [Tooltip("Starting minute when the clock initializes (0-59).")]
        [SerializeField] private int startMinute;

        [Header("Clock Speed")]
        [Tooltip("Real-world seconds required for one full in-game day.")]
        [SerializeField] private float realSecondsPerGameDay = 1800f;

        [Tooltip("When enabled, the clock starts paused until ResumeTime is called.")]
        [SerializeField] private bool pauseTimeOnStart;

        [Header("Phase Boundaries (Hour)")]
        [Tooltip("Hour when Dawn phase begins.")]
        [SerializeField] private int dawnStartHour = 5;

        [Tooltip("Hour when Day phase begins.")]
        [SerializeField] private int dayStartHour = 7;

        [Tooltip("Hour when Dusk phase begins.")]
        [SerializeField] private int duskStartHour = 18;

        [Tooltip("Hour when Night phase begins.")]
        [SerializeField] private int nightStartHour = 20;

        #endregion

        #region Properties

        public int StartDay => startDay;

        public int StartHour => startHour;

        public int StartMinute => startMinute;

        public float RealSecondsPerGameDay => realSecondsPerGameDay;

        public bool PauseTimeOnStart => pauseTimeOnStart;

        public int DawnStartHour => dawnStartHour;

        public int DayStartHour => dayStartHour;

        public int DuskStartHour => duskStartHour;

        public int NightStartHour => nightStartHour;

        #endregion
    }
}
