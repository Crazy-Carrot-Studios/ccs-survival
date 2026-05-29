using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapRequirementEntry
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: Serializable scene object requirement for scene bootstrap profiles.
// PLACEMENT: Listed on CCS_SurvivalSceneBootstrapProfile required/optional scene object lists.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Modules register object names without changing bootstrap architecture.
// =============================================================================

namespace CCS.Survival.Development
{
    [Serializable]
    public sealed class CCS_SurvivalSceneBootstrapRequirementEntry
    {
        #region Variables

        [Tooltip("Save-stable requirement id (example: ccs.survival.bootstrap.player_root).")]
        [SerializeField] private string requirementId = string.Empty;

        [Tooltip("Readable label for validation reports and editor tooling.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Expected root GameObject name in the active scene hierarchy.")]
        [SerializeField] private string sceneObjectName = string.Empty;

        #endregion

        #region Properties

        public string RequirementId => requirementId;

        public string DisplayName => displayName;

        public string SceneObjectName => sceneObjectName;

        public bool HasSceneObjectName => !string.IsNullOrWhiteSpace(sceneObjectName);

        #endregion
    }
}
