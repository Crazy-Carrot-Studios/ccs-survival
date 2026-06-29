using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NetworkPrefabsRegistry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Build-safe registry of all required test network prefabs.
// PLACEMENT: Assets/CCS/Modules/CharacterController/Runtime/Netcode/Resources/
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Editor setup/validation aid only. Runtime host uses NetworkConfig + guard fallbacks.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public sealed class CCS_NetworkPrefabsRegistry : ScriptableObject
    {
        #region Variables

        [SerializeField] private GameObject[] networkPrefabs;

        #endregion

        #region Public Methods

        public GameObject[] NetworkPrefabs => networkPrefabs;

        public GameObject NetworkPlayerPrefab => TryGetValidPrefabAt(0, out GameObject playerPrefab) ? playerPrefab : null;

        public int Count => networkPrefabs != null ? networkPrefabs.Length : 0;

        public GameObject GetPrefab(int index)
        {
            if (networkPrefabs == null || index < 0 || index >= networkPrefabs.Length)
            {
                return null;
            }

            return networkPrefabs[index];
        }

        public GameObject[] GetValidPrefabs()
        {
            List<GameObject> validPrefabs = new List<GameObject>();
            if (networkPrefabs == null)
            {
                return validPrefabs.ToArray();
            }

            for (int i = 0; i < networkPrefabs.Length; i++)
            {
                if (TryGetValidPrefabAt(i, out GameObject prefab))
                {
                    validPrefabs.Add(prefab);
                }
            }

            return validPrefabs.ToArray();
        }

        public bool TryGetValidPrefabAt(int index, out GameObject prefab)
        {
            prefab = null;
            if (networkPrefabs == null || index < 0 || index >= networkPrefabs.Length)
            {
                return false;
            }

            return TryResolvePrefab(networkPrefabs[index], out prefab, out _);
        }

        public static bool TryResolvePrefab(
            GameObject candidate,
            out GameObject prefab,
            out NetworkObject networkObject)
        {
            prefab = null;
            networkObject = null;

            try
            {
                if (ReferenceEquals(candidate, null) || candidate == null)
                {
                    return false;
                }

                networkObject = candidate.GetComponent<NetworkObject>();
                if (networkObject == null)
                {
                    return false;
                }

                prefab = candidate;
                return true;
            }
            catch (MissingReferenceException)
            {
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
