using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcIdentityValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Validation
// PURPOSE: Profile validation, identity id building, role/name resolution helpers.
// PLACEMENT: Used by CCS_NpcIdentityService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — workforce roles active; Doctor/Sheriff placeholders only.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcIdentityValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcIdentityProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC identity profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.DefaultFirstNamePool.Length == 0 || profile.DefaultLastNamePool.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC identity profile missing default name pools.");
            }

            CCS_NpcIdentityDefinition[] definitions = profile.SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcIdentityDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    return CCS_SurvivalValidationResult.Fail("NPC identity settlement definition missing settlement id.");
                }

                if (definition.FirstNamePool.Length == 0 || definition.LastNamePool.Length == 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"NPC identity settlement '{definition.SettlementId}' has empty name pools.");
                }
            }

            CCS_NpcRoleAssignment[] assignments = profile.RoleAssignments;
            if (assignments.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC identity profile has no role assignments.");
            }

            for (int index = 0; index < assignments.Length; index++)
            {
                CCS_NpcRoleAssignment assignment = assignments[index];
                if (assignment == null || assignment.RoleType == CCS_NpcRoleType.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail("NPC role assignment missing role type.");
                }

                if (assignment.WorkforceCategory == CCS_SettlementPopulationCategory.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail("NPC role assignment missing workforce category.");
                }

                if (IsPlaceholderOnlyRole(assignment.RoleType))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Placeholder-only role '{assignment.RoleType}' cannot be workforce-assigned.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("NPC identity profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePersistedStates(CCS_NpcIdentityState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return CCS_SurvivalValidationResult.Pass("No persisted NPC identities.");
            }

            HashSet<string> identityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcIdentityState state = states[index];
                if (state == null || string.IsNullOrWhiteSpace(state.npcIdentityId))
                {
                    return CCS_SurvivalValidationResult.Fail("Persisted NPC identity missing id.");
                }

                if (!identityIds.Add(state.npcIdentityId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate persisted NPC identity id '{state.npcIdentityId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Persisted NPC identity states validated.");
        }

        public static bool IsPlaceholderOnlyRole(CCS_NpcRoleType roleType) =>
            roleType == CCS_NpcRoleType.DoctorPlaceholder || roleType == CCS_NpcRoleType.SheriffPlaceholder;

        public static string BuildIdentityId(string anchorId, int slotIndex)
        {
            string safeAnchor = string.IsNullOrWhiteSpace(anchorId) ? "unknown" : anchorId.Trim();
            return $"{CCS_NpcIdentityContentIds.IdentityIdPrefix}.{safeAnchor}.{slotIndex}";
        }

        public static CCS_NpcRoleType ResolveRole(
            CCS_NpcIdentityProfile profile,
            string settlementId,
            CCS_SettlementPopulationCategory workforceCategory,
            string businessId)
        {
            if (profile == null)
            {
                return CCS_NpcRoleType.Unknown;
            }

            CCS_NpcRoleAssignment[] assignments = profile.RoleAssignments;
            CCS_NpcRoleType businessMatch = CCS_NpcRoleType.Unknown;
            CCS_NpcRoleType workforceMatch = CCS_NpcRoleType.Unknown;

            for (int index = 0; index < assignments.Length; index++)
            {
                CCS_NpcRoleAssignment assignment = assignments[index];
                if (assignment == null || IsPlaceholderOnlyRole(assignment.RoleType))
                {
                    continue;
                }

                if (!assignment.Matches(settlementId, workforceCategory, businessId))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(assignment.BusinessId))
                {
                    businessMatch = assignment.RoleType;
                    break;
                }

                workforceMatch = assignment.RoleType;
            }

            if (businessMatch != CCS_NpcRoleType.Unknown)
            {
                return businessMatch;
            }

            if (workforceMatch != CCS_NpcRoleType.Unknown)
            {
                return workforceMatch;
            }

            return workforceCategory switch
            {
                CCS_SettlementPopulationCategory.Merchants => CCS_NpcRoleType.Merchant,
                CCS_SettlementPopulationCategory.Farmers => CCS_NpcRoleType.Farmer,
                CCS_SettlementPopulationCategory.Ranchers => CCS_NpcRoleType.Rancher,
                CCS_SettlementPopulationCategory.Miners => CCS_NpcRoleType.Miner,
                CCS_SettlementPopulationCategory.LumberWorkers => CCS_NpcRoleType.LumberWorker,
                CCS_SettlementPopulationCategory.Laborers => CCS_NpcRoleType.Laborer,
                _ => CCS_NpcRoleType.Unknown
            };
        }

        public static bool RoleMatchesWorkforce(CCS_NpcRoleType role, CCS_SettlementPopulationCategory workforceCategory)
        {
            if (role == CCS_NpcRoleType.Unknown || workforceCategory == CCS_SettlementPopulationCategory.Unknown)
            {
                return false;
            }

            return role switch
            {
                CCS_NpcRoleType.Merchant => workforceCategory == CCS_SettlementPopulationCategory.Merchants,
                CCS_NpcRoleType.Farmer => workforceCategory == CCS_SettlementPopulationCategory.Farmers,
                CCS_NpcRoleType.Rancher => workforceCategory == CCS_SettlementPopulationCategory.Ranchers,
                CCS_NpcRoleType.StableHand => workforceCategory == CCS_SettlementPopulationCategory.Ranchers,
                CCS_NpcRoleType.Miner => workforceCategory == CCS_SettlementPopulationCategory.Miners,
                CCS_NpcRoleType.Gunsmith => workforceCategory == CCS_SettlementPopulationCategory.Miners
                    || workforceCategory == CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.LumberWorker => workforceCategory == CCS_SettlementPopulationCategory.LumberWorkers,
                CCS_NpcRoleType.Laborer => workforceCategory == CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.Clerk => workforceCategory == CCS_SettlementPopulationCategory.Merchants
                    || workforceCategory == CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.Banker => workforceCategory == CCS_SettlementPopulationCategory.Merchants
                    || workforceCategory == CCS_SettlementPopulationCategory.Laborers,
                CCS_NpcRoleType.Blacksmith => workforceCategory == CCS_SettlementPopulationCategory.Laborers,
                _ => false
            };
        }

        public static string ResolveDisplayName(
            CCS_NpcIdentityProfile profile,
            string settlementId,
            string identityId)
        {
            string[] firstNames = profile.DefaultFirstNamePool;
            string[] lastNames = profile.DefaultLastNamePool;
            if (profile.TryGetSettlementDefinition(settlementId, out CCS_NpcIdentityDefinition definition))
            {
                firstNames = definition.FirstNamePool;
                lastNames = definition.LastNamePool;
            }

            int hash = StableHash(identityId);
            string first = firstNames[Math.Abs(hash) % firstNames.Length];
            string last = lastNames[Math.Abs(hash / firstNames.Length) % lastNames.Length];
            return $"{first} {last}";
        }

        public static string ResolveRoleDisplayName(CCS_NpcIdentityProfile profile, CCS_NpcRoleType roleType)
        {
            if (profile != null)
            {
                CCS_NpcRoleDisplayEntry[] entries = profile.RoleDisplayNames;
                for (int index = 0; index < entries.Length; index++)
                {
                    CCS_NpcRoleDisplayEntry entry = entries[index];
                    if (entry != null && entry.roleType == roleType && !string.IsNullOrWhiteSpace(entry.displayName))
                    {
                        return entry.displayName;
                    }
                }
            }

            return roleType switch
            {
                CCS_NpcRoleType.Merchant => "Merchant",
                CCS_NpcRoleType.Banker => "Banker",
                CCS_NpcRoleType.StableHand => "Stable Hand",
                CCS_NpcRoleType.Gunsmith => "Gunsmith",
                CCS_NpcRoleType.Blacksmith => "Blacksmith",
                CCS_NpcRoleType.Farmer => "Farmer",
                CCS_NpcRoleType.Rancher => "Rancher",
                CCS_NpcRoleType.Miner => "Miner",
                CCS_NpcRoleType.LumberWorker => "Lumber Worker",
                CCS_NpcRoleType.Laborer => "Laborer",
                CCS_NpcRoleType.Clerk => "Clerk",
                CCS_NpcRoleType.DoctorPlaceholder => "Doctor (Placeholder)",
                CCS_NpcRoleType.SheriffPlaceholder => "Sheriff (Placeholder)",
                _ => "Unknown"
            };
        }

        public static CCS_NpcIdentitySnapshot BuildSnapshotFromState(CCS_NpcIdentityState state, CCS_NpcIdentityProfile profile)
        {
            if (state == null)
            {
                return CCS_NpcIdentitySnapshot.Empty;
            }

            CCS_NpcRoleType role = state.ResolvedRoleType;
            return new CCS_NpcIdentitySnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                DisplayName = state.displayName ?? string.Empty,
                Role = role,
                RoleDisplayName = ResolveRoleDisplayName(profile, role),
                SettlementId = state.settlementId ?? string.Empty,
                BusinessId = state.businessId ?? string.Empty,
                WorkforceCategory = state.ResolvedWorkforceCategory,
                HomeHousingId = state.homeHousingId ?? string.Empty
            };
        }

        public static CCS_NpcIdentityState BuildStateFromSnapshot(
            CCS_NpcIdentitySnapshot snapshot,
            string anchorId,
            int slotIndex)
        {
            return new CCS_NpcIdentityState
            {
                npcIdentityId = snapshot.NpcIdentityId,
                displayName = snapshot.DisplayName,
                roleType = (int)snapshot.Role,
                settlementId = snapshot.SettlementId,
                businessId = snapshot.BusinessId ?? string.Empty,
                workforceCategory = (int)snapshot.WorkforceCategory,
                anchorId = anchorId ?? string.Empty,
                slotIndex = slotIndex,
                homeHousingId = snapshot.HomeHousingId ?? string.Empty
            };
        }

        public static CCS_NpcIdentityState[] CloneStates(CCS_NpcIdentityState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcIdentityState>();
            }

            CCS_NpcIdentityState[] clone = new CCS_NpcIdentityState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcIdentityState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcIdentityState()
                    : new CCS_NpcIdentityState
                    {
                        npcIdentityId = entry.npcIdentityId,
                        displayName = entry.displayName,
                        roleType = entry.roleType,
                        settlementId = entry.settlementId,
                        businessId = entry.businessId,
                        workforceCategory = entry.workforceCategory,
                        anchorId = entry.anchorId,
                        slotIndex = entry.slotIndex,
                        homeHousingId = entry.homeHousingId
                    };
            }

            return clone;
        }

        public static CCS_NpcIdentityState TryFindState(CCS_NpcIdentityState[] states, string identityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(identityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcIdentityState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, identityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcIdentityState[] UpsertState(CCS_NpcIdentityState[] states, CCS_NpcIdentityState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.npcIdentityId))
            {
                return states ?? Array.Empty<CCS_NpcIdentityState>();
            }

            List<CCS_NpcIdentityState> working = new List<CCS_NpcIdentityState>(
                states ?? Array.Empty<CCS_NpcIdentityState>());
            bool replaced = false;
            for (int index = 0; index < working.Count; index++)
            {
                if (string.Equals(working[index].npcIdentityId, state.npcIdentityId, StringComparison.OrdinalIgnoreCase))
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

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int index = 0; index < value.Length; index++)
                {
                    hash = (hash * 31) + value[index];
                }

                return hash;
            }
        }
    }
}
