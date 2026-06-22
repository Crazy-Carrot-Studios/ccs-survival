using UnityEngine;



// =============================================================================

// SCRIPT: CCS_CharacterCameraFollowAnchor

// CATEGORY: Modules / CharacterController / Runtime / Components

// PURPOSE: Shared third-person camera rig target with decoupled yaw/pitch look control.

// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / CameraFollowAnchor.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Cinemachine Third Person Follow tracks CameraPitchTarget. Both TP and Aim cameras

//        share this single yaw/pitch state. Body rotation does not re-aim the camera.

// =============================================================================



namespace CCS.Modules.CharacterController

{

    [DefaultExecutionOrder(-240)]

    public sealed class CCS_CharacterCameraFollowAnchor : MonoBehaviour

    {

        #region Variables



        [SerializeField] private Transform bodyRoot;

        [SerializeField] private Transform pitchTarget;

        [SerializeField] private Transform lookTarget;

        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;

        [SerializeField] private CCS_CharacterCameraProfile activeLookProfile;

        [SerializeField] private bool enableRuntimeCameraDebug;



        private float yawDegrees;

        private float pitchDegrees;

        private Quaternion desiredWorldRotation = Quaternion.identity;

        private bool lookProfileInitialized;

        private bool spawnOrientationApplied;



        #endregion



        #region Properties



        public Transform FollowTransform => pitchTarget != null ? pitchTarget : transform;



        public Transform PitchTarget => pitchTarget;



        public Transform LookTarget => lookTarget;



        public Transform BodyRoot => bodyRoot;



        public float YawDegrees => yawDegrees;



        public float PitchDegrees => pitchDegrees;



        public Quaternion DesiredWorldRotation => desiredWorldRotation;



        public Vector3 PlanarForward => GetPlanarForwardInternal();



        public Vector3 PlanarRight => GetPlanarRightInternal();



        #endregion



        #region Unity Callbacks



        private void Awake()

        {

            ResolveReferences();

        }



        private void LateUpdate()

        {

            UpdateAnchorPosition();

            ApplyLookInput();

            ApplyRigRotation();

        }



        #endregion



        #region Public Methods



        public void Configure(

            Transform body,

            Transform lookAtTarget,

            CCS_CharacterCameraProfile lookProfile)

        {

            bodyRoot = body;

            lookTarget = lookAtTarget;

            if (lookProfile != null)

            {

                activeLookProfile = lookProfile;

                lookProfileInitialized = true;

            }



            ResolveReferences();

            ApplyTrackingTargetLayout(lookProfile);

            InitializeSpawnOrientation(force: true);

        }



        public void SetLookProfile(CCS_CharacterCameraProfile lookProfile)

        {

            if (lookProfile == null)

            {

                return;

            }



            activeLookProfile = lookProfile;

            lookProfileInitialized = true;

            ApplyTrackingTargetLayout(lookProfile);

            pitchDegrees = Mathf.Clamp(pitchDegrees, lookProfile.MinPitch, lookProfile.MaxPitch);

        }



        public void InitializeSpawnOrientation(bool force = false)

        {

            if (!force && spawnOrientationApplied)

            {

                return;

            }



            ResolveReferences();

            if (bodyRoot == null)

            {

                return;

            }



            CCS_CharacterCameraProfile profile = ResolveLookProfile();

            pitchDegrees = profile != null ? profile.DefaultPitch : 0f;



            if (profile == null || profile.DefaultYawMode == CCS_CharacterCameraDefaultYawMode.PlayerForward)

            {

                yawDegrees = bodyRoot.eulerAngles.y;

            }

            else

            {

                yawDegrees = 0f;

            }



            pitchDegrees = profile != null

                ? Mathf.Clamp(pitchDegrees, profile.MinPitch, profile.MaxPitch)

                : pitchDegrees;



            ApplyRigRotation();

            desiredWorldRotation = pitchTarget != null ? pitchTarget.rotation : transform.rotation;

            spawnOrientationApplied = true;

        }



        public void ResolveReferences()

        {

            if (bodyRoot == null && transform.parent != null)

            {

                bodyRoot = transform.parent;

            }



            if (pitchTarget == null)

            {

                Transform childPitchTarget = transform.Find(CCS_CharacterControllerConstants.CameraPitchTargetObjectName);

                if (childPitchTarget != null)

                {

                    pitchTarget = childPitchTarget;

                }

            }



            if (lookTarget == null)

            {

                Transform childLookTarget = transform.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName);

                if (childLookTarget != null)

                {

                    lookTarget = childLookTarget;

                }

            }



            if (lookTarget == null && pitchTarget != null)

            {

                lookTarget = pitchTarget.Find(CCS_CharacterControllerConstants.CameraLookTargetObjectName);

            }



            if (inputProvider == null && bodyRoot != null)

            {

                inputProvider = bodyRoot.GetComponent<CCS_CharacterInputActionProvider>();

            }

        }



        #endregion



        #region Private Methods



        private void ApplyTrackingTargetLayout(CCS_CharacterCameraProfile profile)

        {

            transform.localPosition = Vector3.zero;



            if (pitchTarget != null)

            {

                float trackingHeight = profile != null

                    ? profile.TrackingTargetLocalHeight

                    : CCS_CharacterControllerConstants.CameraPitchTargetLocalHeight;

                pitchTarget.localPosition = new Vector3(0f, trackingHeight, 0f);

                pitchTarget.localRotation = Quaternion.identity;

            }



            if (lookTarget != null)

            {

                lookTarget.localPosition = CCS_CharacterControllerConstants.CameraLookTargetLocalPosition;

                lookTarget.localRotation = Quaternion.identity;

            }

        }



        private void UpdateAnchorPosition()

        {

            if (bodyRoot == null)

            {

                return;

            }



            transform.position = bodyRoot.position;

        }



        private void ApplyLookInput()

        {

            if (inputProvider == null || !inputProvider.InputAccepted)

            {

                return;

            }



            CCS_CharacterCameraProfile profile = ResolveLookProfile();

            if (profile == null)

            {

                return;

            }



            Vector2 lookInput = inputProvider.LookInput;

            if (lookInput.sqrMagnitude <= 0.000001f)

            {

                return;

            }



            yawDegrees += lookInput.x * profile.MouseSensitivityX;

            pitchDegrees -= lookInput.y * profile.MouseSensitivityY;

            pitchDegrees = Mathf.Clamp(pitchDegrees, profile.MinPitch, profile.MaxPitch);

        }



        private void ApplyRigRotation()

        {

            transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);



            if (pitchTarget != null)

            {

                pitchTarget.localRotation = Quaternion.Euler(pitchDegrees, 0f, 0f);

                desiredWorldRotation = pitchTarget.rotation;

            }

            else

            {

                transform.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

                desiredWorldRotation = transform.rotation;

            }

        }



        private CCS_CharacterCameraProfile ResolveLookProfile()

        {

            return activeLookProfile;

        }



        private Vector3 GetPlanarForwardInternal()

        {

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

        }



        private Vector3 GetPlanarRightInternal()

        {

            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up);

            return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

        }



        #endregion

    }

}


