using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirstPersonAimCameraOverrideController
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Master Test first-person aim FOV and near-clip tuning to reduce stretched-arm look.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Camera/FOV tuning only — does not scale skeleton. Controlled by Testing Manager.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(90)]
    public sealed class CCS_FirstPersonAimCameraOverrideController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool enableFirstPersonFovOverride = true;
        [SerializeField] private float firstPersonAimFov = CCS_WeaponsConstants.MasterTestFirstPersonAimFovDefault;
        [SerializeField] private float firstPersonAimNearClip = CCS_WeaponsConstants.MasterTestFirstPersonAimNearClipDefault;
        [SerializeField] private CCS_CharacterAimLocomotionController aimLocomotionController;
        [SerializeField] private CCS_CharacterCameraController sceneCameraController;

        private float cachedDefaultNearClip = 0.3f;
        private bool cachedDefaultNearClipCaptured;

        #endregion

        #region Properties

        public bool EnableFirstPersonFovOverride => enableFirstPersonFovOverride;

        public float FirstPersonAimFov => firstPersonAimFov;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            if (!enableFirstPersonFovOverride)
            {
                return;
            }

            ResolveReferences();
            if (aimLocomotionController == null || !aimLocomotionController.IsFirearmAimCameraActive)
            {
                return;
            }

            Camera outputCamera = sceneCameraController != null
                ? sceneCameraController.GetOutputCamera()
                : null;
            if (outputCamera == null)
            {
                return;
            }

            if (!cachedDefaultNearClipCaptured)
            {
                cachedDefaultNearClip = outputCamera.nearClipPlane;
                cachedDefaultNearClipCaptured = true;
            }

            if (!Mathf.Approximately(outputCamera.fieldOfView, firstPersonAimFov))
            {
                outputCamera.fieldOfView = firstPersonAimFov;
            }

            if (!Mathf.Approximately(outputCamera.nearClipPlane, firstPersonAimNearClip))
            {
                outputCamera.nearClipPlane = firstPersonAimNearClip;
            }
        }

        #endregion

        #region Public Methods

        public void SetFirstPersonFovOverrideEnabled(bool enabled)
        {
            enableFirstPersonFovOverride = enabled;
        }

        public void SetFirstPersonAimFov(float fov)
        {
            firstPersonAimFov = Mathf.Clamp(
                fov,
                CCS_WeaponsConstants.MasterTestFirstPersonAimFovMinimum,
                CCS_WeaponsConstants.MasterTestFirstPersonAimFovMaximum);
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (aimLocomotionController == null)
            {
                aimLocomotionController = GetComponent<CCS_CharacterAimLocomotionController>();
            }

            if (sceneCameraController == null)
            {
                sceneCameraController = FindFirstObjectByType<CCS_CharacterCameraController>();
            }
        }

        #endregion
    }
}
