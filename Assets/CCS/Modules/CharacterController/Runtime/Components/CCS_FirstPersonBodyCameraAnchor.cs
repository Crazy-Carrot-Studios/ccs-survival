using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirstPersonBodyCameraAnchor
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Head-tracked first-person camera anchor position with stable input-driven rotation.
// PLACEMENT: Player prefab root alongside CCS_CharacterCameraFollowAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-23
// NOTES: Position follows HumanBodyBones.Head. Yaw/pitch remain on body rig and CameraPitchTarget.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(-200)]
    public sealed class CCS_FirstPersonBodyCameraAnchor : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Animator characterAnimator;
        [SerializeField] private CCS_CharacterCameraFollowAnchor cameraFollowAnchor;
        [SerializeField] private Transform pitchTarget;
        [SerializeField] private Transform firstPersonCameraAnchor;

        private Transform headBone;

        #endregion

        #region Properties

        public bool IsHeadTrackingActive =>
            cameraFollowAnchor != null
            && cameraFollowAnchor.ActiveLookProfile != null
            && cameraFollowAnchor.ActiveLookProfile.CameraMode == CCS_CharacterCameraMode.FirstPersonBodyAware
            && cameraFollowAnchor.ActiveLookProfile.UseHeadTrackedAnchor;

        public bool HasResolvedHeadBone => headBone != null;

        public Vector3 CurrentSightlineOffset { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Reset()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
            ResolveHeadBone();
        }

        private void LateUpdate()
        {
            UpdateFirstPersonAnchorPosition();
        }

        #endregion

        #region Public Methods

        public void ResolveReferences()
        {
            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }

            if (cameraFollowAnchor != null)
            {
                if (pitchTarget == null)
                {
                    pitchTarget = cameraFollowAnchor.PitchTarget;
                }

                if (firstPersonCameraAnchor == null)
                {
                    firstPersonCameraAnchor = cameraFollowAnchor.FirstPersonCameraAnchor;
                }
            }

            if (pitchTarget == null)
            {
                Transform followAnchorTransform = transform.Find(CCS_CharacterControllerConstants.CameraFollowAnchorObjectName);
                if (followAnchorTransform != null)
                {
                    pitchTarget = followAnchorTransform.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName);
                }
            }

            if (firstPersonCameraAnchor == null && pitchTarget != null)
            {
                firstPersonCameraAnchor = pitchTarget.Find(CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName);
            }

            if (characterAnimator == null)
            {
                characterAnimator = GetComponentInChildren<Animator>(true);
            }
        }

        #endregion

        #region Private Methods

        private void ResolveHeadBone()
        {
            headBone = null;
            if (characterAnimator == null || !characterAnimator.isHuman)
            {
                return;
            }

            headBone = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
        }

        private void UpdateFirstPersonAnchorPosition()
        {
            if (!IsHeadTrackingActive || pitchTarget == null || firstPersonCameraAnchor == null)
            {
                return;
            }

            CCS_CharacterCameraProfile profile = cameraFollowAnchor.ActiveLookProfile;
            if (profile == null)
            {
                return;
            }

            if (!profile.UseHeadTrackedAnchor)
            {
                CurrentSightlineOffset = ResolveStaticEyeOffset(profile);
                ApplyStaticAnchorLayout(firstPersonCameraAnchor, profile);
                return;
            }

            if (headBone == null)
            {
                ResolveHeadBone();
            }

            if (headBone == null)
            {
                CurrentSightlineOffset = ResolveStaticEyeOffset(profile);
                ApplyStaticAnchorLayout(firstPersonCameraAnchor, profile);
                return;
            }

            CurrentSightlineOffset = profile.HeadTrackedLocalOffset;
            Vector3 desiredWorldPosition = headBone.TransformPoint(CurrentSightlineOffset);
            Vector3 desiredLocalPosition = pitchTarget.InverseTransformPoint(desiredWorldPosition);
            float lerpSpeed = profile.HeadTrackingPositionLerpSpeed;
            if (lerpSpeed <= 0f)
            {
                firstPersonCameraAnchor.localPosition = desiredLocalPosition;
            }
            else
            {
                float blend = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
                firstPersonCameraAnchor.localPosition = Vector3.Lerp(
                    firstPersonCameraAnchor.localPosition,
                    desiredLocalPosition,
                    blend);
            }

            if (!profile.InheritHeadBoneRotation)
            {
                firstPersonCameraAnchor.localRotation = Quaternion.identity;
            }
        }

        private static Vector3 ResolveStaticEyeOffset(CCS_CharacterCameraProfile profile)
        {
            if (profile == null)
            {
                return Vector3.zero;
            }

            if (profile.UseHeadTrackedAnchor)
            {
                return profile.HeadTrackedLocalOffset;
            }

            return new Vector3(
                0f,
                profile.FirstPersonVerticalEyeOffset,
                profile.FirstPersonForwardEyeOffset);
        }

        private static void ApplyStaticAnchorLayout(Transform anchor, CCS_CharacterCameraProfile profile)
        {
            if (anchor == null || profile == null)
            {
                return;
            }

            anchor.localPosition = ResolveStaticEyeOffset(profile);
            anchor.localRotation = Quaternion.identity;
        }

        #endregion
    }
}
