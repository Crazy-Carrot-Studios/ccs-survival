using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterCameraDebugReporter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Master Test camera diagnostics (console/Markdown, optional overlay).
// PLACEMENT: Auto-added to CCS_DiagnosticsManager by CCS_CharacterControllerDiagnosticsManager.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Test-only. Does not alter camera gameplay behavior.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics {
    [DisallowMultipleComponent]
    public sealed class CCS_CharacterCameraDebugReporter : MonoBehaviour
    {
        private const string MasterTestSceneName = "SCN_CCS_CharacterController_Validation";

        private CCS_CharacterControllerDiagnosticsManager testingManager;
        private CCS_CharacterCameraController cachedCameraController;
        private string cachedOverlayText = string.Empty;
        private float nextConsoleLogTime;

        private void Awake()
        {
            testingManager = GetComponent<CCS_CharacterControllerDiagnosticsManager>();
        }

        private void LateUpdate()
        {
            if (testingManager == null || !IsMasterTestSceneActive())
            {
                cachedOverlayText = string.Empty;
                return;
            }

            if (!testingManager.EnableCameraDiagnostics)
            {
                cachedOverlayText = string.Empty;
                return;
            }

            ResolveCameraController();
            cachedOverlayText = cachedCameraController != null
                ? cachedCameraController.BuildCameraDebugReport()
                : string.Empty;

            if (testingManager.EnableVerboseLogs
                && !string.IsNullOrEmpty(cachedOverlayText)
                && Time.unscaledTime >= nextConsoleLogTime)
            {
                nextConsoleLogTime = Time.unscaledTime + 2f;
                Debug.Log("[Camera Diagnostics]\n" + cachedOverlayText, this);
            }
        }

        private void OnGUI()
        {
            if (testingManager == null
                || !testingManager.EnableVisualDebugHelpers
                || !testingManager.EnableCameraDiagnostics
                || string.IsNullOrEmpty(cachedOverlayText))
            {
                return;
            }

            GUI.Label(new Rect(12f, 12f, 860f, 560f), cachedOverlayText);
        }

        public string BuildReportSection()
        {
            ResolveCameraController();
            return cachedCameraController != null
                ? cachedCameraController.BuildCameraDebugReport()
                : "No CCS_CharacterCameraController found in scene.";
        }

        private void ResolveCameraController()
        {
            if (cachedCameraController != null)
            {
                return;
            }

            cachedCameraController = FindFirstObjectByType<CCS_CharacterCameraController>();
        }

        private static bool IsMasterTestSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.IsValid() && activeScene.name == MasterTestSceneName;
        }
    }
}
