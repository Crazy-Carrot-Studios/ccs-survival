// =============================================================================
// SCRIPT: CCS_StarterLoadoutEvents
// CATEGORY: Survival / Runtime / Player / Loadout
// PURPOSE: Event contracts for starter loadout application.
// PLACEMENT: Raised by CCS_StarterLoadoutService after successful grants.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No UI coupling in 0.9.1.
// =============================================================================

namespace CCS.Survival.Player.Loadout
{
    public delegate void StarterLoadoutAppliedHandler(CCS_StarterLoadoutAppliedEventArgs eventArgs);

    public sealed class CCS_StarterLoadoutAppliedEventArgs
    {
        #region Public Methods

        public CCS_StarterLoadoutAppliedEventArgs(int grantedItemCount, int grantedCurrencyAmount)
        {
            GrantedItemCount = grantedItemCount;
            GrantedCurrencyAmount = grantedCurrencyAmount;
        }

        #endregion

        #region Properties

        public int GrantedItemCount { get; }

        public int GrantedCurrencyAmount { get; }

        #endregion
    }
}
