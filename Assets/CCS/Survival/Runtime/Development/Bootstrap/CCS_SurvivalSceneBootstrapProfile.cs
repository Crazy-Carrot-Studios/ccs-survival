using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapProfile
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: ScriptableObject profile for required/optional scene startup services and objects.
// PLACEMENT: Assets/CCS/Survival/Settings/Development/Bootstrap/ (future). Optional reference.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Lists may be empty in 0.3.6. Future modules append requirements without architecture changes.
// =============================================================================

namespace CCS.Survival.Development
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalSceneBootstrapProfile",
        menuName = "CCS/Survival/Development/Scene Bootstrap Profile")]
    public sealed class CCS_SurvivalSceneBootstrapProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Composition Root")]
        [Tooltip("When true, active scene must contain exactly one CCS_RuntimeHost.")]
        [SerializeField] private bool requireRuntimeHost = true;

        [Tooltip("When true, active scene must contain exactly one CCS_SurvivalBootstrap.")]
        [SerializeField] private bool requireSurvivalBootstrap = true;

        [Header("Required Services")]
        [Tooltip("Service contracts that must be registered on the runtime service registry when lists are populated.")]
        [SerializeField] private List<CCS_SurvivalSceneBootstrapServiceRequirement> requiredServices =
            new List<CCS_SurvivalSceneBootstrapServiceRequirement>();

        [Header("Required Scene Objects")]
        [Tooltip("Scene GameObjects that must exist in the active scene hierarchy.")]
        [SerializeField] private List<CCS_SurvivalSceneBootstrapRequirementEntry> requiredSceneObjects =
            new List<CCS_SurvivalSceneBootstrapRequirementEntry>();

        [Header("Optional Scene Objects")]
        [Tooltip("Scene GameObjects that are recommended but do not fail validation when missing.")]
        [SerializeField] private List<CCS_SurvivalSceneBootstrapRequirementEntry> optionalSceneObjects =
            new List<CCS_SurvivalSceneBootstrapRequirementEntry>();

        #endregion

        #region Properties

        public bool RequireRuntimeHost => requireRuntimeHost;

        public bool RequireSurvivalBootstrap => requireSurvivalBootstrap;

        public IReadOnlyList<CCS_SurvivalSceneBootstrapServiceRequirement> RequiredServices => requiredServices;

        public IReadOnlyList<CCS_SurvivalSceneBootstrapRequirementEntry> RequiredSceneObjects => requiredSceneObjects;

        public IReadOnlyList<CCS_SurvivalSceneBootstrapRequirementEntry> OptionalSceneObjects => optionalSceneObjects;

        #endregion
    }
}
