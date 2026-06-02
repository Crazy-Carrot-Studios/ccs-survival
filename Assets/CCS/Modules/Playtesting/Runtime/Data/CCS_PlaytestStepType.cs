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
        ConfirmEquipmentVisual = 17,
        SelectActiveItem = 18,
        UseActiveItem = 19,
        UseHatchetOnTree = 20,
        UsePickOnRock = 21,
        UseWrongToolOnGatherTarget = 22,
        EquipFishingPole = 23,
        UseFishingPoleOnSpot = 24,
        ValidateFrontierRecipe = 25,
        EquipSpearRegression = 26,
        ObtainFishForTrade = 27,
        SellFishAtVendor = 28,
        VerifyCurrencyIncreased = 29,
        BuyItemFromVendor = 30,
        VerifyCurrencyDecreased = 31,
        VerifyVendorInventoryUpdated = 32,
        ObtainBowForHunt = 33,
        EquipBowForHunt = 34,
        SellHuntingResourceAtVendor = 35,
        VerifyHuntingCurrencyIncreased = 36,
        ObtainTrapForTrapping = 37,
        EquipTrapForTrapping = 38,
        PlaceTrapForTrapping = 39,
        ForceTrapTrigger = 40,
        HarvestTriggeredTrap = 41,
        VerifyTrapHarvestInventory = 42,
        SellTrappingResourceAtVendor = 43,
        VerifyTrappingCurrencyIncreased = 44,
        ObtainRawFoodForCooking = 45,
        VerifyCookedFoodInInventory = 46,
        PreserveFoodAtCampfire = 47,
        SellPreservedFoodAtVendor = 48,
        VerifyCookingCurrencyIncreased = 49,
        BuyHatchetForShelter = 50,
        GatherWoodForShelter = 51,
        AcquireCordageForShelter = 52,
        CraftLeanToShelter = 53,
        PlaceLeanToShelter = 54,
        PlaceCampfireForCamp = 55,
        PlaceBedrollForCamp = 56,
        VerifyTemporaryCampTier = 57,
        SleepInFrontierCamp = 58,
        VerifyCampPersistenceAfterLoad = 59
    }
}
