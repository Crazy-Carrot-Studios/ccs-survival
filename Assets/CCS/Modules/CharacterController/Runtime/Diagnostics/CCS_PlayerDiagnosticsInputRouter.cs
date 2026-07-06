using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// =============================================================================
// SCRIPT: CCS_PlayerDiagnosticsInputRouter
// CATEGORY: Modules / CharacterController / Tests / Runtime / Diagnostics
// PURPOSE: Legacy test-damage input hook retained for scene compatibility only.
// PLACEMENT: Removed from CCS_DiagnosticsManager in stripped baseline v0.7.13.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Inert in stripped baseline. No test damage routing.
// =============================================================================

namespace CCS.Modules.CharacterController.Diagnostics
{
    [MovedFrom(true, "CCS.Modules.CharacterController.Tests", "CCS.Modules.CharacterController.Tests.Runtime", "CCS_TestPlayerAttributeDebugInputRouter")]
    public sealed class CCS_PlayerDiagnosticsInputRouter : MonoBehaviour
    {
    }
}
