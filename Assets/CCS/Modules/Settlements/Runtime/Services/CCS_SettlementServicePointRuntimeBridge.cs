using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_SettlementServicePointRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Registry for settlement service points by stable servicePointId.
// PLACEMENT: Used by NPC service representatives and validation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — no scene scanning; explicit OnEnable registration only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementServicePointRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_SettlementServicePoint> ServicePointLookup =
            new Dictionary<string, CCS_SettlementServicePoint>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterServicePoint(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null || string.IsNullOrWhiteSpace(servicePoint.ServicePointId))
            {
                return;
            }

            ServicePointLookup[servicePoint.ServicePointId] = servicePoint;
        }

        public static void UnregisterServicePoint(CCS_SettlementServicePoint servicePoint)
        {
            if (servicePoint == null || string.IsNullOrWhiteSpace(servicePoint.ServicePointId))
            {
                return;
            }

            if (ServicePointLookup.TryGetValue(servicePoint.ServicePointId, out CCS_SettlementServicePoint existing)
                && existing == servicePoint)
            {
                ServicePointLookup.Remove(servicePoint.ServicePointId);
            }
        }

        public static bool TryGetServicePoint(string servicePointId, out CCS_SettlementServicePoint servicePoint)
        {
            servicePoint = null;
            if (string.IsNullOrWhiteSpace(servicePointId))
            {
                return false;
            }

            return ServicePointLookup.TryGetValue(servicePointId, out servicePoint) && servicePoint != null;
        }

        public static int GetRegisteredServicePointCount()
        {
            return ServicePointLookup.Count;
        }
    }
}
