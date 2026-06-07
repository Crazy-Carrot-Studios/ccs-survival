using System;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Profile validation, affiliation resolution, and state helpers.
// PLACEMENT: Used by CCS_NpcAffiliationService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — loyalty is metadata only with no gameplay effects yet.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcAffiliationValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcAffiliationProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation profile requires profileId.");
            }

            if (profile.MinimumLoyaltyValue < 0 || profile.MaximumLoyaltyValue > 100)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "NPC affiliation profile loyalty range must stay within 0-100.");
            }

            if (profile.MinimumLoyaltyValue > profile.MaximumLoyaltyValue)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "NPC affiliation profile minimum loyalty exceeds maximum loyalty.");
            }

            if (profile.DefaultLoyaltyValue < profile.MinimumLoyaltyValue
                || profile.DefaultLoyaltyValue > profile.MaximumLoyaltyValue)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "NPC affiliation profile default loyalty is outside configured range.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC affiliation profile validated ({profile.ProfileId}). Default loyalty {profile.DefaultLoyaltyValue}.");
        }

        public static CCS_SurvivalValidationResult ValidatePersistedState(CCS_NpcAffiliationState state)
        {
            if (state == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation state is null.");
            }

            if (string.IsNullOrWhiteSpace(state.npcIdentityId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation state requires npcIdentityId.");
            }

            if (string.IsNullOrWhiteSpace(state.settlementId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation state requires settlementId.");
            }

            if (state.loyaltyValue < 0 || state.loyaltyValue > 100)
            {
                return CCS_SurvivalValidationResult.Fail("NPC affiliation loyalty must remain within 0-100.");
            }

            return CCS_SurvivalValidationResult.Pass("NPC affiliation state valid.");
        }

        public static CCS_NpcAffiliationState[] CloneStates(CCS_NpcAffiliationState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcAffiliationState>();
            }

            CCS_NpcAffiliationState[] clone = new CCS_NpcAffiliationState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcAffiliationState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcAffiliationState()
                    : new CCS_NpcAffiliationState
                    {
                        npcIdentityId = entry.npcIdentityId ?? string.Empty,
                        settlementId = entry.settlementId ?? string.Empty,
                        regionId = entry.regionId ?? string.Empty,
                        businessId = entry.businessId ?? string.Empty,
                        workforceCategory = entry.workforceCategory,
                        isServiceRepresentative = entry.isServiceRepresentative,
                        loyaltyValue = entry.loyaltyValue
                    };
            }

            return clone;
        }

        public static CCS_NpcAffiliationState TryFindState(
            CCS_NpcAffiliationState[] states,
            string npcIdentityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcAffiliationState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcAffiliationState[] UpsertState(
            CCS_NpcAffiliationState[] states,
            CCS_NpcAffiliationState updatedState)
        {
            if (updatedState == null || string.IsNullOrWhiteSpace(updatedState.npcIdentityId))
            {
                return states ?? Array.Empty<CCS_NpcAffiliationState>();
            }

            CCS_NpcAffiliationState[] working = states ?? Array.Empty<CCS_NpcAffiliationState>();
            for (int index = 0; index < working.Length; index++)
            {
                CCS_NpcAffiliationState existing = working[index];
                if (existing != null
                    && string.Equals(existing.npcIdentityId, updatedState.npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    working[index] = updatedState;
                    return working;
                }
            }

            CCS_NpcAffiliationState[] expanded = new CCS_NpcAffiliationState[working.Length + 1];
            Array.Copy(working, expanded, working.Length);
            expanded[working.Length] = updatedState;
            return expanded;
        }

        public static CCS_NpcAffiliationSnapshot BuildSnapshotFromState(
            CCS_NpcAffiliationState state,
            string displayName,
            string roleDisplayName,
            string settlementDisplayName,
            string businessDisplayName,
            string workforceDisplayName)
        {
            if (state == null)
            {
                return CCS_NpcAffiliationSnapshot.Empty;
            }

            return new CCS_NpcAffiliationSnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                DisplayName = displayName ?? string.Empty,
                RoleDisplayName = roleDisplayName ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                SettlementDisplayName = settlementDisplayName ?? string.Empty,
                RegionId = state.regionId ?? string.Empty,
                BusinessId = state.businessId ?? string.Empty,
                BusinessDisplayName = businessDisplayName ?? string.Empty,
                WorkforceCategory = state.workforceCategory,
                WorkforceDisplayName = workforceDisplayName ?? string.Empty,
                IsServiceRepresentative = state.isServiceRepresentative,
                LoyaltyValue = state.loyaltyValue
            };
        }

        public static string ResolveWorkforceDisplayName(int workforceCategoryValue)
        {
            if (!Enum.IsDefined(typeof(CCS_SettlementPopulationCategory), workforceCategoryValue))
            {
                return CCS_SettlementPopulationCategory.Unknown.ToString();
            }

            CCS_SettlementPopulationCategory category = (CCS_SettlementPopulationCategory)workforceCategoryValue;
            return category switch
            {
                CCS_SettlementPopulationCategory.Miners => "Miner",
                CCS_SettlementPopulationCategory.Merchants => "Merchant",
                CCS_SettlementPopulationCategory.Farmers => "Farmer",
                CCS_SettlementPopulationCategory.Ranchers => "Rancher",
                CCS_SettlementPopulationCategory.LumberWorkers => "Lumber Worker",
                CCS_SettlementPopulationCategory.Laborers => "Laborer",
                _ => category.ToString()
            };
        }

        public static string BuildAffiliationDebugLine(CCS_NpcAffiliationSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return string.Empty;
            }

            string secondary = snapshot.IsServiceRepresentative
                ? (string.IsNullOrWhiteSpace(snapshot.BusinessDisplayName)
                    ? snapshot.BusinessId
                    : snapshot.BusinessDisplayName)
                : (string.IsNullOrWhiteSpace(snapshot.WorkforceDisplayName)
                    ? ResolveWorkforceDisplayName(snapshot.WorkforceCategory)
                    : snapshot.WorkforceDisplayName);

            string settlement = string.IsNullOrWhiteSpace(snapshot.SettlementDisplayName)
                ? snapshot.SettlementId
                : snapshot.SettlementDisplayName;

            return $"Affiliation: {settlement} / {secondary}";
        }

        public static string BuildAffiliationDetailDebugLine(CCS_NpcAffiliationSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return string.Empty;
            }

            string business = string.IsNullOrWhiteSpace(snapshot.BusinessDisplayName)
                ? snapshot.BusinessId
                : snapshot.BusinessDisplayName;
            string workforce = string.IsNullOrWhiteSpace(snapshot.WorkforceDisplayName)
                ? ResolveWorkforceDisplayName(snapshot.WorkforceCategory)
                : snapshot.WorkforceDisplayName;
            string settlement = string.IsNullOrWhiteSpace(snapshot.SettlementDisplayName)
                ? snapshot.SettlementId
                : snapshot.SettlementDisplayName;

            return
                $"NPC: {snapshot.DisplayName} | {snapshot.RoleDisplayName} | {settlement} | {business} | {workforce} | Loyalty {snapshot.LoyaltyValue}";
        }
    }
}
