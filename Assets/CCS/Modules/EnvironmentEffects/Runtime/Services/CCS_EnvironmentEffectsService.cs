using CCS.Core;
using CCS.Modules.SaveLoad;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsService
// CATEGORY: Modules / EnvironmentEffects / Runtime / Services
// PURPOSE: Authoritative ambient temperature, wetness, and exposure simulation layer.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Reads Time Of Day and Weather only. No Survival Core stat mutation in 0.7.2.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public sealed class CCS_EnvironmentEffectsService : CCS_ISurvivalService, CCS_ISaveable, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_EnvironmentEffectsService]";

        #region Variables

        private readonly CCS_EnvironmentState environmentState = new CCS_EnvironmentState();

        private CCS_EnvironmentEffectsProfile activeProfile;
        private CCS_TimeOfDayService timeOfDayService;
        private CCS_WeatherService weatherService;
        private CCS_WeatherType lastWeatherType = CCS_WeatherType.Clear;
        private CCS_TimeOfDayPhase lastTimePhase = CCS_TimeOfDayPhase.Dawn;
        private bool isInitialized;

        #endregion

        #region Events

        public event EnvironmentChangedHandler EnvironmentChanged;
        public event EnvironmentTemperatureChangedHandler TemperatureChanged;
        public event EnvironmentWetnessChangedHandler WetnessChanged;
        public event EnvironmentExposureChangedHandler ExposureChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_EnvironmentEffectsProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalEnvironment;

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

        public void InitializeFromProfile(CCS_EnvironmentEffectsProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_EnvironmentEffectsValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
            RecalculateEnvironment("Environment effects service initialized.", true);
        }

        public void BindTimeOfDayService(CCS_TimeOfDayService service)
        {
            UnbindTimeOfDayService();
            timeOfDayService = service;

            if (timeOfDayService == null || !timeOfDayService.IsInitialized)
            {
                return;
            }

            timeOfDayService.TimeChanged += HandleTimeOfDayChanged;
            timeOfDayService.PhaseChanged += HandleTimeOfDayChanged;
        }

        public void BindWeatherService(CCS_WeatherService service)
        {
            UnbindWeatherService();
            weatherService = service;

            if (weatherService == null || !weatherService.IsInitialized)
            {
                return;
            }

            weatherService.WeatherChanged += HandleWeatherChanged;
            weatherService.WeatherTransitionStarted += HandleWeatherChanged;
            weatherService.WeatherTransitionCompleted += HandleWeatherChanged;
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || deltaTime <= 0f)
            {
                return;
            }

            RecalculateEnvironment("Environment tick refresh.", false);
        }

        public CCS_EnvironmentSnapshot GetSnapshot()
        {
            if (!EnsureInitialized())
            {
                return CCS_EnvironmentSnapshot.Empty;
            }

            return BuildSnapshot();
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_EnvironmentSaveData());
            }

            CCS_EnvironmentSaveData saveData = new CCS_EnvironmentSaveData
            {
                saveDataVersion = CCS_EnvironmentSaveData.CurrentSaveDataVersion,
                ambientTemperature = environmentState.AmbientTemperature,
                wetness = environmentState.Wetness,
                exposure = environmentState.Exposure
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

            CCS_EnvironmentSaveData saveData = JsonUtility.FromJson<CCS_EnvironmentSaveData>(stateJson);
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

            environmentState.AmbientTemperature = saveData.ambientTemperature;
            environmentState.Wetness = saveData.wetness < 0f ? 0f : saveData.wetness;
            environmentState.Exposure = saveData.exposure < 0f ? 0f : saveData.exposure;

            Debug.Log(
                $"{LogPrefix} RestoreState applied temp {environmentState.AmbientTemperature:0.#}, " +
                $"wetness {environmentState.Wetness:0.##}, exposure {environmentState.Exposure:0.##}.");
            RaiseEnvironmentChanged("Environment restored from save.", true, true, true);
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

        private void HandleTimeOfDayChanged(CCS_TimeOfDayEventArgs eventArgs)
        {
            RecalculateEnvironment("Time of day changed.", false);
        }

        private void HandleWeatherChanged(CCS_WeatherEventArgs eventArgs)
        {
            RecalculateEnvironment("Weather changed.", false);
        }

        private void RecalculateEnvironment(string message, bool forceRaiseAll)
        {
            if (activeProfile == null)
            {
                return;
            }

            CCS_GameTimeSnapshot timeSnapshot = timeOfDayService != null && timeOfDayService.IsInitialized
                ? timeOfDayService.CreateSnapshot()
                : CCS_GameTimeSnapshot.Empty;

            CCS_WeatherSnapshot weatherSnapshot = weatherService != null && weatherService.IsInitialized
                ? weatherService.GetSnapshot()
                : CCS_WeatherSnapshot.Empty;

            CCS_WeatherType weatherType = ResolveEffectiveWeatherType(weatherSnapshot);
            CCS_TimeOfDayPhase timePhase = timeSnapshot.CurrentPhase;

            float previousTemperature = environmentState.AmbientTemperature;
            float previousWetness = environmentState.Wetness;
            float previousExposure = environmentState.Exposure;

            float phaseTemperatureModifier = ResolvePhaseTemperatureModifier(timePhase);
            environmentState.AmbientTemperature =
                phaseTemperatureModifier + activeProfile.GetTemperatureModifier(weatherType);
            environmentState.Wetness = activeProfile.GetWetnessModifier(weatherType);
            environmentState.Exposure = activeProfile.GetExposureModifier(weatherType);

            lastWeatherType = weatherType;
            lastTimePhase = timePhase;

            bool temperatureChanged = forceRaiseAll
                || !Mathf.Approximately(previousTemperature, environmentState.AmbientTemperature);
            bool wetnessChanged = forceRaiseAll
                || !Mathf.Approximately(previousWetness, environmentState.Wetness);
            bool exposureChanged = forceRaiseAll
                || !Mathf.Approximately(previousExposure, environmentState.Exposure);

            if (!temperatureChanged && !wetnessChanged && !exposureChanged)
            {
                return;
            }

            RaiseEnvironmentChanged(message, temperatureChanged, wetnessChanged, exposureChanged);
        }

        private float ResolvePhaseTemperatureModifier(CCS_TimeOfDayPhase timePhase)
        {
            switch (timePhase)
            {
                case CCS_TimeOfDayPhase.Day:
                    return activeProfile.DayTemperatureBonus;
                case CCS_TimeOfDayPhase.Night:
                    return activeProfile.NightTemperaturePenalty;
                default:
                    return 0f;
            }
        }

        private static CCS_WeatherType ResolveEffectiveWeatherType(CCS_WeatherSnapshot weatherSnapshot)
        {
            if (weatherSnapshot.IsTransitioning)
            {
                return weatherSnapshot.TransitionTargetWeather;
            }

            return weatherSnapshot.CurrentWeather;
        }

        private CCS_EnvironmentSnapshot BuildSnapshot()
        {
            return new CCS_EnvironmentSnapshot(
                environmentState.AmbientTemperature,
                environmentState.Wetness,
                environmentState.Exposure,
                lastWeatherType,
                lastTimePhase);
        }

        private void RaiseEnvironmentChanged(
            string message,
            bool temperatureChanged,
            bool wetnessChanged,
            bool exposureChanged)
        {
            CCS_EnvironmentEffectsEventArgs eventArgs =
                new CCS_EnvironmentEffectsEventArgs(GetSnapshot(), message);

            EnvironmentChanged?.Invoke(eventArgs);

            if (temperatureChanged)
            {
                TemperatureChanged?.Invoke(eventArgs);
            }

            if (wetnessChanged)
            {
                WetnessChanged?.Invoke(eventArgs);
            }

            if (exposureChanged)
            {
                ExposureChanged?.Invoke(eventArgs);
            }
        }

        private void UnbindTimeOfDayService()
        {
            if (timeOfDayService == null)
            {
                return;
            }

            timeOfDayService.TimeChanged -= HandleTimeOfDayChanged;
            timeOfDayService.PhaseChanged -= HandleTimeOfDayChanged;
            timeOfDayService = null;
        }

        private void UnbindWeatherService()
        {
            if (weatherService == null)
            {
                return;
            }

            weatherService.WeatherChanged -= HandleWeatherChanged;
            weatherService.WeatherTransitionStarted -= HandleWeatherChanged;
            weatherService.WeatherTransitionCompleted -= HandleWeatherChanged;
            weatherService = null;
        }

        #endregion
    }
}
