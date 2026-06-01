using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingSpotDefinition
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Scriptable definition for a fishable water-source interaction point.
// PLACEMENT: Referenced by CCS_FishingSpot on bootstrap and frontier content.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Uses HarvestMethodType.Fish and ResourceSourceType.Water metadata.
// =============================================================================

namespace CCS.Modules.Fishing
{
    [CreateAssetMenu(
        fileName = "CCS_FishingSpotDefinition",
        menuName = "CCS/Survival/Fishing/Fishing Spot Definition")]
    public sealed class CCS_FishingSpotDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Unique spot id for save and validation.")]
        [SerializeField] private string spotId;

        [Tooltip("Player-facing label for interaction prompts.")]
        [SerializeField] private string displayName = "Fishing Spot";

        [Tooltip("Frontier water body label (river edge, pond, lake, stream).")]
        [SerializeField] private CCS_FishingWaterBodyType waterBodyType = CCS_FishingWaterBodyType.Pond;

        [Header("Resource Metadata")]
        [SerializeField] private CCS_ResourceSourceType resourceSourceType = CCS_ResourceSourceType.Water;

        [SerializeField] private CCS_HarvestMethodType harvestMethod = CCS_HarvestMethodType.Fish;

        [Header("Tooling")]
        [SerializeField] private CCS_ItemToolType requiredToolType = CCS_ItemToolType.FishingPole;

        [Header("Bait")]
        [SerializeField] private CCS_FishingBaitRequirement baitRequirement = new CCS_FishingBaitRequirement();

        [Header("Catch Table")]
        [Tooltip("When empty, CCS_FishingProfile default catch table is used.")]
        [SerializeField] private CCS_FishingCatchDefinition[] catchTable;

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 4f;

        #endregion

        #region Properties

        public string SpotId => spotId;

        public string DisplayName => displayName;

        public CCS_FishingWaterBodyType WaterBodyType => waterBodyType;

        public CCS_ResourceSourceType ResourceSourceType => resourceSourceType;

        public CCS_HarvestMethodType HarvestMethod => harvestMethod;

        public CCS_ItemToolType RequiredToolType => requiredToolType;

        public CCS_FishingBaitRequirement BaitRequirement => baitRequirement;

        public CCS_FishingCatchDefinition[] CatchTable => catchTable;

        public float InteractionDistance => interactionDistance;

        public bool SupportsFishing =>
            resourceSourceType == CCS_ResourceSourceType.Water
            && harvestMethod == CCS_HarvestMethodType.Fish;

        #endregion
    }
}
