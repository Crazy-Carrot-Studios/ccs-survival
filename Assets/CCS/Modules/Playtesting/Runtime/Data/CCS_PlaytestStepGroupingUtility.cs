using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_PlaytestStepGroupingUtility
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Maps playtest step types to HUD checklist groups and display order.
// PLACEMENT: Shared utility for CCS_PlaytestHud and editor validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Every CCS_PlaytestStepType must resolve to a defined group for validation.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public static class CCS_PlaytestStepGroupingUtility
    {
        private static readonly CCS_PlaytestStepGroup[] OrderedGroups =
        {
            CCS_PlaytestStepGroup.CoreSpawnMovement,
            CCS_PlaytestStepGroup.InventoryEquipment,
            CCS_PlaytestStepGroup.GatheringCrafting,
            CCS_PlaytestStepGroup.Fishing,
            CCS_PlaytestStepGroup.Economy,
            CCS_PlaytestStepGroup.Hunting,
            CCS_PlaytestStepGroup.Trapping,
            CCS_PlaytestStepGroup.Cooking,
            CCS_PlaytestStepGroup.ShelterHomestead,
            CCS_PlaytestStepGroup.Industry,
            CCS_PlaytestStepGroup.HorseWagon,
            CCS_PlaytestStepGroup.Firearms,
            CCS_PlaytestStepGroup.Prospecting,
            CCS_PlaytestStepGroup.Settlement
        };

        public static IReadOnlyList<CCS_PlaytestStepGroup> GetOrderedGroups()
        {
            return OrderedGroups;
        }

        public static string GetDisplayName(CCS_PlaytestStepGroup group)
        {
            switch (group)
            {
                case CCS_PlaytestStepGroup.CoreSpawnMovement:
                    return "Core Spawn / Movement";
                case CCS_PlaytestStepGroup.InventoryEquipment:
                    return "Inventory / Equipment";
                case CCS_PlaytestStepGroup.GatheringCrafting:
                    return "Gathering / Crafting";
                case CCS_PlaytestStepGroup.Fishing:
                    return "Fishing";
                case CCS_PlaytestStepGroup.Economy:
                    return "Economy";
                case CCS_PlaytestStepGroup.Hunting:
                    return "Hunting";
                case CCS_PlaytestStepGroup.Trapping:
                    return "Trapping";
                case CCS_PlaytestStepGroup.Cooking:
                    return "Cooking";
                case CCS_PlaytestStepGroup.ShelterHomestead:
                    return "Shelter / Homestead";
                case CCS_PlaytestStepGroup.Industry:
                    return "Industry";
                case CCS_PlaytestStepGroup.HorseWagon:
                    return "Horse / Wagon";
                case CCS_PlaytestStepGroup.Firearms:
                    return "Firearms";
                case CCS_PlaytestStepGroup.Prospecting:
                    return "Prospecting";
                case CCS_PlaytestStepGroup.Settlement:
                    return "Settlement / Services";
                default:
                    return group.ToString();
            }
        }

        public static bool TryGetGroup(CCS_PlaytestStepType stepType, out CCS_PlaytestStepGroup group)
        {
            group = ResolveGroup(stepType);
            return true;
        }

        public static CCS_PlaytestStepGroup ResolveGroup(CCS_PlaytestStepType stepType)
        {
            switch (stepType)
            {
                case CCS_PlaytestStepType.Spawn:
                case CCS_PlaytestStepType.VerifyControllerPolish:
                case CCS_PlaytestStepType.SaveGame:
                case CCS_PlaytestStepType.LoadGame:
                case CCS_PlaytestStepType.TriggerDeath:
                case CCS_PlaytestStepType.Respawn:
                    return CCS_PlaytestStepGroup.CoreSpawnMovement;

                case CCS_PlaytestStepType.EquipWeapon:
                case CCS_PlaytestStepType.ConfirmEquipmentVisual:
                case CCS_PlaytestStepType.SelectActiveItem:
                case CCS_PlaytestStepType.UseActiveItem:
                case CCS_PlaytestStepType.EquipSpearRegression:
                case CCS_PlaytestStepType.UpgradeToIronTool:
                    return CCS_PlaytestStepGroup.InventoryEquipment;

                case CCS_PlaytestStepType.GatherResource:
                case CCS_PlaytestStepType.UseHatchetOnTree:
                case CCS_PlaytestStepType.UsePickOnRock:
                case CCS_PlaytestStepType.UseWrongToolOnGatherTarget:
                case CCS_PlaytestStepType.CraftAtWorkbench:
                case CCS_PlaytestStepType.ValidateFrontierRecipe:
                    return CCS_PlaytestStepGroup.GatheringCrafting;

                case CCS_PlaytestStepType.EquipFishingPole:
                case CCS_PlaytestStepType.UseFishingPoleOnSpot:
                case CCS_PlaytestStepType.ObtainFishForTrade:
                    return CCS_PlaytestStepGroup.Fishing;

                case CCS_PlaytestStepType.SellFishAtVendor:
                case CCS_PlaytestStepType.VerifyCurrencyIncreased:
                case CCS_PlaytestStepType.BuyItemFromVendor:
                case CCS_PlaytestStepType.VerifyCurrencyDecreased:
                case CCS_PlaytestStepType.VerifyVendorInventoryUpdated:
                case CCS_PlaytestStepType.SellHuntingResourceAtVendor:
                case CCS_PlaytestStepType.VerifyHuntingCurrencyIncreased:
                case CCS_PlaytestStepType.SellTrappingResourceAtVendor:
                case CCS_PlaytestStepType.VerifyTrappingCurrencyIncreased:
                case CCS_PlaytestStepType.SellPreservedFoodAtVendor:
                case CCS_PlaytestStepType.VerifyCookingCurrencyIncreased:
                case CCS_PlaytestStepType.BuyHatchetForShelter:
                case CCS_PlaytestStepType.EarnCurrencyForHorse:
                case CCS_PlaytestStepType.EarnCurrencyForWagon:
                case CCS_PlaytestStepType.EarnCurrencyForFirearm:
                case CCS_PlaytestStepType.SellHuntingResourcesAfterFirearm:
                case CCS_PlaytestStepType.SellMiningGoods:
                case CCS_PlaytestStepType.VerifyMiningCurrencyIncreased:
                    return CCS_PlaytestStepGroup.Economy;

                case CCS_PlaytestStepType.HuntWildlife:
                case CCS_PlaytestStepType.HarvestCarcass:
                case CCS_PlaytestStepType.ObtainBowForHunt:
                case CCS_PlaytestStepType.EquipBowForHunt:
                    return CCS_PlaytestStepGroup.Hunting;

                case CCS_PlaytestStepType.ObtainTrapForTrapping:
                case CCS_PlaytestStepType.EquipTrapForTrapping:
                case CCS_PlaytestStepType.PlaceTrapForTrapping:
                case CCS_PlaytestStepType.ForceTrapTrigger:
                case CCS_PlaytestStepType.HarvestTriggeredTrap:
                case CCS_PlaytestStepType.VerifyTrapHarvestInventory:
                    return CCS_PlaytestStepGroup.Trapping;

                case CCS_PlaytestStepType.CookFood:
                case CCS_PlaytestStepType.EatFood:
                case CCS_PlaytestStepType.ObtainRawFoodForCooking:
                case CCS_PlaytestStepType.VerifyCookedFoodInInventory:
                case CCS_PlaytestStepType.PreserveFoodAtCampfire:
                    return CCS_PlaytestStepGroup.Cooking;

                case CCS_PlaytestStepType.PlaceBuilding:
                case CCS_PlaytestStepType.BuildShelter:
                case CCS_PlaytestStepType.UseStorageCrate:
                case CCS_PlaytestStepType.PlaceAndSleepAtBedroll:
                case CCS_PlaytestStepType.GatherWoodForShelter:
                case CCS_PlaytestStepType.AcquireCordageForShelter:
                case CCS_PlaytestStepType.CraftLeanToShelter:
                case CCS_PlaytestStepType.PlaceLeanToShelter:
                case CCS_PlaytestStepType.PlaceCampfireForCamp:
                case CCS_PlaytestStepType.PlaceBedrollForCamp:
                case CCS_PlaytestStepType.VerifyTemporaryCampTier:
                case CCS_PlaytestStepType.SleepInFrontierCamp:
                case CCS_PlaytestStepType.VerifyCampPersistenceAfterLoad:
                case CCS_PlaytestStepType.BuySupplyCrateKitForHomestead:
                case CCS_PlaytestStepType.PlaceSupplyCrateForFrontierCamp:
                case CCS_PlaytestStepType.VerifyFrontierCampTier:
                case CCS_PlaytestStepType.BuyWorkbenchKitForHomestead:
                case CCS_PlaytestStepType.PlaceWorkbenchForHomestead:
                case CCS_PlaytestStepType.VerifyFrontierHomesteadTier:
                case CCS_PlaytestStepType.SaveHomesteadCampState:
                case CCS_PlaytestStepType.VerifyHomesteadCampPersistenceAfterLoad:
                case CCS_PlaytestStepType.VerifyIndustrialHomesteadTier:
                case CCS_PlaytestStepType.SaveIndustryCampState:
                case CCS_PlaytestStepType.VerifyIndustryCampPersistenceAfterLoad:
                    return CCS_PlaytestStepGroup.ShelterHomestead;

                case CCS_PlaytestStepType.GatherWoodForIndustry:
                case CCS_PlaytestStepType.ProduceLumberAtSawTable:
                case CCS_PlaytestStepType.ProduceCharcoalAtKiln:
                case CCS_PlaytestStepType.RefineIronAtPrimitiveForge:
                case CCS_PlaytestStepType.CraftIronHatchetHeadAtForge:
                    return CCS_PlaytestStepGroup.Industry;

                case CCS_PlaytestStepType.BuyHorseFromStable:
                case CCS_PlaytestStepType.SummonHorse:
                case CCS_PlaytestStepType.MountHorse:
                case CCS_PlaytestStepType.RideHorse:
                case CCS_PlaytestStepType.OpenHorseSaddlebag:
                case CCS_PlaytestStepType.SaveHorseState:
                case CCS_PlaytestStepType.VerifyHorsePersistenceAfterLoad:
                case CCS_PlaytestStepType.BuyWagonFromStable:
                case CCS_PlaytestStepType.SummonWagon:
                case CCS_PlaytestStepType.HitchWagonToHorse:
                case CCS_PlaytestStepType.RideHorseWithWagon:
                case CCS_PlaytestStepType.OpenWagonCargo:
                case CCS_PlaytestStepType.SaveWagonState:
                case CCS_PlaytestStepType.VerifyWagonPersistenceAfterLoad:
                case CCS_PlaytestStepType.LoadMiningGoodsIntoWagonCargo:
                    return CCS_PlaytestStepGroup.HorseWagon;

                case CCS_PlaytestStepType.BuyRevolverFromGunsmith:
                case CCS_PlaytestStepType.BuyFirearmAmmo:
                case CCS_PlaytestStepType.EquipFirearm:
                case CCS_PlaytestStepType.ReloadFirearm:
                case CCS_PlaytestStepType.ShootWildlifeWithFirearm:
                case CCS_PlaytestStepType.HarvestWithKnifeAfterFirearm:
                case CCS_PlaytestStepType.SaveFirearmState:
                case CCS_PlaytestStepType.VerifyFirearmPersistenceAfterLoad:
                    return CCS_PlaytestStepGroup.Firearms;

                case CCS_PlaytestStepType.AcquirePickForMining:
                case CCS_PlaytestStepType.MineStoneOutcrop:
                case CCS_PlaytestStepType.MineIronVein:
                case CCS_PlaytestStepType.MineCoalVein:
                case CCS_PlaytestStepType.RefineMinedOreAtForge:
                    return CCS_PlaytestStepGroup.Prospecting;

                case CCS_PlaytestStepType.DiscoverTradingPost:
                case CCS_PlaytestStepType.InteractGeneralStoreServicePoint:
                case CCS_PlaytestStepType.InteractStableServicePoint:
                case CCS_PlaytestStepType.InteractGunsmithServicePoint:
                case CCS_PlaytestStepType.InteractBlacksmithServicePoint:
                case CCS_PlaytestStepType.VerifySettlementVendorRouting:
                case CCS_PlaytestStepType.VerifySettlementBlacksmithRouting:
                case CCS_PlaytestStepType.SaveSettlementDiscovery:
                case CCS_PlaytestStepType.VerifySettlementDiscoveryAfterLoad:
                    return CCS_PlaytestStepGroup.Settlement;

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(stepType), stepType, "Unmapped playtest step type.");
            }
        }
    }
}
