using System;

// =============================================================================
// SCRIPT: CCS_ReputationSnapshot
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Serializable reputation standing for save/load and validation.
// PLACEMENT: Used by CCS_ReputationService and CCS_SaveReputationWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [Serializable]
    public sealed class CCS_ReputationSnapshot
    {
        public string reputationDefinitionId = string.Empty;
        public int scopeType;
        public string targetId = string.Empty;
        public int currentValue;
        public int minValue;
        public int maxValue;
        public int displayTier;
        public string lastEventSummaryPlaceholder = string.Empty;
    }
}
