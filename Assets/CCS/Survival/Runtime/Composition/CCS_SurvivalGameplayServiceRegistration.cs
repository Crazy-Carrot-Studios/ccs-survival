using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SaveLoad;
using CCS.Modules.SurvivalCore;
using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_SurvivalGameplayServiceRegistration
// CATEGORY: Survival / Runtime / Composition
// PURPOSE: Registers gameplay module services on the runtime service registry from profiles.
// PLACEMENT: Invoked by CCS_SurvivalBootstrap after survival install pipeline completes.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Safe when profiles are missing. No singletons or scene name lookups.
// =============================================================================

namespace CCS.Survival.Composition
{
    public static class CCS_SurvivalGameplayServiceRegistration
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.SurvivalBootstrapLogCategory;

        #region Public Methods

        public static void RegisterGameplayServices(
            CCS_RuntimeHost runtimeHost,
            CCS_SurvivalCoreProfile survivalCoreProfile,
            CCS_InteractionProfile interactionProfile,
            CCS_InventoryProfile inventoryProfile,
            CCS_EquipmentProfile equipmentProfile,
            CCS_WorldResourceProfile worldResourceProfile,
            CCS_CraftingProfile craftingProfile,
            CCS_SaveLoadProfile saveLoadProfile,
            bool enableDebugLogs = false)
        {
            if (runtimeHost == null)
            {
                return;
            }

            RegisterService(runtimeHost, CreateSurvivalCoreService(survivalCoreProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateInteractionService(interactionProfile), enableDebugLogs);

            CCS_PlayerInventoryService inventoryService = CreateInventoryService(inventoryProfile);
            RegisterService(runtimeHost, inventoryService, enableDebugLogs);

            RegisterService(runtimeHost, CreateEquipmentService(equipmentProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateResourceHarvestService(worldResourceProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateResourceRespawnService(worldResourceProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateCraftingService(craftingProfile, inventoryService), enableDebugLogs);
            RegisterService(runtimeHost, CreateSaveLoadService(saveLoadProfile), enableDebugLogs);
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalCoreService CreateSurvivalCoreService(CCS_SurvivalCoreProfile profile)
        {
            CCS_SurvivalCoreService service = new CCS_SurvivalCoreService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_InteractionService CreateInteractionService(CCS_InteractionProfile profile)
        {
            CCS_InteractionService service = new CCS_InteractionService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_PlayerInventoryService CreateInventoryService(CCS_InventoryProfile profile)
        {
            CCS_PlayerInventoryService service = new CCS_PlayerInventoryService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_PlayerEquipmentService CreateEquipmentService(CCS_EquipmentProfile profile)
        {
            CCS_PlayerEquipmentService service = new CCS_PlayerEquipmentService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_ResourceHarvestService CreateResourceHarvestService(CCS_WorldResourceProfile profile)
        {
            CCS_ResourceHarvestService service = new CCS_ResourceHarvestService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_ResourceRespawnService CreateResourceRespawnService(CCS_WorldResourceProfile profile)
        {
            CCS_ResourceRespawnService service = new CCS_ResourceRespawnService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_CraftingService CreateCraftingService(
            CCS_CraftingProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_CraftingService service = new CCS_CraftingService();
            service.Initialize();

            if (profile == null || inventoryService == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile, inventoryService);
            return service;
        }

        private static CCS_SaveLoadService CreateSaveLoadService(CCS_SaveLoadProfile profile)
        {
            CCS_SaveLoadService service = new CCS_SaveLoadService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static void RegisterService<TService>(
            CCS_RuntimeHost runtimeHost,
            TService service,
            bool enableDebugLogs)
            where TService : class, CCS_IService
        {
            if (service == null)
            {
                return;
            }

            if (runtimeHost.ServiceRegistry.RegisterService(service))
            {
                CCS_Logger.Log(
                    LogCategory,
                    $"Registered gameplay service: {typeof(TService).Name}",
                    enableDebugLogs);
                return;
            }

            CCS_Logger.LogWarning(
                LogCategory,
                $"Failed to register gameplay service: {typeof(TService).Name}");
        }

        #endregion
    }
}
