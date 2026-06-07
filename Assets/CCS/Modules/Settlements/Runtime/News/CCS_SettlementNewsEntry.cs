// =============================================================================
// SCRIPT: CCS_SettlementNewsEntry
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Lightweight active news entry for settlement information surfaces.
// PLACEMENT: Returned by CCS_SettlementNewsService recent-news queries.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — metadata only; no quest or investigation hooks.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementNewsEntry
    {
        public string NewsId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string OriginSettlementId { get; set; } = string.Empty;

        public CCS_SettlementNewsType NewsType { get; set; } = CCS_SettlementNewsType.Unknown;

        public string Headline { get; set; } = string.Empty;

        public string RumorLine { get; set; } = string.Empty;

        public int DayNumber { get; set; } = 1;

        public int ExpirationDay { get; set; } = 1;

        public bool IsActive { get; set; }

        public static CCS_SettlementNewsEntry FromSnapshot(CCS_SettlementNewsSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return null;
            }

            return new CCS_SettlementNewsEntry
            {
                NewsId = snapshot.NewsId,
                SettlementId = snapshot.ViewingSettlementId,
                OriginSettlementId = snapshot.OriginSettlementId,
                NewsType = snapshot.NewsType,
                Headline = snapshot.Headline,
                RumorLine = snapshot.RumorLine,
                DayNumber = snapshot.DayNumber,
                ExpirationDay = snapshot.ExpirationDay,
                IsActive = snapshot.IsActive
            };
        }
    }
}
