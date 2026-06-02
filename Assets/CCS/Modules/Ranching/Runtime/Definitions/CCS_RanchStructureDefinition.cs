using CCS.Modules.Inventory;
using CCS.Modules.Shelter;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RanchStructureDefinition
// CATEGORY: Modules / Ranching / Runtime / Definitions
// PURPOSE: Placeable ranch structure identity and role metadata.
// PLACEMENT: Assets/CCS/Survival/Content/Ranching/Structures/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    [CreateAssetMenu(
        fileName = "CCS_RanchStructureDefinition",
        menuName = "CCS/Survival/Ranching/Ranch Structure Definition")]
    public sealed class CCS_RanchStructureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string structureDefinitionId = string.Empty;
        [SerializeField] private string displayName = "Ranch Structure";
        [SerializeField] private CCS_RanchStructureKind structureKind = CCS_RanchStructureKind.ChickenCoop;

        [Header("Placement")]
        [SerializeField] private CCS_ItemDefinition placeableKitItem;
        [SerializeField] private float placementForwardDistance = 2f;
        [SerializeField] private float placementMaxGroundRayDistance = 8f;
        [SerializeField] private float placementMaxSlopeAngle = 35f;

        [Header("Camp")]
        [SerializeField] private bool contributesToCampTier;
        [SerializeField] private CCS_CampStructureKind campStructureKind = CCS_CampStructureKind.Livestock;

        [Header("Primitive Visual")]
        [SerializeField] private PrimitiveType placementPrimitive = PrimitiveType.Cube;
        [SerializeField] private Vector3 placedLocalScale = new Vector3(1.6f, 1f, 1.6f);

        public string StructureDefinitionId => structureDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_RanchStructureKind StructureKind => structureKind;

        public CCS_ItemDefinition PlaceableKitItem => placeableKitItem;

        public float PlacementForwardDistance => placementForwardDistance < 0.5f ? 0.5f : placementForwardDistance;

        public float PlacementMaxGroundRayDistance =>
            placementMaxGroundRayDistance < 1f ? 1f : placementMaxGroundRayDistance;

        public float PlacementMaxSlopeAngle => placementMaxSlopeAngle < 1f ? 1f : placementMaxSlopeAngle;

        public bool ContributesToCampTier => contributesToCampTier;

        public CCS_CampStructureKind CampStructureKind => campStructureKind;

        public PrimitiveType PlacementPrimitive => placementPrimitive;

        public Vector3 PlacedLocalScale => placedLocalScale;
    }
}
