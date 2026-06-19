using CCS.Modules.CharacterController;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterJumpNetcodeAuditHookRegistrar
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Bridges jump audit samples into networked jump debug logs.
// PLACEMENT: Runtime static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Registered once at load when jump audit logs are enabled.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_CharacterJumpNetcodeAuditHookRegistrar
    {
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
            if (!CCS_NetworkControllerAuditDiagnostics.JumpLogsEnabled || sample.Source == null)
            {
                return;
            }

            NetworkObject networkObject = sample.Source.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                return;
            }

            CCS_CharacterJumpDebugLog.Log(
                sample,
                networkObject.NetworkObjectId,
                networkObject.IsOwner,
                true);
        }

        #endregion
    }
}
