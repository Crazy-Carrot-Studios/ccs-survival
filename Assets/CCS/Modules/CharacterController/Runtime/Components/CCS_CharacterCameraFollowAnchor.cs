using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraFollowAnchor
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: World-stable Cinemachine follow anchor decoupled from body yaw.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer / CameraFollowAnchor.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Tracks body height only. Rotation stays world-aligned so body turn +
//        camera orbit cannot create a feedback loop during move + look.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(-250)]
    public sealed class CCS_CharacterCameraFollowAnchor : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Transform bodyRoot;
        [SerializeField] private float heightOffset = 1f;
        [SerializeField] private Transform lookTarget;

        #endregion

        #region Properties

        public Transform FollowTransform => transform;

        public Transform LookTarget => lookTarget;

        public Transform BodyRoot => bodyRoot;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            UpdateWorldAnchorPose();
        }

        #endregion

        #region Public Methods

        public void Configure(Transform body, Transform lookAtTarget, float followHeight)
        {
            bodyRoot = body;
            lookTarget = lookAtTarget;
            heightOffset = followHeight;
            UpdateWorldAnchorPose();
        }

        public void ResolveReferences()
        {
            if (bodyRoot == null && transform.parent != null)
            {
                bodyRoot = transform.parent;
            }

            if (lookTarget == null)
            {
                Transform childLookTarget = transform.Find("CameraLookTarget");
                if (childLookTarget != null)
                {
                    lookTarget = childLookTarget;
                }
            }

            if (lookTarget == null && bodyRoot != null)
            {
                Transform pivotLookTarget = bodyRoot.Find("CameraPivot/CameraLookTarget");
                if (pivotLookTarget != null)
                {
                    lookTarget = pivotLookTarget;
                }
            }
        }

        #endregion

        #region Private Methods

        private void UpdateWorldAnchorPose()
        {
            if (bodyRoot == null)
            {
                return;
            }

            transform.position = bodyRoot.position + (Vector3.up * heightOffset);
            transform.rotation = Quaternion.identity;
        }

        #endregion
    }
}
