// =============================================================================
// SCRIPT: CCS_IPopulationPlaceholderIdentityHost
// CATEGORY: Survival / Runtime / Population
// PURPOSE: Contract for applying NPC identity data to population placeholder actors.
// PLACEMENT: Implemented by CCS_PopulationPlaceholderActor; consumed by NPC module bridge.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — avoids Settlements <-> NPCs assembly cycle.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_IPopulationPlaceholderIdentityHost
    {
        string NpcIdentityId { get; }

        string DisplayName { get; }

        int RoleType { get; }

        string RoleDisplayName { get; }

        string SettlementId { get; }

        string BusinessId { get; }

        int WorkforceCategoryValue { get; }

        bool HasIdentity { get; }

        bool IsServiceRepresentative { get; }

        string RepresentativeTitle { get; }

        void BindAnchorContext(string anchorId, int slotIndex, string settlementId, string businessId);

        void ApplyIdentityData(
            string identityId,
            string name,
            int roleType,
            string roleDisplayName,
            string anchorSettlementId,
            string anchorBusinessId,
            int workforceCategory);

        void ApplyServiceRepresentativePresentation(string title);

        void ClearServiceRepresentativePresentation();

        void RefreshIdentityFromBridge();
    }
}
