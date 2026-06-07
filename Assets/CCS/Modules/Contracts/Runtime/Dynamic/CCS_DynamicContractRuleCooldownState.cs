using System;

// =============================================================================
// SCRIPT: CCS_DynamicContractRuleCooldownState
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Tracks last generation day per settlement/rule for cooldown enforcement.
// PLACEMENT: Stored in CCS_SaveContractsWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 anti-spam persistence.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_DynamicContractRuleCooldownState
    {
        public string settlementId = string.Empty;

        public string ruleId = string.Empty;

        public int lastGeneratedDay;
    }
}
