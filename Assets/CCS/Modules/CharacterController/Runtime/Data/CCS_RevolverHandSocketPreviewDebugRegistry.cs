// =============================================================================
// SCRIPT: CCS_RevolverHandSocketPreviewDebugRegistry
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Optional validation-scene registry for revolver hand socket preview debug overrides.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics manager registers on validation scenes only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_RevolverHandSocketPreviewDebugRegistry
    {
        public static CCS_IRevolverHandSocketPreviewDebugSource ActiveSource { get; private set; }

        public static void Register(CCS_IRevolverHandSocketPreviewDebugSource source)
        {
            ActiveSource = source;
        }

        public static void Unregister(CCS_IRevolverHandSocketPreviewDebugSource source)
        {
            if (ActiveSource == source)
            {
                ActiveSource = null;
            }
        }
    }
}
