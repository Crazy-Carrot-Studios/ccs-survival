using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SaveLoad;
using CCS.Modules.SaveSystem;
using CCS.Modules.PlayerDeath;
using CCS.Modules.Playtesting;
using CCS.Modules.SurvivalCore;
using CCS.Modules.EnvironmentEffects;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;
using CCS.Modules.Shelter;
using CCS.Modules.Building;
using CCS.Modules.WorldResources;
using CCS.Modules.Wildlife;
using CCS.Modules.Cooking;
using CCS.Modules.Sleep;
using CCS.Modules.Combat;
using CCS.Modules.Fishing;
using CCS.Modules.Economy;
using CCS.Modules.Gathering;
using CCS.Modules.Hotbar;
using CCS.Modules.CharacterController;
using CCS.Modules.Storage;
using CCS.Modules.Trapping;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Ranching;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.Settlements;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Player.Loadout;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalGameplayServiceHost
// CATEGORY: Survival / Runtime / Composition
// PURPOSE: Registers gameplay module services on the runtime service registry from profiles.
// PLACEMENT: PF_CCS_Survival_BootstrapRoot alongside CCS_RuntimeHost and CCS_SurvivalBootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Runs after survival bootstrap install pipeline. No singletons or scene name lookups.
// =============================================================================

namespace CCS.Survival.Composition
{
    [DefaultExecutionOrder(101)]
    public sealed class CCS_SurvivalGameplayServiceHost : MonoBehaviour
    {
        #region Variables

        [Header("Gameplay Service Profiles")]
        [Tooltip("Default survival core profile used to register CCS_SurvivalCoreService.")]
        [SerializeField] private CCS_SurvivalCoreProfile survivalCoreProfile;

        [Tooltip("Default interaction profile used to register CCS_InteractionService.")]
        [SerializeField] private CCS_InteractionProfile interactionProfile;

        [Tooltip("Default inventory profile used to register CCS_PlayerInventoryService.")]
        [SerializeField] private CCS_InventoryProfile inventoryProfile;

        [Tooltip("Default equipment profile used to register CCS_PlayerEquipmentService.")]
        [SerializeField] private CCS_EquipmentProfile equipmentProfile;

        [Tooltip("Default world resource profile used to register resource harvest and respawn services.")]
        [SerializeField] private CCS_WorldResourceProfile worldResourceProfile;

        [Tooltip("Default wildlife profile used to register CCS_WildlifeHarvestService.")]
        [SerializeField] private CCS_WildlifeProfile wildlifeProfile;

        [Tooltip("Default wildlife AI profile used to register CCS_WildlifeAiService.")]
        [SerializeField] private CCS_WildlifeAiProfile wildlifeAiProfile;

        [Tooltip("Default cooking profile used to register cooking and campfire services.")]
        [SerializeField] private CCS_CookingProfile cookingProfile;

        [Tooltip("Default sleep profile used to register CCS_SleepService.")]
        [SerializeField] private CCS_SleepProfile sleepProfile;

        [Tooltip("Default combat profile used to register CCS_CombatService.")]
        [SerializeField] private CCS_CombatProfile combatProfile;

        [Tooltip("Default active item profile used to register CCS_ActiveItemService.")]
        [SerializeField] private CCS_ActiveItemProfile activeItemProfile;

        [Tooltip("Default gathering profile used to register CCS_GatheringService.")]
        [SerializeField] private CCS_GatheringProfile gatheringProfile;

        [Tooltip("Default fishing profile used to register CCS_FishingService.")]
        [SerializeField] private CCS_FishingProfile fishingProfile;

        [Tooltip("Default trap profile used to register CCS_TrapService.")]
        [SerializeField] private CCS_TrapProfile trapProfile;

        [Tooltip("Default camp definition catalog for frontier shelter placement and camp tier tracking.")]
        [SerializeField] private CCS_CampDefinition campDefinition;

        [Tooltip("Default economy profile used to register currency and vendor services.")]
        [SerializeField] private CCS_EconomyProfile economyProfile;

        [Tooltip("Default crafting profile used to register CCS_CraftingService.")]
        [SerializeField] private CCS_CraftingProfile craftingProfile;

        [Tooltip("Default crafting progression profile used to register CCS_CraftingRecipeService.")]
        [SerializeField] private CCS_CraftingProgressionProfile craftingProgressionProfile;

        [Tooltip("Default save/load profile used to register CCS_SaveLoadService.")]
        [SerializeField] private CCS_SaveLoadProfile saveLoadProfile;

        [Tooltip("Default unified save profile used to register CCS_SaveService.")]
        [SerializeField] private CCS_SaveProfile saveProfile;

        [Tooltip("Default player death profile used to register CCS_PlayerDeathService.")]
        [SerializeField] private CCS_PlayerDeathProfile playerDeathProfile;

