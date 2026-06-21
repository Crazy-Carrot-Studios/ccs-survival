using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPickupItemSpawner
// CATEGORY: Modules / Interaction / Runtime / Components
// PURPOSE: Spawns one test pickup item in front of the player start for Master Test.
// PLACEMENT: CCS_TestPickupItemSpawner object in Master Test scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Offline instantiates locally. Network sessions spawn on the server at runtime.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_TestPickupItemSpawner : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject pickupItemPrefab;

        [SerializeField] private Transform spawnOrigin;

        [SerializeField] private float spawnForwardDistance = 2.5f;

        #endregion

        #region Variables

        private static CCS_TestPickupItemSpawner activeInstance;
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
            if (IsNetworkSessionActive())
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
                activeInstance = FindAnyObjectByType<CCS_TestPickupItemSpawner>();
            }

            if (activeInstance != null)
            {
                activeInstance.EnsureNetworkInstance();
            }
        }

        public void ConfigurePickupSpawner(GameObject prefab, Transform origin, float forwardDistance)
        {
            pickupItemPrefab = prefab;
            spawnOrigin = origin;
            spawnForwardDistance = forwardDistance;
        }

        #endregion

        #region Private Methods

        private void EnsureOfflineInstance()
        {
            if (offlineInstance != null || networkInstance != null)
            {
                return;
            }

            if (!TryGetSpawnPose(out Vector3 position, out Quaternion rotation))
            {
                return;
            }

            offlineInstance = Instantiate(pickupItemPrefab, position, rotation);
            offlineInstance.name = CCS_InteractionConstants.TestPickupInteractableInstanceName;
            Debug.Log("[Interaction Test] Pickup item spawned.", offlineInstance);
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

            if (!TryGetSpawnPose(out Vector3 position, out Quaternion rotation))
            {
                return;
            }

            GameObject instance = Instantiate(pickupItemPrefab, position, rotation);
            instance.name = CCS_InteractionConstants.TestPickupInteractableInstanceName;

            networkInstance = instance.GetComponent<NetworkObject>();
            if (networkInstance == null)
            {
                Debug.LogError("[Interaction Test] Pickup item prefab is missing NetworkObject.", this);
                Destroy(instance);
                return;
            }

            networkInstance.Spawn();
            Debug.Log("[Interaction Test] Pickup item spawned.", instance);
        }

        private bool TryGetSpawnPose(out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;

            if (pickupItemPrefab == null)
            {
                Debug.LogError("[Interaction Test] Pickup item prefab is not assigned.", this);
                return false;
            }

            if (spawnOrigin == null)
            {
                Debug.LogError("[Interaction Test] Pickup spawn origin is not assigned.", this);
                return false;
            }

            position = spawnOrigin.position + spawnOrigin.forward * spawnForwardDistance;
            rotation = spawnOrigin.rotation;
            return true;
        }

        private static bool IsNetworkSessionActive()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            return networkManager != null && networkManager.IsListening;
        }

        #endregion
    }
}
