using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirearmDefinition
// CATEGORY: Modules / Firearms / Runtime / Definitions
// PURPOSE: Generic firearm definition for revolvers, rifles, shotguns, and future repeaters.
// PLACEMENT: Assets/CCS/Survival/Content/Firearms/
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    [CreateAssetMenu(
        fileName = "CCS_FirearmDefinition",
        menuName = "CCS/Survival/Firearms/Firearm Definition")]
    public sealed class CCS_FirearmDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string firearmId = "ccs.survival.firearm.example";
        [SerializeField] private string displayName = "Firearm";
        [SerializeField] private string inventoryItemId = string.Empty;
        [SerializeField] private CCS_WeaponArchetype weaponArchetype = CCS_WeaponArchetype.Revolver;

        [Header("Ammo")]
        [SerializeField] private CCS_AmmoDefinition ammoDefinition;
        [SerializeField] private int magazineCapacity = 6;

        [Header("Combat Placeholder")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 35f;

        [Header("World Visual Placeholder")]
        [SerializeField] private GameObject worldPrefab;

        public string FirearmId => firearmId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string InventoryItemId => inventoryItemId ?? string.Empty;

        public CCS_WeaponArchetype WeaponArchetype => weaponArchetype;

        public CCS_AmmoDefinition AmmoDefinition => ammoDefinition;

        public string AmmoItemId => ammoDefinition != null ? ammoDefinition.InventoryItemId : string.Empty;

        public int MagazineCapacity => magazineCapacity < 1 ? 1 : magazineCapacity;

        public float Damage => damage < 0f ? 0f : damage;

        public float Range => range < 0f ? 0f : range;

        public GameObject WorldPrefab => worldPrefab;
    }
}
