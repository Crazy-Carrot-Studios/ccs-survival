using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingProgressionProfile
// CATEGORY: Modules / Building / Runtime / Profiles
// PURPOSE: Tier-1 primitive building progression recipes and enabled piece catalog.
// PLACEMENT: Assets/CCS/Survival/Profiles/Building/CCS_DefaultBuildingProgressionProfile.asset
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.0 building progression foundation.
// =============================================================================

namespace CCS.Modules.Building
{
    [CreateAssetMenu(
        fileName = "CCS_BuildingProgressionProfile",
        menuName = "CCS/Survival/Building/Building Progression Profile")]
    public sealed class CCS_BuildingProgressionProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Feature")]
        [Tooltip("When false, recipe authorization is skipped and legacy placement costs apply.")]
        [SerializeField] private bool progressionEnabled = true;

        [Tooltip("Emit categorized building progression debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Enabled Pieces")]
        [Tooltip("Primitive tier-1 building definitions available for placement.")]
        [SerializeField] private List<CCS_BuildingPieceDefinition> enabledPieceDefinitions =
            new List<CCS_BuildingPieceDefinition>();

        [Header("Recipes")]
        [Tooltip("Authoritative recipe table for primitive building placement.")]
        [SerializeField] private List<CCS_BuildingRecipe> recipeDefinitions = new List<CCS_BuildingRecipe>();

        [Header("Shelter Minimum")]
        [Tooltip("Minimum foundation pieces required for shelter playtest completion.")]
        [SerializeField] private int minimumFoundationCount = 1;

        [Tooltip("Minimum wall pieces required for shelter playtest completion.")]
        [SerializeField] private int minimumWallCount = 1;

        [Tooltip("Minimum roof pieces required for shelter playtest completion.")]
        [SerializeField] private int minimumRoofCount = 1;

        #endregion

        #region Properties

        public bool ProgressionEnabled => progressionEnabled;

        public bool EnableDebugLogging => enableDebugLogging;

        public IReadOnlyList<CCS_BuildingPieceDefinition> EnabledPieceDefinitions => enabledPieceDefinitions;

        public IReadOnlyList<CCS_BuildingRecipe> RecipeDefinitions => recipeDefinitions;

        public int MinimumFoundationCount => minimumFoundationCount < 1 ? 1 : minimumFoundationCount;

        public int MinimumWallCount => minimumWallCount < 1 ? 1 : minimumWallCount;

        public int MinimumRoofCount => minimumRoofCount < 1 ? 1 : minimumRoofCount;

        #endregion
    }
}
