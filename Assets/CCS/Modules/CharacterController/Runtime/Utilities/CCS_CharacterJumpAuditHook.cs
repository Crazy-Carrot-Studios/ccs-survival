using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterJumpAuditHook
// CATEGORY: Modules / CharacterController / Runtime / Utilities
// PURPOSE: Optional jump audit callback without Netcode dependency in motor.
// PLACEMENT: Static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Runtime and netcode layers register callbacks when jump audit logs are enabled.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterJumpAuditHook
    {
        #region Variables

        public static Action<JumpSample> JumpLogged;

        #endregion

        #region Nested Types

        public struct JumpSample
        {
            public GameObject Source;
            public bool Grounded;
            public bool JumpPressed;
            public float VerticalVelocity;
            public Vector3 PositionBefore;
            public Vector3 PositionAfter;
            public bool JumpExecuted;
        }

        #endregion
    }
}
