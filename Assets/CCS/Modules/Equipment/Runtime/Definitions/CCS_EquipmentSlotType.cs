// =============================================================================
// SCRIPT: CCS_EquipmentSlotType
// CATEGORY: Modules / Equipment / Runtime / Definitions
// PURPOSE: Supported equipment slot identifiers for player loadout architecture.
// PLACEMENT: Referenced by equipment definitions, slots, and player equipment service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Visual attachment and combat slot rules deferred to future milestones.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public enum CCS_EquipmentSlotType
    {
        Head = 0,
        Chest = 1,
        Legs = 2,
        Feet = 3,
        Hands = 4,
        Back = 5,
        Neck = 6,
        Accessory = 7,
        MainHand = 8,
        OffHand = 9,
        Sidearm = 10,
        Tool = 11
    }
}
