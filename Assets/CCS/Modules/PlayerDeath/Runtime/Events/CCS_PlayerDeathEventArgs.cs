using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerDeathEventArgs
// CATEGORY: Modules / PlayerDeath / Runtime / Events
// PURPOSE: Event payload for player death and respawn notifications.
// PLACEMENT: Raised by CCS_PlayerDeathService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Includes cause stat and world position for future UI hooks.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    public sealed class CCS_PlayerDeathEventArgs
    {
        #region Variables

        private readonly string causeMessage;
        private readonly Vector3 worldPosition;
        private readonly string spawnId;

        #endregion

        #region Public Methods

        public CCS_PlayerDeathEventArgs(string causeMessage, Vector3 worldPosition, string spawnId = "")
        {
            this.causeMessage = causeMessage ?? string.Empty;
            this.worldPosition = worldPosition;
            this.spawnId = spawnId ?? string.Empty;
        }

        #endregion

        #region Properties

        public string CauseMessage => causeMessage;

        public Vector3 WorldPosition => worldPosition;

        public string SpawnId => spawnId;

        #endregion
    }
}
