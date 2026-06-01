using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_CookingRequest
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Request payload for campfire cooking attempts.
// PLACEMENT: Built by CCS_CampfireInteractable and passed to CCS_CookingService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses FirePit station classification without world station UI.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingRequest
    {
        #region Public Methods

        public CCS_CookingRequest(
            CCS_CampfireDefinition campfireDefinition,
            CCS_ItemDefinition inputItemDefinition,
            CCS_ItemDefinition outputItemDefinition,
            string campfireInstanceKey,
            float cookTimeSeconds)
        {
            CampfireDefinition = campfireDefinition;
            InputItemDefinition = inputItemDefinition;
            OutputItemDefinition = outputItemDefinition;
            CampfireInstanceKey = campfireInstanceKey ?? string.Empty;
            CookTimeSeconds = cookTimeSeconds < 0f ? 0f : cookTimeSeconds;
        }

        #endregion

        #region Properties

        public CCS_CampfireDefinition CampfireDefinition { get; }

        public CCS_ItemDefinition InputItemDefinition { get; }

        public CCS_ItemDefinition OutputItemDefinition { get; }

        public string CampfireInstanceKey { get; }

        public float CookTimeSeconds { get; }

        #endregion
    }
}
