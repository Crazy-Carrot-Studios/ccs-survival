using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InventoryProfile
// CATEGORY: Modules / Inventory / Runtime / Profiles
// PURPOSE: Tuning profile for player inventory slot count and future weight limits.
// PLACEMENT: Assets/CCS/Survival/Profiles/Inventory/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Weight system placeholder only in 0.4.0. No UI or save references.
// =============================================================================

namespace CCS.Modules.Inventory
{
    [CreateAssetMenu(
        fileName = "CCS_InventoryProfile",
        menuName = "CCS/Survival/Inventory/Inventory Profile")]
    public sealed class CCS_InventoryProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Capacity")]
        [Tooltip("Number of inventory slots available to the player.")]
        [SerializeField] private int inventorySlotCount = 40;

        [Header("Weight (Placeholder)")]
        [Tooltip("When enabled, future systems enforce a maximum carry weight.")]
        [SerializeField] private bool enableWeightLimit;

        [Tooltip("Maximum carry weight when weight limiting is enabled.")]
        [SerializeField] private float maxCarryWeight = 100f;

        [Header("Save Restore")]
        [Tooltip("Item definitions available when resolving inventory save payloads by item ID.")]
        [SerializeField] private CCS_ItemDefinition[] saveRestoreItemDefinitions = System.Array.Empty<CCS_ItemDefinition>();

        #endregion

        #region Properties

        public int InventorySlotCount => inventorySlotCount;

        public bool EnableWeightLimit => enableWeightLimit;

        public float MaxCarryWeight => maxCarryWeight;

        public CCS_ItemDefinition[] SaveRestoreItemDefinitions => saveRestoreItemDefinitions;

        #endregion
    }
}
