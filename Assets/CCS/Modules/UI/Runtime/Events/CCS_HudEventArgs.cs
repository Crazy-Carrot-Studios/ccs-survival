using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;

// =============================================================================
// SCRIPT: CCS_HudEventArgs
// CATEGORY: Modules / UI / Runtime / Events
// PURPOSE: Payload for HUD presentation events and cached display data.
// PLACEMENT: Passed by CCS_HudPresentationService to presenters.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only snapshots. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_HudEventArgs
    {
        #region Public Methods

        public CCS_HudEventArgs(
            string message,
            string interactionPrompt,
            CCS_InventorySnapshot inventorySnapshot,
            CCS_EquipmentSnapshot equipmentSnapshot,
            CCS_SurvivalStatSnapshot healthSnapshot,
            CCS_SurvivalStatSnapshot staminaSnapshot,
            CCS_SurvivalStatSnapshot hungerSnapshot,
            CCS_SurvivalStatSnapshot thirstSnapshot)
        {
            Message = message ?? string.Empty;
            InteractionPrompt = interactionPrompt ?? string.Empty;
            InventorySnapshot = inventorySnapshot;
            EquipmentSnapshot = equipmentSnapshot;
            HealthSnapshot = healthSnapshot;
            StaminaSnapshot = staminaSnapshot;
            HungerSnapshot = hungerSnapshot;
            ThirstSnapshot = thirstSnapshot;
        }

        #endregion

        #region Properties

        public string Message { get; }

        public string InteractionPrompt { get; }

        public CCS_InventorySnapshot InventorySnapshot { get; }

        public CCS_EquipmentSnapshot EquipmentSnapshot { get; }

        public CCS_SurvivalStatSnapshot HealthSnapshot { get; }

        public CCS_SurvivalStatSnapshot StaminaSnapshot { get; }

        public CCS_SurvivalStatSnapshot HungerSnapshot { get; }

        public CCS_SurvivalStatSnapshot ThirstSnapshot { get; }

        #endregion
    }
}
