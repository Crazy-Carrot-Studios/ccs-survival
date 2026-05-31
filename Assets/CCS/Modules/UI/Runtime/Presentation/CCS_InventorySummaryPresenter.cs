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

            if (presentationService == null ||
                presentationService.InventorySnapshot == null ||
                presentationService.InventorySnapshot.SlotCount <= 0)
            {
                summaryText.text = "Inventory\n-- / -- Slots";
                return;
            }

            CCS.Modules.Inventory.CCS_InventorySnapshot snapshot = presentationService.InventorySnapshot;
            int totalSlots = presentationService.EffectiveInventorySlotCount;
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            builder.AppendLine("Inventory");
            builder.AppendLine($"{snapshot.UsedSlotCount} / {totalSlots} Slots");

            int linesAdded = 0;
            const int maxItemLines = 8;
            for (int index = 0; index < snapshot.SlotStacks.Count && linesAdded < maxItemLines; index++)
            {
                CCS.Modules.Inventory.CCS_ItemStack stack = snapshot.SlotStacks[index];
                if (stack.IsEmpty || stack.ItemDefinition == null)
                {
                    continue;
                }

                builder.AppendLine($"{stack.ItemDefinition.DisplayName} x{stack.Quantity}");
                linesAdded++;
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
