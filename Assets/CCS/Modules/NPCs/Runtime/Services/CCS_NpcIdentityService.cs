using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcIdentityService
// CATEGORY: Modules / NPCs / Runtime / Services
// PURPOSE: Resolves and persists NPC names/roles for population placeholder actors.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — no AI, dialogue, schedules, or pathfinding.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcIdentityService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcIdentityService]";

        private CCS_NpcIdentityProfile activeProfile;
        private Func<string, CCS_NpcIdentityState[]> getSettlementNpcStates;
        private Action<string, CCS_NpcIdentityState[]> setSettlementNpcStates;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcIdentityProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcIdentityProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcIdentityValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindSettlementNpcStateAccessors(
            Func<string, CCS_NpcIdentityState[]> getter,
            Action<string, CCS_NpcIdentityState[]> setter)
        {
            getSettlementNpcStates = getter;
            setSettlementNpcStates = setter;
            CCS_NpcRuntimeBridge.BindIdentityResolver(this);
        }

        public bool TryResolveIdentity(
            string anchorId,
            int slotIndex,
            string settlementId,
            CCS_SettlementPopulationCategory workforceCategory,
            string businessId,
            out CCS_NpcIdentitySnapshot snapshot)
        {
            snapshot = CCS_NpcIdentitySnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(anchorId) || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            string identityId = CCS_NpcIdentityValidationUtility.BuildIdentityId(anchorId, slotIndex);
            CCS_NpcIdentityState[] states = getSettlementNpcStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcIdentityState>();
            CCS_NpcIdentityState existing = CCS_NpcIdentityValidationUtility.TryFindState(states, identityId);
            if (existing != null)
            {
                snapshot = CCS_NpcIdentityValidationUtility.BuildSnapshotFromState(existing, activeProfile);
                return snapshot.IsValid;
            }

            CCS_NpcRoleType role = CCS_NpcIdentityValidationUtility.ResolveRole(
                activeProfile,
                settlementId,
                workforceCategory,
                businessId);
            if (role == CCS_NpcRoleType.Unknown)
            {
                return false;
            }

            string displayName = CCS_NpcIdentityValidationUtility.ResolveDisplayName(
                activeProfile,
                settlementId,
                identityId);
            snapshot = new CCS_NpcIdentitySnapshot
            {
                NpcIdentityId = identityId,
                DisplayName = displayName,
                Role = role,
                RoleDisplayName = CCS_NpcIdentityValidationUtility.ResolveRoleDisplayName(activeProfile, role),
                SettlementId = settlementId,
                BusinessId = businessId ?? string.Empty,
                WorkforceCategory = workforceCategory
            };

            CCS_NpcIdentityState persisted =
                CCS_NpcIdentityValidationUtility.BuildStateFromSnapshot(snapshot, anchorId, slotIndex);
            CCS_NpcIdentityState[] merged = CCS_NpcIdentityValidationUtility.UpsertState(states, persisted);
            setSettlementNpcStates?.Invoke(settlementId, merged);
            return snapshot.IsValid;
        }

        public void RefreshAllPlaceholderIdentities()
        {
            CCS_NpcRuntimeBridge.RefreshAllPlaceholderIdentities();
        }

        public void RefreshSettlement(string settlementId)
        {
            CCS_NpcRuntimeBridge.RefreshSettlementIdentities(settlementId);
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
    }
}
