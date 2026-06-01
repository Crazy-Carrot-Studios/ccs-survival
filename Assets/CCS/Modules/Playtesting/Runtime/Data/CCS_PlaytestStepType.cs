// =============================================================================
// SCRIPT: CCS_PlaytestStepType
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Step archetypes for the bootstrap manual playtest checklist.
// PLACEMENT: Used by CCS_PlaytestStepDefinition and event auto-completion rules.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: One definition per core survival loop milestone verification area.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public enum CCS_PlaytestStepType
    {
        Spawn = 0,
        GatherResource = 1,
        EquipWeapon = 2,
        HuntWildlife = 3,
        HarvestCarcass = 4,
        CookFood = 5,
        EatFood = 6,
        PlaceBuilding = 7,
        SaveGame = 8,
        LoadGame = 9,
        TriggerDeath = 10,
        Respawn = 11,
        BuildShelter = 12,
        CraftAtWorkbench = 13,
        UseStorageCrate = 14,
        PlaceAndSleepAtBedroll = 15,
        VerifyControllerPolish = 16,
        ConfirmEquipmentVisual = 17
    }
}
