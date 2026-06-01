// =============================================================================
// SCRIPT: CCS_CombatEventArgs
// CATEGORY: Modules / Combat / Runtime / Events
// PURPOSE: Event payload for combat hits and wildlife kills.
// PLACEMENT: Raised by CCS_CombatService for HUD notification wiring.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No UI references in combat runtime assembly.
// =============================================================================

namespace CCS.Modules.Combat
{
    public sealed class CCS_CombatEventArgs
    {
        #region Properties

        public CCS_CombatHitResult HitResult { get; }

        #endregion

        #region Public Methods

        public CCS_CombatEventArgs(CCS_CombatHitResult hitResult)
        {
            HitResult = hitResult;
        }

        #endregion
    }
}
