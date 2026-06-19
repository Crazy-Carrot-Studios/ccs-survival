using Unity.Netcode.Components;

// =============================================================================
// SCRIPT: CCS_ClientOwnerNetworkTransform
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Owner-authoritative NetworkTransform for CharacterController test players.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Owner runs CCS_CharacterMotor; remotes interpolate position only.
//        Body yaw is replicated separately so motor rotation does not fight NetworkTransform.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_ClientOwnerNetworkTransform : NetworkTransform
    {
        #region Protected Methods

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        #endregion
    }
}
