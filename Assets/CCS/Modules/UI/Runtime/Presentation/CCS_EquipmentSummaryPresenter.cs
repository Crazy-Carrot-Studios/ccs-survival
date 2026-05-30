using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_EquipmentSummaryPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Displays compact equipment summary data from HUD presentation snapshots.
// PLACEMENT: Child of PF_CCS_HUD_Root equipment summary area.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only display. No equipment mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_EquipmentSummaryPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Text summaryText;

        private CCS_HudPresentationService presentationService;

        #endregion

        #region Public Methods

        public void Bind(CCS_HudPresentationService service, CCS_HudProfile profile)
        {
            Unbind();
            presentationService = service;

            if (presentationService != null)
            {
                presentationService.HudDataRefreshed += HandleHudDataRefreshed;
                presentationService.HudInitialized += HandleHudDataRefreshed;
            }

            gameObject.SetActive(profile == null || profile.ShowEquipmentSummary);
            RefreshDisplay();
        }

        public void Unbind()
        {
            if (presentationService != null)
            {
                presentationService.HudDataRefreshed -= HandleHudDataRefreshed;
                presentationService.HudInitialized -= HandleHudDataRefreshed;
            }

            presentationService = null;
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Private Methods

        private void HandleHudDataRefreshed(CCS_HudEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (summaryText == null)
            {
                return;
            }

            if (presentationService?.EquipmentSnapshot == null)
            {
                summaryText.text = "Equipment: unavailable";
                return;
            }

            CCS.Modules.Equipment.CCS_EquipmentSnapshot snapshot = presentationService.EquipmentSnapshot;
            summaryText.text =
                $"Equipment: {snapshot.OccupiedSlotCount}/{snapshot.TotalSlotCount} | +Slots {snapshot.TotalAdditionalInventorySlots} | +Wt {snapshot.TotalAdditionalCarryWeight:0}";
        }

        #endregion
    }
}
