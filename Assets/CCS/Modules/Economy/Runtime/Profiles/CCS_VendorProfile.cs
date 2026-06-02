using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorProfile
// CATEGORY: Modules / Economy / Runtime / Profiles
// PURPOSE: Profile listing vendor definitions registered at startup.
// PLACEMENT: Assets/CCS/Survival/Profiles/Economy/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [CreateAssetMenu(
        fileName = "CCS_VendorProfile",
        menuName = "CCS/Survival/Economy/Vendor Profile")]
    public sealed class CCS_VendorProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Vendors")]
        [Tooltip("Vendor definitions available to CCS_VendorService.")]
        [SerializeField] private CCS_VendorDefinition[] vendorDefinitions = new CCS_VendorDefinition[0];

        #endregion

        #region Properties

        public CCS_VendorDefinition[] VendorDefinitions => vendorDefinitions ?? new CCS_VendorDefinition[0];

        #endregion
    }
}
