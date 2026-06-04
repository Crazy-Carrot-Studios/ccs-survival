// =============================================================================
// SCRIPT: CCS_ReputationPriceModifierUtility
// CATEGORY: Modules / Reputation / Runtime / Validation
// PURPOSE: Resolves vendor buy/sell price modifiers from settlement reputation tier.
// PLACEMENT: Used by CCS_VendorService and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 — returns 1.0 when reputation service or profile is missing.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public static class CCS_ReputationPriceModifierUtility
    {
        public static float ResolveBuyPriceModifier(
            CCS_ReputationService reputationService,
            string settlementId)
        {
            if (!TryResolveStanding(reputationService, settlementId, out CCS_ReputationTier tier, out CCS_ReputationProfile profile)
                || profile == null
                || !profile.EnableBuyPriceModifiers)
            {
                return 1f;
            }

            return ResolveBuyModifierForTier(profile, tier);
        }

        public static float ResolveSellPriceModifier(
            CCS_ReputationService reputationService,
            string settlementId)
        {
            if (!TryResolveStanding(reputationService, settlementId, out CCS_ReputationTier tier, out CCS_ReputationProfile profile)
                || profile == null
                || !profile.EnableSellPriceModifiers)
            {
                return 1f;
            }

            return ResolveSellModifierForTier(profile, tier);
        }

        public static int ApplyModifier(int baseUnitPrice, float modifier)
        {
            if (baseUnitPrice <= 0)
            {
                return 0;
            }

            float clampedModifier = modifier <= 0f ? 1f : modifier;
            int finalPrice = UnityEngine.Mathf.RoundToInt(baseUnitPrice * clampedModifier);
            return finalPrice < 1 ? 1 : finalPrice;
        }

        private static bool TryResolveStanding(
            CCS_ReputationService reputationService,
            string settlementId,
            out CCS_ReputationTier tier,
            out CCS_ReputationProfile profile)
        {
            tier = CCS_ReputationTier.Neutral;
            profile = reputationService?.ActiveProfile;
            if (reputationService == null
                || !reputationService.IsInitialized
                || profile == null
                || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (reputationService.TryGetSettlementStanding(settlementId, out CCS_ReputationStanding standing)
                && standing != null)
            {
                tier = standing.DisplayTier;
            }

            return true;
        }

        private static float ResolveBuyModifierForTier(CCS_ReputationProfile profile, CCS_ReputationTier tier)
        {
            switch (tier)
            {
                case CCS_ReputationTier.Hostile:
                    return profile.HostileBuyPriceModifier;
                case CCS_ReputationTier.Distrusted:
                    return profile.DistrustedBuyPriceModifier;
                case CCS_ReputationTier.Trusted:
                    return profile.TrustedBuyPriceModifier;
                case CCS_ReputationTier.Honored:
                    return profile.HonoredBuyPriceModifier;
                default:
                    return profile.NeutralBuyPriceModifier;
            }
        }

        private static float ResolveSellModifierForTier(CCS_ReputationProfile profile, CCS_ReputationTier tier)
        {
            switch (tier)
            {
                case CCS_ReputationTier.Hostile:
                    return profile.HostileSellPriceModifier;
                case CCS_ReputationTier.Distrusted:
                    return profile.DistrustedSellPriceModifier;
                case CCS_ReputationTier.Trusted:
                    return profile.TrustedSellPriceModifier;
                case CCS_ReputationTier.Honored:
                    return profile.HonoredSellPriceModifier;
                default:
                    return profile.NeutralSellPriceModifier;
            }
        }
    }
}
