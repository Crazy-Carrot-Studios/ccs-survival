using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsHudPresenter
// CATEGORY: Modules / EnvironmentEffects / Runtime / Presentation
// PURPOSE: Read-only HUD display for ambient temperature, wetness, and exposure.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas beneath time and weather panels.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Development/read-only display. No icons or final art.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public sealed class CCS_EnvironmentEffectsHudPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Text element showing ambient temperature, wetness, and exposure.")]
        [SerializeField] private Text statusText;

        private CCS_EnvironmentEffectsService environmentService;

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
            environmentService = null;
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
                statusText.text = "Environment\nUnavailable";
                return;
            }

            CCS_EnvironmentSnapshot snapshot = environmentService.GetSnapshot();
            statusText.text = CCS_EnvironmentEffectsValidationUtility.FormatEnvironmentDisplay(snapshot);
        }

        #endregion

        #region Private Methods

        private bool TryBindService()
        {
            if (environmentService != null && environmentService.IsInitialized)
            {
                return true;
            }

            UnbindServiceEvents();

            if (!CCS_EnvironmentEffectsRuntimeBridge.TryGetEnvironmentEffectsService(out environmentService)
                || environmentService == null
                || !environmentService.IsInitialized)
            {
                environmentService = null;
                return false;
            }

            environmentService.EnvironmentChanged += HandleEnvironmentChanged;
            environmentService.TemperatureChanged += HandleEnvironmentChanged;
            environmentService.WetnessChanged += HandleEnvironmentChanged;
            environmentService.ExposureChanged += HandleEnvironmentChanged;
            return true;
        }

        private void UnbindServiceEvents()
        {
            if (environmentService == null)
            {
                return;
            }

            environmentService.EnvironmentChanged -= HandleEnvironmentChanged;
            environmentService.TemperatureChanged -= HandleEnvironmentChanged;
            environmentService.WetnessChanged -= HandleEnvironmentChanged;
            environmentService.ExposureChanged -= HandleEnvironmentChanged;
        }

        private void HandleEnvironmentChanged(CCS_EnvironmentEffectsEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
