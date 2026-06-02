// =============================================================================
// SCRIPT: CCS_WeaponArchetype
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Stable weapon archetype identity for future combat systems.
// PLACEMENT: Referenced by CCS_ItemDefinition weapon metadata.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Bow and Club reserved for future milestones without combat in 0.9.2.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_WeaponArchetype
    {
        None = 0,
        Knife = 1,
        Spear = 2,
        Bow = 3,
        Club = 4,
        Revolver = 5,
        Rifle = 6,
        Shotgun = 7
    }
}
