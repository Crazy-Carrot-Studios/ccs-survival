using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ResourceRespawnState
// CATEGORY: Modules / WorldResources / Runtime / Respawn
// PURPOSE: Tracks respawn timer state for a depleted resource node.
// PLACEMENT: Managed by CCS_ResourceRespawnService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No world streaming or save integration in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceRespawnState
    {
        #region Public Methods

        public CCS_ResourceRespawnState(
            string nodeKey,
            CCS_ResourceDefinition resourceDefinition,
            float respawnDurationSeconds,
            Action<CCS_ResourceRespawnState> onRespawnReady)
        {
            NodeKey = nodeKey ?? string.Empty;
            ResourceDefinition = resourceDefinition;
            RespawnDurationSeconds = respawnDurationSeconds < 0f ? 0f : respawnDurationSeconds;
            RemainingSeconds = RespawnDurationSeconds;
            OnRespawnReady = onRespawnReady;
        }

        #endregion

        #region Properties

        public string NodeKey { get; }

        public CCS_ResourceDefinition ResourceDefinition { get; }

        public float RespawnDurationSeconds { get; }

        public float RemainingSeconds { get; set; }

        public Action<CCS_ResourceRespawnState> OnRespawnReady { get; }

        #endregion
    }
}
