using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Runtime bridge for affiliation snapshots, refresh, and playtest forcing.
// PLACEMENT: Wired by CCS_NpcAffiliationService and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — hosts enumerated via CCS_PopulationPlaceholderIdentityBridge.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcAffiliationRuntimeBridge
    {
        public static Func<string, string, CCS_NpcAffiliationSnapshot> ResolveAffiliationSnapshot;

        public static Action RefreshAllAffiliations;

        public static bool TryGetAffiliationSnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcAffiliationSnapshot snapshot)
        {
            snapshot = CCS_NpcAffiliationSnapshot.Empty;
            if (ResolveAffiliationSnapshot == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            snapshot = ResolveAffiliationSnapshot.Invoke(settlementId, npcIdentityId)
                ?? CCS_NpcAffiliationSnapshot.Empty;
            return snapshot.IsValid;
        }

        public static bool TryGetFirstHostAffiliationSnapshot(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcAffiliationSnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcAffiliationSnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcAffiliationSnapshot resolvedSnapshot = CCS_NpcAffiliationSnapshot.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                resolvedHost = candidate;
                TryGetAffiliationSnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static bool TryGetRepresentativeAffiliationSnapshot(
            string settlementId,
            string businessId,
            out CCS_NpcAffiliationSnapshot snapshot)
        {
            snapshot = CCS_NpcAffiliationSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(businessId))
            {
                return false;
            }

            bool found = false;
            CCS_NpcAffiliationSnapshot resolvedSnapshot = CCS_NpcAffiliationSnapshot.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || !candidate.IsServiceRepresentative
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(candidate.BusinessId, businessId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                TryGetAffiliationSnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = resolvedSnapshot.IsValid;
            });

            snapshot = resolvedSnapshot;
            return found;
        }

        public static void RefreshAllAffiliationHosts()
        {
            RefreshAllAffiliations?.Invoke();
        }
    }
}
