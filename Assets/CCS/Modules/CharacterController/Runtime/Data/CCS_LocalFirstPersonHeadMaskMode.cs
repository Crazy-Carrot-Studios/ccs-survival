// =============================================================================
// SCRIPT: CCS_LocalFirstPersonHeadMaskMode
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Identifies which local first-person head hiding strategy is active.
// PLACEMENT: Used by CCS_LocalFirstPersonHeadVisibility and camera debug output.
// AUTHOR: James Schilz
// CREATED: 2026-06-24
// NOTES: CombinedBodyHeadlessFallback substitutes a CCS-owned headless body mesh locally.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_LocalFirstPersonHeadMaskMode
    {
        None = 0,
        SeparateRendererMask = 1,
        CombinedBodyHeadlessFallback = 2,
    }
}
