using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionProfile
// CATEGORY: Modules / Regions / Runtime / Profiles
// PURPOSE: Region module profile catalog and tuning.
// PLACEMENT: Assets/CCS/Survival/Profiles/Regions/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Registered on CCS_SurvivalGameplayServiceHost.
// =============================================================================

namespace CCS.Modules.Regions
{
    [CreateAssetMenu(
        fileName = "CCS_RegionProfile",
        menuName = "CCS/Survival/Regions/Region Profile")]
    public sealed class CCS_RegionProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Catalog")]
        [Tooltip("Region definitions known to the region service.")]
        [SerializeField] private CCS_RegionDefinition[] regionDefinitions = new CCS_RegionDefinition[0];

        [Header("Diagnostics")]
        [Tooltip("Emit region service debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        #endregion

        #region Properties

        public CCS_RegionDefinition[] RegionDefinitions =>
            regionDefinitions ?? new CCS_RegionDefinition[0];

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
