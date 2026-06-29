// =============================================================================
// SCRIPT: CCS_LocalMultiplayerPlayerNameCache
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Stores the pending local display name before a netcode player spawns.
// PLACEMENT: Static test-only cache. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No account services or persistent profile storage.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_LocalMultiplayerPlayerNameCache
    {
        #region Variables

        private static string pendingLocalDisplayName = CCS_NetcodeConstants.DefaultDisplayName;

        #endregion

        #region Properties

        public static string PendingLocalDisplayName => pendingLocalDisplayName;

        #endregion

        #region Public Methods

        public static void SetPendingLocalDisplayName(string displayName)
        {
            pendingLocalDisplayName = CCS_MultiplayerPlayerNameUtility.Sanitize(displayName);
        }

        public static void Clear()
        {
            pendingLocalDisplayName = CCS_NetcodeConstants.DefaultDisplayName;
        }

        #endregion
    }
}
