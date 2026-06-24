using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterAimLocomotionController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Owns combat locomotion and local firearm first-person aim camera activation.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.9 — third-person default; first-person aim camera only while firearm aiming (local owner).
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
        private bool isCombatLocomotionActive;
        private bool isFirearmAimCameraActive;

#if UNITY_EDITOR
        private bool editorAimFitOverrideActive;
#endif

        #endregion

        #region Properties

        public bool IsAimMovementActive => isCombatLocomotionActive;

        public bool IsCombatLocomotionActive => isCombatLocomotionActive;

        public bool IsFirearmAimCameraActive => isFirearmAimCameraActive;

        public Vector2 AimMoveInput =>
            isCombatLocomotionActive && inputProvider != null ? inputProvider.MoveInput : Vector2.zero;

        public CCS_CharacterCameraFollowAnchor CameraFollowAnchor => cameraFollowAnchor;

        public bool HasSceneCameraConfigured =>
            sceneCameraController != null && sceneCameraController.HasFirearmAimCameraConfigured;

#if UNITY_EDITOR
        public bool IsEditorAimFitOverrideActive => editorAimFitOverrideActive;

        public void SetEditorAimFitOverride(bool active)
        {
            editorAimFitOverrideActive = active;
            if (active)
            {
                SetFirearmAimCameraActive(true, forceCameraUpdate: true);
                SetCombatLocomotionActive(true, forceCameraUpdate: true);
            }
            else
            {
                SetFirearmAimCameraActive(false, forceCameraUpdate: true);
                SetCombatLocomotionActive(false, forceCameraUpdate: true);
            }
        }
#endif

        #endregion

        #region Events

        public event Action<bool> AimMovementActiveChanged;

        public event Action<bool> FirearmAimCameraActiveChanged;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ResolveWeaponAimGate();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            editorAimFitOverrideActive = false;
#endif
            SetCombatLocomotionActive(false, forceCameraUpdate: true);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (editorAimFitOverrideActive)
            {
                SetCombatLocomotionActive(true, forceCameraUpdate: false);
                SetFirearmAimCameraActive(true, forceCameraUpdate: false);
                return;
            }
#endif

            if (inputProvider == null)
            {
                SetCombatLocomotionActive(false);
                return;
            }

            bool shouldUseCombatLocomotion = weaponAimGate != null && weaponAimGate.CanUseAimMovement;

            if (!inputProvider.InputAccepted)
            {
                SetCombatLocomotionActive(shouldUseCombatLocomotion);
                return;
            }

            SetCombatLocomotionActive(shouldUseCombatLocomotion);
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

            if (weaponAimGateComponent is CCS_IWeaponAimGate fromComponent
                && weaponAimGateComponent is CCS_IWeaponCarryStateCameraSource)
            {
                weaponAimGate = fromComponent;
                return;
            }

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IWeaponAimGate gate
                    && behaviours[i] is CCS_IWeaponCarryStateCameraSource)
                {
                    weaponAimGate = gate;
                    weaponAimGateComponent = behaviours[i];
                    return;
                }
            }
        }

        private void SetCombatLocomotionActive(bool active, bool forceCameraUpdate = false)
        {
            if (!forceCameraUpdate && isCombatLocomotionActive == active)
            {
                return;
            }

            isCombatLocomotionActive = active;

            if (enableMovementDebugLogs)
            {
                Debug.Log(
                    active
                        ? "[Character Motor] Combat locomotion entered."
                        : "[Character Motor] Combat locomotion exited.",
                    this);
            }

            AimMovementActiveChanged?.Invoke(active);
        }

        private void SetFirearmAimCameraActive(bool active, bool forceCameraUpdate = false)
        {
            if (!forceCameraUpdate && isFirearmAimCameraActive == active)
            {
                return;
            }

            isFirearmAimCameraActive = active;

            if (sceneCameraController != null)
            {
                sceneCameraController.SetFirearmAimModeActive(active);
            }

            if (enableMovementDebugLogs)
            {
                Debug.Log(
                    active
                        ? "[Character Camera] Firearm first-person aim entered."
                        : "[Character Camera] Firearm first-person aim exited.",
                    this);
            }

            FirearmAimCameraActiveChanged?.Invoke(active);
        }

        #endregion
    }
}
