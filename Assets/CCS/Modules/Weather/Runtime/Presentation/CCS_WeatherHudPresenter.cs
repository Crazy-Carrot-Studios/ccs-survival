using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_WeatherHudPresenter
// CATEGORY: Modules / Weather / Runtime / Presentation
// PURPOSE: Read-only HUD display for current weather and transition progress.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas near the time-of-day panel.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Development/read-only display. No icons or final weather art.
// =============================================================================

namespace CCS.Modules.Weather
{
    public sealed class CCS_WeatherHudPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Text element showing current weather state.")]
        [SerializeField] private Text statusText;

        private CCS_WeatherService weatherService;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            TryBindService();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            UnbindServiceEvents();
        }

        private void OnDestroy()
        {
            UnbindServiceEvents();
            weatherService = null;
        }

        #endregion

        #region Public Methods

        public void BindStatusText(Text textComponent)
        {
            statusText = textComponent;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (statusText == null)
            {
                return;
            }

            if (!TryBindService())
            {
                statusText.text = "Weather\nUnavailable";
                return;
            }

            CCS_WeatherSnapshot snapshot = weatherService.GetSnapshot();
            statusText.text = CCS_WeatherValidationUtility.FormatWeatherDisplay(snapshot);
        }

        #endregion

        #region Private Methods

        private bool TryBindService()
        {
            if (weatherService != null && weatherService.IsInitialized)
            {
                return true;
            }

            UnbindServiceEvents();

            if (!CCS_WeatherRuntimeBridge.TryGetWeatherService(out weatherService)
                || weatherService == null
                || !weatherService.IsInitialized)
            {
                weatherService = null;
                return false;
            }

            weatherService.WeatherChanged += HandleWeatherChanged;
            weatherService.WeatherTransitionStarted += HandleWeatherChanged;
            weatherService.WeatherTransitionCompleted += HandleWeatherChanged;
            weatherService.WeatherPaused += HandleWeatherChanged;
            weatherService.WeatherResumed += HandleWeatherChanged;
            return true;
        }

        private void UnbindServiceEvents()
        {
            if (weatherService == null)
            {
                return;
            }

            weatherService.WeatherChanged -= HandleWeatherChanged;
            weatherService.WeatherTransitionStarted -= HandleWeatherChanged;
            weatherService.WeatherTransitionCompleted -= HandleWeatherChanged;
            weatherService.WeatherPaused -= HandleWeatherChanged;
            weatherService.WeatherResumed -= HandleWeatherChanged;
        }

        private void HandleWeatherChanged(CCS_WeatherEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
