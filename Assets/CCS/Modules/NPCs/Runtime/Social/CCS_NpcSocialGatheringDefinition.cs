using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcSocialGatheringDefinition
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Maps settlement social gathering areas to world anchor ids.
// PLACEMENT: Serialized on CCS_NpcSocialProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — profile-driven social anchor catalog.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcSocialGatheringDefinition
    {
        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string anchorId = string.Empty;

        [SerializeField] private string displayName = string.Empty;

        [SerializeField] private int priority;

        public string SettlementId => settlementId ?? string.Empty;

        public string AnchorId => anchorId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int Priority => priority;
    }
}
