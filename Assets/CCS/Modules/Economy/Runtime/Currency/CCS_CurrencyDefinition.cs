using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CurrencyDefinition
// CATEGORY: Modules / Economy / Runtime / Currency
// PURPOSE: ScriptableObject describing a generic currency type (dollars, gold, credits, etc.).
// PLACEMENT: Assets/CCS/Survival/Profiles/Economy/Currencies/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Optional inventory backing item syncs wallet with stackable currency items.
// =============================================================================

namespace CCS.Modules.Economy
{
    [CreateAssetMenu(
        fileName = "CCS_CurrencyDefinition",
        menuName = "CCS/Survival/Economy/Currency Definition")]
    public sealed class CCS_CurrencyDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS currency ID.")]
        [SerializeField] private string currencyId = string.Empty;

        [Tooltip("Player-facing currency name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for future UI.")]
        [SerializeField] private string description = string.Empty;

        [Header("Presentation")]
        [Tooltip("Optional icon for future HUD.")]
        [SerializeField] private Sprite icon;

        [Header("Startup")]
        [Tooltip("When true, profile/bootstrap may assign a starting wallet balance.")]
        [SerializeField] private bool supportsStartingBalance = true;

        [Header("Inventory Backing (Optional)")]
        [Tooltip("When set, wallet balance syncs with this stackable inventory item (e.g. Trade Dollars).")]
        [SerializeField] private CCS_ItemDefinition inventoryBackingItem;

        #endregion

        #region Properties

        public string CurrencyId => currencyId;

        public string DisplayName => displayName;

        public string Description => description;

        public Sprite Icon => icon;

        public bool SupportsStartingBalance => supportsStartingBalance;

        public CCS_ItemDefinition InventoryBackingItem => inventoryBackingItem;

        public bool HasInventoryBacking => inventoryBackingItem != null;

        #endregion
    }
}
