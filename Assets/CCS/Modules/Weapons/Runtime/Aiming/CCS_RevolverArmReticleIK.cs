using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.Animations.Rigging;

// =============================================================================
// SCRIPT: CCS_RevolverArmReticleIK
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Arm-only Animation Rigging reticle IK — nudges arm/chest toward reticle without weapon fit changes.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.14 — Rig_RevolverArmReticleIK layers TwoBoneIK + chest/shoulder MultiAim. Never writes weapon/socket.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(265)]
    public sealed class CCS_RevolverArmReticleIK : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private CCS_PlayerEquipmentVisualController equipmentVisualController;
        [SerializeField] private CCS_RevolverHudPresenter hudPresenter;
        [SerializeField] private CCS_RevolverDefinition revolverDefinition;
        [SerializeField] private CCS_CharacterAimLocomotionController aimLocomotionController;
        [SerializeField] private Transform reticleAimWorldTarget;
        [SerializeField] private Transform rightHandReticleIkTarget;
        [SerializeField] private Transform rightElbowHint;
        [SerializeField] private Rig armReticleIkRig;
        [SerializeField] private TwoBoneIKConstraint rightArmTwoBoneIk;
        [SerializeField] private MultiAimConstraint chestAimBias;
        [SerializeField] private MultiAimConstraint rightShoulderAimBias;
        [SerializeField] private float fallbackAimDistance = CCS_WeaponsConstants.RevolverArmReticleIkFallbackDistanceDefault;
        [SerializeField] private float aimTargetSmoothSpeed = 16f;
        [SerializeField] private float rigBlendSpeed = 10f;
        [SerializeField] private float handIkReachOffset = 0.18f;
        [SerializeField] private float twoBoneIkPositionWeight = 0.28f;
        [SerializeField] private float twoBoneIkRotationWeight = 0f;
        [SerializeField] private float twoBoneIkHintWeight = 0.75f;
        [SerializeField] private float chestAimWeight = 0.08f;
        [SerializeField] private float rightShoulderAimWeight = 0.18f;
        [SerializeField] private float maxHorizontalCorrectionDegrees = 10f;
        [SerializeField] private float maxVerticalCorrectionDegrees = 6f;
        [Tooltip("Default OFF. Pulls arm/hand toward reticle via Animation Rigging.")]
        [SerializeField] private bool enableArmToReticleIK;
        [SerializeField] private bool enableRevolverArmIkDebug;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private Transform rightHandBone;
        private Transform rightLowerArmBone;
        private Vector3 smoothedReticleWorldTarget;
        private bool hasSmoothedReticleTarget;
        private float currentRigBlend;
        private string lastAimTargetSourceLabel = "None";
        private string lastActiveCameraName = "None";
        private string lastActiveAimClipName = "None";
        private Vector3 lastEquippedLocalPosition;
        private Vector3 lastEquippedLocalEuler;

        #endregion

        #region Public Properties

        public float CurrentRigBlend => currentRigBlend;

        public bool EnableRevolverArmIkDebug => enableRevolverArmIkDebug;

        public bool EnableArmToReticleIk => enableArmToReticleIK;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            InitializeRigDefaults();
        }

        private void LateUpdate()
        {
            if (animator == null)
            {
                return;
            }

            ResolveReferences();
            bool shouldDrive = ShouldDriveArmReticleIk();
            UpdateRigBlend(shouldDrive);

            if (shouldDrive && currentRigBlend > 0.0001f)
            {
                UpdateReticleTargets();
            }

            ApplyRigWeights(shouldDrive);
            CaptureEquippedFitSnapshot();
        }

        private void OnGUI()
        {
            if (!enableRevolverArmIkDebug)
            {
                return;
            }

            GUI.Label(new Rect(12f, 560f, 980f, 300f), BuildDebugOverlayText());
        }

        #endregion

        #region Public Methods

        public void SetArmToReticleIkEnabled(bool enabled)
        {
            enableArmToReticleIK = enabled;
            if (!enabled)
            {
                currentRigBlend = 0f;
                InitializeRigDefaults();
                hasSmoothedReticleTarget = false;
            }
        }

        public string BuildDebugOverlayText()
        {
            Vector3 profilePosition = equipmentVisualController?.RightHandEquippedFitProfile != null
                ? equipmentVisualController.RightHandEquippedFitProfile.SocketLocalPosition
                : Vector3.zero;
            Vector3 profileEuler = equipmentVisualController?.RightHandEquippedFitProfile != null
                ? equipmentVisualController.RightHandEquippedFitProfile.SocketLocalEulerAngles
                : Vector3.zero;
            Vector3 ikTargetPosition = rightHandReticleIkTarget != null
                ? rightHandReticleIkTarget.position
                : Vector3.zero;
            Vector3 elbowHintPosition = rightElbowHint != null ? rightElbowHint.position : Vector3.zero;
            float liveTwoBonePositionWeight = rightArmTwoBoneIk != null
                ? rightArmTwoBoneIk.data.targetPositionWeight * currentRigBlend
                : 0f;
            float liveTwoBoneRotationWeight = rightArmTwoBoneIk != null
                ? rightArmTwoBoneIk.data.targetRotationWeight * currentRigBlend
                : 0f;

            return "Revolver Arm Reticle IK Debug (v0.6.14)\n"
                + "Active camera: "
                + lastActiveCameraName
                + "\nReticle world target: "
                + smoothedReticleWorldTarget.ToString("F2")
                + "\nAim target source: "
                + lastAimTargetSourceLabel
                + "\nRightHandReticleIKTarget: "
                + ikTargetPosition.ToString("F2")
                + "\nRightElbowHint: "
                + elbowHintPosition.ToString("F2")
                + "\nRig weight: "
                + currentRigBlend.ToString("F3")
                + "\nTwoBoneIK position weight (live): "
                + liveTwoBonePositionWeight.ToString("F3")
                + "\nTwoBoneIK rotation weight (live): "
                + liveTwoBoneRotationWeight.ToString("F3")
                + "\nChest aim weight (live): "
                + (chestAimBias != null ? (chestAimBias.weight * currentRigBlend).ToString("F3") : "missing")
                + "\nShoulder aim weight (live): "
                + (rightShoulderAimBias != null ? (rightShoulderAimBias.weight * currentRigBlend).ToString("F3") : "missing")
                + "\nDirect hand/weapon aim constraint: inactive/missing"
                + "\nWild West aim clip: "
                + lastActiveAimClipName
                + "\nFit profile local pos: "
                + profilePosition.ToString("F3")
                + "\nFit profile local euler: "
                + profileEuler.ToString("F1")
                + "\nRuntime equipped attachment local pos: "
                + lastEquippedLocalPosition.ToString("F3")
                + "\nRuntime equipped attachment local euler: "
                + lastEquippedLocalEuler.ToString("F1");
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (equipmentVisualController == null)
            {
                equipmentVisualController = GetComponentInParent<CCS_PlayerEquipmentVisualController>();
            }

            if (hudPresenter == null)
            {
                hudPresenter = GetComponentInParent<CCS_RevolverHudPresenter>();
            }

            if (revolverDefinition == null)
            {
                CCS_RevolverController revolverController = GetComponentInParent<CCS_RevolverController>();
                if (revolverController != null)
                {
                    revolverDefinition = revolverController.RevolverDefinition;
                }
            }

            if (aimLocomotionController == null)
            {
                aimLocomotionController = GetComponentInParent<CCS_CharacterAimLocomotionController>();
            }

            if (revolverAnimationState == null)
            {
                if (revolverAnimationStateComponent is CCS_IRevolverAnimationState fromComponent)
                {
                    revolverAnimationState = fromComponent;
                }
                else if (revolverAnimationStateComponent == null)
                {
                    revolverAnimationState = GetComponentInParent<CCS_IRevolverAnimationState>();
                }
            }

            Transform ikRoot = transform.Find(CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName);
            if (ikRoot != null)
            {
                if (reticleAimWorldTarget == null)
                {
                    reticleAimWorldTarget = ikRoot.Find(CCS_WeaponsConstants.ReticleAimWorldTargetObjectName);
                }

                if (rightHandReticleIkTarget == null)
                {
                    rightHandReticleIkTarget = ikRoot.Find(CCS_WeaponsConstants.RightHandReticleIkTargetObjectName);
                }

                if (rightElbowHint == null)
                {
                    rightElbowHint = ikRoot.Find(CCS_WeaponsConstants.RightElbowHintObjectName);
                }
            }

            if (armReticleIkRig == null && animator != null)
            {
                Transform rigTransform = animator.transform.Find(CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName);
                if (rigTransform != null)
                {
                    armReticleIkRig = rigTransform.GetComponent<Rig>();
                }
            }

            if (rightArmTwoBoneIk == null && armReticleIkRig != null)
            {
                Transform constraintTransform = armReticleIkRig.transform.Find(
                    CCS_WeaponsConstants.RightArmTwoBoneIkConstraintObjectName);
                if (constraintTransform != null)
                {
                    rightArmTwoBoneIk = constraintTransform.GetComponent<TwoBoneIKConstraint>();
                }
            }

            if (chestAimBias == null && armReticleIkRig != null)
            {
                Transform constraintTransform = armReticleIkRig.transform.Find(
                    CCS_WeaponsConstants.ChestAimBiasConstraintObjectName);
                if (constraintTransform != null)
                {
                    chestAimBias = constraintTransform.GetComponent<MultiAimConstraint>();
                }
            }

            if (rightShoulderAimBias == null && armReticleIkRig != null)
            {
                Transform constraintTransform = armReticleIkRig.transform.Find(
                    CCS_WeaponsConstants.RightShoulderAimBiasConstraintObjectName);
                if (constraintTransform != null)
                {
                    rightShoulderAimBias = constraintTransform.GetComponent<MultiAimConstraint>();
                }
            }

            if (animator != null && animator.isHuman)
            {
                if (rightHandBone == null)
                {
                    rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                }

                if (rightLowerArmBone == null)
                {
                    rightLowerArmBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                }
            }
        }

        private void InitializeRigDefaults()
        {
            if (armReticleIkRig != null)
            {
                armReticleIkRig.weight = 0f;
            }

            SetConstraintWeight(rightArmTwoBoneIk, 0f);
            SetConstraintWeight(chestAimBias, 0f);
            SetConstraintWeight(rightShoulderAimBias, 0f);
            currentRigBlend = 0f;
        }

        private bool ShouldDriveArmReticleIk()
        {
            if (!enableArmToReticleIK || revolverAnimationState == null)
            {
                return false;
            }

            return revolverAnimationState.IsRevolverOwned
                && revolverAnimationState.RevolverAimHeld
                && !revolverAnimationState.RevolverIsReloading;
        }

        private void UpdateRigBlend(bool shouldDrive)
        {
            float targetBlend = shouldDrive ? 1f : 0f;
            currentRigBlend = Mathf.MoveTowards(currentRigBlend, targetBlend, rigBlendSpeed * Time.deltaTime);
        }

        private void UpdateReticleTargets()
        {
            if (!TryResolveReticleWorldTarget(out Vector3 rawReticleTarget))
            {
                return;
            }

            if (!hasSmoothedReticleTarget)
            {
                smoothedReticleWorldTarget = rawReticleTarget;
                hasSmoothedReticleTarget = true;
            }
            else
            {
                float smoothFactor = 1f - Mathf.Exp(-aimTargetSmoothSpeed * Time.deltaTime);
                smoothedReticleWorldTarget = Vector3.Lerp(smoothedReticleWorldTarget, rawReticleTarget, smoothFactor);
            }

            if (reticleAimWorldTarget != null)
            {
                reticleAimWorldTarget.position = smoothedReticleWorldTarget;
            }

            UpdateHandIkTarget(smoothedReticleWorldTarget);
            UpdateElbowHint();
            lastActiveAimClipName = ResolveActiveAimClipName();
        }

        private void UpdateHandIkTarget(Vector3 reticleWorldTarget)
        {
            if (rightHandReticleIkTarget == null || rightHandBone == null)
            {
                return;
            }

            Vector3 handPosition = rightHandBone.position;
            Vector3 referenceForward = rightHandBone.forward;
            Transform muzzle = equipmentVisualController != null && equipmentVisualController.HasEquippedMuzzlePoint
                ? equipmentVisualController.CurrentEquippedMuzzlePoint
                : null;
            if (muzzle != null)
            {
                referenceForward = muzzle.forward;
            }

            Vector3 toReticle = reticleWorldTarget - handPosition;
            if (toReticle.sqrMagnitude <= 0.0001f)
            {
                rightHandReticleIkTarget.position = handPosition;
                rightHandReticleIkTarget.rotation = rightHandBone.rotation;
                return;
            }

            Camera aimCamera = ResolveAimCamera();
            if (aimCamera != null)
            {
                DecomposeCorrection(
                    aimCamera.transform,
                    referenceForward,
                    toReticle.normalized,
                    out float yawDegrees,
                    out float pitchDegrees);
                yawDegrees = Mathf.Clamp(yawDegrees, -maxHorizontalCorrectionDegrees, maxHorizontalCorrectionDegrees);
                pitchDegrees = Mathf.Clamp(pitchDegrees, -maxVerticalCorrectionDegrees, maxVerticalCorrectionDegrees);
                Vector3 correctedDirection = ApplyDirectionCorrection(
                    referenceForward,
                    yawDegrees,
                    pitchDegrees,
                    aimCamera.transform);
                rightHandReticleIkTarget.position = handPosition + (correctedDirection * handIkReachOffset);
            }
            else
            {
                Vector3 clampedDirection = Vector3.RotateTowards(
                    referenceForward,
                    toReticle.normalized,
                    Mathf.Deg2Rad * maxHorizontalCorrectionDegrees,
                    0f);
                rightHandReticleIkTarget.position = handPosition + (clampedDirection * handIkReachOffset);
            }

            rightHandReticleIkTarget.rotation = rightHandBone.rotation;
        }

        private void UpdateElbowHint()
        {
            if (rightElbowHint == null || rightLowerArmBone == null)
            {
                return;
            }

            Vector3 elbowOffset = rightLowerArmBone.right * 0.18f + rightLowerArmBone.up * -0.08f;
            rightElbowHint.position = rightLowerArmBone.position + elbowOffset;
            rightElbowHint.rotation = rightLowerArmBone.rotation;
        }

        private void ApplyRigWeights(bool shouldDrive)
        {
            float rigWeight = shouldDrive ? currentRigBlend : currentRigBlend;
            if (armReticleIkRig != null)
            {
                armReticleIkRig.weight = rigWeight;
            }

            SetConstraintWeight(rightArmTwoBoneIk, rigWeight);
            SetConstraintWeight(chestAimBias, rigWeight * chestAimWeight);
            SetConstraintWeight(rightShoulderAimBias, rigWeight * rightShoulderAimWeight);

            if (rightArmTwoBoneIk != null)
            {
                TwoBoneIKConstraintData ikData = rightArmTwoBoneIk.data;
                bool dataChanged = false;
                if (!Mathf.Approximately(ikData.targetPositionWeight, twoBoneIkPositionWeight))
                {
                    ikData.targetPositionWeight = twoBoneIkPositionWeight;
                    dataChanged = true;
                }

                if (!Mathf.Approximately(ikData.targetRotationWeight, twoBoneIkRotationWeight))
                {
                    ikData.targetRotationWeight = twoBoneIkRotationWeight;
                    dataChanged = true;
                }

                if (!Mathf.Approximately(ikData.hintWeight, twoBoneIkHintWeight))
                {
                    ikData.hintWeight = twoBoneIkHintWeight;
                    dataChanged = true;
                }

                if (dataChanged)
                {
                    rightArmTwoBoneIk.data = ikData;
                }
            }
        }

        private bool TryResolveReticleWorldTarget(out Vector3 aimPoint)
        {
            aimPoint = default;
            Camera aimCamera = ResolveAimCamera();
            if (aimCamera == null)
            {
                return false;
            }

            lastActiveCameraName = aimCamera.name;
            Vector2 viewportPoint = hudPresenter != null
                ? hudPresenter.GetReticleViewportPoint()
                : CCS_WeaponAimResolver.DefaultReticleViewportPoint;
            float maxRange = revolverDefinition != null
                ? revolverDefinition.MaxRange
                : fallbackAimDistance;
            LayerMask hitMask = revolverDefinition != null ? revolverDefinition.HitMask : Physics.DefaultRaycastLayers;
            Transform ignoreRoot = transform.root;

            Ray cameraRay = aimCamera.ViewportPointToRay(new Vector3(viewportPoint.x, viewportPoint.y, 0f));
            bool hasHit = TryRaycast(cameraRay, maxRange, hitMask, ignoreRoot, out RaycastHit hit);
            aimPoint = hasHit
                ? hit.point
                : cameraRay.origin + (cameraRay.direction * fallbackAimDistance);
            lastAimTargetSourceLabel = hasHit ? "RayHit" : "Fallback";
            return true;
        }

        private void CaptureEquippedFitSnapshot()
        {
            if (equipmentVisualController == null)
            {
                return;
            }

            Transform equippedRoot = FindDeepChild(
                equipmentVisualController.transform,
                CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName);
            if (equippedRoot == null)
            {
                return;
            }

            lastEquippedLocalPosition = equippedRoot.localPosition;
            lastEquippedLocalEuler = equippedRoot.localEulerAngles;
        }

        private string ResolveActiveAimClipName()
        {
            return "LocomotionOnly";
        }

        private static void DecomposeCorrection(
            Transform cameraTransform,
            Vector3 fromDirection,
            Vector3 toDirection,
            out float yawDegrees,
            out float pitchDegrees)
        {
            Vector3 upAxis = cameraTransform.up;
            Vector3 rightAxis = cameraTransform.right;
            Vector3 fromPlanar = Vector3.ProjectOnPlane(fromDirection, upAxis);
            Vector3 toPlanar = Vector3.ProjectOnPlane(toDirection, upAxis);
            if (fromPlanar.sqrMagnitude <= 0.0001f || toPlanar.sqrMagnitude <= 0.0001f)
            {
                yawDegrees = 0f;
                pitchDegrees = 0f;
                return;
            }

            fromPlanar.Normalize();
            toPlanar.Normalize();
            yawDegrees = Vector3.SignedAngle(fromPlanar, toPlanar, upAxis);

            float fromPitch = Vector3.SignedAngle(fromPlanar, fromDirection, rightAxis);
            float toPitch = Vector3.SignedAngle(toPlanar, toDirection, rightAxis);
            pitchDegrees = toPitch - fromPitch;
        }

        private static Vector3 ApplyDirectionCorrection(
            Vector3 forward,
            float yawDegrees,
            float pitchDegrees,
            Transform cameraTransform)
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDegrees, cameraTransform.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitchDegrees, cameraTransform.right);
            return (pitchRotation * yawRotation * forward).normalized;
        }

        private Camera ResolveAimCamera()
        {
            return CCS_CharacterMovementCameraContext.HasActiveCamera
                ? CCS_CharacterMovementCameraContext.ActiveCamera
                : Camera.main;
        }

        private static bool TryRaycast(
            Ray ray,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot,
            out RaycastHit closestHit)
        {
            closestHit = default;
            RaycastHit[] hits = Physics.RaycastAll(
                ray.origin,
                ray.direction,
                maxRange,
                hitMask,
                QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                if (ignoreRoot != null && hit.collider.transform.IsChildOf(ignoreRoot))
                {
                    continue;
                }

                closestHit = hit;
                return true;
            }

            return false;
        }

        private static void SetConstraintWeight(TwoBoneIKConstraint constraint, float weight)
        {
            if (constraint != null)
            {
                constraint.weight = weight;
            }
        }

        private static void SetConstraintWeight(MultiAimConstraint constraint, float weight)
        {
            if (constraint != null)
            {
                constraint.weight = weight;
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        #endregion
    }
}
