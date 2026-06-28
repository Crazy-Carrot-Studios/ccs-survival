using CCS.Modules.Attributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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
        private const float NavMeshSampleRadius = 5f;

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

            Vector3 intendedPosition = ResolveSpawnPosition() + spawnOffset;
            Vector3 spawnPosition = SampleNavMeshSpawnPosition(intendedPosition, out bool foundNavMesh);
            if (!foundNavMesh)
            {
                Debug.LogError(
                    "[AI Bandit Spawner] No NavMesh position found within "
                    + NavMeshSampleRadius
                    + "m of intended spawn "
                    + intendedPosition
                    + ". Using fallback position for debug.",
                    this);
            }

            Quaternion spawnRotation = Quaternion.identity;
            GameObject instance = Instantiate(aiBanditPrefab, spawnPosition, spawnRotation);
            instance.name = aiBanditPrefab.name;

            NavMeshAgent navMeshAgent = instance.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                if (navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.Warp(spawnPosition);
                }
                else if (NavMesh.SamplePosition(
                    spawnPosition,
                    out NavMeshHit warpHit,
                    NavMeshSampleRadius,
                    NavMesh.AllAreas))
                {
                    navMeshAgent.Warp(warpHit.position);
                    spawnPosition = warpHit.position;
                }
            }

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
            Debug.Log("[AI Bandit Spawner] Spawned AI_Bandit on NavMesh at " + spawnPosition + ".", this);

            if (enableSpawnerDebugLogs)
            {
                Debug.Log("[AI] Spawned AI bandit.", this);
            }
        }

        private static Vector3 SampleNavMeshSpawnPosition(Vector3 intendedPosition, out bool foundNavMesh)
        {
            if (NavMesh.SamplePosition(
                intendedPosition,
                out NavMeshHit hit,
                NavMeshSampleRadius,
                NavMesh.AllAreas))
            {
                foundNavMesh = true;
                return hit.position;
            }

            foundNavMesh = false;
            return intendedPosition;
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
