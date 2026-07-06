using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraDebugReporter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Legacy camera diagnostics hook retained for scene compatibility only.
// PLACEMENT: Removed from CCS_DiagnosticsManager in stripped baseline v0.7.13.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Inert in stripped baseline. No OnGUI, logging, or camera routing.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class CCS_CharacterCameraDebugReporter : MonoBehaviour
    {
        public string BuildReportSection()
        {
            return string.Empty;
        }
    }
}
