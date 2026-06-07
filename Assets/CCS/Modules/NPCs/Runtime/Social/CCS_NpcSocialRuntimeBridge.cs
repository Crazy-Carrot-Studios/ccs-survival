using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcSocialRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Runtime bridge for social evaluation, group snapshots, and playtest hooks.
// PLACEMENT: Wired by CCS_NpcSocialService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — groups rebuilt from persisted social state after load.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcSocialRuntimeBridge
    {
        public static Func<string, string, CCS_NpcSocialSnapshot> ResolveSocialSnapshot;

        public static Func<string, CCS_NpcSocialGroupSnapshot[]> ResolveGroupsForSettlement;

        public static Action RefreshAllSocialHosts;

        public delegate bool ResolveNearestSocialAnchorHandler(
            CCS_INpcMovementHost host,
            out Vector3 targetPosition,
            out string anchorId,
            out string anchorDisplayName);

        public static ResolveNearestSocialAnchorHandler TryResolveNearestSocialAnchorForHost;

        public static void RefreshAllSocialPresence()
        {
            RefreshAllSocialHosts?.Invoke();
        }

        public static bool TryGetSocialSnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcSocialSnapshot snapshot)
        {
            snapshot = ResolveSocialSnapshot?.Invoke(settlementId, npcIdentityId) ?? CCS_NpcSocialSnapshot.Empty;
            return snapshot != null && snapshot.IsValid;
        }

        public static bool TryGetGroupsForSettlement(
            string settlementId,
            out CCS_NpcSocialGroupSnapshot[] groups)
        {
            groups = ResolveGroupsForSettlement?.Invoke(settlementId) ?? Array.Empty<CCS_NpcSocialGroupSnapshot>();
            return groups != null && groups.Length > 0;
        }

        public static bool TryGetFirstSocializingSnapshot(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcSocialSnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcSocialSnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcSocialSnapshot resolvedSnapshot = CCS_NpcSocialSnapshot.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || candidate.IsServiceRepresentative
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!TryGetSocialSnapshot(settlementId, candidate.NpcIdentityId, out CCS_NpcSocialSnapshot candidateSnapshot)
                    || candidateSnapshot == null
                    || !candidateSnapshot.IsSocializing)
                {
                    return;
                }

                resolvedHost = candidate;
                resolvedSnapshot = candidateSnapshot;
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static bool TryGetRepresentativeSocialSnapshot(
            string settlementId,
            out CCS_NpcSocialSnapshot snapshot)
        {
            snapshot = CCS_NpcSocialSnapshot.Empty;
            bool found = false;
            CCS_NpcSocialSnapshot resolvedSnapshot = CCS_NpcSocialSnapshot.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || !candidate.IsServiceRepresentative
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!TryGetSocialSnapshot(settlementId, candidate.NpcIdentityId, out CCS_NpcSocialSnapshot candidateSnapshot)
                    || candidateSnapshot == null
                    || !candidateSnapshot.IsSocializing)
                {
                    return;
                }

                resolvedSnapshot = candidateSnapshot;
                found = true;
            });

            snapshot = resolvedSnapshot;
            return found;
        }
    }
}
