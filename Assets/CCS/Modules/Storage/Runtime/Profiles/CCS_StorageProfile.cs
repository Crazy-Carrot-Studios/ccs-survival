using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageProfile
// CATEGORY: Modules / Storage / Runtime / Profiles
// PURPOSE: Tuning profile for storage service startup and default container definitions.
// PLACEMENT: Assets/CCS/Survival/Profiles/Storage/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation.
// =============================================================================

namespace CCS.Modules.Storage
{
    [CreateAssetMenu(
        fileName = "CCS_StorageProfile",
        menuName = "CCS/Survival/Storage/Storage Profile")]
    public sealed class CCS_StorageProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Startup")]
        [Tooltip("Default primitive storage crate definition used for placement helpers.")]
        [SerializeField] private CCS_StorageContainerDefinition defaultContainerDefinition;

        [Header("Diagnostics")]
        [Tooltip("Emit storage service debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        #endregion

        #region Properties

        public CCS_StorageContainerDefinition DefaultContainerDefinition => defaultContainerDefinition;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
