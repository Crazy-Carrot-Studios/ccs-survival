using System;
using CCS.Modules.PlayerDeath;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SleepSpot
// CATEGORY: Modules / Sleep / Runtime / Components
// PURPOSE: World placeable bedroll sleep spot with save snapshot and respawn linkage.
// PLACEMENT: PF_CCS_PrimitiveBedroll and dynamically spawned placed bedrolls.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No direct inventory logic. Interactable hands off sleep to CCS_SleepService.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public sealed class CCS_SleepSpot : MonoBehaviour
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Sleep spot definition id copied from CCS_SleepSpotDefinition at configure time.")]
        [SerializeField] private string sleepSpotId = string.Empty;

        [Tooltip("Unique runtime instance id for save and service registration.")]
        [SerializeField] private string instanceId = string.Empty;

        [Tooltip("Player-facing sleep spot label.")]
        [SerializeField] private string displayName = "Bedroll";

        [Header("Respawn")]
        [Tooltip("Spawn id used when this bedroll is the assigned respawn point.")]
        [SerializeField] private string respawnSpawnId = string.Empty;

        [Header("Diagnostics")]
        [Tooltip("Emit sleep spot debug logs.")]
        [SerializeField] private bool enableDebugLogging;

        private bool isAssignedRespawn;
        private bool isSleeping;
        private bool isRegisteredWithService;

        #endregion

        #region Properties

        public string SleepSpotId => sleepSpotId ?? string.Empty;

        public string InstanceId => instanceId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public string RespawnSpawnId => respawnSpawnId ?? string.Empty;

        public bool IsAssignedRespawn => isAssignedRespawn;

        public bool IsSleeping => isSleeping;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            TryRegisterWithService();
        }

        private void Start()
        {
            TryRegisterWithService();
        }

        private void Update()
        {
            if (!isRegisteredWithService)
            {
                TryRegisterWithService();
            }
        }

        private void OnDisable()
        {
            UnregisterFromService();
        }

        #endregion

        #region Public Methods

        public void ConfigureFromDefinition(CCS_SleepSpotDefinition definition, string configuredInstanceId)
        {
            if (definition == null)
            {
                return;
            }

            sleepSpotId = definition.SleepSpotId;
            displayName = definition.DisplayName;
            enableDebugLogging = definition.EnableDebugLogging;
            instanceId = string.IsNullOrWhiteSpace(configuredInstanceId)
                ? GenerateInstanceId()
                : configuredInstanceId;

            respawnSpawnId = $"ccs.survival.respawn.bedroll.{instanceId}";
            EnsureRespawnPointComponent();
        }

        public void AssignInstanceId(string configuredInstanceId)
        {
            instanceId = string.IsNullOrWhiteSpace(configuredInstanceId)
                ? GenerateInstanceId()
                : configuredInstanceId;

            if (string.IsNullOrWhiteSpace(respawnSpawnId))
            {
                respawnSpawnId = $"ccs.survival.respawn.bedroll.{instanceId}";
            }

            EnsureRespawnPointComponent();
        }

        public bool CanSleep()
        {
            return isActiveAndEnabled && !isSleeping;
        }

        public bool Sleep()
        {
            if (!CanSleep())
            {
                return false;
            }

            if (!CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService sleepService)
                || sleepService == null
                || !sleepService.IsInitialized)
            {
                return false;
            }

            return sleepService.TrySleep(this);
        }

        public void SetSleepingState(bool sleeping)
        {
            isSleeping = sleeping;
        }

        public void SetAssignedRespawn(bool assigned)
        {
            isAssignedRespawn = assigned;
            EnsureRespawnPointComponent();
            if (assigned)
            {
                CCS_PlayerRespawnPoint respawnPoint = GetComponent<CCS_PlayerRespawnPoint>();
                if (respawnPoint != null)
                {
                    respawnPoint.ConfigureRuntime(respawnSpawnId);
                }
            }
        }

        public CCS_SleepSpotSaveState CaptureState()
        {
            CCS_SleepSpotSaveState saveState = new CCS_SleepSpotSaveState
            {
                sleepSpotDefinitionId = sleepSpotId,
                instanceId = instanceId,
                displayName = displayName,
                assignedRespawnSpotId = respawnSpawnId,
                isAssignedRespawn = isAssignedRespawn
            };

            Vector3 position = transform.position;
            saveState.positionX = position.x;
            saveState.positionY = position.y;
            saveState.positionZ = position.z;

            Quaternion rotation = transform.rotation;
            saveState.rotationX = rotation.x;
            saveState.rotationY = rotation.y;
            saveState.rotationZ = rotation.z;
            saveState.rotationW = rotation.w;
            return saveState;
        }

        public void RestoreState(CCS_SleepSpotSaveState saveState)
        {
            if (saveState == null)
            {
                return;
            }

            sleepSpotId = saveState.sleepSpotDefinitionId;
            instanceId = saveState.instanceId;
            displayName = saveState.displayName;
            respawnSpawnId = string.IsNullOrWhiteSpace(saveState.assignedRespawnSpotId)
                ? $"ccs.survival.respawn.bedroll.{instanceId}"
                : saveState.assignedRespawnSpotId;
            isAssignedRespawn = saveState.isAssignedRespawn;
            isSleeping = false;

            Vector3 position = new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ);
            Quaternion rotation = new Quaternion(
                saveState.rotationX,
                saveState.rotationY,
                saveState.rotationZ,
                saveState.rotationW);
            transform.SetPositionAndRotation(position, rotation);
            EnsureRespawnPointComponent();
        }

        #endregion

        #region Private Methods

        private void EnsureRespawnPointComponent()
        {
            CCS_PlayerRespawnPoint respawnPoint = GetComponent<CCS_PlayerRespawnPoint>();
            if (respawnPoint == null)
            {
                respawnPoint = gameObject.AddComponent<CCS_PlayerRespawnPoint>();
            }

            respawnPoint.ConfigureRuntime(respawnSpawnId);
        }

        private void UnregisterFromService()
        {
            if (!isRegisteredWithService)
            {
                return;
            }

            if (CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService sleepService)
                && sleepService != null
                && sleepService.IsInitialized)
            {
                sleepService.UnregisterSleepSpot(this);
            }

            isRegisteredWithService = false;
        }

        private void TryRegisterWithService()
        {
            if (isRegisteredWithService || !isActiveAndEnabled)
            {
                return;
            }

            if (!CCS_SleepRuntimeBridge.TryGetSleepService(out CCS_SleepService sleepService)
                || sleepService == null
                || !sleepService.IsInitialized)
            {
                return;
            }

            sleepService.RegisterSleepSpot(this);
            isRegisteredWithService = true;
        }

        private static string GenerateInstanceId()
        {
            return $"ccs.survival.sleep.instance.{Guid.NewGuid():N}";
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[CCS_SleepSpot] {message}");
            }
        }

        #endregion
    }
}
