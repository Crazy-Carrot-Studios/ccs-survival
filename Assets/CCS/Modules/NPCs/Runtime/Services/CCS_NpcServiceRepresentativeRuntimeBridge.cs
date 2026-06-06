using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Services
// PURPOSE: Tracks spawned representative actors and resolves display names for interactables.
// PLACEMENT: Used by representative service and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — syncs placeholders without scene scanning.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public delegate bool ResolveRepresentativeSnapshotHandler(
        string settlementId,
        string businessId,
        out CCS_NpcServiceRepresentativeSnapshot snapshot);

    public static class CCS_NpcServiceRepresentativeRuntimeBridge
    {
        private static readonly Dictionary<string, GameObject> SpawnedRepresentatives =
            new Dictionary<string, GameObject>(System.StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> RepresentativeDisplayNames =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        public static void RegisterDisplayName(string representativeId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(representativeId))
            {
                return;
            }

            RepresentativeDisplayNames[representativeId] = displayName ?? string.Empty;
        }

        public static string ResolveDisplayName(CCS_NpcServiceRepresentativeInteractable interactable)
        {
            if (interactable == null)
            {
                return "Service Representative";
            }

            if (RepresentativeDisplayNames.TryGetValue(interactable.RepresentativeId, out string displayName)
                && !string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return "Service Representative";
        }

        public static bool TryGetActiveRepresentativeSnapshot(
            string settlementId,
            string businessId,
            out CCS_NpcServiceRepresentativeSnapshot snapshot)
        {
            snapshot = CCS_NpcServiceRepresentativeSnapshot.Empty;
            if (ResolveRepresentativeService == null)
            {
                return false;
            }

            return ResolveRepresentativeService.Invoke(settlementId, businessId, out snapshot)
                && snapshot != null
                && snapshot.IsValid
                && snapshot.IsActive;
        }

        public static System.Func<string, string, bool, CCS_NpcServiceRepresentativeSnapshot, bool> SyncRepresentativeActor;

        public static ResolveRepresentativeSnapshotHandler ResolveRepresentativeService;

        public static bool TryGetRepresentativeCount(string settlementId, out int count)
        {
            count = 0;
            if (ResolveAllActiveRepresentatives == null)
            {
                return false;
            }

            CCS_NpcServiceRepresentativeSnapshot[] snapshots =
                ResolveAllActiveRepresentatives.Invoke(settlementId) ?? System.Array.Empty<CCS_NpcServiceRepresentativeSnapshot>();
            for (int index = 0; index < snapshots.Length; index++)
            {
                if (snapshots[index] != null && snapshots[index].IsActive)
                {
                    count++;
                }
            }

            return true;
        }

        public static System.Func<string, CCS_NpcServiceRepresentativeSnapshot[]> ResolveAllActiveRepresentatives;

        public static System.Action RefreshAllRepresentatives;

        public static void RefreshAllRepresentativeAssignments()
        {
            RefreshAllRepresentatives?.Invoke();
        }

        public static bool TrySimulateRepresentativeInteraction(string settlementId, string businessId)
        {
            if (!TryGetActiveRepresentativeSnapshot(settlementId, businessId, out CCS_NpcServiceRepresentativeSnapshot snapshot)
                || snapshot == null)
            {
                return false;
            }

            if (!CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(
                    snapshot.ServicePointId,
                    out CCS_SettlementServicePoint servicePoint)
                || servicePoint == null)
            {
                return false;
            }

            CCS_SettlementServiceActivationResult result = CCS_SettlementServiceRouteResolver.TryActivate(servicePoint);
            CCS_NpcServiceRepresentativeDebugHud.NotifyRouteResult(
                snapshot.RepresentativeId,
                snapshot.ServicePointId,
                snapshot.BusinessId,
                snapshot.SettlementId,
                result.RouteType,
                result.IsSuccess,
                false,
                result.Message);
            return result.IsSuccess;
        }

        public static void ClearSpawnedRepresentatives()
        {
            foreach (KeyValuePair<string, GameObject> pair in SpawnedRepresentatives)
            {
                if (pair.Value != null)
                {
                    Object.Destroy(pair.Value);
                }
            }

            SpawnedRepresentatives.Clear();
        }

        public static GameObject EnsureSpawnedRepresentativeRoot(string representativeId, Transform parent, Vector3 localOffset)
        {
            if (string.IsNullOrWhiteSpace(representativeId) || parent == null)
            {
                return null;
            }

            if (SpawnedRepresentatives.TryGetValue(representativeId, out GameObject existing) && existing != null)
            {
                existing.transform.SetParent(parent, false);
                existing.transform.localPosition = localOffset;
                return existing;
            }

            GameObject root = new GameObject($"CCS_ServiceRepresentative_{representativeId}");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localOffset;
            SpawnedRepresentatives[representativeId] = root;
            return root;
        }
    }
}
