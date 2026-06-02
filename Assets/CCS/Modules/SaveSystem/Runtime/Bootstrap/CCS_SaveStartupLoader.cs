using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.CharacterController;
using CCS.Modules.Sleep;
using CCS.Modules.Storage;
using CCS.Modules.Trapping;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.Shelter;
using CCS.Modules.Gathering;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.PlayerDeath;
using CCS.Modules.SurvivalCore;
using CCS.Survival.Player;
using CCS.Survival.Player.Loadout;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveStartupLoader
// CATEGORY: Modules / SaveSystem / Runtime / Bootstrap
// PURPOSE: Binds player transform and applies unified save or starter loadout on play start.
// PLACEMENT: PF_CCS_Survival_BootstrapRoot or bootstrap scene service host object.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Runs after gameplay services register in Awake.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    [DefaultExecutionOrder(250)]
    public sealed class CCS_SaveStartupLoader : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_SaveStartupLoader]";

        #region Unity Callbacks

        private void Start()
        {
            if (!CCS_SaveRuntimeBridge.TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return;
            }

            if (!runtimeHost.ServiceRegistry.TryGetService(out CCS_SaveService saveService)
                || saveService == null
                || !saveService.IsInitialized)
            {
                return;
            }

            runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerInventoryService inventoryService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_SurvivalCoreService survivalCoreService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_GatheringService gatheringService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_BuildingService buildingService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_StorageService storageService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_SleepService sleepService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_TrapService trapService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_FrontierShelterService frontierShelterService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_CampService campService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_FrontierHomesteadStructureService homesteadStructureService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_FrontierStoragePlacementService frontierStoragePlacementService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_IndustryService industryService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_MountService mountService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_VehicleService vehicleService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_FirearmService firearmService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_CharacterMovementService movementService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_StarterLoadoutService starterLoadoutService);
            runtimeHost.ServiceRegistry.TryGetService(out CCS_CurrencyService currencyService);

            Transform playerTransform = ResolvePlayerTransform();
            saveService.BindGameplayServices(
                inventoryService,
                survivalCoreService,
                gatheringService,
                buildingService,
                storageService,
                sleepService,
                currencyService,
                trapService,
                frontierShelterService,
                campService,
                homesteadStructureService,
                frontierStoragePlacementService,
                industryService,
                mountService,
                vehicleService,
                firearmService,
                playerTransform);

            if (mountService != null && mountService.IsInitialized && playerTransform != null)
            {
                mountService.BindPlayerTransform(playerTransform);
                if (movementService != null && movementService.IsInitialized)
                {
                    CCS_PlayerCinemachineCameraDriver cameraDriver =
                        playerTransform.GetComponentInChildren<CCS_PlayerCinemachineCameraDriver>();
                    mountService.BindRidingIntegration(
                        () =>
                        {
                            CCS_CharacterInputSnapshot input = movementService.InputProvider != null
                                ? movementService.InputProvider.GetInputSnapshot()
                                : CCS_CharacterInputSnapshot.Empty;
                            float yaw = movementService.LookState.YawDegrees;
                            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                            Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
                            return forward * input.Move.y + right * input.Move.x;
                        },
                        () => movementService.InputProvider != null
                            && movementService.InputProvider.GetInputSnapshot().SprintHeld,
                        movementService.SetMovementLocked,
                        movementService.TickLookOnly,
                        active => cameraDriver?.SetHorseRidingCameraActive(active));
                }
            }

            if (vehicleService != null && vehicleService.IsInitialized && playerTransform != null)
            {
                vehicleService.BindPlayerTransform(playerTransform);
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerDeathService playerDeathService)
                && movementService != null)
            {
                playerDeathService.BindGameplayServices(
                    survivalCoreService,
                    movementService,
                    playerTransform);
                playerDeathService.BindAssignedRespawnSpawnIdProvider(
                    () => sleepService != null && sleepService.IsInitialized
                        ? sleepService.AssignedRespawnSpawnId
                        : string.Empty);
            }

            if (saveService.TryLoadOnStartup())
            {
                Debug.Log($"{LogPrefix} Unified save loaded from {saveService.SaveFilePath}.");
                return;
            }

            if (starterLoadoutService != null && inventoryService != null)
            {
                starterLoadoutService.TryApplyStarterLoadout(inventoryService);
            }

            if (currencyService != null && currencyService.IsInitialized)
            {
                currencyService.ImportBalancesFromInventoryBacking();
            }
        }

        #endregion

        #region Private Methods

        private static Transform ResolvePlayerTransform()
        {
            CCS_PlayerGameplayController[] controllers =
                Object.FindObjectsByType<CCS_PlayerGameplayController>(FindObjectsSortMode.None);
            if (controllers == null || controllers.Length == 0)
            {
                return null;
            }

            return controllers[0].transform;
        }

        #endregion
    }
}
