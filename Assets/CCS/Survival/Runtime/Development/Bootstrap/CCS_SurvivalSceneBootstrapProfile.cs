using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalSceneBootstrapProfile
// CATEGORY: Survival / Runtime / Development / Bootstrap
// PURPOSE: ScriptableObject profile describing required scene startup expectations for future validation.
// PLACEMENT: Assets/CCS/Survival/Settings/Development/Bootstrap/ (future). Optional reference.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Defines required prefab/service placeholders. No gameplay modules instantiated in 0.3.6.
// =============================================================================

namespace CCS.Survival.Development
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalSceneBootstrapProfile",
        menuName = "CCS/Survival/Development/Scene Bootstrap Profile")]
    public sealed class CCS_SurvivalSceneBootstrapProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Scene Requirements")]
        [Tooltip("When true, active scene must contain exactly one CCS_RuntimeHost.")]
        [SerializeField] private bool requireRuntimeHost = true;

        [Tooltip("When true, active scene must contain exactly one CCS_SurvivalBootstrap.")]
        [SerializeField] private bool requireSurvivalBootstrap = true;

        [Tooltip("Optional required prefab asset names (content validation only; not auto-spawned).")]
        [SerializeField] private List<string> requiredPrefabAssetNames = new List<string>();

        [Tooltip("Optional required service contract type names for future registration checks.")]
        [SerializeField] private List<string> requiredServiceContractNames = new List<string>();

        #endregion

        #region Properties

        public bool RequireRuntimeHost => requireRuntimeHost;

        public bool RequireSurvivalBootstrap => requireSurvivalBootstrap;

        public IReadOnlyList<string> RequiredPrefabAssetNames => requiredPrefabAssetNames;

        public IReadOnlyList<string> RequiredServiceContractNames => requiredServiceContractNames;

        #endregion
    }
}
