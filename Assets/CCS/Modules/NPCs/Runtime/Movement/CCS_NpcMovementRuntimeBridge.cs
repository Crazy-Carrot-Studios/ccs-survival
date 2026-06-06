using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcMovementRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Host registry and transform application for lightweight NPC movement.
// PLACEMENT: Wired by CCS_NpcMovementService and population placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — hosts enumerated via CCS_PopulationPlaceholderIdentityBridge.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcMovementRuntimeBridge
    {
        public static Func<string, string, CCS_NpcMovementSnapshot> ResolveMovementSnapshot;

        public static Action RefreshAllMovement;

        public static int GetRegisteredHostCount()
        {
            return CCS_PopulationPlaceholderIdentityBridge.GetRegisteredHostCount();
        }

        public static int GetRegisteredHostCountWithIdentity()
        {
            int count = 0;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null && host.HasIdentity)
                {
                    count++;
                }
            });
            return count;
        }

        public static bool TryGetMovementSnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcMovementSnapshot snapshot)
        {
            snapshot = CCS_NpcMovementSnapshot.Empty;
            if (ResolveMovementSnapshot == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            snapshot = ResolveMovementSnapshot.Invoke(settlementId, npcIdentityId) ?? CCS_NpcMovementSnapshot.Empty;
            return snapshot.IsValid;
        }

        public static bool TryGetFirstHostWithIdentity(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcMovementSnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcMovementSnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcMovementSnapshot resolvedSnapshot = CCS_NpcMovementSnapshot.Empty;
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
                TryGetMovementSnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static bool TryGetRepresentativeHostWithIdentity(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcMovementSnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcMovementSnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcMovementSnapshot resolvedSnapshot = CCS_NpcMovementSnapshot.Empty;
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

                resolvedHost = candidate;
                TryGetMovementSnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static void ForEachHost(Action<CCS_INpcMovementHost> visitor)
        {
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(visitor);
        }

        public static void ApplyMovementTransform(CCS_INpcMovementHost host, Vector3 position, float yawDegrees)
        {
            if (host?.MovementTransform == null)
            {
                return;
            }

            host.MovementTransform.position = position;
            host.MovementTransform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        public static void ApplyIdleRotation(CCS_INpcMovementHost host, float idleRotationSpeed, float deltaTime)
        {
            if (host?.MovementTransform == null || idleRotationSpeed <= 0f || deltaTime <= 0f)
            {
                return;
            }

            host.MovementTransform.Rotate(0f, idleRotationSpeed * deltaTime, 0f, Space.World);
        }

        public static void RefreshAllMovementHosts()
        {
            RefreshAllMovement?.Invoke();
        }
    }
}
