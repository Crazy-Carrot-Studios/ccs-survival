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
            ApplyTypography(profile);
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

            if (presentationService == null || presentationService.EquipmentSnapshot == null)
            {
                summaryText.text = "Equipment\n--";
                return;
            }

            CCS.Modules.Equipment.CCS_EquipmentSnapshot snapshot = presentationService.EquipmentSnapshot;
            if (snapshot.TotalSlotCount <= 0)
            {
                summaryText.text = "Equipment\n--";
                return;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            builder.AppendLine("Equipment");
            builder.AppendLine($"{snapshot.OccupiedSlotCount} Equipped");

            int linesAdded = 0;
            const int maxEquippedLines = 4;
            for (int index = 0; index < snapshot.EquippedItems.Count && linesAdded < maxEquippedLines; index++)
            {
                CCS.Modules.Equipment.CCS_EquippedItem equippedItem = snapshot.EquippedItems[index];
                if (equippedItem?.ItemDefinition == null)
                {
                    continue;
                }

                builder.AppendLine(equippedItem.ItemDefinition.DisplayName);
                linesAdded++;
            }

            if (snapshot.TotalAdditionalInventorySlots > 0)
            {
                builder.AppendLine($"+{snapshot.TotalAdditionalInventorySlots} Inventory Slots");
            }

            summaryText.text = builder.ToString().TrimEnd();
        }

        private void ApplyTypography(CCS_HudProfile profile)
        {
            if (profile?.LayoutSettings == null || summaryText == null)
            {
                return;
            }

            CCS_HudLayoutApplicator.ApplyTypography(summaryText, profile.LayoutSettings.SummaryFontSize);
        }

        #endregion
    }
}
