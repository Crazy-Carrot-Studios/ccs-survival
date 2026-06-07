using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementNewsProfile
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Profile catalog for settlement news headlines and rumor propagation tuning.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/News/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — event-driven news only; no quest generation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementNewsProfile",
        menuName = "CCS/Survival/Settlements/Settlement News Profile")]
    public sealed class CCS_SettlementNewsProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_SettlementNewsDefinition[] newsDefinitions =
            Array.Empty<CCS_SettlementNewsDefinition>();

        [SerializeField] private int maxRecentNewsEntries = 3;

        [SerializeField] private int evaluationIntervalHours = 6;

        public CCS_SettlementNewsDefinition[] NewsDefinitions =>
            newsDefinitions ?? Array.Empty<CCS_SettlementNewsDefinition>();

        public int MaxRecentNewsEntries => maxRecentNewsEntries < 1 ? 1 : maxRecentNewsEntries;

        public int EvaluationIntervalHours => evaluationIntervalHours < 1 ? 1 : evaluationIntervalHours;

        public bool TryGetDefinitionForType(
            CCS_SettlementNewsType newsType,
            out CCS_SettlementNewsDefinition definition)
        {
            definition = null;
            CCS_SettlementNewsDefinition[] definitions = NewsDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementNewsDefinition candidate = definitions[index];
                if (candidate != null && candidate.NewsType == newsType)
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
