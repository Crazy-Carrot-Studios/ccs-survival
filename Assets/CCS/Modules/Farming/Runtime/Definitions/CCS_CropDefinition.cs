using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CropDefinition
// CATEGORY: Modules / Farming / Runtime / Definitions
// PURPOSE: Crop identity, seed/harvest items, and timer growth metadata.
// PLACEMENT: Assets/CCS/Survival/Content/Farming/Crops/
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — generic crop framework for future seasons/irrigation.
// =============================================================================

namespace CCS.Modules.Farming
{
    [CreateAssetMenu(
        fileName = "CCS_CropDefinition",
        menuName = "CCS/Survival/Farming/Crop Definition")]
    public sealed class CCS_CropDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string cropId = string.Empty;
        [SerializeField] private string displayName = "Crop";

        [Header("Items")]
        [SerializeField] private CCS_ItemDefinition seedItem;
        [SerializeField] private CCS_ItemDefinition harvestItem;

        [Header("Growth")]
        [SerializeField] private float growthDurationSeconds = 120f;
        [SerializeField] private int seedReturnQuantity = 1;

        [Header("Classification")]
        [SerializeField] private bool isFoodCrop = true;
        [SerializeField] private bool isFutureLivestockFeed;

        [Header("Primitive Visual")]
        [SerializeField] private GameObject cropVisualPrefab;
        [SerializeField] private PrimitiveType fallbackCropPrimitive = PrimitiveType.Cylinder;
        [SerializeField] private Color matureCropColor = new Color(0.45f, 0.75f, 0.25f);

        public string CropId => cropId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_ItemDefinition SeedItem => seedItem;

        public CCS_ItemDefinition HarvestItem => harvestItem;

        public float GrowthDurationSeconds => growthDurationSeconds < 1f ? 1f : growthDurationSeconds;

        public int SeedReturnQuantity => seedReturnQuantity < 0 ? 0 : seedReturnQuantity;

        public bool IsFoodCrop => isFoodCrop;

        public bool IsFutureLivestockFeed => isFutureLivestockFeed;

        public GameObject CropVisualPrefab => cropVisualPrefab;

        public PrimitiveType FallbackCropPrimitive => fallbackCropPrimitive;

        public Color MatureCropColor => matureCropColor;
    }
}
