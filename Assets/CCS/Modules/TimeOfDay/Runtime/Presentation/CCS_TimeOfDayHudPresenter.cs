using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_TimeOfDayHudPresenter
// CATEGORY: Modules / TimeOfDay / Runtime / Presentation
// PURPOSE: Read-only HUD display for day, clock time, and current phase.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas in bootstrap verification scenes.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Development/read-only display. Not final clock art.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public sealed class CCS_TimeOfDayHudPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Text element showing day, clock time, and phase.")]
        [SerializeField] private Text statusText;

        private CCS_TimeOfDayService timeOfDayService;

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
            timeOfDayService = null;
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
                statusText.text = "Time\nUnavailable";
                return;
            }

            CCS_GameTimeSnapshot snapshot = timeOfDayService.CreateSnapshot();
            string pausedSuffix = snapshot.IsPaused ? " (Paused)" : string.Empty;
            statusText.text =
                "Time\n" +
                $"Day {snapshot.DayNumber}\n" +
                $"{snapshot.Hour:D2}:{snapshot.Minute:D2}{pausedSuffix}\n" +
                $"Phase: {snapshot.CurrentPhase}";
        }

        #endregion

        #region Private Methods

        private bool TryBindService()
        {
            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                return true;
            }

            UnbindServiceEvents();

            if (!CCS_TimeOfDayRuntimeBridge.TryGetTimeOfDayService(out timeOfDayService)
                || timeOfDayService == null
                || !timeOfDayService.IsInitialized)
            {
                timeOfDayService = null;
                return false;
            }

            timeOfDayService.TimeChanged += HandleTimeChanged;
            timeOfDayService.PhaseChanged += HandleTimeChanged;
            timeOfDayService.TimePaused += HandleTimeChanged;
            timeOfDayService.TimeResumed += HandleTimeChanged;
            return true;
        }

        private void UnbindServiceEvents()
        {
            if (timeOfDayService == null)
            {
                return;
            }

            timeOfDayService.TimeChanged -= HandleTimeChanged;
            timeOfDayService.PhaseChanged -= HandleTimeChanged;
            timeOfDayService.TimePaused -= HandleTimeChanged;
            timeOfDayService.TimeResumed -= HandleTimeChanged;
        }

        private void HandleTimeChanged(CCS_TimeOfDayEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
