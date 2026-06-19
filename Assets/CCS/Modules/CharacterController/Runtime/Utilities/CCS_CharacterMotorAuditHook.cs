using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMotorAuditHook
// CATEGORY: Modules / CharacterController / Runtime / Utilities
// PURPOSE: Optional motor move audit callback without Netcode dependency in motor.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Netcode test layer registers the callback when audit logs are enabled.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterMotorAuditHook
    {
        #region Variables

        public static Action<MotorMoveSample> MoveLogged;

        #endregion

        #region Nested Types

        public struct MotorMoveSample
        {
            public GameObject Source;
            public float DeltaTime;
            public Vector2 MoveInput;
            public Vector3 Velocity;
            public Vector3 PositionBefore;
            public Vector3 PositionAfter;
            public Vector3 CameraForward;
            public Camera MovementCamera;
        }

        #endregion
    }
}
