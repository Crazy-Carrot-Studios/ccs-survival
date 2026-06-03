using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandClaimDefinition
// CATEGORY: Modules / Land / Runtime / Definitions
// PURPOSE: Placeable land claim type identity, radius rules, and deed item link.
// PLACEMENT: Assets/CCS/Survival/Content/Land/Claims/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — no taxes, banks, or deeds UI yet.
// =============================================================================

namespace CCS.Modules.Land
{
    [CreateAssetMenu(
        fileName = "CCS_LandClaimDefinition",
        menuName = "CCS/Survival/Land/Land Claim Definition")]
    public sealed class CCS_LandClaimDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string claimDefinitionId = string.Empty;
        [SerializeField] private string displayName = "Land Claim";

        [Header("Claim Rules")]
        [SerializeField] private float claimRadius = 12f;
        [SerializeField] private int maxStructuresPlaceholder = 32;
        [SerializeField] private string[] allowedStructureKinds = new[]
        {
            CCS_LandClaimStructureKind.Shelter,
            CCS_LandClaimStructureKind.Campfire,
            CCS_LandClaimStructureKind.Bedroll,
            CCS_LandClaimStructureKind.Storage,
            CCS_LandClaimStructureKind.Workbench,
            CCS_LandClaimStructureKind.IndustryStation,
            CCS_LandClaimStructureKind.RanchStructure,
            CCS_LandClaimStructureKind.FarmPlot
        };
        [SerializeField] private int registrationCost = 250;
        [SerializeField] private string optionalRegionId = string.Empty;

        [Header("Deed Item")]
        [SerializeField] private CCS_ItemDefinition claimDeedItem;

        [Header("Placement")]
        [SerializeField] private float placementForwardDistance = 2f;
        [SerializeField] private float placementMaxGroundRayDistance = 12f;
        [SerializeField] private float placementMaxSlopeAngle = 30f;
        [SerializeField] private float minimumClaimSeparation = 4f;

        [Header("Debug Visual")]
        [SerializeField] private Color previewColor = new Color(0.35f, 0.65f, 0.95f, 0.35f);
        [SerializeField] private Color placedColor = new Color(0.25f, 0.55f, 0.85f, 0.25f);

        public string ClaimDefinitionId => claimDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public float ClaimRadius => claimRadius < 2f ? 2f : claimRadius;

        public int MaxStructuresPlaceholder => maxStructuresPlaceholder < 1 ? 1 : maxStructuresPlaceholder;

        public string[] AllowedStructureKinds => allowedStructureKinds ?? System.Array.Empty<string>();

        public int RegistrationCost => registrationCost < 0 ? 0 : registrationCost;

        public string OptionalRegionId => optionalRegionId ?? string.Empty;

        public CCS_ItemDefinition ClaimDeedItem => claimDeedItem;

        public float PlacementForwardDistance =>
            placementForwardDistance < 0.5f ? 0.5f : placementForwardDistance;

        public float PlacementMaxGroundRayDistance =>
            placementMaxGroundRayDistance < 1f ? 1f : placementMaxGroundRayDistance;

        public float PlacementMaxSlopeAngle => placementMaxSlopeAngle < 1f ? 1f : placementMaxSlopeAngle;

        public float MinimumClaimSeparation => minimumClaimSeparation < 1f ? 1f : minimumClaimSeparation;

        public Color PreviewColor => previewColor;

        public Color PlacedColor => placedColor;

        public bool AllowsStructureKind(string structureKind)
        {
            if (string.IsNullOrWhiteSpace(structureKind))
            {
                return false;
            }

            string[] kinds = AllowedStructureKinds;
            for (int index = 0; index < kinds.Length; index++)
            {
                if (string.Equals(kinds[index], structureKind, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
