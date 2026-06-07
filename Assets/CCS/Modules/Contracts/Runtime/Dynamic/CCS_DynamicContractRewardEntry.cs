using System;

// =============================================================================
// SCRIPT: CCS_DynamicContractRewardEntry
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: JsonUtility-compatible reward payload for persisted generated contracts.
// PLACEMENT: Embedded in CCS_DynamicContractState.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 save/load support.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_DynamicContractRewardEntry
    {
        public int tradeDollars;

        public int reputationGain = 2;

        public float prosperityGain = 1f;

        public int supplyType;

        public float supplyAmount = 1f;
    }
}
