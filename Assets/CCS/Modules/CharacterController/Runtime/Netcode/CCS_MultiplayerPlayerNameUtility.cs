// =============================================================================
// SCRIPT: CCS_MultiplayerPlayerNameUtility
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Sanitizes multiplayer test display names for UI and net sync.
// PLACEMENT: Runtime utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-only name cleanup shared by hosting UI and server validation.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode
{
    public static class CCS_MultiplayerPlayerNameUtility
    {
        #region Public Methods

        public static string Sanitize(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return CCS_NetcodeConstants.DefaultDisplayName;
            }

            string trimmed = rawName.Trim();
            if (trimmed.Length > CCS_NetcodeConstants.MaxDisplayNameLength)
            {
                trimmed = trimmed.Substring(0, CCS_NetcodeConstants.MaxDisplayNameLength);
            }

            return trimmed;
        }

        #endregion
    }
}
