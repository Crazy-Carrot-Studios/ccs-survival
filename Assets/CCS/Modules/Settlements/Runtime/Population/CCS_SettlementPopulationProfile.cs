using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationProfile
// CATEGORY: Modules / Settlements / Runtime / Population
// PURPOSE: Profile defaults for settlement population growth, capacity, and workforce bias.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/Population/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 — wired on CCS_WorldSimulationProfile.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementPopulationProfile",
        menuName = "CCS/Survival/Settlements/Settlement Population Profile")]
    public sealed class CCS_SettlementPopulationProfile : CCS_SurvivalProfileBase
    {
        [Header("Growth Modifiers")]
        [SerializeField] private float basePassiveGrowthRate = 0.25f;

        [SerializeField] private float prosperityGrowthFactor = 0.015f;

        [SerializeField] private float contractCompletionGrowthBonus = 2f;

        [SerializeField] private float poorSupplyThresholdPercent = 25f;

        [SerializeField] private float poorSupplyGrowthMultiplier = 0.5f;

        [SerializeField] private float trustedGrowthMultiplier = 1.1f;

        [SerializeField] private float honoredGrowthMultiplier = 1.15f;

        [SerializeField] private float distrustedGrowthMultiplier = 0.9f;

        [SerializeField] private float hostileGrowthMultiplier = 0.75f;

        [Header("Capacity")]
        [SerializeField] private int basePopulationCapacity = 40;

        [SerializeField] private float capacityPerProsperityPoint = 1.25f;

        [Header("Settlement Defaults")]
        [SerializeField] private CCS_SettlementPopulationSettlementEntry[] settlementEntries =
            Array.Empty<CCS_SettlementPopulationSettlementEntry>();

        public float BasePassiveGrowthRate => basePassiveGrowthRate < 0f ? 0f : basePassiveGrowthRate;

        public float ProsperityGrowthFactor => prosperityGrowthFactor < 0f ? 0f : prosperityGrowthFactor;

        public float ContractCompletionGrowthBonus =>
            contractCompletionGrowthBonus < 0f ? 0f : contractCompletionGrowthBonus;

        public float PoorSupplyThresholdPercent => Mathf.Clamp(poorSupplyThresholdPercent, 0f, 100f);

        public float PoorSupplyGrowthMultiplier =>
            poorSupplyGrowthMultiplier <= 0f ? 1f : poorSupplyGrowthMultiplier;

        public float TrustedGrowthMultiplier => trustedGrowthMultiplier <= 0f ? 1f : trustedGrowthMultiplier;

        public float HonoredGrowthMultiplier => honoredGrowthMultiplier <= 0f ? 1f : honoredGrowthMultiplier;

        public float DistrustedGrowthMultiplier =>
            distrustedGrowthMultiplier <= 0f ? 1f : distrustedGrowthMultiplier;

        public float HostileGrowthMultiplier => hostileGrowthMultiplier <= 0f ? 1f : hostileGrowthMultiplier;

        public int BasePopulationCapacity => basePopulationCapacity < 0 ? 0 : basePopulationCapacity;

        public float CapacityPerProsperityPoint =>
            capacityPerProsperityPoint < 0f ? 0f : capacityPerProsperityPoint;

        public CCS_SettlementPopulationSettlementEntry[] SettlementEntries =>
            settlementEntries ?? Array.Empty<CCS_SettlementPopulationSettlementEntry>();

        public bool TryGetSettlementEntry(
            string settlementId,
            out CCS_SettlementPopulationSettlementEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_SettlementPopulationSettlementEntry[] entries = SettlementEntries;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_SettlementPopulationSettlementEntry candidate = entries[index];
                if (candidate != null
                    && string.Equals(candidate.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class CCS_SettlementPopulationSettlementEntry
    {
        public string settlementId = string.Empty;
        public int startingPopulation = 12;
        public int startingCapacity = 60;
    }
}
