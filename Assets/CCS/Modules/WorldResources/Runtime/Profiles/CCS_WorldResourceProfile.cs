using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldResourceProfile
// CATEGORY: Modules / WorldResources / Runtime / Profiles
// PURPOSE: Tuning profile for respawn rules and future world resource policy.
// PLACEMENT: Assets/CCS/Survival/Profiles/WorldResources/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, save, terrain, or final art references in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    [CreateAssetMenu(
        fileName = "CCS_WorldResourceProfile",
        menuName = "CCS/Survival/World Resources/World Resource Profile")]
    public sealed class CCS_WorldResourceProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Respawn")]
        [Tooltip("When enabled, depleted nodes may respawn after their timer completes.")]
        [SerializeField] private bool enableRespawn = true;

        [Tooltip("Multiplier applied to resource respawn time seconds.")]
        [SerializeField] private float globalRespawnMultiplier = 1f;

        #endregion

        #region Properties

        public bool EnableRespawn => enableRespawn;

        public float GlobalRespawnMultiplier => globalRespawnMultiplier;

        #endregion
    }
}
