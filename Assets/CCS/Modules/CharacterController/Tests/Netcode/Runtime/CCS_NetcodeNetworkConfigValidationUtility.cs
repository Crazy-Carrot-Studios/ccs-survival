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

            if (!networkConfig.EnableSceneManagement)
            {
                errorMessage = "NetworkConfig.EnableSceneManagement must be enabled for host scene load.";
                return false;
            }

            GameObject playerPrefab = networkConfig.PlayerPrefab;
            if (!IsValidNetworkPrefabReference(playerPrefab, out string playerPrefabError))
            {
                errorMessage = "NetworkConfig.PlayerPrefab is invalid: " + playerPrefabError;
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
                    if (!IsValidNetworkPrefabReference(networkPrefab.Prefab, out string prefabError))
                    {
                        errorMessage =
                            $"{prefabsList.name} entry [{prefabIndex}] is invalid: {prefabError}";
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool HasValidNetworkObjectPrefab(GameObject prefabReference)
        {
            return IsValidNetworkPrefabReference(prefabReference, out _);
        }

        #endregion

        #region Private Methods

        private static bool IsValidNetworkPrefabReference(GameObject prefabReference, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!IsAliveUnityObject(prefabReference))
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }

#if UNITY_EDITOR
            if (!TryIsEditorProjectPrefabAsset(prefabReference, out errorMessage))
            {
                return false;
            }
#else
            if (!TryGetNetworkObject(prefabReference, out _, out errorMessage))
            {
                return false;
            }

            return true;
#endif

            if (!TryGetNetworkObject(prefabReference, out _, out errorMessage))
            {
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

            if (!IsAliveUnityObject(prefabReference))
            {
                errorMessage = InvalidPrefabMessage;
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

#if UNITY_EDITOR
        private static bool TryIsEditorProjectPrefabAsset(GameObject prefabReference, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                if (prefabReference.scene.IsValid())
                {
                    errorMessage = InvalidPrefabMessage;
                    return false;
                }

                return true;
            }
            catch (MissingReferenceException)
            {
                errorMessage = InvalidPrefabMessage;
                return false;
            }
        }
#endif

        #endregion
    }
}
