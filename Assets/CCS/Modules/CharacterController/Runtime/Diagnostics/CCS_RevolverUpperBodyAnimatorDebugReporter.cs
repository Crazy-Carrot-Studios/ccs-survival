using CCS.Modules.CharacterController;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyAnimatorDebugReporter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Master Test animation diagnostics (console/Markdown, optional overlay).
// PLACEMENT: Auto-added to CCS_DiagnosticsManager by CCS_CharacterControllerDiagnosticsManager.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Test-only. Does not alter revolver aim gameplay behavior.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics {
    [DisallowMultipleComponent]
    public sealed class CCS_RevolverUpperBodyAnimatorDebugReporter : MonoBehaviour
    {
        private const string MasterTestSceneName = "SCN_CCS_CharacterController_Validation";

        private CCS_CharacterControllerDiagnosticsManager testingManager;
        private CCS_RevolverUpperBodyAnimator cachedAnimator;
        private string cachedOverlayText = string.Empty;
        private float nextConsoleLogTime;

        private void Awake()
        {
            testingManager = GetComponent<CCS_CharacterControllerDiagnosticsManager>();
        }

        private void Update()
        {
            if (testingManager == null || !IsMasterTestSceneActive())
            {
                cachedOverlayText = string.Empty;
                return;
            }

            if (!testingManager.EnableAnimationDiagnostics)
            {
                cachedOverlayText = string.Empty;
                return;
            }

            ResolveAnimator();
            if (cachedAnimator == null)
            {
                cachedOverlayText = string.Empty;
                return;
            }

            cachedOverlayText = cachedAnimator.BuildRuntimeDebugSnapshot();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            HandleForceAimDebugHotkey();
#endif

            if (testingManager.EnableVerboseLogs && Time.unscaledTime >= nextConsoleLogTime)
            {
                nextConsoleLogTime = Time.unscaledTime + 2f;
                Debug.Log("[Animation Diagnostics]\n" + cachedOverlayText, this);
            }
        }

        private void OnGUI()
        {
            if (testingManager == null
                || !testingManager.EnableVisualDebugHelpers
                || !testingManager.EnableAnimationDiagnostics
                || string.IsNullOrEmpty(cachedOverlayText))
            {
                return;
            }

            GUI.Label(new Rect(12f, 280f, 960f, 280f), cachedOverlayText);
        }

        public string BuildReportSection()
        {
            ResolveAnimator();
            return cachedAnimator != null
                ? cachedAnimator.BuildRuntimeDebugSnapshot()
                : "No CCS_RevolverUpperBodyAnimator found in scene.";
        }

        private void ResolveAnimator()
        {
            if (cachedAnimator != null)
            {
                return;
            }

            cachedAnimator = FindFirstObjectByType<CCS_RevolverUpperBodyAnimator>();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void HandleForceAimDebugHotkey()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.f9Key.wasPressedThisFrame)
            {
                return;
            }

            ResolveAnimator();
            cachedAnimator?.ForceDebugPlayWildWestAimStateForTesting();
        }
#endif

        private static bool IsMasterTestSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.IsValid() && activeScene.name == MasterTestSceneName;
        }
    }
}
