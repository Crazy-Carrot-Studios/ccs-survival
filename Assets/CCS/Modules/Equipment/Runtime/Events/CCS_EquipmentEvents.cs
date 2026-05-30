// =============================================================================
// SCRIPT: CCS_EquipmentEvents
// CATEGORY: Modules / Equipment / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for equipment systems.
// PLACEMENT: Instance events on CCS_PlayerEquipmentService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Subscribers react to equipment data changes without UI or combat coupling.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentEvents
    {
        public const string ItemEquippedEventName = "Equipment.ItemEquipped";
        public const string ItemUnequippedEventName = "Equipment.ItemUnequipped";
        public const string EquipmentChangedEventName = "Equipment.Changed";
        public const string DurabilityChangedEventName = "Equipment.DurabilityChanged";
        public const string EquipmentFailedEventName = "Equipment.Failed";
    }

    public delegate void EquipmentItemEquippedHandler(CCS_EquipmentEventArgs eventArgs);

    public delegate void EquipmentItemUnequippedHandler(CCS_EquipmentEventArgs eventArgs);

    public delegate void EquipmentChangedHandler(CCS_EquipmentEventArgs eventArgs);

    public delegate void EquipmentDurabilityChangedHandler(CCS_EquipmentEventArgs eventArgs);

    public delegate void EquipmentFailedHandler(CCS_EquipmentEventArgs eventArgs);
}
