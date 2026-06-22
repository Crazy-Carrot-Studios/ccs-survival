using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterAimLocomotionController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Owns aim movement mode state and scene aim camera activation.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Reads AimHeld from CCS_CharacterInputActionProvider. Weapons read state only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(40)]
    public sealed class CCS_CharacterAimLocomotionController : MonoBehaviour, CCS_ICharacterAimLocomotionState
    {
        #region Variables

        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_CharacterCameraFollowAnchor cameraFollowAnchor;
        [SerializeField] private Component weaponAimGateComponent;
        [SerializeField] private bool enableMovementDebugLogs;

        private CCS_IWeaponAimGate weaponAimGate;

        private CCS_CharacterCameraController sceneCameraController;
        private bool isAimMovementActive;

        #endregion

        #region Properties

        public bool IsAimMovementActive => isAimMovementActive;

        public Vector2 AimMoveInput =>
            isAimMovementActive && inputProvider != null ? inputProvider.MoveInput : Vector2.zero;

        public CCS_CharacterCameraFollowAnchor CameraFollowAnchor => cameraFollowAnchor;

        public bool HasSceneCameraConfigured =>
            sceneCameraController != null && sceneCameraController.HasAimCameraConfigured;

        #endregion

        #region Events

        public event Action<bool> AimMovementActiveChanged;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ResolveWeaponAimGate();
        }

        private void OnDisable()
        {
            SetAimMovementActive(false, forceCameraUpdate: true);
        }

        private void Update()
        {
            if (inputProvider == null || !inputProvider.InputAccepted)
            {
                SetAimMovementActive(false);
                return;
            }

            bool shouldAim = inputProvider.AimHeld
                && HasSceneCameraConfigured
                && CanUseAimMovement();
            SetAimMovementActive(shouldAim);
        }

        #endregion

        #region Public Methods

        public void ConfigureSceneCamera(CCS_CharacterCameraController cameraController)
        {
            sceneCameraController = cameraController;
        }

        public void SetInputProvider(CCS_CharacterInputActionProvider provider)
        {
            inputProvider = provider;
        }

        public void SetCameraFollowAnchor(CCS_CharacterCameraFollowAnchor followAnchor)
        {
            cameraFollowAnchor = followAnchor;
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }

            ResolveWeaponAimGate();
        }

        private void ResolveWeaponAimGate()
        {
            if (weaponAimGate != null)
            {
                return;
            }

            if (weaponAimGateComponent is CCS_IWeaponAimGate fromComponent)
            {
                weaponAimGate = fromComponent;
                return;
            }

            weaponAimGate = GetComponent<CCS_IWeaponAimGate>();
        }

        private bool CanUseAimMovement()
        {
            return weaponAimGate == null || weaponAimGate.CanUseAimMovement;
        }

        private void SetAimMovementActive(bool active, bool forceCameraUpdate = false)
        {
            if (!forceCameraUpdate && isAimMovementActive == active)
            {
                return;
            }

            isAimMovementActive = active;

            if (sceneCameraController != null)
            {
                sceneCameraController.SetAimModeActive(active);
            }

            if (enableMovementDebugLogs)
            {
                if (active)
                {
                    Debug.Log("[Character Motor] Aim movement entered.", this);
                }
                else
                {
                    Debug.Log("[Character Motor] Aim movement exited.", this);
                }
            }

            AimMovementActiveChanged?.Invoke(active);
        }

        #endregion
    }
}
