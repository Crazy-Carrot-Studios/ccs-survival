using System.Collections.Generic;
using CCS.Modules.Crafting;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPieceDefinition
// CATEGORY: Modules / Building / Runtime / Definitions
// PURPOSE: ScriptableObject identity and metadata for a buildable structure piece.
// PLACEMENT: Create assets under Assets/CCS/Survival/Content/Building/.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No placement logic. Prefab and crafting fields are placeholders for future milestones.
// =============================================================================

namespace CCS.Modules.Building
{
    [CreateAssetMenu(
        fileName = "CCS_BuildingPieceDefinition",
        menuName = "CCS/Survival/Building/Building Piece Definition")]
    public sealed class CCS_BuildingPieceDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS piece ID for save and runtime identity.")]
        [SerializeField] private string pieceId = string.Empty;

        [Tooltip("Player-facing piece name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for future UI and tooltips.")]
        [SerializeField] private string description = string.Empty;

        [Tooltip("Building piece category.")]
        [SerializeField] private CCS_BuildingPieceType buildingPieceType = CCS_BuildingPieceType.Custom;

        [Header("Future Prefab")]
        [Tooltip("Placeholder prefab reference for future placement spawning.")]
        [SerializeField] private GameObject prefabReference;

        [Header("Crafting Placeholder")]
        [Tooltip("Placeholder crafting requirements for future build mode.")]
        [SerializeField] private List<CCS_CraftingIngredientDefinition> craftingRequirements =
            new List<CCS_CraftingIngredientDefinition>();

        [Header("Shelter Placeholder")]
        [Tooltip("When true, placed pieces may contribute to shelter protection in a future milestone.")]
        [SerializeField] private bool contributesToShelter;

        [Tooltip("Placeholder wetness protection contribution.")]
        [SerializeField] private float shelterWetnessContribution;

        [Tooltip("Placeholder exposure protection contribution.")]
        [SerializeField] private float shelterExposureContribution;

        [Tooltip("Placeholder temperature protection contribution.")]
        [SerializeField] private float shelterTemperatureContribution;

        #endregion

        #region Properties

        public string PieceId => pieceId;

        public string DisplayName => displayName;

        public string Description => description;

        public CCS_BuildingPieceType BuildingPieceType => buildingPieceType;

        public GameObject PrefabReference => prefabReference;

        public IReadOnlyList<CCS_CraftingIngredientDefinition> CraftingRequirements => craftingRequirements;

        public bool ContributesToShelter => contributesToShelter;

        public float ShelterWetnessContribution => shelterWetnessContribution < 0f ? 0f : shelterWetnessContribution;

        public float ShelterExposureContribution => shelterExposureContribution < 0f ? 0f : shelterExposureContribution;

        public float ShelterTemperatureContribution => shelterTemperatureContribution;

        #endregion
    }
}
