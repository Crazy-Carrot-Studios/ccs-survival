using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPlayerDisplayProfile
// CATEGORY: Modules / CharacterController / Tests / Runtime / Profiles
// PURPOSE: Lightweight display and tuning references for master test players.
// PLACEMENT: ScriptableObject asset under Profiles/TestPlayer/.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: References existing movement/camera profiles. No gameplay logic.
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
        [SerializeField] private Vector3 nameplateLocalPosition = new Vector3(0f, 2.2f, 0f);

        [Header("Tuning References")]
        [Tooltip("Movement profile applied to offline solo test players.")]
        [SerializeField] private CCS_CharacterMovementProfile movementProfile;

        [Tooltip("Camera profile used for follow height and polish prep.")]
        [SerializeField] private CCS_CharacterCameraProfile cameraProfile;

        [Header("Visual Prep")]
        [Tooltip("Local position for VisualGlasses. Polish pass may adjust this value.")]
        [SerializeField] private Vector3 glassesLocalPosition = new Vector3(0f, 1.65f, 0.222f);

        [Tooltip("Local scale for VisualGlasses. Polish pass may adjust this value.")]
        [SerializeField] private Vector3 glassesLocalScale = new Vector3(0.3f, 0.3f, 0.3f);

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
            : 1.05f;

        public Vector3 GlassesLocalPosition => glassesLocalPosition;

        public Vector3 GlassesLocalScale => glassesLocalScale;

        #endregion
    }
}
