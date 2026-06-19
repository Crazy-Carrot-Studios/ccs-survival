using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterJumpAuditHookRegistrar
// CATEGORY: Modules / CharacterController / Runtime / Diagnostics
// PURPOSE: Registers jump audit logging for solo and local motor debugging.
// PLACEMENT: Runtime static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Skips networked objects; netcode test layer logs those with ownership context.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterJumpAuditHookRegistrar
    {
        private const string NetworkObjectTypeName = "Unity.Netcode.NetworkObject";

        #region Public Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void RegisterJumpAuditHook()
        {
            CCS_CharacterJumpAuditHook.JumpLogged += HandleJumpSample;
        }

        #endregion

        #region Private Methods

        private static void HandleJumpSample(CCS_CharacterJumpAuditHook.JumpSample sample)
        {
            if (!CCS_CharacterJumpDebugLog.IsEnabled || sample.Source == null)
            {
                return;
            }

            if (sample.Source.GetComponent(NetworkObjectTypeName) != null)
            {
                return;
            }

            CCS_CharacterJumpDebugLog.Log(sample, 0UL, true, false);
        }

        #endregion
    }
}
