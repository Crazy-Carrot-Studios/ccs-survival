using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementNewsDefinition
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Profile entry for event-driven headlines and rumor lines.
// PLACEMENT: Serialized on CCS_SettlementNewsProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — templates resolved with settlement display names.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementNewsDefinition
    {
        [SerializeField] private string definitionId = string.Empty;

        [SerializeField] private CCS_SettlementNewsType newsType = CCS_SettlementNewsType.Unknown;

        [SerializeField] private string headlineTemplate = string.Empty;

        [SerializeField] private string rumorLineTemplate = string.Empty;

        [SerializeField] private int newsDurationDays = 3;

        [SerializeField] private int propagationDelayDays = 1;

        public string DefinitionId => definitionId ?? string.Empty;

        public CCS_SettlementNewsType NewsType => newsType;

        public string HeadlineTemplate => headlineTemplate ?? string.Empty;

        public string RumorLineTemplate => rumorLineTemplate ?? string.Empty;

        public int NewsDurationDays => newsDurationDays < 1 ? 1 : newsDurationDays;

        public int PropagationDelayDays => propagationDelayDays < 0 ? 0 : propagationDelayDays;
    }
}
