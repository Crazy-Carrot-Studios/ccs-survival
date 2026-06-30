// =============================================================================
// SCRIPT: CCS_RevolverAimSetupPoseDebugRegistry
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Optional validation-scene registry for revolver aim setup pose debug overrides.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics manager registers on validation scenes only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_RevolverAimSetupPoseDebugRegistry
    {
        public static CCS_IRevolverAimSetupPoseDebugSource ActiveSource { get; private set; }

        public static void Register(CCS_IRevolverAimSetupPoseDebugSource source)
        {
            ActiveSource = source;
        }

        public static void Unregister(CCS_IRevolverAimSetupPoseDebugSource source)
        {
            if (ActiveSource == source)
            {
                ActiveSource = null;
            }
        }
    }
}
