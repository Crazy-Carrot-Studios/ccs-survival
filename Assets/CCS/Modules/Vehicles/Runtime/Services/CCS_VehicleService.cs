using System;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Modules.Mounts;
using CCS.Survival;
using UnityEngine;
using Object = UnityEngine.Object;

// =============================================================================
// SCRIPT: CCS_VehicleService
// CATEGORY: Modules / Vehicles / Runtime / Services
// PURPOSE: Generic owned-vehicle lifecycle: purchase, summon, park, hitch, cargo, save.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public sealed class CCS_VehicleService : CCS_ISurvivalService, CCS_IUpdatable
    {
        public delegate void VehicleStateChangedHandler(CCS_VehicleInstance instance, CCS_VehicleState previousState);
        public delegate void WagonOwnershipChangedHandler(bool ownsWagon);

        private const string LogPrefix = "[CCS_VehicleService]";
        private const string DefaultWagonName = "Frontier Wagon";
        private const float HitchMaxDistance = 6f;

        private CCS_VehicleProfile activeProfile;
        private CCS_VehicleInstance ownedVehicle;
        private CCS_MountService mountService;
        private CCS_PlayerInventoryService inventoryService;
        private Transform playerTransform;
        private bool isInitialized;

        public event VehicleStateChangedHandler VehicleStateChanged;
        public event WagonOwnershipChangedHandler WagonOwnershipChanged;

        public bool IsInitialized => isInitialized;

        public bool OwnsWagon => ownedVehicle != null;

        public bool IsHitched =>
            ownedVehicle != null
            && (ownedVehicle.State == CCS_VehicleState.Hitched
                || ownedVehicle.State == CCS_VehicleState.Moving);

        public string ActiveVehicleInstanceId => ownedVehicle?.InstanceId ?? string.Empty;

        public string ActiveCargoInstanceId => ownedVehicle?.CargoInstanceId ?? string.Empty;

        public string WagonDisplayName => ownedVehicle?.DisplayName ?? string.Empty;

        public CCS_VehicleSnapshot CurrentSnapshot => CaptureSnapshot();

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_VehicleProfile profile)
        {
            activeProfile = profile;
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_VehicleValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            isInitialized = validation.IsSuccess;
        }

        public void BindInventoryService(CCS_PlayerInventoryService inventory)
        {
            inventoryService = inventory;
        }

        public void BindMountService(CCS_MountService mounts)
        {
            mountService = mounts;
        }

        public void BindPlayerTransform(Transform playerRoot)
        {
            playerTransform = playerRoot;
        }

        public bool TryGrantWagonOwnership(string displayName = null)
        {
            if (!isInitialized
                || ownedVehicle != null
                || !activeProfile.TryGetVehicleById(
                    CCS_VehicleContentIds.FrontierWagonVehicleId,
                    out CCS_VehicleDefinition wagonDefinition))
            {
                return false;
            }

            string instanceId = GenerateInstanceId();
            CCS_VehicleWorldActor actor = SpawnWorldActor(wagonDefinition, instanceId, Vector3.zero, 0f);
            ownedVehicle = new CCS_VehicleInstance(instanceId, wagonDefinition, displayName ?? DefaultWagonName, actor);
            ownedVehicle.CargoInstanceId = $"{instanceId}.cargo";
            if (actor != null)
            {
                actor.Configure(
                    instanceId,
                    wagonDefinition,
                    actor.Cargo,
                    ownedVehicle.CargoInstanceId,
                    wagonDefinition.CargoSlotCount);
            }

            SetVehicleState(ownedVehicle, CCS_VehicleState.Stored);
            WagonOwnershipChanged?.Invoke(true);
            return true;
        }

        public bool TryConsumeWagonPurchaseItem(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null
                || !string.Equals(
                    itemDefinition.ItemId,
                    CCS_VehicleContentIds.FrontierWagonItemId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ownedVehicle != null)
            {
                return false;
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                inventoryService.RemoveItem(itemDefinition, 1);
            }

            return TryGrantWagonOwnership();
        }

        public bool TrySummonWagonNearPlayer()
        {
            if (ownedVehicle == null || playerTransform == null)
            {
                return false;
            }

            if (ownedVehicle.WorldActor == null)
            {
                CCS_VehicleDefinition definition = ownedVehicle.Definition;
                ownedVehicle.BindWorldActor(
                    SpawnWorldActor(definition, ownedVehicle.InstanceId, Vector3.zero, 0f));
                if (ownedVehicle.WorldActor != null)
                {
                    ownedVehicle.WorldActor.Configure(
                        ownedVehicle.InstanceId,
                        definition,
                        ownedVehicle.WorldActor.Cargo,
                        ownedVehicle.CargoInstanceId,
                        definition.CargoSlotCount);
                }
            }

            if (ownedVehicle.WorldActor == null)
            {
                return false;
            }

            Vector3 spawnOffset = playerTransform.right * -2.5f + playerTransform.forward * -2f;
            Vector3 spawnPosition = playerTransform.position + spawnOffset;
            ownedVehicle.WorldActor.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.Euler(0f, playerTransform.eulerAngles.y, 0f));
            SetVehicleState(ownedVehicle, CCS_VehicleState.Idle);
            return true;
        }

        public bool TryParkWagon()
        {
            if (ownedVehicle == null)
            {
                return false;
            }

            if (IsHitched)
            {
                TryUnhitchFromHorse();
            }

            SetVehicleState(ownedVehicle, CCS_VehicleState.Parked);
            return true;
        }

        public bool TryStoreWagon()
        {
            if (ownedVehicle == null)
            {
                return false;
            }

            if (IsHitched)
            {
                TryUnhitchFromHorse();
            }

            DestroyOwnedWorldActor();
            SetVehicleState(ownedVehicle, CCS_VehicleState.Stored);
            return true;
        }

        public bool TryHitchToOwnedHorse()
        {
            if (ownedVehicle?.WorldActor == null
                || mountService == null
                || !mountService.IsInitialized
                || !mountService.OwnsHorse)
            {
                return false;
            }

            Transform hitchPoint = mountService.GetOwnedHorseWagonHitchPoint();
            if (hitchPoint == null)
            {
                return false;
            }

            CCS_VehicleDefinition definition = ownedVehicle.Definition;
            if (definition == null
                || !definition.IsHitchCompatible(mountService.GetOwnedHorseMountDefinitionId()))
            {
                return false;
            }

            float distance = Vector3.Distance(ownedVehicle.WorldActor.transform.position, hitchPoint.position);
            if (distance > HitchMaxDistance)
            {
                return false;
            }

            ownedVehicle.HitchedMountInstanceId = mountService.ActiveMountInstanceId;
            SetVehicleState(ownedVehicle, CCS_VehicleState.Hitched);
            ownedVehicle.WorldActor.TickFollowHitch(
                hitchPoint,
                definition.FollowOffsetLocal,
                definition.FollowSmoothing,
                1f);
            return true;
        }

        public bool TryUnhitchFromHorse()
        {
            if (ownedVehicle == null || !IsHitched)
            {
                return false;
            }

            ownedVehicle.HitchedMountInstanceId = string.Empty;
            SetVehicleState(ownedVehicle, CCS_VehicleState.Parked);
            return true;
        }

        public CCS_VehicleSnapshot CaptureSnapshot()
        {
            if (ownedVehicle == null)
            {
                return CCS_VehicleSnapshot.Empty;
            }

            Vector3 position = ownedVehicle.WorldPosition;
            return new CCS_VehicleSnapshot
            {
                ownsVehicle = true,
                vehicleDefinitionId = ownedVehicle.Definition?.VehicleId ?? string.Empty,
                instanceId = ownedVehicle.InstanceId,
                displayName = ownedVehicle.DisplayName,
                vehicleState = (int)ownedVehicle.State,
                positionX = position.x,
                positionY = position.y,
                positionZ = position.z,
                rotationY = ownedVehicle.WorldRotationY,
                cargoInstanceId = ownedVehicle.CargoInstanceId ?? string.Empty,
                hitchedMountInstanceId = ownedVehicle.HitchedMountInstanceId ?? string.Empty
            };
        }

        public void RestoreSnapshot(CCS_VehicleSnapshot snapshot)
        {
            DestroyOwnedWorldActor();
            ownedVehicle = null;

            if (snapshot == null || !snapshot.ownsVehicle)
            {
                WagonOwnershipChanged?.Invoke(false);
                return;
            }

            if (!activeProfile.TryGetVehicleById(snapshot.vehicleDefinitionId, out CCS_VehicleDefinition definition))
            {
                activeProfile.TryGetVehicleById(
                    CCS_VehicleContentIds.FrontierWagonVehicleId,
                    out definition);
            }

            if (definition == null)
            {
                return;
            }

            CCS_VehicleState restoredState = snapshot.VehicleState;
            Vector3 position = snapshot.Position;
            CCS_VehicleWorldActor actor = null;
            if (restoredState != CCS_VehicleState.Stored)
            {
                actor = SpawnWorldActor(definition, snapshot.instanceId, position, snapshot.rotationY);
            }

            ownedVehicle = new CCS_VehicleInstance(
                snapshot.instanceId,
                definition,
                snapshot.displayName,
                actor);
            ownedVehicle.CargoInstanceId = snapshot.cargoInstanceId;
            ownedVehicle.HitchedMountInstanceId = snapshot.hitchedMountInstanceId;
            if (actor != null)
            {
                actor.Configure(
                    snapshot.instanceId,
                    definition,
                    actor.Cargo,
                    ownedVehicle.CargoInstanceId,
                    definition.CargoSlotCount);
            }

            SetVehicleState(ownedVehicle, restoredState);
            if (!string.IsNullOrWhiteSpace(ownedVehicle.HitchedMountInstanceId))
            {
                TryRestoreHitchAfterLoad(ownedVehicle.HitchedMountInstanceId);
            }

            WagonOwnershipChanged?.Invoke(true);
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || ownedVehicle == null || deltaTime <= 0f)
            {
                return;
            }

            if (!IsHitched || ownedVehicle.WorldActor == null)
            {
                return;
            }

            Transform hitchPoint = mountService?.GetOwnedHorseWagonHitchPoint();
            if (hitchPoint == null)
            {
                return;
            }

            CCS_VehicleDefinition definition = ownedVehicle.Definition;
            ownedVehicle.WorldActor.TickFollowHitch(
                hitchPoint,
                definition.FollowOffsetLocal,
                definition.FollowSmoothing,
                deltaTime);

            bool horseMoving = mountService != null
                && mountService.IsInitialized
                && mountService.IsMounted;
            CCS_VehicleState targetState = horseMoving ? CCS_VehicleState.Moving : CCS_VehicleState.Hitched;
            if (ownedVehicle.State != targetState)
            {
                SetVehicleState(ownedVehicle, targetState);
            }
        }

        private void TryRestoreHitchAfterLoad(string expectedMountInstanceId)
        {
            if (mountService == null
                || !mountService.IsInitialized
                || !mountService.OwnsHorse
                || !string.Equals(
                    mountService.ActiveMountInstanceId,
                    expectedMountInstanceId,
                    StringComparison.OrdinalIgnoreCase))
            {
                ownedVehicle.HitchedMountInstanceId = string.Empty;
                SetVehicleState(ownedVehicle, CCS_VehicleState.Parked);
                return;
            }

            Transform hitchPoint = mountService.GetOwnedHorseWagonHitchPoint();
            if (hitchPoint == null)
            {
                return;
            }

            SetVehicleState(ownedVehicle, CCS_VehicleState.Hitched);
            ownedVehicle.WorldActor?.TickFollowHitch(
                hitchPoint,
                ownedVehicle.Definition.FollowOffsetLocal,
                ownedVehicle.Definition.FollowSmoothing,
                1f);
        }

        private void SetVehicleState(CCS_VehicleInstance instance, CCS_VehicleState newState)
        {
            if (instance == null)
            {
                return;
            }

            CCS_VehicleState previous = instance.State;
            if (previous == newState)
            {
                return;
            }

            instance.State = newState;
            VehicleStateChanged?.Invoke(instance, previous);
        }

        private CCS_VehicleWorldActor SpawnWorldActor(
            CCS_VehicleDefinition definition,
            string instanceId,
            Vector3 position,
            float rotationY)
        {
            if (definition?.WorldPrefab == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(definition.WorldPrefab, position, Quaternion.Euler(0f, rotationY, 0f));
            instance.name = $"{definition.DisplayName}_{instanceId}";
            CCS_VehicleWorldActor actor = instance.GetComponent<CCS_VehicleWorldActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<CCS_VehicleWorldActor>();
            }

            return actor;
        }

        private void DestroyOwnedWorldActor()
        {
            if (ownedVehicle?.WorldActor == null)
            {
                return;
            }

            Object.Destroy(ownedVehicle.WorldActor.gameObject);
            ownedVehicle.BindWorldActor(null);
        }

        private static string GenerateInstanceId()
        {
            return $"ccs.survival.vehicle.instance.{Guid.NewGuid():N}";
        }
    }
}
