using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_LocalMultiplayerHostDiscovery
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Discovers real local host sessions for the join-game server list.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses cross-process heartbeat cache only. No fake localhost seeding.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_LocalMultiplayerHostDiscovery
    {
        #region Public Methods

        public static List<CCS_MultiplayerServerListEntry> DiscoverLocalHosts()
        {
            List<CCS_MultiplayerServerListEntry> discoveredHosts = new List<CCS_MultiplayerServerListEntry>();

            if (CCS_LocalMultiplayerHostSessionCache.TryReadActiveSession(out CCS_MultiplayerServerListEntry activeSession))
            {
                discoveredHosts.Add(activeSession);
            }

            return discoveredHosts;
        }

        #endregion
    }
}
