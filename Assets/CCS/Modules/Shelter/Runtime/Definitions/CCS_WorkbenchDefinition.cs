using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorkbenchDefinition
// CATEGORY: Modules / Shelter / Runtime / Definitions
// PURPOSE: Frontier workbench identity and placement kit for homestead camp progression.
// PLACEMENT: Assets/CCS/Survival/Content/Structures/Frontier/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [CreateAssetMenu(
        fileName = "CCS_WorkbenchDefinition",
        menuName = "CCS/Survival/Shelter/Workbench Definition")]
    public sealed class CCS_WorkbenchDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string workbenchDefinitionId = "ccs.survival.workbench.frontier";
        [SerializeField] private string displayName = "Frontier Workbench";

        [Header("Placement")]
        [SerializeField] private CCS_ItemDefinition placeableKitItem;
        [SerializeField] private float placementForwardDistance = 2f;
        [SerializeField] private float placementMaxGroundRayDistance = 8f;
        [SerializeField] private float placementMaxSlopeAngle = 35f;

        [Header("Camp")]
        [SerializeField] private bool contributesToCampTier = true;
        [SerializeField] private CCS_CampStructureKind campStructureKind = CCS_CampStructureKind.WorkArea;

        [Header("Primitive Visual")]
        [SerializeField] private PrimitiveType placementPrimitive = PrimitiveType.Cube;
        [SerializeField] private Vector3 placedLocalScale = new Vector3(1.6f, 0.9f, 1.2f);

        public string WorkbenchDefinitionId => workbenchDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

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
