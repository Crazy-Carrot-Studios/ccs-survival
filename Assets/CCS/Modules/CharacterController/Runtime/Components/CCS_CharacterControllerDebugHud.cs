using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerDebugHud
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Dev/test OnGUI HUD for character controller diagnostics.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Dev/test only. Not production HUD.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterControllerDebugHud : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private CCS_CharacterControllerService controllerService;

        [SerializeField] private bool showHud = true;

        #endregion

        #region Unity Callbacks

        private void OnGUI()
        {
            if (!showHud || controllerService == null)
            {
                return;
            }

            CCS_CharacterControllerSnapshot snapshot = controllerService.Snapshot;
            GUILayout.BeginArea(new Rect(12f, 12f, 360f, 420f), GUI.skin.box);
            GUILayout.Label("CCS Character Controller Debug HUD");
            GUILayout.Label($"Version: {CCS_CharacterControllerConstants.ModuleVersion}");
            GUILayout.Label($"Movement Mode: {snapshot.MovementMode}");
            GUILayout.Label($"Camera Mode: {snapshot.CameraMode}");
            GUILayout.Label($"Grounded: {snapshot.IsGrounded}");
            GUILayout.Label($"Current Speed: {snapshot.CurrentSpeed:0.00}");
            GUILayout.Label($"Target Speed: {snapshot.TargetSpeed:0.00}");
            GUILayout.Label($"Sprinting: {snapshot.IsSprinting}");
            GUILayout.Label($"Input Device: {snapshot.InputDeviceLabel}");
            GUILayout.Label($"Movement Input: {snapshot.MovementInput}");
            GUILayout.Label($"Look Input (diagnostics): {snapshot.LookInput}");
            GUILayout.Label($"Active Camera Yaw: {snapshot.Yaw:0.0}");
            GUILayout.Label($"Active Camera Pitch: {snapshot.Pitch:0.0}");
            GUILayout.Label($"Camera Profile: {snapshot.ActiveCameraProfileName}");
            GUILayout.Label($"Cinemachine Rig: {snapshot.ActiveCinemachineRigDescription}");
            GUILayout.Label($"Sensitivity X/Y: {snapshot.ActiveMouseSensitivityX:0.###}/{snapshot.ActiveMouseSensitivityY:0.###}");
            GUILayout.EndArea();
        }

        #endregion

        #region Public Methods

        public void SetControllerService(CCS_CharacterControllerService service)
        {
            controllerService = service;
        }

        public void SetShowHud(bool enabled)
        {
            showHud = enabled;
        }

        #endregion
    }
}
