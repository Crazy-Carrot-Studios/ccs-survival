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
// NOTES: Runtime uses NetworkConfig + serialized fallbacks only. No Resources registry.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CCS_NetworkPrefabReferenceGuard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject networkedPlayerPrefabFallback;
        [SerializeField] private GameObject toggleInteractablePrefabFallback;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            NetworkManager networkManager = GetComponent<NetworkManager>();
            if (networkManager == null || networkManager.NetworkConfig == null)
            {
                return;
            }

            GameObject resolvedPlayerPrefab = ResolvePlayerPrefab(networkManager.NetworkConfig);
            if (resolvedPlayerPrefab == null)
            {
                Debug.LogError(
                    "[Netcode] Network player prefab reference is invalid. "
                    + "Run CCS -> Character Controller -> Scene -> Setup And Validate Multiplayer Hosting Scene.");
                return;
            }

            RepairNetworkConfigReferences(
                networkManager.NetworkConfig,
                resolvedPlayerPrefab,
                networkedPlayerPrefabFallback,
                toggleInteractablePrefabFallback);

            if (CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(networkManager, out _))
            {
                Debug.Log("[Netcode] Network player prefab references validated.");
            }
            else
            {
                Debug.Log("[Netcode] Network prefab references repaired from serialized fallbacks.");
            }
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
            return CCS_NetworkTestPrefabsRegistry.TryResolvePrefab(candidate, out validPrefab, out _);
        }

        private static void RepairNetworkConfigReferences(
            NetworkConfig networkConfig,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback)
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

                RepairPrefabsListEntries(prefabsList, playerPrefab, playerFallback, toggleFallback);
            }
        }

        private static void RepairPrefabsListEntries(
            NetworkPrefabsList prefabsList,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback)
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

                GameObject replacementPrefab = ResolveReplacementPrefab(
                    i,
                    playerPrefab,
                    playerFallback,
                    toggleFallback);
                if (replacementPrefab == null)
                {
                    continue;
                }

                entry.Prefab = replacementPrefab;
                entry.SourcePrefabToOverride = null;
                entry.OverridingTargetPrefab = null;
                entry.SourceHashToOverride = 0;
                entries[i] = entry;
            }
        }

        private static GameObject ResolveReplacementPrefab(
            int entryIndex,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback)
        {
            if (entryIndex == 0)
            {
                if (TryGetValidPrefab(playerPrefab, out GameObject validPlayer))
                {
                    return validPlayer;
                }

                if (TryGetValidPrefab(playerFallback, out validPlayer))
                {
                    return validPlayer;
                }

                return null;
            }

            if (entryIndex == 1 && TryGetValidPrefab(toggleFallback, out GameObject validToggle))
            {
                return validToggle;
            }

            return null;
        }

        #endregion
    }
}
