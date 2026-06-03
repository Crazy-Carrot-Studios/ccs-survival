using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmPlotDefinition
// CATEGORY: Modules / Farming / Runtime / Definitions
// PURPOSE: Placeable farm plot identity and placement metadata.
// PLACEMENT: Assets/CCS/Survival/Content/Farming/Structures/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Supports one planted crop per plot. Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    [CreateAssetMenu(
        fileName = "CCS_FarmPlotDefinition",
        menuName = "CCS/Survival/Farming/Farm Plot Definition")]
    public sealed class CCS_FarmPlotDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string plotDefinitionId = string.Empty;
        [SerializeField] private string displayName = "Farm Plot";

        [Header("Placement")]
        [SerializeField] private CCS_ItemDefinition placeableKitItem;
        [SerializeField] private float placementForwardDistance = 2f;
        [SerializeField] private float placementMaxGroundRayDistance = 8f;
        [SerializeField] private float placementMaxSlopeAngle = 35f;
        [SerializeField] private float interactionDistance = 3f;

        [Header("Camp")]
        [SerializeField] private bool contributesToCampTier;
        [SerializeField] private bool registersAgriculturePresence;

        [Header("Primitive Visual")]
        [SerializeField] private PrimitiveType placementPrimitive = PrimitiveType.Cube;
        [SerializeField] private Vector3 placedLocalScale = new Vector3(1.8f, 0.15f, 1.8f);
        [SerializeField] private Color plotColor = new Color(0.45f, 0.3f, 0.15f);

        public string PlotDefinitionId => plotDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ItemDefinition PlaceableKitItem => placeableKitItem;

        public float PlacementForwardDistance => placementForwardDistance < 0.5f ? 0.5f : placementForwardDistance;

        public float PlacementMaxGroundRayDistance =>
            placementMaxGroundRayDistance < 1f ? 1f : placementMaxGroundRayDistance;

        public float PlacementMaxSlopeAngle => placementMaxSlopeAngle < 1f ? 1f : placementMaxSlopeAngle;

        public float InteractionDistance => interactionDistance < 1f ? 1f : interactionDistance;

        public bool ContributesToCampTier => contributesToCampTier;

        public bool RegistersAgriculturePresence => registersAgriculturePresence;

        public PrimitiveType PlacementPrimitive => placementPrimitive;

        public Vector3 PlacedLocalScale => placedLocalScale;

        public Color PlotColor => plotColor;
    }
}
