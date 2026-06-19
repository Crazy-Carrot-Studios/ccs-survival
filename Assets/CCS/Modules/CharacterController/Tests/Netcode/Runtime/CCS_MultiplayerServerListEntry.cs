using System.Globalization;

// =============================================================================
// SCRIPT: CCS_MultiplayerServerListEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Placeholder server list row data for local multiplayer hosting UI.
// PLACEMENT: Runtime data. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only. LAN discovery can populate this later.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public readonly struct CCS_MultiplayerServerListEntry
    {
        #region Variables

        public string DisplayName { get; }
        public string Address { get; }
        public ushort Port { get; }

        #endregion

        #region Public Methods

        public CCS_MultiplayerServerListEntry(string displayName, string address, ushort port)
        {
            DisplayName = displayName;
            Address = address;
            Port = port;
        }

        public string GetListLabel()
        {
            return $"{DisplayName} - {Address}:{Port.ToString(CultureInfo.InvariantCulture)}";
        }

        public static CCS_MultiplayerServerListEntry CreateLocalhostDefault()
        {
            return new CCS_MultiplayerServerListEntry(
                CCS_NetcodeTestConstants.DefaultLocalhostServerDisplayName,
                CCS_NetcodeTestConstants.DefaultLocalhostAddress,
                CCS_NetcodeTestConstants.DefaultServerPort);
        }

        #endregion
    }
}
