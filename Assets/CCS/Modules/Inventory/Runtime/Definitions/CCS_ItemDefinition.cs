using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ItemDefinition
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: ScriptableObject identity and stacking rules for inventory items.
// PLACEMENT: Create assets under project content folders. Referenced by item stacks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No equipment stats, crafting recipes, or UI in 0.4.0 foundation.
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

        [Header("Tool Identity (Placeholder)")]
        [Tooltip("When enabled, this item satisfies matching harvest tool requirements from inventory.")]
        [SerializeField] private bool hasToolIdentity;

        [Tooltip("Harvest tool category satisfied when this item is present in inventory.")]
        [SerializeField] private CCS_ItemToolType toolType = CCS_ItemToolType.None;

        [Header("Presentation (Placeholder)")]
        [Tooltip("Optional icon sprite reference for future UI.")]
        [SerializeField] private Sprite icon;

        #endregion

        #region Properties

        public string ItemId => itemId;

        public string DisplayName => displayName;

        public string Description => description;

        public CCS_ItemCategory Category => category;

        public int MaxStackSize => maxStackSize;

        public bool IsStackable => isStackable;

        public float Weight => weight;

        public bool HasToolIdentity => hasToolIdentity;

        public CCS_ItemToolType ToolType => toolType;

        public Sprite Icon => icon;

        #endregion
    }
}