        [Tooltip("Default manual playtest profile used to register CCS_PlaytestService.")]
        [SerializeField] private CCS_PlaytestProfile playtestProfile;

        [Tooltip("Default time-of-day profile used to register CCS_TimeOfDayService.")]
        [SerializeField] private CCS_TimeOfDayProfile timeOfDayProfile;

        [Tooltip("Default weather profile used to register CCS_WeatherService.")]
        [SerializeField] private CCS_WeatherProfile weatherProfile;

        [Tooltip("Default shelter profile used to register CCS_ShelterService.")]
        [SerializeField] private CCS_ShelterProfile shelterProfile;

        [Tooltip("Default environment effects profile used to register CCS_EnvironmentEffectsService.")]
        [SerializeField] private CCS_EnvironmentEffectsProfile environmentEffectsProfile;

        [Tooltip("Default building profile used to register CCS_BuildingService.")]
        [SerializeField] private CCS_BuildingProfile buildingProfile;

        [Tooltip("Default building progression profile used to register CCS_BuildingRecipeService.")]
        [SerializeField] private CCS_BuildingProgressionProfile buildingProgressionProfile;

        [Tooltip("Default storage profile used to register CCS_StorageService.")]
        [SerializeField] private CCS_StorageProfile storageProfile;

        [Tooltip("Frontier storage definitions that contribute to camp tier progression.")]
        [SerializeField] private CCS_FrontierStorageCampProfile frontierStorageCampProfile;

        [Tooltip("Frontier industry processing profile for lumber, charcoal, and forge production.")]
        [SerializeField] private CCS_IndustryProfile industryProfile;

        [Tooltip("Frontier mount profile for horse ownership, riding, and saddlebags.")]
        [SerializeField] private CCS_MountProfile mountProfile;

        [Tooltip("Frontier livestock profile for ranch ownership, structures, and production.")]
        [SerializeField] private CCS_LivestockProfile livestockProfile;

        [Tooltip("Frontier vehicle profile for wagon ownership, hitching, and cargo.")]
        [SerializeField] private CCS_VehicleProfile vehicleProfile;

        [Tooltip("Frontier firearm profile for revolver, rifle, shotgun, and ammunition.")]
        [SerializeField] private CCS_FirearmProfile firearmProfile;

        [Tooltip("Default settlement profile used to register CCS_SettlementService.")]
        [SerializeField] private CCS_SettlementProfile settlementProfile;

        [Tooltip("Default region profile used to register CCS_RegionService.")]
        [SerializeField] private CCS_RegionProfile regionProfile;

        [Tooltip("Default world simulation profile used to register CCS_WorldSimulationService.")]
        [SerializeField] private CCS_WorldSimulationProfile worldSimulationProfile;

        [Tooltip("Default character controller profile used to register CCS_CharacterMovementService.")]
        [SerializeField] private CCS_CharacterControllerProfile characterControllerProfile;

        [Tooltip("Default starter loadout profile used to grant early-game items on fresh starts.")]
        [SerializeField] private CCS_StarterLoadoutProfile starterLoadoutProfile;

        [Header("Diagnostics")]
        [Tooltip("Emit gameplay service registration logs.")]
        [SerializeField] private bool enableDebugLogs;

        private CCS_RuntimeHost runtimeHost;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            runtimeHost = GetComponent<CCS_RuntimeHost>();
            if (runtimeHost == null)
            {
                runtimeHost = GetComponentInParent<CCS_RuntimeHost>();
            }

            if (runtimeHost == null)
            {
                CCS_Logger.LogWarning(
                    CCS_SurvivalRuntimeConstants.SurvivalBootstrapLogCategory,
                    "CCS_SurvivalGameplayServiceHost could not find CCS_RuntimeHost.");
                return;
            }

            CCS_SurvivalGameplayServiceRegistration.RegisterGameplayServices(
                runtimeHost,
                survivalCoreProfile,
                interactionProfile,
                inventoryProfile,
                equipmentProfile,
                worldResourceProfile,
                wildlifeProfile,
                wildlifeAiProfile,
                cookingProfile,
                sleepProfile,
                combatProfile,
                activeItemProfile,
                gatheringProfile,
                fishingProfile,
                trapProfile,
                campDefinition,
                economyProfile,
                craftingProfile,
                craftingProgressionProfile,
                saveLoadProfile,
                saveProfile,
                playerDeathProfile,
                playtestProfile,
                timeOfDayProfile,
                weatherProfile,
                shelterProfile,
                environmentEffectsProfile,
                buildingProfile,
                buildingProgressionProfile,
                storageProfile,
                frontierStorageCampProfile,
                industryProfile,
                mountProfile,
                livestockProfile,
                vehicleProfile,
                firearmProfile,
                settlementProfile,
                regionProfile,
                worldSimulationProfile,
                characterControllerProfile,
                starterLoadoutProfile,
                enableDebugLogs);
        }

        #endregion
    }
}
