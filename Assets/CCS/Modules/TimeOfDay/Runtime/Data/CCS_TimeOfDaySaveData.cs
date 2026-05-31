using System;

// =============================================================================
// SCRIPT: CCS_TimeOfDaySaveData
// CATEGORY: Modules / TimeOfDay / Runtime / Data
// PURPOSE: Serializable save payload for the global game clock.
// PLACEMENT: Serialized by CCS_TimeOfDayService CaptureState / RestoreState.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: saveDataVersion reserved for future migration. No migration system in 0.7.0.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    [Serializable]
    public sealed class CCS_TimeOfDaySaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public int dayNumber = 1;

        public float totalMinutesIntoDay;

        public float timeScale = 1f;

        public bool isPaused;

        #endregion
    }
}
