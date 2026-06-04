using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ServiceAccessRequirement
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Requirement fields for settlement service access rules.
// PLACEMENT: Embedded in CCS_ServiceAccessRule and evaluated by access utility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 2.8.0 — camp tier and land claim checks are placeholders.
// =============================================================================

namespace CCS.Modules.Reputation
{
    [Serializable]
    public sealed class CCS_ServiceAccessRequirement
    {
        [SerializeField] private bool enabled = true;

        [SerializeField] private CCS_ReputationTier minimumReputationTier = CCS_ReputationTier.Neutral;

        [SerializeField] private int minimumReputationValue = -100;

        [SerializeField] private bool requireDiscoveredSettlement;

        [SerializeField] private int requiredCampTier = -1;

        [SerializeField] private bool requireLandClaim;

        [SerializeField] private string futureHookPlaceholder = "service.access.placeholder";

        public bool Enabled => enabled;

        public CCS_ReputationTier MinimumReputationTier => minimumReputationTier;

        public int MinimumReputationValue => minimumReputationValue;

        public bool RequireDiscoveredSettlement => requireDiscoveredSettlement;

        public int RequiredCampTier => requiredCampTier;

        public bool RequireLandClaim => requireLandClaim;

        public string FutureHookPlaceholder => futureHookPlaceholder ?? string.Empty;
    }
}
