using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerRespawnPoint
// CATEGORY: Modules / PlayerDeath / Runtime / Components
// PURPOSE: World spawn transform used by CCS_PlayerDeathService on respawn.
// PLACEMENT: Bootstrap scene spawn markers such as CCS_PlayerRespawnPoint_Bootstrap.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Optional spawn id for future multi-spawn support.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    public sealed class CCS_PlayerRespawnPoint : MonoBehaviour
    {
        #region Variables

        [Header("Spawn Identity")]
        [Tooltip("Stable spawn id used to resolve this respawn point.")]
        [SerializeField] private string spawnId = "ccs.survival.spawn.bootstrap";

        #endregion

        #region Properties

        public string SpawnId => spawnId;

        public Vector3 SpawnPosition => transform.position;

        public Quaternion SpawnRotation => transform.rotation;

        #endregion

        #region Public Methods

        public void ConfigureRuntime(string configuredSpawnId)
        {
            if (!string.IsNullOrWhiteSpace(configuredSpawnId))
            {
                spawnId = configuredSpawnId;
            }
        }

        #endregion
    }
}
