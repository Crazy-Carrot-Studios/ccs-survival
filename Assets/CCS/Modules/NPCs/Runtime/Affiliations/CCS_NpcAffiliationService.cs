using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationService
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Assigns and persists NPC settlement/business/workforce affiliations.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — metadata only; loyalty has no gameplay effects yet.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcAffiliationService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcAffiliationService]";

        private CCS_NpcAffiliationProfile activeProfile;
        private Func<string, CCS_NpcAffiliationState[]> getAffiliationStates;
        private Action<string, CCS_NpcAffiliationState[]> setAffiliationStates;
        private Func<string, string> resolveRegionId;
        private Func<string, string> resolveSettlementDisplayName;
        private Func<string, string> resolveBusinessDisplayName;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcAffiliationProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcAffiliationProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcAffiliationValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindAffiliationStateAccessors(
            Func<string, CCS_NpcAffiliationState[]> getter,
            Action<string, CCS_NpcAffiliationState[]> setter)
        {
            getAffiliationStates = getter;
            setAffiliationStates = setter;
            BindRuntimeBridge();
        }

        public void BindRegionResolver(Func<string, string> resolver)
        {
            resolveRegionId = resolver;
        }

        public void BindSettlementDisplayNameResolver(Func<string, string> resolver)
        {
            resolveSettlementDisplayName = resolver;
        }

        public void BindBusinessDisplayNameResolver(Func<string, string> resolver)
        {
            resolveBusinessDisplayName = resolver;
        }

        public bool TryGetAffiliationSnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcAffiliationSnapshot snapshot)
        {
            snapshot = CCS_NpcAffiliationSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            CCS_NpcAffiliationState state = CCS_NpcAffiliationValidationUtility.TryFindState(
                getAffiliationStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcAffiliationState>(),
                npcIdentityId);
            if (state == null)
            {
                return false;
            }

            (string displayName, string roleDisplayName) =
                ResolveHostPresentation(settlementId, npcIdentityId);
            snapshot = CCS_NpcAffiliationValidationUtility.BuildSnapshotFromState(
                state,
                displayName,
                roleDisplayName,
                ResolveSettlementDisplayName(state.settlementId),
                ResolveBusinessDisplayName(state.businessId),
                CCS_NpcAffiliationValidationUtility.ResolveWorkforceDisplayName(state.workforceCategory));
            return snapshot.IsValid;
        }

        public void EvaluateForHost(CCS_INpcMovementHost host)
        {
            if (!isInitialized || activeProfile == null || host == null || !host.HasIdentity || setAffiliationStates == null)
            {
                return;
            }

            CCS_NpcAffiliationState[] states =
                getAffiliationStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcAffiliationState>();
            CCS_NpcAffiliationState existing =
                CCS_NpcAffiliationValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcAffiliationState updated = existing ?? new CCS_NpcAffiliationState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId,
                loyaltyValue = activeProfile.DefaultLoyaltyValue
            };

            updated.settlementId = host.SettlementId ?? string.Empty;
            updated.regionId = resolveRegionId?.Invoke(host.SettlementId) ?? string.Empty;
            updated.businessId = host.BusinessId ?? string.Empty;
            updated.workforceCategory = host.WorkforceCategoryValue;
            updated.isServiceRepresentative = host.IsServiceRepresentative;
            updated.loyaltyValue = activeProfile.ClampLoyalty(
                existing?.loyaltyValue ?? activeProfile.DefaultLoyaltyValue);

            setAffiliationStates.Invoke(
                host.SettlementId,
                CCS_NpcAffiliationValidationUtility.UpsertState(states, updated));
            RefreshHostPresentation(host);
        }

        public void RefreshAllAffiliations()
        {
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null && host.HasIdentity)
                {
                    EvaluateForHost(host);
                }
            });
        }

        public void RefreshSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null
                    && host.HasIdentity
                    && string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    EvaluateForHost(host);
                }
            });
        }

        public void HandleSettlementDiscovered(CCS_SettlementSnapshot settlementSnapshot)
        {
            if (settlementSnapshot == null)
            {
                return;
            }

            RefreshSettlement(settlementSnapshot.SettlementId);
        }

        public void HandleSettlementPopulationChanged(CCS_SettlementPopulationChangedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            RefreshSettlement(eventArgs.Snapshot.SettlementId);
        }

        private void BindRuntimeBridge()
        {
            CCS_NpcAffiliationRuntimeBridge.ResolveAffiliationSnapshot = (settlementId, npcIdentityId) =>
            {
                TryGetAffiliationSnapshot(settlementId, npcIdentityId, out CCS_NpcAffiliationSnapshot snapshot);
                return snapshot;
            };
            CCS_NpcAffiliationRuntimeBridge.RefreshAllAffiliations = RefreshAllAffiliations;

            CCS_NpcAffiliationLabelBridge.ResolveSettlementDisplayLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetAffiliationSnapshot(settlementId, npcIdentityId, out CCS_NpcAffiliationSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return string.IsNullOrWhiteSpace(snapshot.SettlementDisplayName)
                    ? snapshot.SettlementId
                    : snapshot.SettlementDisplayName;
            };

            CCS_NpcAffiliationLabelBridge.ResolveAffiliationDebugLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetAffiliationSnapshot(settlementId, npcIdentityId, out CCS_NpcAffiliationSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return CCS_NpcAffiliationValidationUtility.BuildAffiliationDebugLine(snapshot);
            };

            CCS_NpcAffiliationLabelBridge.ResolveAffiliationDetailDebugLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetAffiliationSnapshot(settlementId, npcIdentityId, out CCS_NpcAffiliationSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return CCS_NpcAffiliationValidationUtility.BuildAffiliationDetailDebugLine(snapshot);
            };
        }

        private static (string displayName, string roleDisplayName) ResolveHostPresentation(
            string settlementId,
            string npcIdentityId)
        {
            string displayName = string.Empty;
            string roleDisplayName = string.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (!string.IsNullOrWhiteSpace(displayName)
                    || host == null
                    || !host.HasIdentity
                    || !string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(host.NpcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (host is CCS_IPopulationPlaceholderIdentityHost identityHost)
                {
                    displayName = identityHost.DisplayName;
                    roleDisplayName = identityHost.IsServiceRepresentative
                        && !string.IsNullOrWhiteSpace(identityHost.RepresentativeTitle)
                        ? identityHost.RepresentativeTitle
                        : identityHost.RoleDisplayName;
                }
            });

            return (displayName, roleDisplayName);
        }

        private string ResolveSettlementDisplayName(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return string.Empty;
            }

            return resolveSettlementDisplayName?.Invoke(settlementId) ?? settlementId;
        }

        private string ResolveBusinessDisplayName(string businessId)
        {
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return string.Empty;
            }

            return resolveBusinessDisplayName?.Invoke(businessId) ?? businessId;
        }

        private static void RefreshHostPresentation(CCS_INpcMovementHost host)
        {
            if (host is CCS_INpcPresentationHost presentationHost)
            {
                presentationHost.RefreshPresentation();
            }
        }
    }
}
