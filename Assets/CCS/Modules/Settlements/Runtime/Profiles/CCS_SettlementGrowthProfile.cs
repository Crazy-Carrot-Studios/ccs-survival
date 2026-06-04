using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthProfile
// CATEGORY: Modules / Settlements / Runtime / Profiles
// PURPOSE: Catalog of settlement growth stage requirements and starting stages.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementGrowthProfile",
        menuName = "CCS/Survival/Settlements/Settlement Growth Profile")]
    public sealed class CCS_SettlementGrowthProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_SettlementGrowthDefinition[] growthDefinitions =
            Array.Empty<CCS_SettlementGrowthDefinition>();

        [SerializeField] private CCS_SettlementGrowthStartingEntry[] startingEntries =
            Array.Empty<CCS_SettlementGrowthStartingEntry>();

        public CCS_SettlementGrowthDefinition[] GrowthDefinitions =>
            growthDefinitions ?? Array.Empty<CCS_SettlementGrowthDefinition>();

        public CCS_SettlementGrowthStartingEntry[] StartingEntries =>
            startingEntries ?? Array.Empty<CCS_SettlementGrowthStartingEntry>();

        public bool TryGetStartingStage(string settlementId, out CCS_SettlementGrowthStage startingStage)
        {
            startingStage = CCS_SettlementGrowthStage.Outpost;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_SettlementGrowthStartingEntry[] entries = StartingEntries;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_SettlementGrowthStartingEntry entry = entries[index];
                if (entry == null
                    || !string.Equals(entry.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                startingStage = entry.StartingGrowthStage;
                return true;
            }

            return true;
        }

        public bool TryGetDefinition(
            CCS_SettlementGrowthStage stage,
            out CCS_SettlementGrowthDefinition definition)
        {
            definition = null;
            CCS_SettlementGrowthDefinition[] definitions = GrowthDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementGrowthDefinition candidate = definitions[index];
                if (candidate != null && candidate.GrowthStage == stage)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class CCS_SettlementGrowthStartingEntry
    {
        public string settlementId = string.Empty;
        public int startingGrowthStage = (int)CCS_SettlementGrowthStage.Outpost;

        public CCS_SettlementGrowthStage StartingGrowthStage =>
            Enum.IsDefined(typeof(CCS_SettlementGrowthStage), startingGrowthStage)
                ? (CCS_SettlementGrowthStage)startingGrowthStage
                : CCS_SettlementGrowthStage.Outpost;
    }
}
