using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPlayerDisplayProfile
// CATEGORY: Modules / CharacterController / Tests / Runtime / Profiles
// PURPOSE: Lightweight display and tuning references for master test players.
// PLACEMENT: ScriptableObject asset under Profiles/TestPlayer/.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: References existing movement/camera profiles. Visual layout tuned in v0.2.3.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [CreateAssetMenu(
        fileName = "CCS_TestPlayerDisplayProfile",
        menuName = "CCS/Character Controller/Test Player Display Profile",
        order = 2)]
    public sealed class CCS_TestPlayerDisplayProfile : ScriptableObject
    {
        #region Variables

        [Header("Display")]
        [Tooltip("Default overhead name when no network display name is available.")]
        [SerializeField] private string defaultDisplayName = "Player";

        [Tooltip("Local position for NameplateRoot on the test player prefab.")]
        [SerializeField] private Vector3 nameplateLocalPosition = new Vector3(0f, 2.12f, 0f);

        [Header("Tuning References")]
        [Tooltip("Movement profile applied to offline solo test players.")]
        [SerializeField] private CCS_CharacterMovementProfile movementProfile;

        [Tooltip("Camera profile used for follow height and polish prep.")]
        [SerializeField] private CCS_CharacterCameraProfile cameraProfile;

        [Header("Body Visual")]
        [Tooltip("Local position for CapsuleVisual. Must align with CharacterController center.")]
        [SerializeField] private Vector3 capsuleVisualLocalPosition = new Vector3(0f, 1f, 0f);

        [Tooltip("Local scale for CapsuleVisual. Default capsule mesh becomes 2m tall and 0.35m radius.")]
        [SerializeField] private Vector3 capsuleVisualLocalScale = new Vector3(0.7f, 1f, 0.7f);

        [Header("Glasses Visual")]
        [Tooltip("Local position for VisualGlasses.")]
        [SerializeField] private Vector3 glassesLocalPosition = new Vector3(0f, 1.68f, 0.26f);

        [Tooltip("Local euler rotation for VisualGlasses.")]
        [SerializeField] private Vector3 glassesLocalEuler = new Vector3(0f, 0f, 90f);

        [Tooltip("Local scale for VisualGlasses.")]
        [SerializeField] private Vector3 glassesLocalScale = new Vector3(0.22f, 0.05f, 0.05f);

        #endregion

        #region Properties

        public string DefaultDisplayName => string.IsNullOrWhiteSpace(defaultDisplayName)
            ? "Player"
            : defaultDisplayName.Trim();

        public Vector3 NameplateLocalPosition => nameplateLocalPosition;

        public CCS_CharacterMovementProfile MovementProfile => movementProfile;

        public CCS_CharacterCameraProfile CameraProfile => cameraProfile;

        public float CameraFollowHeight => cameraProfile != null
            ? cameraProfile.FollowTargetHeight
            : 0.92f;

        public Vector3 CapsuleVisualLocalPosition => capsuleVisualLocalPosition;

        public Vector3 CapsuleVisualLocalScale => capsuleVisualLocalScale;

        public Vector3 GlassesLocalPosition => glassesLocalPosition;

        public Vector3 GlassesLocalEuler => glassesLocalEuler;

        public Vector3 GlassesLocalScale => glassesLocalScale;

        #endregion
    }
}
