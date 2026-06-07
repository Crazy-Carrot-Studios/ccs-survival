using System;

// =============================================================================
// SCRIPT: CCS_NpcSocialSnapshot
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Runtime social gathering snapshot for labels, playtest, and debug HUD.
// PLACEMENT: Built by CCS_NpcSocialService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — no relationships or friendship simulation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcSocialSnapshot
    {
        public static readonly CCS_NpcSocialSnapshot Empty = new CCS_NpcSocialSnapshot();

        public string NpcIdentityId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string GroupId { get; set; } = string.Empty;

        public string AnchorId { get; set; } = string.Empty;

        public string AnchorDisplayName { get; set; } = string.Empty;

        public int ParticipantCount { get; set; }

        public int LastEvaluatedHour { get; set; } = -1;

        public bool IsSocializing { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(SettlementId);
    }

    public sealed class CCS_NpcSocialGroupSnapshot
    {
        public static readonly CCS_NpcSocialGroupSnapshot Empty = new CCS_NpcSocialGroupSnapshot();

        public string GroupId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string AnchorId { get; set; } = string.Empty;

        public int ParticipantCount { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(GroupId)
            && !string.IsNullOrWhiteSpace(SettlementId)
            && !string.IsNullOrWhiteSpace(AnchorId)
            && ParticipantCount > 0;
    }
}
