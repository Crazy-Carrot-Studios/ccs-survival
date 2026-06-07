using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventProfile
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Profile catalog of dynamic settlement events and generation thresholds.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/Events/
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — simulation-driven; no quests or AI behavior trees.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_SettlementEventProfile",
        menuName = "CCS/Survival/Settlements/Settlement Event Profile")]
    public sealed class CCS_SettlementEventProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_SettlementEventDefinition[] eventDefinitions =
            Array.Empty<CCS_SettlementEventDefinition>();

        [SerializeField] private int evaluationIntervalHours = 6;

        [SerializeField] private bool allowSimulationGeneration = true;

        public CCS_SettlementEventDefinition[] EventDefinitions =>
            eventDefinitions ?? Array.Empty<CCS_SettlementEventDefinition>();

        public int EvaluationIntervalHours => evaluationIntervalHours < 1 ? 1 : evaluationIntervalHours;

        public bool AllowSimulationGeneration => allowSimulationGeneration;

        public bool TryGetDefinitionById(string eventId, out CCS_SettlementEventDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            CCS_SettlementEventDefinition[] definitions = EventDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementEventDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.EventId, eventId, StringComparison.OrdinalIgnoreCase))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetDefinitionForType(
            CCS_SettlementEventType eventType,
            string settlementId,
            out CCS_SettlementEventDefinition definition)
        {
            definition = null;
            CCS_SettlementEventDefinition[] definitions = EventDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementEventDefinition candidate = definitions[index];
                if (candidate == null || candidate.EventType != eventType)
                {
                    continue;
                }

                if (CCS_SettlementEventValidationUtility.IsSettlementEligible(candidate, settlementId))
                {
                    definition = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
