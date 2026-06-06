using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeUtility
// CATEGORY: Modules / NPCs / Runtime / Validation
// PURPOSE: Representative id building, role/route mapping, and state helpers.
// PLACEMENT: Used by representative service, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — routes through existing settlement service resolver.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcServiceRepresentativeUtility
    {
        public static string BuildRepresentativeId(string settlementId, string businessId)
        {
            string safeSettlement = string.IsNullOrWhiteSpace(settlementId) ? "unknown" : settlementId.Trim();
            string safeBusiness = string.IsNullOrWhiteSpace(businessId) ? "unknown" : businessId.Trim();
            return $"{CCS_NpcServiceRepresentativeContentIds.RepresentativeIdPrefix}.{safeSettlement}.{safeBusiness}";
        }

        public static string BuildRepresentativeAnchorId(string settlementId, string businessId)
        {
            string safeSettlement = string.IsNullOrWhiteSpace(settlementId) ? "unknown" : settlementId.Trim();
            string safeBusiness = string.IsNullOrWhiteSpace(businessId) ? "unknown" : businessId.Trim();
            return $"{CCS_NpcServiceRepresentativeContentIds.RepresentativeAnchorPrefix}.{safeSettlement}.{safeBusiness}";
        }

        public static CCS_SettlementServiceRouteType ResolveRouteType(CCS_NpcRoleType role)
        {
            return role switch
            {
                CCS_NpcRoleType.Merchant => CCS_SettlementServiceRouteType.Vendor,
                CCS_NpcRoleType.Banker => CCS_SettlementServiceRouteType.Bank,
                CCS_NpcRoleType.StableHand => CCS_SettlementServiceRouteType.Vendor,
                CCS_NpcRoleType.Gunsmith => CCS_SettlementServiceRouteType.Vendor,
                CCS_NpcRoleType.Blacksmith => CCS_SettlementServiceRouteType.Industry,
                CCS_NpcRoleType.Clerk => CCS_SettlementServiceRouteType.ContractBoard,
                CCS_NpcRoleType.Farmer => CCS_SettlementServiceRouteType.Vendor,
                CCS_NpcRoleType.Miner => CCS_SettlementServiceRouteType.Vendor,
                CCS_NpcRoleType.LumberWorker => CCS_SettlementServiceRouteType.Vendor,
                _ => CCS_SettlementServiceRouteType.Unknown
            };
        }

        public static CCS_SettlementPopulationCategory ResolveWorkforceCategory(CCS_NpcRoleType role)
        {
            return role switch
            {
                CCS_NpcRoleType.Merchant => CCS_SettlementPopulationCategory.Merchants,
                CCS_NpcRoleType.Banker => CCS_SettlementPopulationCategory.Merchants,
                CCS_NpcRoleType.StableHand => CCS_SettlementPopulationCategory.Ranchers,
                CCS_NpcRoleType.Gunsmith => CCS_SettlementPopulationCategory.Miners,
                CCS_NpcRoleType.Blacksmith => CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.Clerk => CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.Farmer => CCS_SettlementPopulationCategory.Farmers,
                CCS_NpcRoleType.Miner => CCS_SettlementPopulationCategory.Miners,
                CCS_NpcRoleType.LumberWorker => CCS_SettlementPopulationCategory.LumberWorkers,
                _ => CCS_SettlementPopulationCategory.Laborers
            };
        }

        public static CCS_NpcRoleType ResolveRoleForBusinessType(CCS_BusinessType businessType)
        {
            return businessType switch
            {
                CCS_BusinessType.GeneralStore => CCS_NpcRoleType.Merchant,
                CCS_BusinessType.Bank => CCS_NpcRoleType.Banker,
                CCS_BusinessType.Stable => CCS_NpcRoleType.StableHand,
                CCS_BusinessType.Gunsmith => CCS_NpcRoleType.Gunsmith,
                CCS_BusinessType.Blacksmith => CCS_NpcRoleType.Blacksmith,
                CCS_BusinessType.ContractOffice => CCS_NpcRoleType.Clerk,
                CCS_BusinessType.FarmSupply => CCS_NpcRoleType.Farmer,
                CCS_BusinessType.MiningSupplier => CCS_NpcRoleType.Miner,
                CCS_BusinessType.LumberYard => CCS_NpcRoleType.LumberWorker,
                _ => CCS_NpcRoleType.Unknown
            };
        }

        public static string ResolveDefaultTitle(CCS_NpcRoleType role, CCS_NpcIdentityProfile identityProfile)
        {
            string roleDisplay = CCS_NpcIdentityValidationUtility.ResolveRoleDisplayName(identityProfile, role);
            return role switch
            {
                CCS_NpcRoleType.Merchant => $"Frontier {roleDisplay}",
                CCS_NpcRoleType.Banker => $"Frontier {roleDisplay}",
                CCS_NpcRoleType.StableHand => $"Settlement {roleDisplay}",
                CCS_NpcRoleType.Gunsmith => $"Settlement {roleDisplay}",
                CCS_NpcRoleType.Blacksmith => $"Settlement {roleDisplay}",
                CCS_NpcRoleType.Clerk => $"Settlement {roleDisplay}",
                CCS_NpcRoleType.Farmer => $"Settlement {roleDisplay}",
                CCS_NpcRoleType.Miner => $"Camp {roleDisplay}",
                CCS_NpcRoleType.LumberWorker => $"Camp {roleDisplay}",
                _ => roleDisplay
            };
        }

        public static CCS_NpcServiceRepresentativeState BuildStateFromAssignment(
            CCS_NpcServiceRepresentativeAssignment assignment)
        {
            if (assignment == null)
            {
                return new CCS_NpcServiceRepresentativeState();
            }

            return new CCS_NpcServiceRepresentativeState
            {
                representativeId = assignment.representativeId,
                settlementId = assignment.settlementId,
                businessId = assignment.businessId,
                servicePointId = assignment.servicePointId,
                requiredRole = assignment.requiredRole,
                assignedNpcIdentityId = assignment.assignedNpcIdentityId,
                displayTitle = assignment.displayTitle,
                isActive = assignment.isActive,
                fallbackToServicePoint = assignment.fallbackToServicePoint
            };
        }

        public static CCS_NpcServiceRepresentativeSnapshot BuildSnapshotFromState(
            CCS_NpcServiceRepresentativeState state,
            string displayName)
        {
            if (state == null)
            {
                return CCS_NpcServiceRepresentativeSnapshot.Empty;
            }

            CCS_NpcRoleType role = state.ResolvedRequiredRole;
            return new CCS_NpcServiceRepresentativeSnapshot
            {
                RepresentativeId = state.representativeId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                BusinessId = state.businessId ?? string.Empty,
                ServicePointId = state.servicePointId ?? string.Empty,
                RequiredRole = role,
                AssignedNpcIdentityId = state.assignedNpcIdentityId ?? string.Empty,
                DisplayName = displayName ?? string.Empty,
                DisplayTitle = state.displayTitle ?? string.Empty,
                IsActive = state.isActive,
                FallbackToServicePoint = state.fallbackToServicePoint,
                RouteType = ResolveRouteType(role)
            };
        }

        public static CCS_NpcServiceRepresentativeState[] CloneStates(CCS_NpcServiceRepresentativeState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcServiceRepresentativeState>();
            }

            CCS_NpcServiceRepresentativeState[] clone = new CCS_NpcServiceRepresentativeState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcServiceRepresentativeState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcServiceRepresentativeState()
                    : new CCS_NpcServiceRepresentativeState
                    {
                        representativeId = entry.representativeId,
                        settlementId = entry.settlementId,
                        businessId = entry.businessId,
                        servicePointId = entry.servicePointId,
                        requiredRole = entry.requiredRole,
                        assignedNpcIdentityId = entry.assignedNpcIdentityId,
                        displayTitle = entry.displayTitle,
                        isActive = entry.isActive,
                        fallbackToServicePoint = entry.fallbackToServicePoint
                    };
            }

            return clone;
        }

        public static CCS_NpcServiceRepresentativeState TryFindState(
            CCS_NpcServiceRepresentativeState[] states,
            string representativeId)
        {
            if (states == null || string.IsNullOrWhiteSpace(representativeId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcServiceRepresentativeState state = states[index];
                if (state != null
                    && string.Equals(state.representativeId, representativeId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcServiceRepresentativeState[] UpsertState(
            CCS_NpcServiceRepresentativeState[] states,
            CCS_NpcServiceRepresentativeState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.representativeId))
            {
                return states ?? Array.Empty<CCS_NpcServiceRepresentativeState>();
            }

            List<CCS_NpcServiceRepresentativeState> working = new List<CCS_NpcServiceRepresentativeState>(
                states ?? Array.Empty<CCS_NpcServiceRepresentativeState>());
            bool replaced = false;
            for (int index = 0; index < working.Count; index++)
            {
                if (string.Equals(working[index].representativeId, state.representativeId, StringComparison.OrdinalIgnoreCase))
                {
                    working[index] = state;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                working.Add(state);
            }

            return working.ToArray();
        }

        public static bool TryFindHostByBusinessId(
            string settlementId,
            string businessId,
            out CCS_IPopulationPlaceholderIdentityHost host)
        {
            host = null;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(businessId))
            {
                return false;
            }

            if (!CCS_PopulationPlaceholderIdentityBridge.TryGetFirstHostByBusinessId(
                    settlementId,
                    businessId,
                    out host))
            {
                return false;
            }

            return host != null && host.HasIdentity;
        }
    }
}
