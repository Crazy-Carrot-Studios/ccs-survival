using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepSpotDefinition
// CATEGORY: Modules / Sleep / Runtime / Definitions
// PURPOSE: ScriptableObject definition for primitive placeable bedroll sleep spots.
// PLACEMENT: Assets/CCS/Survival/Content/Sleep/Primitive/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 sleep and bedroll foundation. Primitives only.
// =============================================================================

namespace CCS.Modules.Sleep
{
    [CreateAssetMenu(
        fileName = "CCS_SleepSpotDefinition",
        menuName = "CCS/Survival/Sleep/Sleep Spot Definition")]
    public sealed class CCS_SleepSpotDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS sleep spot definition id.")]
        [SerializeField] private string sleepSpotId = string.Empty;

        [Tooltip("Player-facing sleep spot label.")]
        [SerializeField] private string displayName = "Bedroll";

        [Header("Placement")]
        [Tooltip("Prefab spawned when this sleep spot is placed in the world.")]
        [SerializeField] private GameObject prefabReference;

        [Header("Diagnostics")]
        [Tooltip("Emit sleep spot debug logs for this definition.")]
        [SerializeField] private bool enableDebugLogging;

        #endregion

        #region Properties

        public string SleepSpotId => sleepSpotId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public GameObject PrefabReference => prefabReference;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
