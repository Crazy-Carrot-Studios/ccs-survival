using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeProfile
// CATEGORY: Modules / Wildlife / Runtime / Profiles
// PURPOSE: Tuning profile for wildlife carcass harvesting foundation rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Wildlife/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No AI, combat, or spawning policy in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    [CreateAssetMenu(
        fileName = "CCS_WildlifeProfile",
        menuName = "CCS/Survival/Wildlife/Wildlife Profile")]
    public sealed class CCS_WildlifeProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Harvest")]
        [Tooltip("When enabled, carcass placeholders may be harvested through interaction.")]
        [SerializeField] private bool enableCarcassHarvesting = true;

        [Tooltip("Default harvest count applied when definitions omit a valid max harvest count.")]
        [SerializeField] private int defaultHarvestCount = 1;

        [Header("Future Systems")]
        [Tooltip("Placeholder for future wildlife respawn systems.")]
        [SerializeField] private bool enableRespawnPlaceholder;

        #endregion

        #region Properties

        public bool EnableCarcassHarvesting => enableCarcassHarvesting;

        public int DefaultHarvestCount => defaultHarvestCount;

        public bool EnableRespawnPlaceholder => enableRespawnPlaceholder;

        #endregion
    }
}
