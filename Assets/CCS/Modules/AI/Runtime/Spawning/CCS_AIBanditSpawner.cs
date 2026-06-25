using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditSpawner
// CATEGORY: Modules / AI / Runtime / Spawning
// PURPOSE: Spawns one AI bandit in server and offline master-test sessions.
// PLACEMENT: Master Test scene root object (CCS_AIBanditSpawner).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Skips duplicate spawn and offsets from host spawn/player position.
// =============================================================================

namespace CCS.Modules.AI
{
    [DefaultExecutionOrder(110)]
    public sealed class CCS_AIBanditSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject aiBanditPrefab;
        [SerializeField] private Transform spawnReference;
        [SerializeField] private Vector3 spawnOffset = default;
        [SerializeField] private bool enableSpawnerDebugLogs;

        private bool spawned;

        private void Awake()
        {
            if (spawnOffset == default)
            {
                spawnOffset = new Vector3(
                    CCS_AIConstants.DefaultSpawnSideOffset,
                    0f,
                    CCS_AIConstants.DefaultSpawnDistanceFromPlayer);
            }
        }

        private void Start()
        {
            TrySpawn();
        }

        private void TrySpawn()
        {
            if (spawned)
            {
                return;
            }

            if (aiBanditPrefab == null)
            {
                Debug.LogWarning("[AI] Bandit spawner missing prefab reference.", this);
                return;
            }

            if (NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsListening
                && !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            Vector3 basePosition = ResolveSpawnPosition();
            Quaternion spawnRotation = Quaternion.identity;
            GameObject instance = Instantiate(aiBanditPrefab, basePosition + spawnOffset, spawnRotation);
            instance.name = aiBanditPrefab.name;

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject != null
                && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsListening
                && NetworkManager.Singleton.IsServer
                && !networkObject.IsSpawned)
            {
                networkObject.Spawn(destroyWithScene: true);
            }

            spawned = true;

            if (enableSpawnerDebugLogs)
            {
                Debug.Log("[AI] Spawned AI bandit.", this);
            }
        }

        private Vector3 ResolveSpawnPosition()
        {
            if (spawnReference != null)
            {
                return spawnReference.position;
            }

            GameObject hostSpawn = GameObject.Find("TP_Spawn_Host");
            if (hostSpawn != null)
            {
                return hostSpawn.transform.position;
            }

            return transform.position;
        }
    }
}
