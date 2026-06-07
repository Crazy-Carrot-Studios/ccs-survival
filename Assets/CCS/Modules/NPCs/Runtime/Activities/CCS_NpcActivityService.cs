using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcActivityService
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Derives visible NPC activities from schedule blocks and movement status.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — traveling overrides block activity while moving.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcActivityService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcActivityService]";

        private CCS_NpcActivityProfile activeProfile;
        private Func<string, CCS_NpcActivityState[]> getActivityStates;
        private Action<string, CCS_NpcActivityState[]> setActivityStates;
        private Func<int> resolveCurrentHour;
        private bool scheduleServiceAvailable;
        private bool movementServiceAvailable;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcActivityProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcActivityProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcActivityValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindActivityStateAccessors(
            Func<string, CCS_NpcActivityState[]> getter,
            Action<string, CCS_NpcActivityState[]> setter)
        {
            getActivityStates = getter;
            setActivityStates = setter;
            BindRuntimeBridge();
        }

        public void BindScheduleHourResolver(Func<int> resolver)
        {
            resolveCurrentHour = resolver;
        }

        public void BindScheduleServiceAvailability(bool isAvailable)
        {
            scheduleServiceAvailable = isAvailable;
        }

        public void BindMovementServiceAvailability(bool isAvailable)
        {
            movementServiceAvailable = isAvailable;
        }

        public bool TryGetActivitySnapshot(string settlementId, string npcIdentityId, out CCS_NpcActivitySnapshot snapshot)
        {
            snapshot = CCS_NpcActivitySnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            CCS_NpcActivityState state = CCS_NpcActivityValidationUtility.TryFindState(
                getActivityStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcActivityState>(),
                npcIdentityId);
            if (state == null)
            {
                return false;
            }

            snapshot = CCS_NpcActivityValidationUtility.BuildSnapshotFromState(state);
            EnrichSnapshotContext(settlementId, npcIdentityId, snapshot);
            return snapshot.IsValid;
        }

        public void EvaluateForHost(CCS_INpcMovementHost host)
        {
            if (!isInitialized || activeProfile == null || host == null || !host.HasIdentity || setActivityStates == null)
            {
                return;
            }

            int currentHour = resolveCurrentHour?.Invoke() ?? 12;
            CCS_NpcScheduleSnapshot scheduleSnapshot = CCS_NpcScheduleSnapshot.Empty;
            CCS_NpcMovementSnapshot movementSnapshot = CCS_NpcMovementSnapshot.Empty;
            bool scheduleAvailable = scheduleServiceAvailable
                && CCS_NpcScheduleRuntimeBridge.TryGetScheduleSnapshot(
                    host.SettlementId,
                    host.NpcIdentityId,
                    out scheduleSnapshot)
                && scheduleSnapshot.IsValid;
            bool movementAvailable = movementServiceAvailable
                && CCS_NpcMovementRuntimeBridge.TryGetMovementSnapshot(
                    host.SettlementId,
                    host.NpcIdentityId,
                    out movementSnapshot)
                && movementSnapshot.IsValid;

            CCS_NpcScheduleBlockType blockType = scheduleAvailable
                ? scheduleSnapshot.CurrentBlockType
                : CCS_NpcScheduleBlockType.Unknown;
            CCS_NpcMovementStatus movementStatus = movementAvailable
                ? movementSnapshot.Status
                : CCS_NpcMovementStatus.Unknown;

            CCS_NpcActivityType activityType = CCS_NpcActivityValidationUtility.ResolveActivity(
                activeProfile,
                blockType,
                movementStatus,
                scheduleAvailable,
                movementAvailable);

            PersistActivity(host, activityType, currentHour);
            RefreshHostPresentation(host);
        }

        public void ForceEvaluateActivityForHost(CCS_INpcMovementHost host, CCS_NpcActivityType activityType, int hour)
        {
            if (!isInitialized || host == null || !host.HasIdentity || setActivityStates == null)
            {
                return;
            }

            PersistActivity(host, activityType, hour);
            RefreshHostPresentation(host);
        }

        public void RefreshAllActivities()
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
            CCS_NpcActivityRuntimeBridge.ResolveActivitySnapshot = (settlementId, npcIdentityId) =>
            {
                TryGetActivitySnapshot(settlementId, npcIdentityId, out CCS_NpcActivitySnapshot snapshot);
                return snapshot;
            };
            CCS_NpcActivityRuntimeBridge.RefreshAllActivities = RefreshAllActivities;
            CCS_NpcActivityRuntimeBridge.ForceEvaluateActivity = (settlementId, npcIdentityId, activityType, hour) =>
            {
                CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
                {
                    if (host != null
                        && host.HasIdentity
                        && string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(host.NpcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                    {
                        ForceEvaluateActivityForHost(host, activityType, hour);
                    }
                });
            };

            CCS_NpcActivityLabelBridge.ResolveActivityDisplayLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetActivitySnapshot(settlementId, npcIdentityId, out CCS_NpcActivitySnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return snapshot.CurrentActivityType.ToString();
            };

            CCS_NpcActivityLabelBridge.ResolveActivityDebugLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetActivitySnapshot(settlementId, npcIdentityId, out CCS_NpcActivitySnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return $"{snapshot.CurrentActivityType} | {snapshot.ScheduleBlockType} | {snapshot.MovementStatus}";
            };
        }

        private void PersistActivity(CCS_INpcMovementHost host, CCS_NpcActivityType activityType, int currentHour)
        {
            CCS_NpcActivityState[] states =
                getActivityStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcActivityState>();
            CCS_NpcActivityState existing = CCS_NpcActivityValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcActivityState updated = existing ?? new CCS_NpcActivityState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId
            };

            updated.currentActivityType = (int)activityType;
            updated.lastEvaluatedHour = currentHour;
            setActivityStates.Invoke(
                host.SettlementId,
                CCS_NpcActivityValidationUtility.UpsertState(states, updated));
        }

        private static void EnrichSnapshotContext(
            string settlementId,
            string npcIdentityId,
            CCS_NpcActivitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            if (CCS_NpcScheduleRuntimeBridge.TryGetScheduleSnapshot(settlementId, npcIdentityId, out CCS_NpcScheduleSnapshot scheduleSnapshot)
                && scheduleSnapshot != null)
            {
                snapshot.ScheduleBlockType = scheduleSnapshot.CurrentBlockType;
            }

            if (CCS_NpcMovementRuntimeBridge.TryGetMovementSnapshot(settlementId, npcIdentityId, out CCS_NpcMovementSnapshot movementSnapshot)
                && movementSnapshot != null)
            {
                snapshot.MovementStatus = movementSnapshot.Status;
            }
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
