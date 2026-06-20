// =============================================================================
// SCRIPT: CCS_MultiplayerServerNameUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Sanitizes and builds default local server names for hosting UI.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only server name cleanup shared by hosting UI and session cache.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_MultiplayerServerNameUtility
    {
        #region Public Methods

        public static string Sanitize(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return CCS_NetcodeTestConstants.DefaultServerSessionLabel;
            }

            string trimmed = rawName.Trim();
            if (trimmed.Length > CCS_NetcodeTestConstants.MaxServerNameLength)
            {
                trimmed = trimmed.Substring(0, CCS_NetcodeTestConstants.MaxServerNameLength);
            }

            return trimmed;
        }

        public static string CreateDefaultServerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return CCS_NetcodeTestConstants.DefaultServerSessionLabel;
            }

            string sanitizedPlayerName = CCS_MultiplayerPlayerNameUtility.Sanitize(playerName);
            string possessiveName = $"{sanitizedPlayerName}'s Session";
            if (possessiveName.Length > CCS_NetcodeTestConstants.MaxServerNameLength)
            {
                return possessiveName.Substring(0, CCS_NetcodeTestConstants.MaxServerNameLength);
            }

            return possessiveName;
        }

        #endregion
    }
}
