using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetcodeNetworkConfigValidationUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Validates NetworkManager prefab wiring before host/join in test scenes.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Report-only at runtime. Does not auto-repair prefab references.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetcodeNetworkConfigValidationUtility
    {
        private const string InvalidPrefabMessage =
            "Network prefab entry is missing, destroyed, or not a project prefab asset.";

        #region Public Methods

        public static bool TryValidateForStart(
            NetworkManager networkManager,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (networkManager == null)
            {
                errorMessage =
                    "NetworkManager is missing. Open SCN_CCS_MultiplayerHosting and run "
                    + "CCS > Character Controller > Scene > Setup And Validate Multiplayer Hosting Scene.";
                return false;
            }

            NetworkConfig networkConfig = networkManager.NetworkConfig;
            if (networkConfig == null)
            {
                errorMessage = "NetworkManager.NetworkConfig is null.";
                return false;
            }

            GameObject playerPrefab = networkConfig.PlayerPrefab;
            if (!IsValidProjectPrefabReference(playerPrefab, out string playerPrefabError))
            {
                errorMessage = "NetworkConfig.PlayerPrefab is invalid: " + playerPrefabError;
                return false;
            }

            if (!TryGetNetworkObject(playerPrefab, out _, out string playerNetworkObjectError))
            {
                errorMessage = "NetworkConfig.PlayerPrefab is invalid: " + playerNetworkObjectError;
                return false;
            }

            if (networkConfig.Prefabs == null)
            {
                errorMessage = "NetworkConfig.Prefabs is null.";
                return false;
            }

            if (networkConfig.Prefabs.NetworkPrefabsLists == null
                || networkConfig.Prefabs.NetworkPrefabsLists.Count == 0)
            {
                errorMessage =
                    "NetworkConfig.Prefabs.NetworkPrefabsLists is empty. Run Setup Multiplayer Hosting Scene.";
                return false;
            }

            for (int listIndex = 0; listIndex < networkConfig.Prefabs.NetworkPrefabsLists.Count; listIndex++)
            {
                NetworkPrefabsList prefabsList = networkConfig.Prefabs.NetworkPrefabsLists[listIndex];
                if (prefabsList == null)
                {
                    errorMessage = $"NetworkPrefabsLists[{listIndex}] is null.";
                    return false;
                }

                if (prefabsList.PrefabList == null || prefabsList.PrefabList.Count == 0)
                {
                    errorMessage = $"{prefabsList.name} contains no registered prefabs.";
                    return false;
                }

                for (int prefabIndex = 0; prefabIndex < prefabsList.PrefabList.Count; prefabIndex++)
                {
                    NetworkPrefab networkPrefab = prefabsList.PrefabList[prefabIndex];
                    if (!IsValidProjectPrefabReference(networkPrefab.Prefab, out string prefabError))
                    {
                        errorMessage =
                            $"{prefabsList.name} entry [{prefabIndex}] is invalid: {prefabError}";
                        return false;
                    }

                    if (!TryGetNetworkObject(networkPrefab.Prefab, out _, out string networkObjectError))
                    {
                        errorMessage =
                            $"{prefabsList.name} entry [{prefabIndex}] is invalid: {networkObjectError}";
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Private Methods

        private static bool IsValidProjectPrefabReference(GameObject prefabReference, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!IsAliveUnityObject(prefabReference))
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }

            if (!TryIsProjectPrefabAsset(prefabReference, out bool isProjectPrefabAsset))
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }

            if (!isProjectPrefabAsset)
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }

            return true;
        }

        private static bool TryGetNetworkObject(
            GameObject prefabReference,
            out NetworkObject networkObject,
            out string errorMessage)
        {
            networkObject = null;
            errorMessage = string.Empty;

            if (!IsValidProjectPrefabReference(prefabReference, out errorMessage))
            {
                return false;
            }

            try
            {
                networkObject = prefabReference.GetComponent<NetworkObject>();
            }
            catch (MissingReferenceException)
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }

            if (networkObject == null)
            {
                errorMessage = "Registered network prefab is missing NetworkObject.";
                return false;
            }

            return true;
        }

        private static bool IsAliveUnityObject(Object unityObject)
        {
            if (ReferenceEquals(unityObject, null))
            {
                return false;
            }

            return unityObject != null;
        }

        private static bool TryIsProjectPrefabAsset(GameObject prefabReference, out bool isProjectPrefabAsset)
        {
            isProjectPrefabAsset = false;

            try
            {
#if UNITY_EDITOR
                isProjectPrefabAsset = !prefabReference.scene.IsValid();
                return true;
#else
                isProjectPrefabAsset = TryGetNetworkObject(prefabReference, out _, out _);
                return true;
#endif
            }
            catch (MissingReferenceException)
            {
                return false;
            }
        }

        #endregion
    }
}
