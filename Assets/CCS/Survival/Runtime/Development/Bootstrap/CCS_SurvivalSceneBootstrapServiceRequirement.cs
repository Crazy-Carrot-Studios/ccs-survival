using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapServiceRequirement
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: Serializable required service contract entry for scene bootstrap profiles.
// PLACEMENT: Listed on CCS_SurvivalSceneBootstrapProfile required services list.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Contract name is assembly-qualified or full type name for future registry checks.
// =============================================================================

namespace CCS.Survival.Development
{
    [Serializable]
    public sealed class CCS_SurvivalSceneBootstrapServiceRequirement
    {
        #region Variables

        [Tooltip("Save-stable requirement id (example: ccs.survival.bootstrap.service.inventory).")]
        [SerializeField] private string requirementId = string.Empty;

        [Tooltip("Full or short service contract type name (example: CCS_ISurvivalInventoryService).")]
        [SerializeField] private string serviceContractName = string.Empty;

        #endregion

        #region Properties

        public string RequirementId => requirementId;

        public string ServiceContractName => serviceContractName;

        public bool HasServiceContractName => !string.IsNullOrWhiteSpace(serviceContractName);

        #endregion
    }
}
