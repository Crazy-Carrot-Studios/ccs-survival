using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SharedDualAimPointPresenter
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Resolves camera forward ray into one shared invisible dual-aim world point.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model.
// AUTHOR: James Schilz
// CREATED: 2026-07-02
// NOTES: Camera forward ray -> SharedDualAimPoint. No hand/wrist or weapon transform changes.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(-50)]
    public sealed class CCS_SharedDualAimPointPresenter : MonoBehaviour
    {
        [SerializeField] private Component aimPresentationInputComponent;
        [SerializeField] private Component cameraControllerComponent;
        [SerializeField] private Transform sharedDualAimPoint;
        [SerializeField] private float convergenceDistance = CCS_CharacterControllerConstants.SharedDualAimPointDefaultDistance;
        [SerializeField] private float targetSmoothTime = CCS_CharacterControllerConstants.SharedDualAimPointDefaultSmoothTime;
        [SerializeField] private bool drawSceneGizmos = true;

        private CCS_IRevolverAimPresentationInput aimPresentationInput;
        private CCS_CharacterCameraController characterCameraController;
        private Camera fallbackCamera;
        private Vector3 smoothedAimPointPosition;
        private Vector3 aimPointVelocity;
        private bool setupResolved;

        public Transform SharedDualAimPointTransform => sharedDualAimPoint;

        private void Awake()
        {
            ResolveReferences();
            InitializeAimPointPosition();
            CCS_SharedDualAimPointRegistry.Clear();
        }

        private void OnDisable()
        {
            CCS_SharedDualAimPointRegistry.Clear();
        }

        private void LateUpdate()
        {
            if (!setupResolved || sharedDualAimPoint == null)
            {
                CCS_SharedDualAimPointRegistry.Clear();
                return;
            }

            bool shouldActivate = ShouldActivateSharedAimPoint();
            Vector3 cameraPosition;
            Vector3 cameraForward;
            if (!TryResolveCameraBasis(out cameraPosition, out cameraForward))
            {
                cameraPosition = transform.position + Vector3.up;
                cameraForward = transform.forward;
            }

            if (!shouldActivate)
            {
                CCS_SharedDualAimPointRegistry.Clear();
                return;
            }

            Vector3 desiredPosition = cameraPosition + cameraForward * convergenceDistance;
            float smoothTime = Mathf.Max(0.0001f, targetSmoothTime);
            smoothedAimPointPosition = Vector3.SmoothDamp(
                smoothedAimPointPosition,
                desiredPosition,
                ref aimPointVelocity,
                smoothTime);

            sharedDualAimPoint.position = smoothedAimPointPosition;

            CCS_SharedDualAimPointRegistry.UpdateState(
                true,
                smoothedAimPointPosition,
                cameraPosition,
                cameraForward);
        }

        private void OnDrawGizmos()
        {
            if (!ShouldDrawGizmos())
            {
                return;
            }

            DrawSharedAimGizmos(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShouldDrawGizmos())
            {
                return;
            }

            DrawSharedAimGizmos(true);
        }

        private bool ShouldDrawGizmos()
        {
            return setupResolved && drawSceneGizmos;
        }

        private void DrawSharedAimGizmos(bool selected)
        {
            Gizmos.color = new Color(0.2f, 0.85f, 1f, selected ? 1f : 0.85f);
            Gizmos.DrawSphere(smoothedAimPointPosition, 0.08f);

            if (TryResolveCameraBasis(out Vector3 cameraPosition, out Vector3 cameraForward))
            {
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.75f);
                Gizmos.DrawRay(cameraPosition, cameraForward * 2.5f);
                Gizmos.DrawLine(cameraPosition, smoothedAimPointPosition);
            }
        }

        private void ResolveReferences()
        {
            if (aimPresentationInputComponent is CCS_IRevolverAimPresentationInput fromComponent)
            {
                aimPresentationInput = fromComponent;
            }
            else if (aimPresentationInput == null)
            {
                aimPresentationInput = GetComponentInParent<CCS_IRevolverAimPresentationInput>();
            }

            if (cameraControllerComponent is CCS_CharacterCameraController fromCamera)
            {
                characterCameraController = fromCamera;
            }

            if (characterCameraController == null)
            {
                characterCameraController = GetComponentInParent<CCS_CharacterCameraController>(true);
            }

            if (characterCameraController == null)
            {
                characterCameraController = FindAnyObjectByType<CCS_CharacterCameraController>();
            }

            setupResolved = sharedDualAimPoint != null;
        }

        private void InitializeAimPointPosition()
        {
            if (sharedDualAimPoint != null)
            {
                smoothedAimPointPosition = sharedDualAimPoint.position;
            }
            else
            {
                smoothedAimPointPosition = transform.position + transform.forward * 10f;
            }
        }

        private bool ShouldActivateSharedAimPoint()
        {
            if (!CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon)
            {
                return false;
            }

            return aimPresentationInput != null && aimPresentationInput.IsAimPresentationActive;
        }

        private bool TryResolveCameraBasis(out Vector3 cameraPosition, out Vector3 cameraForward)
        {
            if (characterCameraController != null)
            {
                Transform pivot = characterCameraController.CameraPivot;
                Transform lookTarget = characterCameraController.CameraLookTarget;
                if (pivot != null)
                {
                    cameraPosition = pivot.position;
                    if (lookTarget != null)
                    {
                        cameraForward = (lookTarget.position - pivot.position).normalized;
                        if (cameraForward.sqrMagnitude > 0.0001f)
                        {
                            return true;
                        }
                    }

                    cameraForward = pivot.forward;
                    return true;
                }
            }

            if (fallbackCamera == null)
            {
                fallbackCamera = Camera.main;
            }

            if (fallbackCamera != null)
            {
                cameraPosition = fallbackCamera.transform.position;
                cameraForward = fallbackCamera.transform.forward;
                return true;
            }

            cameraPosition = Vector3.zero;
            cameraForward = Vector3.forward;
            return false;
        }
    }
}
