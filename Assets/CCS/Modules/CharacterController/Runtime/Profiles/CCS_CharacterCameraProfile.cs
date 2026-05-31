using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Look sensitivity and pitch clamp tuning for character camera foundation.
// PLACEMENT: Embedded on CCS_CharacterControllerProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No Cinemachine or collision in 0.3.8. Full input actions deferred.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [System.Serializable]
    public sealed class CCS_CharacterCameraProfile
    {
        #region Variables

        [Header("Look Sensitivity")]
        [Tooltip("Yaw degrees per second per unit of horizontal look input.")]
        [SerializeField] private float horizontalSensitivity = 120f;

        [Tooltip("Pitch degrees per second per unit of vertical look input.")]
        [SerializeField] private float verticalSensitivity = 90f;

        [Header("Pitch Clamp")]
        [Tooltip("Minimum pitch angle in degrees (looking down).")]
        [SerializeField] private float minPitch = -80f;

        [Tooltip("Maximum pitch angle in degrees (looking up).")]
        [SerializeField] private float maxPitch = 80f;

        [Header("Follow")]
        [Tooltip("Optional vertical offset for camera follow anchor.")]
        [SerializeField] private float followHeightOffset = 1.6f;

        [Header("Pointer Look")]
        [Tooltip("Scale applied to mouse/pointer look deltas (not multiplied by deltaTime).")]
        [SerializeField] private float pointerLookScale = 0.08f;

        [Tooltip("Input magnitude above this value is treated as pointer delta instead of stick input.")]
        [SerializeField] private float pointerLookThreshold = 1f;

        #endregion

        #region Properties

        public float HorizontalSensitivity => horizontalSensitivity;

        public float VerticalSensitivity => verticalSensitivity;

        public float MinPitch => minPitch;

        public float MaxPitch => maxPitch;

        public float FollowHeightOffset => followHeightOffset;

        public float PointerLookScale => pointerLookScale;

        public float PointerLookThreshold => pointerLookThreshold;

        #endregion
    }
}
