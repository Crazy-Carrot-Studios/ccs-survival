using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMotorAuditHookRegistrar
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Bridges motor audit samples into network controller audit logs.
// PLACEMENT: Runtime static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Registered once at load when audit logs are enabled.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_CharacterMotorAuditHookRegistrar
    {
        #region Public Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void RegisterMotorAuditHook()
        {
            CCS_CharacterMotorAuditHook.MoveLogged = HandleMotorMoveSample;
        }

        #endregion

        #region Private Methods

        private static void HandleMotorMoveSample(CCS_CharacterMotorAuditHook.MotorMoveSample sample)
        {
            if (!CCS_NetworkControllerAuditDiagnostics.MotorMoveLogsEnabled || sample.Source == null)
            {
                return;
            }

            Unity.Netcode.NetworkObject networkObject = sample.Source.GetComponent<Unity.Netcode.NetworkObject>();
            CCS_NetworkControllerAuditDiagnostics.LogMotorMove(
                networkObject,
                networkObject != null && networkObject.IsOwner,
                sample.DeltaTime,
                sample.MoveInput,
                sample.Velocity,
                sample.PositionBefore,
                sample.PositionAfter,
                sample.CameraForward,
                sample.MovementCamera);
        }

        #endregion
    }
}
