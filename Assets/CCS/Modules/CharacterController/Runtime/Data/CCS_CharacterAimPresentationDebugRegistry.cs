// =============================================================================
// SCRIPT: CCS_CharacterAimPresentationDebugRegistry
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Optional validation-scene registry for presentation-only aim debug overrides.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Diagnostics manager registers on validation scenes only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterAimPresentationDebugRegistry
    {
        public static CCS_ICharacterAimPresentationDebugSource ActiveSource { get; private set; }

        public static void Register(CCS_ICharacterAimPresentationDebugSource source)
        {
            ActiveSource = source;
        }

        public static void Unregister(CCS_ICharacterAimPresentationDebugSource source)
        {
            if (ActiveSource == source)
            {
                ActiveSource = null;
            }
        }
    }
}
