using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HudRootPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: HUD composition root that owns presentation service and child presenters.
// PLACEMENT: Root of PF_CCS_HUD_Root prefab.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only bridge to gameplay services. Safe when services are missing.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_HudRootPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_HudProfile hudProfile;
        [SerializeField] private RectTransform survivalBarArea;
        [SerializeField] private RectTransform interactionPromptArea;
        [SerializeField] private RectTransform inventorySummaryArea;
        [SerializeField] private RectTransform equipmentSummaryArea;
        [SerializeField] private RectTransform notificationArea;
        [SerializeField] private CCS_SurvivalBarPresenter survivalBarPresenter;
        [SerializeField] private CCS_InteractionPromptPresenter interactionPromptPresenter;
        [SerializeField] private CCS_InventorySummaryPresenter inventorySummaryPresenter;
        [SerializeField] private CCS_EquipmentSummaryPresenter equipmentSummaryPresenter;
        [SerializeField] private CCS_NotificationQueue notificationQueue;

        private CCS_HudPresentationService presentationService;

        #endregion

        #region Properties

        public CCS_HudPresentationService PresentationService => presentationService;

        public CCS_HudProfile HudProfile => hudProfile;

        public RectTransform SurvivalBarArea => survivalBarArea;

        public RectTransform InteractionPromptArea => interactionPromptArea;

        public RectTransform InventorySummaryArea => inventorySummaryArea;

        public RectTransform EquipmentSummaryArea => equipmentSummaryArea;

        public RectTransform NotificationArea => notificationArea;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            presentationService = new CCS_HudPresentationService();
            presentationService.Initialize();

            if (hudProfile != null)
            {
                presentationService.InitializeFromProfile(hudProfile);
            }

            BindPresenters();
            ApplyLayoutFromProfile();
            presentationService.RefreshCachedData("HUD root initialized.");
        }

        private void OnDestroy()
        {
            UnbindPresenters();
            presentationService?.Shutdown();
            presentationService = null;
        }

        #endregion

        #region Public Methods

        public void SetHudProfile(CCS_HudProfile profile)
        {
            hudProfile = profile;

            if (presentationService != null && hudProfile != null)
            {
                presentationService.InitializeFromProfile(hudProfile);
            }

            BindPresenters();
            ApplyLayoutFromProfile();
        }

        #endregion

        #region Private Methods

        private void ApplyLayoutFromProfile()
        {
            if (hudProfile == null)
            {
                return;
            }

            CCS_HudLayoutApplicator.Apply(this, hudProfile);
        }

        private void BindPresenters()
        {
            survivalBarPresenter?.Bind(presentationService, hudProfile);
            interactionPromptPresenter?.Bind(presentationService, hudProfile);
            inventorySummaryPresenter?.Bind(presentationService, hudProfile);
            equipmentSummaryPresenter?.Bind(presentationService, hudProfile);
            notificationQueue?.Bind(presentationService, hudProfile);
        }

        private void UnbindPresenters()
        {
            survivalBarPresenter?.Unbind();
            interactionPromptPresenter?.Unbind();
            inventorySummaryPresenter?.Unbind();
            equipmentSummaryPresenter?.Unbind();
            notificationQueue?.Unbind();
        }

        #endregion
    }
}
