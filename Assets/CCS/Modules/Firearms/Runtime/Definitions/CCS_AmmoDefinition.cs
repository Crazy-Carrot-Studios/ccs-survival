using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AmmoDefinition
// CATEGORY: Modules / Firearms / Runtime / Definitions
// PURPOSE: Ammunition type definition linked to inventory items and compatible firearms.
// PLACEMENT: Assets/CCS/Survival/Content/Firearms/Ammo/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    [CreateAssetMenu(
        fileName = "CCS_AmmoDefinition",
        menuName = "CCS/Survival/Firearms/Ammo Definition")]
    public sealed class CCS_AmmoDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string ammoId = "ccs.survival.ammo.example";
        [SerializeField] private string displayName = "Ammo";
        [SerializeField] private string inventoryItemId = string.Empty;
        [SerializeField] private CCS_ItemDefinition inventoryItem;

        [Header("Consumption")]
        [SerializeField] private int roundsPerInventoryUnit = 1;

        public string AmmoId => ammoId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string InventoryItemId =>
            inventoryItem != null ? inventoryItem.ItemId : inventoryItemId ?? string.Empty;

        public CCS_ItemDefinition InventoryItem => inventoryItem;

        public int RoundsPerInventoryUnit => roundsPerInventoryUnit < 1 ? 1 : roundsPerInventoryUnit;
    }
}
