using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_PopulationPlaceholderIdentityBridge
// CATEGORY: Survival / Runtime / Population
// PURPOSE: Host registry and identity resolver hook for population placeholder NPCs.
// PLACEMENT: Resolver assigned by CCS_NpcIdentityService; hosts are placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — breaks Settlements/NPCs assembly dependency cycle.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_PopulationPlaceholderIdentityBridge
    {
        public static Func<string, int, string, int, string, CCS_PopulationPlaceholderIdentityData> ResolveIdentity;

        private static readonly List<CCS_IPopulationPlaceholderIdentityHost> RegisteredHosts =
            new List<CCS_IPopulationPlaceholderIdentityHost>();

        public static void RegisterHost(CCS_IPopulationPlaceholderIdentityHost host)
        {
            if (host == null || RegisteredHosts.Contains(host))
            {
                return;
            }

            RegisteredHosts.Add(host);
        }

        public static void UnregisterHost(CCS_IPopulationPlaceholderIdentityHost host)
        {
            if (host == null)
            {
                return;
            }

            RegisteredHosts.Remove(host);
        }

        public static bool TryAssignIdentity(
            CCS_IPopulationPlaceholderIdentityHost host,
            string anchorId,
            int slotIndex,
            string settlementId,
            int workforceCategory,
            string businessId)
        {
            if (host == null || ResolveIdentity == null)
            {
                return false;
            }

            CCS_PopulationPlaceholderIdentityData data = ResolveIdentity.Invoke(
                anchorId,
                slotIndex,
                settlementId,
                workforceCategory,
                businessId);
            if (data == null || !data.IsValid)
            {
                return false;
            }

            host.ApplyIdentityData(
                data.NpcIdentityId,
                data.DisplayName,
                data.RoleType,
                data.RoleDisplayName,
                data.SettlementId,
                data.BusinessId,
                data.WorkforceCategory);
            return true;
        }

        public static void RefreshAllHosts()
        {
            for (int index = RegisteredHosts.Count - 1; index >= 0; index--)
            {
                CCS_IPopulationPlaceholderIdentityHost host = RegisteredHosts[index];
                if (host == null)
                {
                    RegisteredHosts.RemoveAt(index);
                    continue;
                }

                host.RefreshIdentityFromBridge();
            }
        }

        public static void RefreshSettlementHosts(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            for (int index = RegisteredHosts.Count - 1; index >= 0; index--)
            {
                CCS_IPopulationPlaceholderIdentityHost host = RegisteredHosts[index];
                if (host == null)
                {
                    RegisteredHosts.RemoveAt(index);
                    continue;
                }

                if (string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    host.RefreshIdentityFromBridge();
                }
            }
        }

        public static int GetHostCountWithIdentity(string settlementId, int workforceCategory)
        {
            int count = 0;
            for (int index = 0; index < RegisteredHosts.Count; index++)
            {
                CCS_IPopulationPlaceholderIdentityHost host = RegisteredHosts[index];
                if (host == null || !host.HasIdentity)
                {
                    continue;
                }

                if (!string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (host.WorkforceCategoryValue != workforceCategory)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        public static bool TryGetFirstHostIdentity(
            string settlementId,
            int workforceCategory,
            out CCS_PopulationPlaceholderIdentityData data)
        {
            data = null;
            for (int index = 0; index < RegisteredHosts.Count; index++)
            {
                CCS_IPopulationPlaceholderIdentityHost host = RegisteredHosts[index];
                if (host == null || !host.HasIdentity)
                {
                    continue;
                }

                if (!string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (host.WorkforceCategoryValue != workforceCategory)
                {
                    continue;
                }

                data = new CCS_PopulationPlaceholderIdentityData
                {
                    NpcIdentityId = host.NpcIdentityId,
                    DisplayName = host.DisplayName,
                    RoleType = host.RoleType,
                    RoleDisplayName = host.RoleDisplayName,
                    SettlementId = host.SettlementId,
                    BusinessId = host.BusinessId,
                    WorkforceCategory = host.WorkforceCategoryValue
                };
                return data.IsValid;
            }

            return false;
        }

        public static bool TryGetFirstHostByBusinessId(
            string settlementId,
            string businessId,
            out CCS_IPopulationPlaceholderIdentityHost host)
        {
            host = null;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(businessId))
            {
                return false;
            }

            for (int index = 0; index < RegisteredHosts.Count; index++)
            {
                CCS_IPopulationPlaceholderIdentityHost candidate = RegisteredHosts[index];
                if (candidate == null || !candidate.HasIdentity)
                {
                    continue;
                }

                if (!string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(candidate.BusinessId, businessId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                host = candidate;
                return true;
            }

            return false;
        }
    }

    public sealed class CCS_PopulationPlaceholderIdentityData
    {
        public string NpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int RoleType { get; set; }

        public string RoleDisplayName { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public int WorkforceCategory { get; set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(DisplayName)
            && RoleType > 0
            && WorkforceCategory > 0;
    }
}
