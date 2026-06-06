using System;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Services
// PURPOSE: Wires NPC identity resolution into the survival placeholder identity bridge.
// PLACEMENT: Called from CCS_NpcIdentityService and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — no scene scanning; uses CCS_PopulationPlaceholderIdentityBridge.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcRuntimeBridge
    {
        public static void BindIdentityResolver(CCS_NpcIdentityService service)
        {
            if (service == null)
            {
                return;
            }

            CCS_PopulationPlaceholderIdentityBridge.ResolveIdentity =
                (anchorId, slotIndex, settlementId, workforceCategory, businessId) =>
                {
                    CCS_SettlementPopulationCategory category = Enum.IsDefined(
                            typeof(CCS_SettlementPopulationCategory),
                            workforceCategory)
                        ? (CCS_SettlementPopulationCategory)workforceCategory
                        : CCS_SettlementPopulationCategory.Unknown;
                    if (!service.TryResolveIdentity(
                            anchorId,
                            slotIndex,
                            settlementId,
                            category,
                            businessId,
                            out CCS_NpcIdentitySnapshot snapshot)
                        || snapshot == null)
                    {
                        return null;
                    }

                    return new CCS_PopulationPlaceholderIdentityData
                    {
                        NpcIdentityId = snapshot.NpcIdentityId,
                        DisplayName = snapshot.DisplayName,
                        RoleType = (int)snapshot.Role,
                        RoleDisplayName = snapshot.RoleDisplayName,
                        SettlementId = snapshot.SettlementId,
                        BusinessId = snapshot.BusinessId,
                        WorkforceCategory = (int)snapshot.WorkforceCategory,
                        HomeHousingId = snapshot.HomeHousingId
                    };
                };
        }

        public static void RefreshAllPlaceholderIdentities()
        {
            CCS_PopulationPlaceholderIdentityBridge.RefreshAllHosts();
        }

        public static void RefreshSettlementIdentities(string settlementId)
        {
            CCS_PopulationPlaceholderIdentityBridge.RefreshSettlementHosts(settlementId);
        }

        public static int GetPlaceholderCountWithIdentity(
            string settlementId,
            CCS_SettlementPopulationCategory workforceCategory) =>
            CCS_PopulationPlaceholderIdentityBridge.GetHostCountWithIdentity(
                settlementId,
                (int)workforceCategory);

        public static bool TryGetFirstPlaceholderIdentity(
            string settlementId,
            CCS_SettlementPopulationCategory workforceCategory,
            out CCS_NpcIdentitySnapshot snapshot)
        {
            snapshot = CCS_NpcIdentitySnapshot.Empty;
            if (!CCS_PopulationPlaceholderIdentityBridge.TryGetFirstHostIdentity(
                    settlementId,
                    (int)workforceCategory,
                    out CCS_PopulationPlaceholderIdentityData data)
                || data == null)
            {
                return false;
            }

            snapshot = new CCS_NpcIdentitySnapshot
            {
                NpcIdentityId = data.NpcIdentityId,
                DisplayName = data.DisplayName,
                Role = (CCS_NpcRoleType)data.RoleType,
                RoleDisplayName = data.RoleDisplayName,
                SettlementId = data.SettlementId,
                BusinessId = data.BusinessId,
                WorkforceCategory = Enum.IsDefined(typeof(CCS_SettlementPopulationCategory), data.WorkforceCategory)
                    ? (CCS_SettlementPopulationCategory)data.WorkforceCategory
                    : CCS_SettlementPopulationCategory.Unknown
            };
            return snapshot.IsValid;
        }
    }
}
