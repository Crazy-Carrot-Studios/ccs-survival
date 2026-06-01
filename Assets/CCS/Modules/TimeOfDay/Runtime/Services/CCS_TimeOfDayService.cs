using CCS.Core;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TimeOfDayService
// CATEGORY: Modules / TimeOfDay / Runtime / Services
// PURPOSE: Global game clock with phase tracking, events, and save/load integration.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No lighting or weather dependencies in 0.7.0. Ticks via CCS_IUpdatable.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public sealed class CCS_TimeOfDayService : CCS_ISurvivalService, CCS_ISaveable, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_TimeOfDayService]";

        #region Variables

        private readonly CCS_GameClockState clockState = new CCS_GameClockState();

        private CCS_TimeOfDayProfile activeProfile;
        private float gameMinutesPerRealSecond;
        private int previousHour = -1;
        private int previousDay = -1;
        private CCS_TimeOfDayPhase previousPhase = CCS_TimeOfDayPhase.Dawn;
        private bool isInitialized;

        #endregion

        #region Events

        public event TimeOfDayChangedHandler TimeChanged;
        public event TimeOfDayHourChangedHandler HourChanged;
        public event TimeOfDayDayChangedHandler DayChanged;
        public event TimeOfDayPhaseChangedHandler PhaseChanged;
        public event TimeOfDayPausedHandler TimePaused;
        public event TimeOfDayResumedHandler TimeResumed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_TimeOfDayProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalTimeOfDay;

        public float TimeScale => clockState.TimeScale;

        public bool IsPaused => clockState.IsPaused;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_TimeOfDayProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_TimeOfDayValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            gameMinutesPerRealSecond = CCS_GameClockState.MinutesPerDay / profile.RealSecondsPerGameDay;
            clockState.DayNumber = profile.StartDay < 1 ? 1 : profile.StartDay;
            clockState.MinutesIntoDay = CCS_TimeOfDayValidationUtility.ConvertHourMinuteToMinutesIntoDay(
                profile.StartHour,
                profile.StartMinute);
            clockState.TimeScale = 1f;
            clockState.IsPaused = profile.PauseTimeOnStart;
            clockState.CurrentPhase = CCS_TimeOfDayValidationUtility.ResolvePhase(
                GetHourFromMinutes(clockState.MinutesIntoDay),
                profile);

            CachePreviousTrackingValues();
            isInitialized = true;
            RaiseTimeChanged("Time of day service initialized.");
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || clockState.IsPaused || deltaTime <= 0f)
            {
                return;
            }

            if (activeProfile == null || gameMinutesPerRealSecond <= 0f)
            {
                return;
            }

            int previousTrackedDay = clockState.DayNumber;
            int previousTrackedHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            CCS_TimeOfDayPhase previousTrackedPhase = clockState.CurrentPhase;

            float scaledDeltaMinutes = deltaTime * clockState.TimeScale * gameMinutesPerRealSecond;
            clockState.MinutesIntoDay += scaledDeltaMinutes;

            while (clockState.MinutesIntoDay >= CCS_GameClockState.MinutesPerDay)
            {
                clockState.MinutesIntoDay -= CCS_GameClockState.MinutesPerDay;
                clockState.DayNumber++;
            }

            int currentHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            clockState.CurrentPhase = CCS_TimeOfDayValidationUtility.ResolvePhase(currentHour, activeProfile);

            RaiseTimeChanged("Time advanced.");

            if (currentHour != previousTrackedHour)
            {
                RaiseHourChanged();
            }

            if (clockState.DayNumber != previousTrackedDay)
            {
                RaiseDayChanged();
            }

            if (clockState.CurrentPhase != previousTrackedPhase)
            {
                RaisePhaseChanged();
            }
        }

        public void PauseTime()
        {
            if (!EnsureInitialized() || clockState.IsPaused)
            {
                return;
            }

            clockState.IsPaused = true;
            RaiseTimePaused();
            RaiseTimeChanged("Time paused.");
        }

        public void ResumeTime()
        {
            if (!EnsureInitialized() || !clockState.IsPaused)
            {
                return;
            }

            clockState.IsPaused = false;
            RaiseTimeResumed();
            RaiseTimeChanged("Time resumed.");
        }

        public void SetTime(int dayNumber, int hour, int minute)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            clockState.DayNumber = dayNumber < 1 ? 1 : dayNumber;
            clockState.MinutesIntoDay = CCS_TimeOfDayValidationUtility.ConvertHourMinuteToMinutesIntoDay(hour, minute);
            clockState.CurrentPhase = CCS_TimeOfDayValidationUtility.ResolvePhase(
                GetHourFromMinutes(clockState.MinutesIntoDay),
                activeProfile);

            RaiseTimeChanged("Time set manually.");
            RaiseHourChanged();
            RaiseDayChanged();
            RaisePhaseChanged();
        }

        public void SetTimeScale(float timeScale)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            clockState.TimeScale = timeScale < 0f ? 0f : timeScale;
            RaiseTimeChanged("Time scale updated.");
        }

        public void AdvanceTimeByHours(float hours)
        {
            if (!EnsureInitialized() || hours <= 0f)
            {
                return;
            }

            float minutesToAdvance = hours * 60f;
            int previousTrackedDay = clockState.DayNumber;
            int previousTrackedHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            CCS_TimeOfDayPhase previousTrackedPhase = clockState.CurrentPhase;

            clockState.MinutesIntoDay += minutesToAdvance;

            while (clockState.MinutesIntoDay >= CCS_GameClockState.MinutesPerDay)
            {
                clockState.MinutesIntoDay -= CCS_GameClockState.MinutesPerDay;
                clockState.DayNumber++;
            }

            int currentHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            clockState.CurrentPhase = CCS_TimeOfDayValidationUtility.ResolvePhase(currentHour, activeProfile);

            RaiseTimeChanged($"Time advanced by {hours:0.#} hour(s).");

            if (currentHour != previousTrackedHour)
            {
                RaiseHourChanged();
            }

            if (clockState.DayNumber != previousTrackedDay)
            {
                RaiseDayChanged();
            }

            if (clockState.CurrentPhase != previousTrackedPhase)
            {
                RaisePhaseChanged();
            }
        }

        public CCS_GameTimeSnapshot CreateSnapshot()
        {
            if (!EnsureInitialized())
            {
                return CCS_GameTimeSnapshot.Empty;
            }

            int hour = GetHourFromMinutes(clockState.MinutesIntoDay);
            int minute = GetMinuteFromMinutes(clockState.MinutesIntoDay);
            float normalizedProgress = clockState.MinutesIntoDay / CCS_GameClockState.MinutesPerDay;

            return new CCS_GameTimeSnapshot(
                clockState.DayNumber,
                hour,
                minute,
                normalizedProgress,
                clockState.CurrentPhase,
                clockState.IsPaused,
                clockState.TimeScale);
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_TimeOfDaySaveData());
            }

            CCS_TimeOfDaySaveData saveData = new CCS_TimeOfDaySaveData
            {
                saveDataVersion = CCS_TimeOfDaySaveData.CurrentSaveDataVersion,
                dayNumber = clockState.DayNumber,
                totalMinutesIntoDay = clockState.MinutesIntoDay,
                timeScale = clockState.TimeScale,
                isPaused = clockState.IsPaused
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreState(string stateJson)
        {
            if (!EnsureInitialized())
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because service is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                return;
            }

            CCS_TimeOfDaySaveData saveData = JsonUtility.FromJson<CCS_TimeOfDaySaveData>(stateJson);
            if (saveData == null)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because payload could not be parsed.");
                return;
            }

            if (saveData.saveDataVersion <= 0)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because saveDataVersion is missing.");
                return;
            }

            clockState.DayNumber = saveData.dayNumber < 1 ? 1 : saveData.dayNumber;
            clockState.MinutesIntoDay = saveData.totalMinutesIntoDay;
            if (clockState.MinutesIntoDay < 0f)
            {
                clockState.MinutesIntoDay = 0f;
            }

            while (clockState.MinutesIntoDay >= CCS_GameClockState.MinutesPerDay)
            {
                clockState.MinutesIntoDay -= CCS_GameClockState.MinutesPerDay;
                clockState.DayNumber++;
            }

            clockState.TimeScale = saveData.timeScale < 0f ? 0f : saveData.timeScale;
            clockState.IsPaused = saveData.isPaused;
            clockState.CurrentPhase = CCS_TimeOfDayValidationUtility.ResolvePhase(
                GetHourFromMinutes(clockState.MinutesIntoDay),
                activeProfile);

            CachePreviousTrackingValues();
            Debug.Log($"{LogPrefix} RestoreState applied day {clockState.DayNumber} at {CreateSnapshot().Hour:D2}:{CreateSnapshot().Minute:D2}.");
            RaiseTimeChanged("Time restored from save.");
            RaiseHourChanged();
            RaiseDayChanged();
            RaisePhaseChanged();

            if (clockState.IsPaused)
            {
                RaiseTimePaused();
            }
            else
            {
                RaiseTimeResumed();
            }
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            if (isInitialized && activeProfile != null)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
        }

        private void CachePreviousTrackingValues()
        {
            previousDay = clockState.DayNumber;
            previousHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            previousPhase = clockState.CurrentPhase;
        }

        private static int GetHourFromMinutes(float minutesIntoDay)
        {
            int totalMinutes = Mathf.FloorToInt(minutesIntoDay);
            if (totalMinutes < 0)
            {
                totalMinutes = 0;
            }

            return (totalMinutes / 60) % 24;
        }

        private static int GetMinuteFromMinutes(float minutesIntoDay)
        {
            int totalMinutes = Mathf.FloorToInt(minutesIntoDay);
            if (totalMinutes < 0)
            {
                totalMinutes = 0;
            }

            return totalMinutes % 60;
        }

        private void RaiseTimeChanged(string message)
        {
            TimeChanged?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), message));
        }

        private void RaiseHourChanged()
        {
            int currentHour = GetHourFromMinutes(clockState.MinutesIntoDay);
            if (currentHour == previousHour)
            {
                return;
            }

            previousHour = currentHour;
            HourChanged?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), "Hour changed."));
        }

        private void RaiseDayChanged()
        {
            if (clockState.DayNumber == previousDay)
            {
                return;
            }

            previousDay = clockState.DayNumber;
            DayChanged?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), "Day changed."));
        }

        private void RaisePhaseChanged()
        {
            if (clockState.CurrentPhase == previousPhase)
            {
                return;
            }

            previousPhase = clockState.CurrentPhase;
            PhaseChanged?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), "Phase changed."));
        }

        private void RaiseTimePaused()
        {
            TimePaused?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), "Time paused."));
        }

        private void RaiseTimeResumed()
        {
            TimeResumed?.Invoke(new CCS_TimeOfDayEventArgs(CreateSnapshot(), "Time resumed."));
        }

        #endregion
    }
}
