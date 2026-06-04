using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionDefinition
// CATEGORY: Modules / Regions / Runtime / Definitions
// PURPOSE: ScriptableObject definition for a frontier world region.
// PLACEMENT: Assets/CCS/Survival/Content/Regions/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific region content lives under Survival assets.
// =============================================================================

namespace CCS.Modules.Regions
{
    [CreateAssetMenu(
        fileName = "CCS_RegionDefinition",
        menuName = "CCS/Survival/Regions/Region Definition")]
    public sealed class CCS_RegionDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS region id.")]
        [SerializeField] private string regionId = string.Empty;

        [Tooltip("Player-facing region name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for debug and future UI.")]
        [SerializeField] private string description = string.Empty;

        [Tooltip("Generic region archetype.")]
        [SerializeField] private CCS_RegionType regionType = CCS_RegionType.Other;

        [Header("World")]
        [Tooltip("Default world position used for discovery map placeholder.")]
        [SerializeField] private Vector3 defaultWorldPosition;

        [Header("Ownership / Metadata")]
        [Tooltip("Settlement ids owned by this region.")]
        [SerializeField] private string[] settlementIds = new string[0];

        [Tooltip("Resource metadata tags available in this region.")]
        [SerializeField] private string[] resourceMetadataTags = new string[0];

        [Header("Regional Economy")]
        [Tooltip("Primary economic identity for contracts, prosperity, and trade.")]
        [SerializeField] private CCS_RegionSpecializationType specializationType = CCS_RegionSpecializationType.Unknown;

        [Tooltip("Production and prosperity modifiers plus preferred contract categories.")]
        [SerializeField] private CCS_RegionProductionModifier productionModifier = new CCS_RegionProductionModifier();

        #endregion

        #region Properties

        public string RegionId => regionId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string Description => description ?? string.Empty;

        public CCS_RegionType RegionType => regionType;

        public Vector3 DefaultWorldPosition => defaultWorldPosition;

        public string[] SettlementIds => settlementIds ?? new string[0];

        public string[] ResourceMetadataTags => resourceMetadataTags ?? new string[0];

        public CCS_RegionSpecializationType SpecializationType => specializationType;

        public CCS_RegionProductionModifier ProductionModifier => productionModifier;

        #endregion
    }
}
