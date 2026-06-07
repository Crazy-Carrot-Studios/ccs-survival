using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleService
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Profile-driven daily schedule evaluation for placeholder NPCs.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — no AI, dialogue, quests, or pathfinding.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcScheduleService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcScheduleService]";

        private CCS_NpcScheduleProfile activeProfile;
        private Func<string, CCS_NpcScheduleState[]> getScheduleStates;
        private Action<string, CCS_NpcScheduleState[]> setScheduleStates;
        private Func<string, CCS_NpcIdentityState[]> getIdentityStates;
        private Func<int> resolveCurrentHour;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcScheduleProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcScheduleProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcScheduleValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindScheduleStateAccessors(
            Func<string, CCS_NpcScheduleState[]> getter,
            Action<string, CCS_NpcScheduleState[]> setter)
        {
            getScheduleStates = getter;
            setScheduleStates = setter;
            BindRuntimeBridge();
        }

        public void BindIdentityStateAccessors(Func<string, CCS_NpcIdentityState[]> getter)
        {
            getIdentityStates = getter;
        }

        public void BindScheduleHourResolver(Func<int> resolver)
        {
            resolveCurrentHour = resolver;
        }

        public bool TryGetScheduleSnapshot(string settlementId, string npcIdentityId, out CCS_NpcScheduleSnapshot snapshot)
        {
            snapshot = CCS_NpcScheduleSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            CCS_NpcScheduleState state = CCS_NpcScheduleValidationUtility.TryFindState(
                getScheduleStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcScheduleState>(),
                npcIdentityId);
            if (state == null)
            {
                return false;
            }

            snapshot = CCS_NpcScheduleValidationUtility.BuildSnapshotFromState(state);
            return snapshot.IsValid;
        }

        public bool TryEvaluateForHost(
            CCS_INpcMovementHost host,
            int currentHour,
            out CCS_NpcScheduleBlockType blockType,
            out CCS_NpcScheduleTargetKind targetKind,
            out string targetId)
        {
            blockType = CCS_NpcScheduleBlockType.Unknown;
            targetKind = CCS_NpcScheduleTargetKind.Unknown;
            targetId = string.Empty;
            if (!isInitialized || activeProfile == null || host == null || !host.HasIdentity || setScheduleStates == null)
            {
                return false;
            }

            CCS_NpcIdentityState identityState = ResolveIdentityState(host);
            string scheduleId = CCS_NpcScheduleValidationUtility.ResolveScheduleIdForHost(
                activeProfile,
                host,
                identityState);
            if (string.IsNullOrWhiteSpace(scheduleId))
            {
                return false;
            }

            if (!activeProfile.TryGetDefinition(scheduleId, out CCS_NpcScheduleDefinition definition))
            {
                return false;
            }

            if (!CCS_NpcScheduleValidationUtility.TryResolveBlockTypeAtHour(
                    definition,
                    activeProfile.GapFallbackBlockType,
                    currentHour,
                    out blockType))
            {
                return false;
            }

            targetKind = CCS_NpcScheduleValidationUtility.ResolveTargetKind(blockType);
            PersistEvaluation(host, scheduleId, blockType, targetKind, targetId, currentHour, identityState);
            CCS_NpcScheduleState persisted = CCS_NpcScheduleValidationUtility.TryFindState(
                getScheduleStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcScheduleState>(),
                host.NpcIdentityId);
            targetId = persisted?.currentTargetId ?? string.Empty;
            return true;
        }

        public void ForceEvaluateBlockForHost(
            CCS_INpcMovementHost host,
            CCS_NpcScheduleBlockType blockType,
            int hour)
        {
            if (!isInitialized || activeProfile == null || host == null || !host.HasIdentity || setScheduleStates == null)
            {
                return;
            }

            CCS_NpcIdentityState identityState = ResolveIdentityState(host);
            string scheduleId = CCS_NpcScheduleValidationUtility.ResolveScheduleIdForHost(
                activeProfile,
                host,
                identityState);
            if (string.IsNullOrWhiteSpace(scheduleId))
            {
                return;
            }

            CCS_NpcScheduleTargetKind targetKind = CCS_NpcScheduleValidationUtility.ResolveTargetKind(blockType);
            string homeHousingId = identityState?.homeHousingId ?? host.HomeHousingId ?? string.Empty;
            CCS_NpcScheduleValidationUtility.TryResolveTargetForBlock(
                host,
                homeHousingId,
                blockType,
                out _,
                out string targetId,
                out targetKind);
            PersistEvaluation(host, scheduleId, blockType, targetKind, targetId, hour, identityState);
        }

        public void RefreshAllSchedules()
        {
            int currentHour = resolveCurrentHour?.Invoke() ?? 12;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null && host.HasIdentity)
                {
                    TryEvaluateForHost(host, currentHour, out _, out _, out _);
                }
            });
        }

        public void RefreshSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            int currentHour = resolveCurrentHour?.Invoke() ?? 12;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null
                    && host.HasIdentity
                    && string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    TryEvaluateForHost(host, currentHour, out _, out _, out _);
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
            CCS_NpcScheduleRuntimeBridge.ResolveScheduleSnapshot = (settlementId, npcIdentityId) =>
            {
                TryGetScheduleSnapshot(settlementId, npcIdentityId, out CCS_NpcScheduleSnapshot snapshot);
                return snapshot;
            };
            CCS_NpcScheduleRuntimeBridge.RefreshAllSchedules = RefreshAllSchedules;
            CCS_NpcScheduleRuntimeBridge.ForceEvaluateBlock = (settlementId, npcIdentityId, blockType, hour) =>
            {
                CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
                {
                    if (host != null
                        && host.HasIdentity
                        && string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(host.NpcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                    {
                        ForceEvaluateBlockForHost(host, blockType, hour);
                    }
                });
            };
            CCS_NpcScheduleLabelBridge.ResolveScheduleDebugLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetScheduleSnapshot(settlementId, npcIdentityId, out CCS_NpcScheduleSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return $"{snapshot.CurrentBlockType} | {snapshot.ActiveScheduleId} | {snapshot.CurrentTargetKind}";
            };
        }

        private void PersistEvaluation(
            CCS_INpcMovementHost host,
            string scheduleId,
            CCS_NpcScheduleBlockType blockType,
            CCS_NpcScheduleTargetKind targetKind,
            string targetId,
            int currentHour,
            CCS_NpcIdentityState identityState)
        {
            string homeHousingId = identityState?.homeHousingId ?? host.HomeHousingId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(targetId))
            {
                CCS_NpcScheduleValidationUtility.TryResolveTargetForBlock(
                    host,
                    homeHousingId,
                    blockType,
                    out _,
                    out targetId,
                    out targetKind);
            }

            CCS_NpcScheduleState[] states =
                getScheduleStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcScheduleState>();
            CCS_NpcScheduleState existing = CCS_NpcScheduleValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcScheduleState updated = existing ?? new CCS_NpcScheduleState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId
            };

            updated.activeScheduleId = scheduleId ?? string.Empty;
            updated.currentBlockType = (int)blockType;
            updated.currentTargetKind = (int)targetKind;
            updated.currentTargetId = targetId ?? string.Empty;
            updated.lastEvaluatedHour = currentHour;
            setScheduleStates.Invoke(
                host.SettlementId,
                CCS_NpcScheduleValidationUtility.UpsertState(states, updated));
        }

        private CCS_NpcIdentityState ResolveIdentityState(CCS_INpcMovementHost host)
        {
            if (getIdentityStates == null || host == null)
            {
                return null;
            }

            CCS_NpcIdentityState[] states =
                getIdentityStates.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcIdentityState>();
            return CCS_NpcIdentityValidationUtility.TryFindState(states, host.NpcIdentityId);
        }
    }
}
