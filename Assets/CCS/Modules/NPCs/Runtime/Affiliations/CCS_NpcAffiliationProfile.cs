using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationProfile
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Default loyalty and affiliation assignment policy for placeholder NPCs.
// PLACEMENT: Assets/CCS/Survival/Profiles/NPCs/Affiliations/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — wired on CCS_NpcAffiliationService.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [CreateAssetMenu(
        fileName = "CCS_NpcAffiliationProfile",
        menuName = "CCS/Survival/NPCs/NPC Affiliation Profile")]
    public sealed class CCS_NpcAffiliationProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private int defaultLoyaltyValue = 50;

        [SerializeField] private int minimumLoyaltyValue;

        [SerializeField] private int maximumLoyaltyValue = 100;

        [SerializeField] private bool requireSettlementAffiliation = true;

        [SerializeField] private bool requireWorkforceAffiliationForWorkers = true;

        [SerializeField] private bool requireBusinessAffiliationForRepresentatives = true;

        public int DefaultLoyaltyValue => defaultLoyaltyValue;

        public int MinimumLoyaltyValue => minimumLoyaltyValue;

        public int MaximumLoyaltyValue => maximumLoyaltyValue;

        public bool RequireSettlementAffiliation => requireSettlementAffiliation;

        public bool RequireWorkforceAffiliationForWorkers => requireWorkforceAffiliationForWorkers;

        public bool RequireBusinessAffiliationForRepresentatives => requireBusinessAffiliationForRepresentatives;

        public int ClampLoyalty(int loyaltyValue)
        {
            if (loyaltyValue < minimumLoyaltyValue)
            {
                return minimumLoyaltyValue;
            }

            if (loyaltyValue > maximumLoyaltyValue)
            {
                return maximumLoyaltyValue;
            }

            return loyaltyValue;
        }
    }
}
