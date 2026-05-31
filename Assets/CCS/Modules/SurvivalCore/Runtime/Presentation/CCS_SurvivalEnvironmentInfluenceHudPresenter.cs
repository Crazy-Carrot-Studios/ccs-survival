using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SurvivalEnvironmentInfluenceHudPresenter
// CATEGORY: Modules / SurvivalCore / Runtime / Presentation
// PURPOSE: Read-only HUD display for environment influence rates on survival stats.
// PLACEMENT: Child of PF_CCS_HUD_Root canvas beneath the environment effects panel.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Debug verification only. No icons or final art.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public sealed class CCS_SurvivalEnvironmentInfluenceHudPresenter : MonoBehaviour
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Text element showing temperature, fatigue, and thirst influence rates.")]
        [SerializeField] private Text statusText;

        private CCS_SurvivalCoreService survivalCoreService;

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
            survivalCoreService = null;
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
                statusText.text = "Influence\nUnavailable";
                return;
            }

            CCS_SurvivalEnvironmentInfluence influence = survivalCoreService.CurrentEnvironmentInfluence;
            statusText.text = CCS_SurvivalEnvironmentInfluenceUtility.FormatInfluenceDisplay(influence);
        }

        #endregion

        #region Private Methods

        private bool TryBindService()
        {
            if (survivalCoreService != null && survivalCoreService.IsInitialized)
            {
                return true;
            }

            UnbindServiceEvents();

            if (!CCS_SurvivalCoreRuntimeBridge.TryGetSurvivalCoreService(out survivalCoreService)
                || survivalCoreService == null
                || !survivalCoreService.IsInitialized)
            {
                survivalCoreService = null;
                return false;
            }

            survivalCoreService.EnvironmentInfluenceChanged += HandleEnvironmentInfluenceChanged;
            return true;
        }

        private void UnbindServiceEvents()
        {
            if (survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.EnvironmentInfluenceChanged -= HandleEnvironmentInfluenceChanged;
        }

        private void HandleEnvironmentInfluenceChanged(CCS_SurvivalEnvironmentEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
