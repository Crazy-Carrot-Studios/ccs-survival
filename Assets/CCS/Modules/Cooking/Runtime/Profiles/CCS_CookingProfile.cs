using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingProfile
// CATEGORY: Modules / Cooking / Runtime / Profiles
// PURPOSE: Tuning profile for campfire cooking and consumable food foundation rules.
// PLACEMENT: Assets/CCS/Survival/Profiles/Cooking/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No fuel systems, cooking UI, or health restore in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [CreateAssetMenu(
        fileName = "CCS_CookingProfile",
        menuName = "CCS/Survival/Cooking/Cooking Profile")]
    public sealed class CCS_CookingProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Cooking")]
        [Tooltip("When enabled, campfire interactables may cook food through interaction.")]
        [SerializeField] private bool enableCooking = true;

        [Tooltip("Default seconds required to cook one item when definitions omit a valid cook time.")]
        [SerializeField] private float defaultCookTimeSeconds = 5f;

        [Tooltip("When enabled, newly registered campfires start lit.")]
        [SerializeField] private bool autoLightCampfiresOnPlacement = true;

        [Header("Campfire")]
        [Tooltip("Default campfire definition used by bootstrap campfires and placed campfires.")]
        [SerializeField] private CCS_CampfireDefinition defaultCampfireDefinition;

        [Tooltip("Building piece placed when consuming a campfire kit.")]
        [SerializeField] private CCS_BuildingPieceDefinition campfireBuildingPiece;

        [Header("Cooking Items")]
        [Tooltip("Raw meat consumed when cooking on a lit campfire.")]
        [SerializeField] private CCS_ItemDefinition rawMeatItemDefinition;

        [Tooltip("Cooked meat granted when cooking completes.")]
        [SerializeField] private CCS_ItemDefinition cookedMeatItemDefinition;

        [Header("Consumable Food")]
        [Tooltip("Food items that restore hunger when consumed.")]
        [SerializeField] private List<CCS_ConsumableFoodDefinition> consumableFoodDefinitions =
            new List<CCS_ConsumableFoodDefinition>();

        #endregion

        #region Properties

        public bool EnableCooking => enableCooking;

        public float DefaultCookTimeSeconds => defaultCookTimeSeconds;

        public bool AutoLightCampfiresOnPlacement => autoLightCampfiresOnPlacement;

        public CCS_CampfireDefinition DefaultCampfireDefinition => defaultCampfireDefinition;

        public CCS_BuildingPieceDefinition CampfireBuildingPiece => campfireBuildingPiece;

        public CCS_ItemDefinition RawMeatItemDefinition => rawMeatItemDefinition;

        public CCS_ItemDefinition CookedMeatItemDefinition => cookedMeatItemDefinition;

        public IReadOnlyList<CCS_ConsumableFoodDefinition> ConsumableFoodDefinitions => consumableFoodDefinitions;

        #endregion
    }
}
