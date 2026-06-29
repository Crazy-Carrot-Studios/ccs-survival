using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterJumpDebugLog
// CATEGORY: Modules / CharacterController / Runtime / Diagnostics
// PURPOSE: Optional solo jump audit logging for character motor debugging.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Toggle via CCS_CharacterControllerConstants.EnableJumpDebugLogs.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics {
    public static class CCS_CharacterJumpDebugLog
    {
        private const string LogPrefix = "[CCS Jump Debug]";

        #region Properties

        public static bool IsEnabled => CCS_CharacterControllerConstants.EnableJumpDebugLogs;

        #endregion

        #region Public Methods

        public static void Log(
            CCS_CharacterJumpAuditHook.JumpSample sample,
            ulong networkObjectId,
            bool isOwner,
            bool hasNetworkObject)
        {
            if (!IsEnabled || sample.Source == null)
            {
                return;
            }

            string networkObjectIdLabel = hasNetworkObject ? networkObjectId.ToString() : "n/a";

            Debug.Log(
                LogPrefix
                + $" NetworkObjectId={networkObjectIdLabel}"
                + $" IsOwner={isOwner}"
                + $" Grounded={sample.Grounded}"
                + $" JumpPressed={sample.JumpPressed}"
                + $" VerticalVelocity={Format(sample.VerticalVelocity)}"
                + $" PositionBefore={Format(sample.PositionBefore)}"
                + $" PositionAfter={Format(sample.PositionAfter)}"
                + (sample.JumpExecuted ? " JumpExecuted=true" : string.Empty),
                sample.Source);
        }

        #endregion

        #region Private Methods

        private static string Format(Vector3 value)
        {
            return $"({Format(value.x)},{Format(value.y)},{Format(value.z)})";
        }

        private static string Format(float value)
        {
            return value.ToString("0.###");
        }

        #endregion
    }
}
