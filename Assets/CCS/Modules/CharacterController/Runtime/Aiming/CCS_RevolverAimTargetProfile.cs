using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetProfile
// CATEGORY: Modules / CharacterController / Runtime / Aiming
// PURPOSE: Tunable camera/mouse aim target resolver settings.
// PLACEMENT: ScriptableObject under Profiles/Aiming/.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only. Does not drive gameplay fire, damage, ammo, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_RevolverAimTargetProfile",
        menuName = "CCS/Character Controller/Revolver Aim Target Profile",
        order = 22)]
    public sealed class CCS_RevolverAimTargetProfile : ScriptableObject
    {
        [SerializeField] private LayerMask aimLayerMask = Physics.DefaultRaycastLayers;

        [SerializeField] private LayerMask obstructionLayerMask = Physics.DefaultRaycastLayers;

        [SerializeField] private float cameraRayDistance = 120f;

        [SerializeField] private float fallbackDistance = 80f;

        [SerializeField] private float targetSmoothingTime = 0.045f;

        [SerializeField] private float maxTargetSnapDistance = 8f;

        [SerializeField] private float lastValidTargetHoldSeconds = 0.2f;

        [SerializeField] private float minimumValidDistance = 1f;

        [SerializeField] private float nearCameraRejectDistance = 0.35f;

        [SerializeField] private bool holdLastValidTargetWhenInvalid = true;

        [SerializeField] private bool smoothTarget = true;

        [SerializeField] private bool drawDebugRayWhenDiagnosticsEnabled = true;

        public LayerMask AimLayerMask => aimLayerMask;

        public LayerMask ObstructionLayerMask => obstructionLayerMask;

        public float CameraRayDistance => cameraRayDistance;

        public float FallbackDistance => fallbackDistance;

        public float TargetSmoothingTime => targetSmoothingTime;

        public float MaxTargetSnapDistance => maxTargetSnapDistance;

        public float LastValidTargetHoldSeconds => lastValidTargetHoldSeconds;

        public float MinimumValidDistance => minimumValidDistance;

        public float NearCameraRejectDistance => nearCameraRejectDistance;

        public bool HoldLastValidTargetWhenInvalid => holdLastValidTargetWhenInvalid;

        public bool SmoothTarget => smoothTarget;

        public bool DrawDebugRayWhenDiagnosticsEnabled => drawDebugRayWhenDiagnosticsEnabled;
    }
}
