using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimPresentationGate
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Shared presentation-only aim state for stripped baseline consumers.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked root (non-presentation gameplay removed).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Aim active when RMB held OR diagnostics Enable Aim Pose. No shooting/reticle.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_RevolverAimPresentationGate : MonoBehaviour,
        CCS_IRevolverAimPresentationInput,
        CCS_IWeaponAimGate,
        CCS_IWeaponCarryStateCameraSource
    {
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private Component aimSetupPoseDebugSourceComponent;

        private CCS_IRevolverAimSetupPoseDebugSource aimSetupPoseDebugSource;
        private bool resolvedAimSetupPoseDebugSource;
        private bool previousAimPresentationActive;

        public event Action CarryStateChanged;

        public bool IsAimPresentationActive => ResolveAimPresentationActive();

        public bool IsAimInputHeld => ResolveAimInputHeld();

        public bool IsDiagnosticsAimPoseEnabled => ResolveDiagnosticsAimPoseEnabled();

        public bool IsAimPresentationRequested => IsAimPresentationActive;

        public bool CanUseAimMovement => IsAimPresentationActive;

        public bool CanUseFirearmAimCamera => IsAimPresentationActive;

        public bool ShouldDriveLocalCamera => true;

        public bool WantsAimOverShoulderCamera => IsAimPresentationActive;

        public byte CarryStateValue => IsAimPresentationActive ? (byte)2 : (byte)1;

        private void Awake()
        {
            if (inputProvider == null)
            {
                inputProvider = GetComponent<CCS_CharacterInputActionProvider>();
            }

            ResolveAimSetupPoseDebugSource();
            previousAimPresentationActive = IsAimPresentationActive;
        }

        private void Update()
        {
            bool active = IsAimPresentationActive;
            if (active == previousAimPresentationActive)
            {
                return;
            }

            previousAimPresentationActive = active;
            CarryStateChanged?.Invoke();
        }

        private bool ResolveAimPresentationActive()
        {
            return ResolveDiagnosticsAimPoseEnabled() || ResolveAimInputHeld();
        }

        private bool ResolveAimInputHeld()
        {
            return inputProvider != null
                && inputProvider.InputAccepted
                && inputProvider.AimHeld;
        }

        private bool ResolveDiagnosticsAimPoseEnabled()
        {
            CCS_IRevolverAimSetupPoseDebugSource activeSource = aimSetupPoseDebugSource
                ?? CCS_RevolverAimSetupPoseDebugRegistry.ActiveSource;
            return activeSource != null && activeSource.EnableAimPose;
        }

        private void ResolveAimSetupPoseDebugSource()
        {
            if (resolvedAimSetupPoseDebugSource)
            {
                return;
            }

            resolvedAimSetupPoseDebugSource = true;
            if (aimSetupPoseDebugSourceComponent is CCS_IRevolverAimSetupPoseDebugSource fromComponent)
            {
                aimSetupPoseDebugSource = fromComponent;
            }
        }
    }
}
