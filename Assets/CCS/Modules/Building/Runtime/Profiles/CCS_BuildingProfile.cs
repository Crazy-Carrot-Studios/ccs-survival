using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingProfile
// CATEGORY: Modules / Building / Runtime / Profiles
// PURPOSE: Tuning profile for building feature flags and startup definition registration.
// PLACEMENT: Assets/CCS/Survival/Profiles/Building/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Architecture only. Placement, demolition, and upgrades disabled in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    [CreateAssetMenu(
        fileName = "CCS_BuildingProfile",
        menuName = "CCS/Survival/Building/Building Profile")]
    public sealed class CCS_BuildingProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Feature Flags")]
        [Tooltip("When true, future milestones may enable structure placement.")]
        [SerializeField] private bool allowPlacement;

        [Tooltip("When true, future milestones may enable structure demolition.")]
        [SerializeField] private bool allowDemolition;

        [Tooltip("When true, future milestones may enable structure upgrades.")]
        [SerializeField] private bool allowUpgrades;

        [Header("Startup Definitions")]
        [Tooltip("Building piece definitions registered when the service initializes.")]
        [SerializeField] private List<CCS_BuildingPieceDefinition> startupDefinitions =
            new List<CCS_BuildingPieceDefinition>();

        #endregion

        #region Properties

        public bool AllowPlacement => allowPlacement;

        public bool AllowDemolition => allowDemolition;

        public bool AllowUpgrades => allowUpgrades;

        public IReadOnlyList<CCS_BuildingPieceDefinition> StartupDefinitions => startupDefinitions;

        #endregion
    }
}
