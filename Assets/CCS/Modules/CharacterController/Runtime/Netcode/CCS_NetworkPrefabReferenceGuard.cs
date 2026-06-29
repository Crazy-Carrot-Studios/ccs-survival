using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkPrefabReferenceGuard
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Repairs broken network prefab references before NetworkManager.Awake.
// PLACEMENT: PF_CCS_NetworkManager root (execution order -1000).
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Runtime uses NetworkConfig + serialized fallbacks only. No Resources registry.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    [DefaultExecutionOrder(-1000)]
    public sealed class CCS_NetworkPrefabReferenceGuard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private GameObject networkedPlayerPrefabFallback;
        [SerializeField] private GameObject toggleInteractablePrefabFallback;
        [SerializeField] private GameObject aiBanditPrefabFallback;

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

            int repairedCount = RepairNetworkConfigReferences(
                networkManager.NetworkConfig,
                resolvedPlayerPrefab,
                networkedPlayerPrefabFallback,
                toggleInteractablePrefabFallback,
                aiBanditPrefabFallback);

            if (CCS_NetcodeNetworkConfigValidationUtility.TryValidateForStart(networkManager, out _))
            {
                Debug.Log("[Netcode] Network player prefab references validated.");
            }
            else if (repairedCount > 0)
            {
                Debug.Log($"[Netcode] Repaired {repairedCount} network prefab reference(s) from serialized fallbacks.");
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
            return CCS_NetworkPrefabsRegistry.TryResolvePrefab(candidate, out validPrefab, out _);
        }

        private static int RepairNetworkConfigReferences(
            NetworkConfig networkConfig,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback,
            GameObject banditFallback)
        {
            int repairedCount = 0;

            if (!TryGetValidPrefab(networkConfig.PlayerPrefab, out _))
            {
                networkConfig.PlayerPrefab = playerPrefab;
                repairedCount++;
            }

            if (networkConfig.Prefabs == null || networkConfig.Prefabs.NetworkPrefabsLists == null)
            {
                return repairedCount;
            }

            for (int i = 0; i < networkConfig.Prefabs.NetworkPrefabsLists.Count; i++)
            {
                NetworkPrefabsList prefabsList = networkConfig.Prefabs.NetworkPrefabsLists[i];
                if (prefabsList == null)
                {
                    continue;
                }

                repairedCount += RepairPrefabsListEntries(
                    prefabsList,
                    playerPrefab,
                    playerFallback,
                    toggleFallback,
                    banditFallback);
            }

            return repairedCount;
        }

        private static int RepairPrefabsListEntries(
            NetworkPrefabsList prefabsList,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback,
            GameObject banditFallback)
        {
            FieldInfo listField = typeof(NetworkPrefabsList).GetField(
                "List",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (listField == null)
            {
                return 0;
            }

            if (listField.GetValue(prefabsList) is not List<NetworkPrefab> entries)
            {
                return 0;
            }

            int repairedCount = 0;
            for (int i = entries.Count - 1; i >= 0; i--)
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
                    toggleFallback,
                    banditFallback);
                if (replacementPrefab == null)
                {
                    entries.RemoveAt(i);
                    repairedCount++;
                    continue;
                }

                entry.Prefab = replacementPrefab;
                entry.SourcePrefabToOverride = null;
                entry.OverridingTargetPrefab = null;
                entry.SourceHashToOverride = 0;
                entries[i] = entry;
                repairedCount++;
            }

            return repairedCount;
        }

        private static GameObject ResolveReplacementPrefab(
            int entryIndex,
            GameObject playerPrefab,
            GameObject playerFallback,
            GameObject toggleFallback,
            GameObject banditFallback)
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

            if (entryIndex == 1)
            {
                if (TryGetValidPrefab(toggleFallback, out GameObject validToggle))
                {
                    return validToggle;
                }

                return ResolvePrefabAtRequiredPath(1);
            }

            if (entryIndex == 2)
            {
                if (TryGetValidPrefab(banditFallback, out GameObject validBandit))
                {
                    return validBandit;
                }

                return ResolvePrefabAtRequiredPath(2);
            }

            return ResolvePrefabAtRequiredPath(entryIndex);
        }

        private static GameObject ResolvePrefabAtRequiredPath(int entryIndex)
        {
            CCS_NetworkPrefabsRegistry registry = Resources.Load<CCS_NetworkPrefabsRegistry>(
                CCS_NetcodeConstants.NetworkTestPrefabsRegistryResourceName);
            if (registry != null && registry.TryGetValidPrefabAt(entryIndex, out GameObject prefab))
            {
                return prefab;
            }

            return null;
        }

        #endregion
    }
}
