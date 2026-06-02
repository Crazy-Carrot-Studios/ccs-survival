using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ItemDefinition
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: ScriptableObject identity and stacking rules for inventory items.
// PLACEMENT: Create assets under project content folders. Referenced by item stacks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Tool and weapon gameplay classifications added at 0.9.2 foundation.
// =============================================================================

namespace CCS.Modules.Inventory
{
    [CreateAssetMenu(
        fileName = "CCS_ItemDefinition",
        menuName = "CCS/Survival/Inventory/Item Definition")]
    public sealed class CCS_ItemDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS item ID for save and runtime identity.")]
        [SerializeField] private string itemId = string.Empty;

        [Tooltip("Player-facing item name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for future UI and tooltips.")]
        [SerializeField] private string description = string.Empty;

        [Tooltip("High-level item classification.")]
        [SerializeField] private CCS_ItemCategory category = CCS_ItemCategory.Generic;

        [Header("Stacking")]
        [Tooltip("Maximum quantity per inventory slot.")]
        [SerializeField] private int maxStackSize = 1;

        [Tooltip("When true, multiple units may occupy one slot up to max stack size.")]
        [SerializeField] private bool isStackable = true;

        [Header("Physical (Placeholder)")]
        [Tooltip("Item weight for future encumbrance systems.")]
        [SerializeField] private float weight = 0f;

        [Header("Gameplay Classification (0.9.2)")]
        [Tooltip("High-level gameplay role used by tool, weapon, and progression systems.")]
        [SerializeField] private CCS_ItemGameplayKind gameplayKind = CCS_ItemGameplayKind.Generic;

        [Header("Tool Identity")]
        [Tooltip("When enabled, this item satisfies matching harvest tool requirements.")]
        [SerializeField] private bool hasToolIdentity;

        [Tooltip("Harvest tool category satisfied when this item is present or equipped.")]
        [SerializeField] private CCS_ItemToolType toolType = CCS_ItemToolType.None;

        [Tooltip("Stable tool archetype for progression and crafting rules.")]
        [SerializeField] private CCS_ToolArchetype toolArchetype = CCS_ToolArchetype.None;

        [Tooltip("Technology tier used by future harvesting effectiveness rules.")]
        [SerializeField] private CCS_ToolTier toolTier = CCS_ToolTier.None;

        [Header("Weapon Identity (Placeholder)")]
        [Tooltip("When enabled, this item participates in future weapon/combat systems.")]
        [SerializeField] private bool hasWeaponIdentity;

        [Tooltip("Stable weapon archetype for future combat progression.")]
        [SerializeField] private CCS_WeaponArchetype weaponArchetype = CCS_WeaponArchetype.None;

        [Tooltip("High-level weapon behavior placeholder.")]
        [SerializeField] private CCS_WeaponType weaponType = CCS_WeaponType.None;

        [Tooltip("Damage category placeholder.")]
        [SerializeField] private CCS_DamageType damageType = CCS_DamageType.None;

        [Tooltip("Engagement range placeholder.")]
        [SerializeField] private CCS_RangeType rangeType = CCS_RangeType.None;

        [Header("Melee Combat (0.9.8)")]
        [Tooltip("Melee damage dealt per primary attack when this weapon is used.")]
        [SerializeField] private float meleeDamage;

        [Tooltip("Melee attack reach in meters from the camera origin.")]
        [SerializeField] private float meleeRange = 2f;

        [Header("Presentation (Placeholder)")]
        [Tooltip("Optional icon sprite reference for future UI.")]
        [SerializeField] private Sprite icon;

        [Header("Economy (Optional)")]
        [Tooltip("When enabled, buy/sell values and vendor category are used by economy systems.")]
        [SerializeField] private bool hasEconomyValues;

        [Tooltip("Default vendor purchase price per unit. Zero means not buyable unless vendor override.")]
        [SerializeField] private int buyValue;

        [Tooltip("Default vendor sell price per unit. Zero means not sellable unless vendor override.")]
        [SerializeField] private int sellValue;

        [Tooltip("Optional vendor grouping for catalogs and future UI.")]
        [SerializeField] private CCS_ItemVendorCategory vendorCategory = CCS_ItemVendorCategory.None;

        #endregion

        #region Properties

        public string ItemId => itemId;

        public string DisplayName => displayName;

        public string Description => description;

        public CCS_ItemCategory Category => category;

        public int MaxStackSize => maxStackSize;

        public bool IsStackable => isStackable;

        public float Weight => weight;

        public CCS_ItemGameplayKind GameplayKind => gameplayKind;

        public bool HasToolIdentity => hasToolIdentity;

        public CCS_ItemToolType ToolType => toolType;

        public CCS_ToolArchetype ToolArchetype => toolArchetype;

        public CCS_ToolTier ToolTier => toolTier;

        public bool HasWeaponIdentity => hasWeaponIdentity;

        public CCS_WeaponArchetype WeaponArchetype => weaponArchetype;

        public CCS_WeaponType WeaponType => weaponType;

        public CCS_DamageType DamageType => damageType;

        public CCS_RangeType RangeType => rangeType;

        public float MeleeDamage => meleeDamage;

        public float MeleeRange => meleeRange;

        public Sprite Icon => icon;

        public bool HasEconomyValues => hasEconomyValues;

        public int BuyValue => hasEconomyValues ? buyValue : 0;

        public int SellValue => hasEconomyValues ? sellValue : 0;

        public CCS_ItemVendorCategory VendorCategory => vendorCategory;

        #endregion
    }
}
