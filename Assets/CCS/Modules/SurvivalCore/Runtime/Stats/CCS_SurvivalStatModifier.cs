// =============================================================================
// SCRIPT: CCS_SurvivalStatModifier
// CATEGORY: Survival / Runtime / SurvivalCore / Stats
// PURPOSE: Additive or multiplicative modifier applied to a survival stat value.
// PLACEMENT: Passed to CCS_SurvivalStatState and CCS_SurvivalCoreService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No inventory or equipment coupling in 0.3.7.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public readonly struct CCS_SurvivalStatModifier
    {
        #region Public Methods

        public CCS_SurvivalStatModifier(float amount, bool isMultiplicative = false)
        {
            Amount = amount;
            IsMultiplicative = isMultiplicative;
        }

        public static CCS_SurvivalStatModifier Add(float amount)
        {
            return new CCS_SurvivalStatModifier(amount, false);
        }

        public static CCS_SurvivalStatModifier Multiply(float multiplier)
        {
            return new CCS_SurvivalStatModifier(multiplier, true);
        }

        #endregion

        #region Properties

        public float Amount { get; }

        public bool IsMultiplicative { get; }

        #endregion
    }
}
