using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRecipePlacementRules
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Placement restrictions for a building recipe category.
// PLACEMENT: Embedded on CCS_BuildingRecipe.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.0 primitive shelter placement rules.
// =============================================================================

namespace CCS.Modules.Building
{
    [Serializable]
    public sealed class CCS_BuildingRecipePlacementRules
    {
        [Header("Placement Mode")]
        [Tooltip("When true, the piece may be placed without snapping.")]
        [SerializeField] private bool allowsFreePlacement;

        [Tooltip("When true, placement requires a valid compatible snap match.")]
        [SerializeField] private bool requiresSnapPoint;

        [Header("Progression Requirements")]
        [Tooltip("When true, a foundation piece must exist within search radius.")]
        [SerializeField] private bool requiresFoundationNearby;

        [Tooltip("When true, the active snap target must be a wall or doorway piece.")]
        [SerializeField] private bool requiresWallOrDoorwaySupport;

        [Tooltip("World-space radius used when searching for nearby foundation pieces.")]
        [SerializeField] private float foundationSearchRadius = 12f;

        #region Properties

        public bool AllowsFreePlacement => allowsFreePlacement;

        public bool RequiresSnapPoint => requiresSnapPoint;

        public bool RequiresFoundationNearby => requiresFoundationNearby;

        public bool RequiresWallOrDoorwaySupport => requiresWallOrDoorwaySupport;

        public float FoundationSearchRadius => foundationSearchRadius < 1f ? 12f : foundationSearchRadius;

        #endregion
    }
}
