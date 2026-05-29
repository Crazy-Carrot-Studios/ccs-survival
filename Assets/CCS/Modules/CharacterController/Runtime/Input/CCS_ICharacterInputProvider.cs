// =============================================================================
// SCRIPT: CCS_ICharacterInputProvider
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Abstracts locomotion and look input for future New Input System wiring.
// PLACEMENT: Implemented by bridges, player components, or test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Full Input Actions asset hookup deferred past 0.3.8 foundation.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_ICharacterInputProvider
    {
        #region Public Methods

        CCS_CharacterInputSnapshot GetInputSnapshot();

        #endregion
    }
}
