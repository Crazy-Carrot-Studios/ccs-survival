using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkPrefabReferenceGuard
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Repairs broken network prefab references before NetworkManager.Awake.
// PLACEMENT: PF_CCS_TestNetworkManager root (execution order -1000).
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Guards against stale sub-object fileID references in NetworkPrefabsList.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CCS_NetworkPrefabReferenceGuard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject networkedPlayerPrefabFallback;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            NetworkManager networkManager = GetComponent<NetworkManager>();
            if (networkManager == null || networkManager.NetworkConfig == null)
            {
                return;
            }

            GameObject resolvedPrefab = ResolvePlayerPrefab(networkManager.NetworkConfig);
            if (resolvedPrefab == null)
            {
                Debug.LogError(
                    "[Netcode] Network player prefab reference is invalid. "
                    + "Run CCS -> Character Controller -> Scene -> Setup And Validate Multiplayer Hosting Scene.");
                return;
            }

            RepairNetworkConfigReferences(networkManager.NetworkConfig, resolvedPrefab);
        }

        #endregion

        #region Private Methods

        private GameObject ResolvePlayerPrefab(NetworkConfig networkConfig)
        {
            if (TryGetValidPrefab(networkConfig.PlayerPrefab, out GameObject validPrefab))
            {
                return validPrefab;
            }

            if (TryGetValidPrefab(networkedPlayerPrefabFallback, out validPrefab))
            {
                return validPrefab;
            }

            return null;
        }

        private static bool TryGetValidPrefab(GameObject candidate, out GameObject validPrefab)
        {
            validPrefab = null;
            if (candidate == null)
            {
                return false;
            }

            try
            {
                if (candidate.GetComponent<NetworkObject>() == null)
                {
                    return false;
                }

                validPrefab = candidate;
                return true;
            }
            catch (MissingReferenceException)
            {
                return false;
            }
        }

        private static void RepairNetworkConfigReferences(NetworkConfig networkConfig, GameObject playerPrefab)
        {
            if (!TryGetValidPrefab(networkConfig.PlayerPrefab, out _))
            {
                networkConfig.PlayerPrefab = playerPrefab;
            }

            if (networkConfig.Prefabs == null || networkConfig.Prefabs.NetworkPrefabsLists == null)
            {
                return;
            }

            for (int i = 0; i < networkConfig.Prefabs.NetworkPrefabsLists.Count; i++)
            {
                NetworkPrefabsList prefabsList = networkConfig.Prefabs.NetworkPrefabsLists[i];
                if (prefabsList == null)
                {
                    continue;
                }

                RepairPrefabsListEntries(prefabsList, playerPrefab);
            }
        }

        private static void RepairPrefabsListEntries(NetworkPrefabsList prefabsList, GameObject playerPrefab)
        {
            FieldInfo listField = typeof(NetworkPrefabsList).GetField(
                "List",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (listField == null)
            {
                return;
            }

            if (listField.GetValue(prefabsList) is not List<NetworkPrefab> entries)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                NetworkPrefab entry = entries[i];
                if (entry.Override != NetworkPrefabOverride.None)
                {
                    continue;
                }

                if (TryGetValidPrefab(entry.Prefab, out _))
                {
                    continue;
                }

                entry.Prefab = playerPrefab;
                entry.SourcePrefabToOverride = null;
                entry.OverridingTargetPrefab = null;
                entry.SourceHashToOverride = 0;
                entries[i] = entry;
            }
        }

        #endregion
    }
}
