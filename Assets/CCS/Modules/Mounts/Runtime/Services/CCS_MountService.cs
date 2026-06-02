using System;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;
using Object = UnityEngine.Object;

// =============================================================================
// SCRIPT: CCS_MountService
// CATEGORY: Modules / Mounts / Runtime / Services
// PURPOSE: Generic owned-mount lifecycle: purchase, summon, mount, ride, wait, save.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public sealed class CCS_MountService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_MountService]";
        private const string DefaultHorseName = "Frontier Horse";

        private CCS_MountProfile activeProfile;
        private CCS_MountInstance ownedMount;
        private CCS_PlayerInventoryService inventoryService;
        private Transform playerTransform;
        private Func<Vector3> riderPlanarInputProvider;
        private Func<bool> riderSprintHeldProvider;
        private Action<bool> movementLockedSetter;
        private Action<float> lookTickProvider;
        private Action<bool> horseCameraModeSetter;
        private Func<Vector3, float, bool> campPresenceQuery;
        private bool isInitialized;

        public event MountStateChangedHandler MountStateChanged;
        public event HorseOwnershipChangedHandler HorseOwnershipChanged;

        public bool IsInitialized => isInitialized;

        public CCS_MountProfile ActiveProfile => activeProfile;

        public bool OwnsHorse => ownedMount != null;

        public bool IsMounted => ownedMount != null && ownedMount.State == CCS_MountState.Mounted;

        public string ActiveMountInstanceId => ownedMount?.InstanceId ?? string.Empty;

        public string HorseDisplayName => ownedMount?.DisplayName ?? string.Empty;

        public CCS_MountSnapshot CurrentSnapshot => CaptureSnapshot();

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_MountProfile profile)
        {
            activeProfile = profile;
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_MountValidationUtility.ValidateProfile(profile);
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

        public void BindPlayerTransform(Transform playerRoot)
        {
            playerTransform = playerRoot;
        }

        public void BindRidingIntegration(
            Func<Vector3> planarInputProvider,
            Func<bool> sprintHeldProvider,
            Action<bool> setMovementLocked,
            Action<float> tickLook,
            Action<bool> setHorseCameraMode)
        {
            riderPlanarInputProvider = planarInputProvider;
            riderSprintHeldProvider = sprintHeldProvider;
            movementLockedSetter = setMovementLocked;
            lookTickProvider = tickLook;
            horseCameraModeSetter = setHorseCameraMode;
        }

        public void BindCampPresenceQuery(Func<Vector3, float, bool> query)
        {
            campPresenceQuery = query;
        }

        public bool TryGrantHorseOwnership(string displayName = null)
        {
            if (!isInitialized
                || ownedMount != null
                || !activeProfile.TryGetMountById(CCS_MountContentIds.HorseMountId, out CCS_MountDefinition horseDefinition))
            {
                return false;
            }

            string instanceId = GenerateInstanceId();
            CCS_MountWorldActor actor = SpawnWorldActor(horseDefinition, instanceId, Vector3.zero, 0f);
            ownedMount = new CCS_MountInstance(instanceId, horseDefinition, displayName ?? DefaultHorseName, actor);
            ownedMount.SaddlebagInstanceId = $"{instanceId}.saddlebag";
            if (actor != null)
            {
                actor.Configure(
                    instanceId,
                    horseDefinition,
                    actor.Saddlebag,
                    ownedMount.SaddlebagInstanceId,
                    horseDefinition.SaddlebagSlotCount);
            }

            SetMountState(ownedMount, CCS_MountState.Idle);
            HorseOwnershipChanged?.Invoke(true);
            return true;
        }

        public bool TryConsumeHorsePurchaseItem(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null
                || !string.Equals(
                    itemDefinition.ItemId,
                    CCS_MountContentIds.FrontierHorseItemId,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ownedMount != null)
            {
                return false;
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                inventoryService.RemoveItem(itemDefinition, 1);
            }

            return TryGrantHorseOwnership();
        }

        public bool TrySummonHorseNearPlayer()
        {
            if (ownedMount?.WorldActor == null || playerTransform == null)
            {
                return false;
            }

            Vector3 spawnOffset = playerTransform.right * 2f + playerTransform.forward * -1.5f;
            Vector3 spawnPosition = playerTransform.position + spawnOffset;
            ownedMount.WorldActor.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.Euler(0f, playerTransform.eulerAngles.y, 0f));
            SetMountState(ownedMount, CCS_MountState.Following);
            ownedMount.WorldActor.SetFollowTarget(playerTransform.position, true);
            return true;
        }

        public bool TryCallHorse()
        {
            if (ownedMount == null)
            {
                return false;
            }

            if (ownedMount.WorldActor == null)
            {
                return TrySummonHorseNearPlayer();
            }

            SetMountState(ownedMount, CCS_MountState.Returning);
            ownedMount.WorldActor.SetFollowTarget(playerTransform != null ? playerTransform.position : Vector3.zero, true);
            return true;
        }

        public bool TryWaitHorse()
        {
            if (ownedMount == null || IsMounted)
            {
                return false;
            }

            SetMountState(ownedMount, CCS_MountState.Waiting);
            ownedMount.WorldActor?.SetFollowTarget(Vector3.zero, false);
            return true;
        }

        public bool TryMount(string instanceId)
        {
            if (ownedMount == null
                || string.IsNullOrWhiteSpace(instanceId)
                || !string.Equals(ownedMount.InstanceId, instanceId, StringComparison.OrdinalIgnoreCase)
                || ownedMount.WorldActor == null
                || playerTransform == null)
            {
                return false;
            }

            float distance = Vector3.Distance(playerTransform.position, ownedMount.WorldActor.transform.position);
            if (distance > 4f)
            {
                return false;
            }

            SetMountState(ownedMount, CCS_MountState.Mounted);
            movementLockedSetter?.Invoke(true);
            horseCameraModeSetter?.Invoke(true);
            SnapPlayerToMount();
            return true;
        }

        public bool TryDismount()
        {
            if (!IsMounted || ownedMount?.WorldActor == null || playerTransform == null)
            {
                return false;
            }

            Vector3 dismountOffset = ownedMount.WorldActor.transform.right * 1.25f;
            playerTransform.position = ownedMount.WorldActor.transform.position + dismountOffset;
            SetMountState(ownedMount, CCS_MountState.Idle);
            movementLockedSetter?.Invoke(false);
            horseCameraModeSetter?.Invoke(false);
            ownedMount.WorldActor.SetFollowTarget(Vector3.zero, false);
            return true;
        }

        public bool HasHorseCampPresence(Vector3 campCenter, float radius)
        {
            if (!OwnsHorse || ownedMount?.WorldActor == null)
            {
                return false;
            }

            Vector3 horsePosition = ownedMount.WorldActor.transform.position;
            return Vector3.Distance(campCenter, horsePosition) <= radius;
        }

        public CCS_MountSnapshot CaptureSnapshot()
        {
            if (ownedMount == null)
            {
                return CCS_MountSnapshot.Empty;
            }

            Vector3 position = ownedMount.WorldPosition;
            return new CCS_MountSnapshot
            {
                ownsMount = true,
                mountDefinitionId = ownedMount.Definition?.MountId ?? string.Empty,
                instanceId = ownedMount.InstanceId,
                displayName = ownedMount.DisplayName,
                mountState = (int)ownedMount.State,
                positionX = position.x,
                positionY = position.y,
                positionZ = position.z,
                rotationY = ownedMount.WorldRotationY,
                saddlebagInstanceId = ownedMount.SaddlebagInstanceId ?? string.Empty
            };
        }

        public void RestoreSnapshot(CCS_MountSnapshot snapshot)
        {
            DestroyOwnedWorldActor();
            ownedMount = null;

            if (snapshot == null || !snapshot.ownsMount)
            {
                HorseOwnershipChanged?.Invoke(false);
                return;
            }

            if (!activeProfile.TryGetMountById(snapshot.mountDefinitionId, out CCS_MountDefinition definition))
            {
                activeProfile.TryGetMountById(CCS_MountContentIds.HorseMountId, out definition);
            }

            if (definition == null)
            {
                return;
            }

            Vector3 position = snapshot.Position;
            CCS_MountWorldActor actor = SpawnWorldActor(definition, snapshot.instanceId, position, snapshot.rotationY);
            ownedMount = new CCS_MountInstance(
                snapshot.instanceId,
                definition,
                snapshot.displayName,
                actor);
            ownedMount.SaddlebagInstanceId = snapshot.saddlebagInstanceId;
            if (actor != null)
            {
                actor.Configure(
                    snapshot.instanceId,
                    definition,
                    actor.Saddlebag,
                    ownedMount.SaddlebagInstanceId,
                    definition.SaddlebagSlotCount);
            }

            SetMountState(ownedMount, snapshot.MountState);
            if (IsMounted)
            {
                movementLockedSetter?.Invoke(true);
                horseCameraModeSetter?.Invoke(true);
                SnapPlayerToMount();
            }

            HorseOwnershipChanged?.Invoke(true);
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || ownedMount?.WorldActor == null || deltaTime <= 0f)
            {
                return;
            }

            if (IsMounted)
            {
                lookTickProvider?.Invoke(deltaTime);
                Vector3 planar = riderPlanarInputProvider != null ? riderPlanarInputProvider() : Vector3.zero;
                bool sprint = riderSprintHeldProvider != null && riderSprintHeldProvider();
                ownedMount.WorldActor.TickLocomotion(
                    CCS_MountState.Mounted,
                    planar,
                    ownedMount.Definition.MovementSpeed,
                    ownedMount.Definition.SprintSpeed,
                    sprint,
                    deltaTime);
                SnapPlayerToMount();
                return;
            }

            if (ownedMount.State == CCS_MountState.Following
                || ownedMount.State == CCS_MountState.Returning)
            {
                if (playerTransform != null)
                {
                    ownedMount.WorldActor.SetFollowTarget(playerTransform.position, true);
                }

                ownedMount.WorldActor.TickLocomotion(
                    ownedMount.State,
                    Vector3.zero,
                    ownedMount.Definition.MovementSpeed,
                    ownedMount.Definition.SprintSpeed,
                    false,
                    deltaTime);
            }
        }

        private void SnapPlayerToMount()
        {
            if (playerTransform == null || ownedMount?.WorldActor == null)
            {
                return;
            }

            Transform mountTransform = ownedMount.WorldActor.transform;
            playerTransform.position = mountTransform.position + Vector3.up * 1.35f;
            playerTransform.rotation = mountTransform.rotation;
        }

        private void SetMountState(CCS_MountInstance instance, CCS_MountState newState)
        {
            if (instance == null)
            {
                return;
            }

            CCS_MountState previous = instance.State;
            if (previous == newState)
            {
                return;
            }

            instance.State = newState;
            MountStateChanged?.Invoke(instance, previous);
        }

        private CCS_MountWorldActor SpawnWorldActor(
            CCS_MountDefinition definition,
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
            CCS_MountWorldActor actor = instance.GetComponent<CCS_MountWorldActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<CCS_MountWorldActor>();
            }

            return actor;
        }

        private void DestroyOwnedWorldActor()
        {
            if (ownedMount?.WorldActor == null)
            {
                return;
            }

            if (IsMounted)
            {
                TryDismount();
            }

            Object.Destroy(ownedMount.WorldActor.gameObject);
        }

        private static string GenerateInstanceId()
        {
            return $"ccs.survival.mount.instance.{Guid.NewGuid():N}";
        }
    }
}
