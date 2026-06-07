// =============================================================================
// SCRIPT: CCS_SettlementNewsSnapshot
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Runtime news snapshot for contract boards, dialogue, and playtest hooks.
// PLACEMENT: Built by CCS_SettlementNewsService from persisted news state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — read-only presentation snapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementNewsSnapshot
    {
        public static readonly CCS_SettlementNewsSnapshot Empty = new CCS_SettlementNewsSnapshot();

        public string NewsId { get; set; } = string.Empty;

        public string OriginSettlementId { get; set; } = string.Empty;

        public string ViewingSettlementId { get; set; } = string.Empty;

        public CCS_SettlementNewsType NewsType { get; set; } = CCS_SettlementNewsType.Unknown;

        public string Headline { get; set; } = string.Empty;

        public string RumorLine { get; set; } = string.Empty;

        public int DayNumber { get; set; } = 1;

        public int ExpirationDay { get; set; } = 1;

        public bool IsActive { get; set; }

        public bool IsValid =>
            IsActive
            && !string.IsNullOrWhiteSpace(NewsId)
            && !string.IsNullOrWhiteSpace(Headline);
    }
}
