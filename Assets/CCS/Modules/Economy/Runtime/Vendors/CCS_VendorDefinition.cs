using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorDefinition
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: ScriptableObject describing a buy/sell vendor and catalog.
// PLACEMENT: Assets/CCS/Survival/Content/Vendors/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Western-specific vendors live in Survival content; framework stays generic.
// =============================================================================

namespace CCS.Modules.Economy
{
    [CreateAssetMenu(
        fileName = "CCS_VendorDefinition",
        menuName = "CCS/Survival/Economy/Vendor Definition")]
    public sealed class CCS_VendorDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS vendor ID.")]
        [SerializeField] private string vendorId = string.Empty;

        [Tooltip("Player-facing vendor name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for interaction prompts.")]
        [SerializeField] private string description = string.Empty;

        [Header("Economy")]
        [Tooltip("Currency accepted by this vendor.")]
        [SerializeField] private CCS_CurrencyDefinition currencyDefinition;

        [Header("Catalog")]
        [SerializeField] private CCS_VendorInventory vendorInventory = new CCS_VendorInventory();

        #endregion

        #region Properties

        public string VendorId => vendorId;

        public string DisplayName => displayName;

        public string Description => description;

        public CCS_CurrencyDefinition CurrencyDefinition => currencyDefinition;

        public CCS_VendorInventory VendorInventory => vendorInventory;

        #endregion
    }
}
