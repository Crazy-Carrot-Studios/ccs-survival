using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EconomyProfile
// CATEGORY: Modules / Economy / Runtime / Profiles
// PURPOSE: Economy module tuning profile for currencies and vendors.
// PLACEMENT: Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [CreateAssetMenu(
        fileName = "CCS_EconomyProfile",
        menuName = "CCS/Survival/Economy/Economy Profile")]
    public sealed class CCS_EconomyProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Currencies")]
        [Tooltip("Currency definitions registered by CCS_CurrencyService.")]
        [SerializeField] private CCS_CurrencyDefinition[] currencyDefinitions = new CCS_CurrencyDefinition[0];

        [Tooltip("Default currency used when vendor omits an explicit currency.")]
        [SerializeField] private CCS_CurrencyDefinition defaultCurrencyDefinition;

        [Header("Vendors")]
        [Tooltip("Vendor catalog profile.")]
        [SerializeField] private CCS_VendorProfile vendorProfile;

        [Header("Diagnostics")]
        [Tooltip("Emit economy service debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        #endregion

        #region Properties

        public CCS_CurrencyDefinition[] CurrencyDefinitions =>
            currencyDefinitions ?? new CCS_CurrencyDefinition[0];

        public CCS_CurrencyDefinition DefaultCurrencyDefinition => defaultCurrencyDefinition;

        public CCS_VendorProfile VendorProfile => vendorProfile;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
