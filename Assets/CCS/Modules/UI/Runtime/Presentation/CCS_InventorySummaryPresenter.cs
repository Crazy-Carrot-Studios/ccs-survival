using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_InventorySummaryPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Displays compact inventory summary data from HUD presentation snapshots.
// PLACEMENT: Child of PF_CCS_HUD_Root inventory summary area.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only display. No inventory mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_InventorySummaryPresenter : MonoBehaviour
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

            gameObject.SetActive(profile == null || profile.ShowInventorySummary);
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

            if (presentationService?.InventorySnapshot == null)
            {
                summaryText.text = "Inventory: unavailable";
                return;
            }

            CCS.Modules.Inventory.CCS_InventorySnapshot snapshot = presentationService.InventorySnapshot;
            summaryText.text =
                $"Inventory: {snapshot.UsedSlotCount}/{snapshot.SlotCount} slots | Qty {snapshot.TotalItemQuantity}";
        }

        #endregion
    }
}
