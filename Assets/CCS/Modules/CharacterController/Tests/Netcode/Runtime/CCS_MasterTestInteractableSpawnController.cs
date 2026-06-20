using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Interaction;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MasterTestInteractableSpawnController
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Spawns the test toggle interactable offline or on the server at runtime.
// PLACEMENT: CCS_MasterTestInteractableSpawnController object in Master Test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Avoids scene-placed NetworkObjects that drift hashes between build and editor.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_MasterTestInteractableSpawnController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject toggleInteractablePrefab;

        #endregion

        #region Variables

        private static CCS_MasterTestInteractableSpawnController activeInstance;
        private GameObject offlineInstance;
        private NetworkObject networkInstance;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            activeInstance = this;
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        private void Start()
        {
            if (CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
            {
                return;
            }

            EnsureOfflineInstance();
        }

        #endregion

        #region Public Methods

        public static void EnsureNetworkInstanceIfServer()
        {
            if (activeInstance == null)
            {
                activeInstance = FindFirstObjectByType<CCS_MasterTestInteractableSpawnController>();
            }

            if (activeInstance != null)
            {
                activeInstance.EnsureNetworkInstance();
            }
        }

        public void SetToggleInteractablePrefab(GameObject prefab)
        {
            toggleInteractablePrefab = prefab;
        }

        #endregion

        #region Private Methods

        private void EnsureOfflineInstance()
        {
            if (offlineInstance != null || networkInstance != null)
            {
                return;
            }

            if (toggleInteractablePrefab == null)
            {
                Debug.LogError("[Master Test Interactable Spawn] Toggle interactable prefab is not assigned.");
                return;
            }

            offlineInstance = Instantiate(
                toggleInteractablePrefab,
                CCS_InteractionConstants.TestToggleInteractablePosition,
                Quaternion.identity);
            offlineInstance.name = CCS_InteractionConstants.TestToggleInteractableInstanceName;
            Debug.Log("[Master Test Interactable Spawn] Offline toggle cube instantiated.");
        }

        private void EnsureNetworkInstance()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer)
            {
                return;
            }

            if (networkInstance != null || offlineInstance != null)
            {
                return;
            }

            if (toggleInteractablePrefab == null)
            {
                Debug.LogError("[Master Test Interactable Spawn] Toggle interactable prefab is not assigned.");
                return;
            }

            GameObject instance = Instantiate(
                toggleInteractablePrefab,
                CCS_InteractionConstants.TestToggleInteractablePosition,
                Quaternion.identity);
            instance.name = CCS_InteractionConstants.TestToggleInteractableInstanceName;

            networkInstance = instance.GetComponent<NetworkObject>();
            if (networkInstance == null)
            {
                Debug.LogError("[Master Test Interactable Spawn] Toggle interactable prefab is missing NetworkObject.");
                Destroy(instance);
                return;
            }

            networkInstance.Spawn();
            Debug.Log(
                $"[Master Test Interactable Spawn] Network toggle cube spawned. hash={CCS_NetcodeNetworkObjectHashUtility.GetHash(networkInstance)}");
        }

        #endregion
    }
}
