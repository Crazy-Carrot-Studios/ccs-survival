using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TrapDefinition
// CATEGORY: Modules / Trapping / Runtime / Definitions
// PURPOSE: ScriptableObject defining a placeable trap and capture/harvest tuning.
// PLACEMENT: Assets/CCS/Survival/Content/Trapping/ (frontier content shell).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    [CreateAssetMenu(
        fileName = "CCS_TrapDefinition",
        menuName = "CCS/Survival/Trapping/Trap Definition")]
    public sealed class CCS_TrapDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [SerializeField] private string trapDefinitionId = "ccs.survival.trap.frontier.simple";
        [SerializeField] private string displayName = "Simple Trap";

        [Header("Placement Item")]
        [Tooltip("Inventory item consumed when this trap is placed.")]
        [SerializeField] private CCS_ItemDefinition placeableItem;

        [Header("Capture Targets")]
        [Tooltip("Wildlife definition used for harvest drops after a successful capture.")]
        [SerializeField] private CCS_WildlifeDefinition capturedWildlifeDefinition;

        [Tooltip("Passive AI species this trap can capture. Deer is excluded for frontier foundation.")]
        [SerializeField] private List<CCS_WildlifeAiSpecies> allowedSpecies = new List<CCS_WildlifeAiSpecies>
        {
            CCS_WildlifeAiSpecies.Rabbit,
            CCS_WildlifeAiSpecies.Turkey
        };

        [Header("Timer Capture")]
        [SerializeField] private float triggerDelaySeconds = 8f;
        [SerializeField] [Range(0f, 1f)] private float captureChance = 0.65f;
        [SerializeField] [Range(0f, 1f)] private float breakChance = 0.05f;
        [SerializeField] private float captureRadius = 6f;

        [Header("Placement")]
        [SerializeField] private float placementForwardDistance = 2f;
        [SerializeField] private float placementMaxGroundRayDistance = 6f;
        [SerializeField] private float placementMaxSlopeAngle = 35f;

        [Header("Primitive Visual")]
        [SerializeField] private PrimitiveType placementPrimitive = PrimitiveType.Cube;
        [SerializeField] private Vector3 placedLocalScale = new Vector3(0.45f, 0.2f, 0.45f);

        #endregion

        #region Properties

        public string TrapDefinitionId => trapDefinitionId;

        public string DisplayName => displayName;

        public CCS_ItemDefinition PlaceableItem => placeableItem;

        public CCS_WildlifeDefinition CapturedWildlifeDefinition => capturedWildlifeDefinition;

        public IReadOnlyList<CCS_WildlifeAiSpecies> AllowedSpecies => allowedSpecies;

        public float TriggerDelaySeconds => triggerDelaySeconds < 0f ? 0f : triggerDelaySeconds;

        public float CaptureChance => Mathf.Clamp01(captureChance);

        public float BreakChance => Mathf.Clamp01(breakChance);

        public float CaptureRadius => captureRadius < 0f ? 0f : captureRadius;

        public float PlacementForwardDistance => placementForwardDistance < 0.5f ? 0.5f : placementForwardDistance;

        public float PlacementMaxGroundRayDistance =>
            placementMaxGroundRayDistance < 1f ? 1f : placementMaxGroundRayDistance;

        public float PlacementMaxSlopeAngle => placementMaxSlopeAngle < 1f ? 1f : placementMaxSlopeAngle;

        public PrimitiveType PlacementPrimitive => placementPrimitive;

        public Vector3 PlacedLocalScale => placedLocalScale;

        #endregion

        #region Public Methods

        public bool AllowsSpecies(CCS_WildlifeAiSpecies species)
        {
            if (allowedSpecies == null || allowedSpecies.Count == 0)
            {
                return species == CCS_WildlifeAiSpecies.Rabbit;
            }

            return allowedSpecies.Contains(species);
        }

        #endregion
    }
}
