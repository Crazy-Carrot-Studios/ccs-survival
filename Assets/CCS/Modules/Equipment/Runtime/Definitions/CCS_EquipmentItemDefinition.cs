using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentItemDefinition
// CATEGORY: Modules / Equipment / Runtime / Definitions
// PURPOSE: Equipment-specific extension data referencing inventory item definitions.
// PLACEMENT: Create assets under project content folders. Referenced by equipped items.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: References CCS_ItemDefinition. No combat stats or visuals in 0.4.1 foundation.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [CreateAssetMenu(
        fileName = "CCS_EquipmentItemDefinition",
        menuName = "CCS/Survival/Equipment/Equipment Item Definition")]
    public sealed class CCS_EquipmentItemDefinition : ScriptableObject
    {
        #region Variables

        [Header("Inventory Link")]
        [Tooltip("Base inventory item identity referenced by this equipment entry.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Header("Slot Rules")]
        [Tooltip("Equipment slot this item may occupy when equipped.")]
        [SerializeField] private CCS_EquipmentSlotType allowedSlot = CCS_EquipmentSlotType.Head;

        [Header("Durability")]
        [Tooltip("When enabled, equipped instances track durability state.")]
        [SerializeField] private bool durabilityEnabled;

        [Tooltip("Maximum durability when durability is enabled.")]
        [SerializeField] private float maxDurability = 100f;

        [Header("Stats (Placeholder)")]
        [Tooltip("Future stat modifier key or serialized placeholder for authoring tools.")]
        [SerializeField] private string statModifierPlaceholder = string.Empty;

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public CCS_EquipmentSlotType AllowedSlot => allowedSlot;

        public bool DurabilityEnabled => durabilityEnabled;

        public float MaxDurability => maxDurability;

        public string StatModifierPlaceholder => statModifierPlaceholder;

        #endregion
    }
}
