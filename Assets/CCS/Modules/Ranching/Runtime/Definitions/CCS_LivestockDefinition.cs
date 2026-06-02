using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LivestockDefinition
// CATEGORY: Modules / Ranching / Runtime / Definitions
// PURPOSE: Profile-driven livestock production and purchase metadata.
// PLACEMENT: Assets/CCS/Survival/Content/Ranching/Livestock/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    [CreateAssetMenu(
        fileName = "CCS_LivestockDefinition",
        menuName = "CCS/Survival/Ranching/Livestock Definition")]
    public sealed class CCS_LivestockDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string livestockId = string.Empty;
        [SerializeField] private string displayName = "Livestock";
        [SerializeField] private CCS_LivestockType livestockType = CCS_LivestockType.Chicken;

        [Header("Purchase")]
        [SerializeField] private CCS_ItemDefinition purchaseItem;
        [SerializeField] private GameObject worldPrefab;

        [Header("Production")]
        [SerializeField] private float productionIntervalSeconds = 60f;
        [SerializeField] private bool requiresFeed = true;
        [SerializeField] private bool requiresWater = true;
        [SerializeField] private string productionItemId = string.Empty;
        [SerializeField] private CCS_ItemDefinition productionItem;
        [SerializeField] private int productionQuantity = 1;

        [Header("Structure Requirements")]
        [SerializeField] private CCS_RanchStructureKind requiredStructureKind = CCS_RanchStructureKind.ChickenCoop;
        [SerializeField] private float structureProximityRadius = 8f;
        [SerializeField] private float supportStructureProximityRadius = 6f;

        public string LivestockId => livestockId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public CCS_LivestockType LivestockType => livestockType;

        public CCS_ItemDefinition PurchaseItem => purchaseItem;

        public GameObject WorldPrefab => worldPrefab;

        public float ProductionIntervalSeconds => productionIntervalSeconds < 1f ? 1f : productionIntervalSeconds;

        public bool RequiresFeed => requiresFeed;

        public bool RequiresWater => requiresWater;

        public string ProductionItemId =>
            productionItem != null ? productionItem.ItemId : productionItemId ?? string.Empty;

        public CCS_ItemDefinition ProductionItem => productionItem;

        public int ProductionQuantity => productionQuantity < 1 ? 1 : productionQuantity;

        public CCS_RanchStructureKind RequiredStructureKind => requiredStructureKind;

        public float StructureProximityRadius => structureProximityRadius < 1f ? 1f : structureProximityRadius;

        public float SupportStructureProximityRadius =>
            supportStructureProximityRadius < 1f ? 1f : supportStructureProximityRadius;
    }
}
