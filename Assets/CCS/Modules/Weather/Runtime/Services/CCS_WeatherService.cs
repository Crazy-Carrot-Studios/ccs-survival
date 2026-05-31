using CCS.Core;
using CCS.Modules.SaveLoad;
using CCS.Modules.TimeOfDay;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeatherService
// CATEGORY: Modules / Weather / Runtime / Services
// PURPOSE: Authoritative global weather state with transitions, events, and save/load.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: One-way Time Of Day dependency only. No VFX, lighting, or audio in 0.7.1.
// =============================================================================

namespace CCS.Modules.Weather
{
    public sealed class CCS_WeatherService : CCS_ISurvivalService, CCS_ISaveable, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_WeatherService]";

        #region Variables

        private readonly CCS_WeatherState weatherState = new CCS_WeatherState();

        private CCS_WeatherProfile activeProfile;
        private CCS_TimeOfDayService timeOfDayService;
        private bool isInitialized;

        #endregion

        #region Events

        public event WeatherChangedHandler WeatherChanged;
        public event WeatherTransitionStartedHandler WeatherTransitionStarted;
        public event WeatherTransitionCompletedHandler WeatherTransitionCompleted;
        public event WeatherPausedHandler WeatherPaused;
        public event WeatherResumedHandler WeatherResumed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_WeatherProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalWeather;

        public bool IsPaused => weatherState.IsPaused;

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

        public void InitializeFromProfile(CCS_WeatherProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WeatherValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            weatherState.CurrentWeather = profile.StartingWeather;
            weatherState.PreviousWeather = profile.StartingWeather;
            weatherState.TransitionTargetWeather = profile.StartingWeather;
            weatherState.TransitionProgress = 0f;
            weatherState.IsTransitioning = false;
            weatherState.IsPaused = false;
            weatherState.RemainingDurationSeconds = RollNextWeatherDuration();

            isInitialized = true;
            RaiseWeatherChanged("Weather service initialized.");
        }

        public void BindTimeOfDayService(CCS_TimeOfDayService service)
        {
            timeOfDayService = service;
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || weatherState.IsPaused || deltaTime <= 0f || activeProfile == null)
            {
                return;
            }

            if (weatherState.IsTransitioning)
            {
                AdvanceTransition(deltaTime);
                return;
            }

            if (!activeProfile.WeatherChangeEnabled)
            {
                return;
            }

            weatherState.RemainingDurationSeconds -= deltaTime;
            if (weatherState.RemainingDurationSeconds > 0f)
            {
                return;
            }

            CCS_WeatherType nextWeather = SelectNextWeather(weatherState.CurrentWeather);
            BeginTransition(nextWeather, CCS_WeatherTransitionMode.Timed, "Automatic weather transition started.");
        }

        public void SetWeather(CCS_WeatherType weatherType, CCS_WeatherTransitionMode transitionMode)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            if (!CCS_WeatherValidationUtility.IsDefinedWeatherType(weatherType))
            {
                Debug.LogWarning($"{LogPrefix} SetWeather ignored unsupported weather type.");
                return;
            }

            if (transitionMode == CCS_WeatherTransitionMode.Instant)
            {
                ApplyInstantWeather(weatherType, "Weather set instantly.");
                return;
            }

            BeginTransition(weatherType, transitionMode, "Manual weather transition started.");
        }

        public void PauseWeather()
        {
            if (!EnsureInitialized() || weatherState.IsPaused)
            {
                return;
            }

            weatherState.IsPaused = true;
            RaiseWeatherPaused();
            RaiseWeatherChanged("Weather paused.");
        }

        public void ResumeWeather()
        {
            if (!EnsureInitialized() || !weatherState.IsPaused)
            {
                return;
            }

            weatherState.IsPaused = false;
            RaiseWeatherResumed();
            RaiseWeatherChanged("Weather resumed.");
        }

        public CCS_WeatherSnapshot GetSnapshot()
        {
            if (!EnsureInitialized())
            {
                return CCS_WeatherSnapshot.Empty;
            }

            return BuildSnapshot();
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_WeatherSaveData());
            }

            CCS_WeatherSaveData saveData = new CCS_WeatherSaveData
            {
                saveDataVersion = CCS_WeatherSaveData.CurrentSaveDataVersion,
                currentWeather = weatherState.CurrentWeather,
                previousWeather = weatherState.PreviousWeather,
                targetWeather = weatherState.TransitionTargetWeather,
                transitionProgress = weatherState.TransitionProgress,
                remainingDuration = weatherState.RemainingDurationSeconds,
                isPaused = weatherState.IsPaused
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

            CCS_WeatherSaveData saveData = JsonUtility.FromJson<CCS_WeatherSaveData>(stateJson);
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

            weatherState.CurrentWeather = saveData.currentWeather;
            weatherState.PreviousWeather = saveData.previousWeather;
            weatherState.TransitionTargetWeather = saveData.targetWeather;
            weatherState.TransitionProgress = saveData.transitionProgress < 0f ? 0f : saveData.transitionProgress;
            weatherState.RemainingDurationSeconds = saveData.remainingDuration < 0f ? 0f : saveData.remainingDuration;
            weatherState.IsPaused = saveData.isPaused;
            weatherState.IsTransitioning = weatherState.TransitionProgress > 0f
                && weatherState.TransitionProgress < 1f
                && weatherState.CurrentWeather != weatherState.TransitionTargetWeather;

            if (weatherState.IsTransitioning && weatherState.TransitionProgress >= 1f)
            {
                CompleteTransition("Weather transition completed on restore.");
            }

            Debug.Log($"{LogPrefix} RestoreState applied weather {weatherState.CurrentWeather}.");
            RaiseWeatherChanged("Weather restored from save.");

            if (weatherState.IsPaused)
            {
                RaiseWeatherPaused();
            }
            else
            {
                RaiseWeatherResumed();
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

        private CCS_WeatherSnapshot BuildSnapshot()
        {
            float temperatureModifier = activeProfile.GetTemperatureModifier(weatherState.CurrentWeather);
            float wetnessModifier = activeProfile.GetWetnessModifier(weatherState.CurrentWeather);

            return new CCS_WeatherSnapshot(
                weatherState.CurrentWeather,
                weatherState.PreviousWeather,
                weatherState.TransitionTargetWeather,
                weatherState.TransitionProgress,
                weatherState.RemainingDurationSeconds,
                temperatureModifier,
                wetnessModifier,
                weatherState.IsTransitioning,
                weatherState.IsPaused);
        }

        private void AdvanceTransition(float deltaTime)
        {
            float transitionDuration = activeProfile.TransitionDurationSeconds;
            if (transitionDuration <= 0f)
            {
                CompleteTransition("Timed transition completed instantly.");
                return;
            }

            weatherState.TransitionProgress += deltaTime / transitionDuration;
            RaiseWeatherChanged("Weather transition advanced.");

            if (weatherState.TransitionProgress >= 1f)
            {
                CompleteTransition("Weather transition completed.");
            }
        }

        private void BeginTransition(
            CCS_WeatherType targetWeather,
            CCS_WeatherTransitionMode transitionMode,
            string message)
        {
            if (targetWeather == weatherState.CurrentWeather && !weatherState.IsTransitioning)
            {
                weatherState.RemainingDurationSeconds = RollNextWeatherDuration();
                RaiseWeatherChanged("Weather duration refreshed.");
                return;
            }

            weatherState.PreviousWeather = weatherState.CurrentWeather;
            weatherState.TransitionTargetWeather = targetWeather;
            weatherState.TransitionProgress = 0f;
            weatherState.IsTransitioning = true;

            if (transitionMode == CCS_WeatherTransitionMode.Instant
                || activeProfile.TransitionDurationSeconds <= 0f)
            {
                CompleteTransition("Instant weather transition completed.");
                return;
            }

            RaiseWeatherTransitionStarted(message);
            RaiseWeatherChanged(message);
        }

        private void CompleteTransition(string message)
        {
            weatherState.CurrentWeather = weatherState.TransitionTargetWeather;
            weatherState.PreviousWeather = weatherState.TransitionTargetWeather;
            weatherState.TransitionProgress = 1f;
            weatherState.IsTransitioning = false;
            weatherState.RemainingDurationSeconds = RollNextWeatherDuration();

            RaiseWeatherTransitionCompleted(message);
            RaiseWeatherChanged(message);
        }

        private void ApplyInstantWeather(CCS_WeatherType weatherType, string message)
        {
            weatherState.PreviousWeather = weatherState.CurrentWeather;
            weatherState.CurrentWeather = weatherType;
            weatherState.TransitionTargetWeather = weatherType;
            weatherState.TransitionProgress = 0f;
            weatherState.IsTransitioning = false;
            weatherState.RemainingDurationSeconds = RollNextWeatherDuration();
            RaiseWeatherChanged(message);
        }

        private float RollNextWeatherDuration()
        {
            if (activeProfile == null)
            {
                return 0f;
            }

            float minimum = activeProfile.MinimumWeatherDurationSeconds;
            float maximum = activeProfile.MaximumWeatherDurationSeconds;
            if (maximum <= minimum)
            {
                return minimum;
            }

            return Random.Range(minimum, maximum);
        }

        private CCS_WeatherType SelectNextWeather(CCS_WeatherType currentWeather)
        {
            ReadTimeOfDaySnapshotForFutureWeighting();

            int weatherCount = System.Enum.GetValues(typeof(CCS_WeatherType)).Length;
            if (weatherCount <= 1)
            {
                return currentWeather;
            }

            CCS_WeatherType nextWeather = currentWeather;
            int attempts = 0;
            while (nextWeather == currentWeather && attempts < 8)
            {
                int randomIndex = Random.Range(0, weatherCount);
                nextWeather = (CCS_WeatherType)randomIndex;
                attempts++;
            }

            return nextWeather;
        }

        private void ReadTimeOfDaySnapshotForFutureWeighting()
        {
            if (timeOfDayService == null || !timeOfDayService.IsInitialized)
            {
                return;
            }

            // Foundation hook only. Future milestones may weight fog at dawn or storms at night.
            _ = timeOfDayService.CreateSnapshot();
        }

        private void RaiseWeatherChanged(string message)
        {
            WeatherChanged?.Invoke(new CCS_WeatherEventArgs(GetSnapshot(), message));
        }

        private void RaiseWeatherTransitionStarted(string message)
        {
            WeatherTransitionStarted?.Invoke(new CCS_WeatherEventArgs(GetSnapshot(), message));
        }

        private void RaiseWeatherTransitionCompleted(string message)
        {
            WeatherTransitionCompleted?.Invoke(new CCS_WeatherEventArgs(GetSnapshot(), message));
        }

        private void RaiseWeatherPaused()
        {
            WeatherPaused?.Invoke(new CCS_WeatherEventArgs(GetSnapshot(), "Weather paused."));
        }

        private void RaiseWeatherResumed()
        {
            WeatherResumed?.Invoke(new CCS_WeatherEventArgs(GetSnapshot(), "Weather resumed."));
        }

        #endregion
    }
}
