using System.Collections.Generic;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterDefinition
// CATEGORY: Modules / Shelter / Runtime / Definitions
// PURPOSE: Frontier shelter identity, costs, shelter effects, and placement item link.
// PLACEMENT: Assets/CCS/Survival/Content/Structures/Frontier/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    [CreateAssetMenu(
        fileName = "CCS_ShelterDefinition",
        menuName = "CCS/Survival/Shelter/Shelter Definition")]
    public sealed class CCS_ShelterDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [SerializeField] private string shelterDefinitionId = "ccs.survival.shelter.frontier.leanto";
        [SerializeField] private string displayName = "Lean-To";
        [SerializeField] private int shelterTier = 1;
        [SerializeField] private bool isFunctional = true;
        [SerializeField] private CCS_CampTier grantedCampTier = CCS_CampTier.TemporaryCamp;

        [Header("Placement")]
        [SerializeField] private CCS_ItemDefinition placeableKitItem;
        [SerializeField] private float placementForwardDistance = 2.5f;
        [SerializeField] private float placementMaxGroundRayDistance = 8f;
        [SerializeField] private float placementMaxSlopeAngle = 35f;
        [SerializeField] private float shelterCoverageRadius = 4f;

        [Header("Shelter Effects")]
        [SerializeField] private float warmthBonus = 0.35f;
        [SerializeField] private float sleepBonus = 0.25f;
        [SerializeField] private float weatherProtectionPercent = 0.55f;

        [Header("Protection Contributions")]
        [SerializeField] private float wetnessProtection = 0.7f;
        [SerializeField] private float exposureProtection = 0.55f;
        [SerializeField] private float temperatureProtection = 0.4f;

        [Header("Craft Costs")]
        [SerializeField] private List<CCS_ShelterResourceCost> craftCosts = new List<CCS_ShelterResourceCost>();

        [Header("Primitive Visual")]
        [SerializeField] private PrimitiveType placementPrimitive = PrimitiveType.Cube;
        [SerializeField] private Vector3 placedLocalScale = new Vector3(2.2f, 1.4f, 1.6f);

        [Header("Future Hooks")]
        [SerializeField] private bool enablesCampOwnershipMarker = true;
        [SerializeField] private bool supportsFutureRespawnHook = true;

        #endregion

        #region Properties

        public string ShelterDefinitionId => shelterDefinitionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int ShelterTier => shelterTier < 1 ? 1 : shelterTier;

        public bool IsFunctional => isFunctional;

        public CCS_CampTier GrantedCampTier => grantedCampTier;

        public CCS_ItemDefinition PlaceableKitItem => placeableKitItem;

        public float PlacementForwardDistance => placementForwardDistance < 0.5f ? 0.5f : placementForwardDistance;

        public float PlacementMaxGroundRayDistance =>
            placementMaxGroundRayDistance < 1f ? 1f : placementMaxGroundRayDistance;

        public float PlacementMaxSlopeAngle => placementMaxSlopeAngle < 1f ? 1f : placementMaxSlopeAngle;

        public float ShelterCoverageRadius => shelterCoverageRadius < 1f ? 1f : shelterCoverageRadius;

        public float WarmthBonus => Mathf.Clamp01(warmthBonus);

        public float SleepBonus => Mathf.Clamp01(sleepBonus);

        public float WeatherProtectionPercent => Mathf.Clamp01(weatherProtectionPercent);

        public float WetnessProtection => Mathf.Clamp01(wetnessProtection);

        public float ExposureProtection => Mathf.Clamp01(exposureProtection);

        public float TemperatureProtection => temperatureProtection < 0f ? 0f : temperatureProtection;

        public IReadOnlyList<CCS_ShelterResourceCost> CraftCosts => craftCosts;

        public PrimitiveType PlacementPrimitive => placementPrimitive;

        public Vector3 PlacedLocalScale => placedLocalScale;

        public bool EnablesCampOwnershipMarker => enablesCampOwnershipMarker;

        public bool SupportsFutureRespawnHook => supportsFutureRespawnHook;

        #endregion
    }
}
