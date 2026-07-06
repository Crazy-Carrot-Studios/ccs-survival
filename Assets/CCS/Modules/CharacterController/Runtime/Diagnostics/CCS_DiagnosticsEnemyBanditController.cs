using CCS.Modules.AI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

// =============================================================================
// SCRIPT: CCS_DiagnosticsEnemyBanditController
// CATEGORY: Modules / CharacterController / Runtime / Diagnostics
// PURPOSE: Spawns or despawns one AI bandit when validation diagnostics toggle changes.
// PLACEMENT: CCS_DiagnosticsManager on SCN_CCS_CharacterController_Validation only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Replaces AIBanditSpawner auto-start for stripped baseline validation scenes.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics
{
    [DefaultExecutionOrder(115)]
    public sealed class CCS_DiagnosticsEnemyBanditController : MonoBehaviour
    {
        private const float NavMeshSampleRadius = 5f;

        [SerializeField] private GameObject aiBanditPrefab;
        [SerializeField] private Transform spawnReference;
        [SerializeField] private Vector3 spawnOffset = new Vector3(
            CCS_AIConstants.DefaultSpawnSideOffset,
            0f,
            CCS_AIConstants.DefaultSpawnDistanceFromPlayer);

        private GameObject spawnedInstance;
        private bool previousSpawnRequested;

        private void Start()
        {
            previousSpawnRequested = CCS_DiagnosticsEnemyBanditRegistry.EnableEnemy;
            ApplySpawnState(previousSpawnRequested);
        }

        private void Update()
        {
            bool requested = CCS_DiagnosticsEnemyBanditRegistry.EnableEnemy;
            if (requested == previousSpawnRequested)
            {
                return;
            }

            previousSpawnRequested = requested;
            ApplySpawnState(requested);
        }

        private void OnDestroy()
        {
            DespawnBandit();
        }

        private void ApplySpawnState(bool shouldSpawn)
        {
            if (shouldSpawn)
            {
                SpawnBandit();
            }
            else
            {
                DespawnBandit();
            }
        }

        private void SpawnBandit()
        {
            if (spawnedInstance != null || aiBanditPrefab == null)
            {
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
                Debug.LogWarning(
                    "[Diagnostics Enemy Bandit] No NavMesh near "
                    + intendedPosition
                    + "; using fallback position.",
                    this);
            }

            spawnedInstance = Instantiate(aiBanditPrefab, spawnPosition, Quaternion.identity);
            spawnedInstance.name = aiBanditPrefab.name;

            NavMeshAgent navMeshAgent = spawnedInstance.GetComponent<NavMeshAgent>();
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
                }
            }

            NetworkObject networkObject = spawnedInstance.GetComponent<NetworkObject>();
            if (networkObject != null
                && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsListening
                && NetworkManager.Singleton.IsServer
                && !networkObject.IsSpawned)
            {
                networkObject.Spawn(destroyWithScene: true);
            }
        }

        private void DespawnBandit()
        {
            if (spawnedInstance == null)
            {
                return;
            }

            NetworkObject networkObject = spawnedInstance.GetComponent<NetworkObject>();
            if (networkObject != null
                && networkObject.IsSpawned
                && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsServer)
            {
                networkObject.Despawn(true);
            }
            else
            {
                Destroy(spawnedInstance);
            }

            spawnedInstance = null;
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
    }
}
