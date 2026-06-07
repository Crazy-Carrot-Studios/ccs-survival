using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcSocialService
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Evaluates leisure-period social gathering state and temporary group membership.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — no AI conversations or relationship simulation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcSocialService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcSocialService]";

        private CCS_NpcSocialProfile activeProfile;
        private Func<string, CCS_NpcSocialState[]> getSocialStates;
        private Action<string, CCS_NpcSocialState[]> setSocialStates;
        private Func<int> resolveCurrentHour;
        private bool scheduleServiceAvailable;
        private bool movementServiceAvailable;
        private bool isInitialized;

        private readonly Dictionary<string, CCS_NpcSocialGroupSnapshot[]> groupCache =
            new Dictionary<string, CCS_NpcSocialGroupSnapshot[]>(StringComparer.OrdinalIgnoreCase);

        public bool IsInitialized => isInitialized;

        public CCS_NpcSocialProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcSocialProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcSocialValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindSocialStateAccessors(
            Func<string, CCS_NpcSocialState[]> getter,
            Action<string, CCS_NpcSocialState[]> setter)
        {
            getSocialStates = getter;
            setSocialStates = setter;
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

        public bool TryGetSocialSnapshot(string settlementId, string npcIdentityId, out CCS_NpcSocialSnapshot snapshot)
        {
            snapshot = CCS_NpcSocialSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            CCS_NpcSocialState state = CCS_NpcSocialValidationUtility.TryFindState(
                getSocialStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcSocialState>(),
                npcIdentityId);
            if (state == null)
            {
                return false;
            }

            int participantCount = CCS_NpcSocialValidationUtility.CountParticipants(
                getSocialStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcSocialState>(),
                state.groupId);
            string anchorDisplayName = ResolveAnchorDisplayName(state.anchorId);
            snapshot = CCS_NpcSocialValidationUtility.BuildSnapshotFromState(state, participantCount, anchorDisplayName);
            return snapshot.IsValid;
        }

        public CCS_NpcSocialGroupSnapshot[] GetGroupsForSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return Array.Empty<CCS_NpcSocialGroupSnapshot>();
            }

            if (groupCache.TryGetValue(settlementId, out CCS_NpcSocialGroupSnapshot[] cached)
                && cached != null)
            {
                return cached;
            }

            return Array.Empty<CCS_NpcSocialGroupSnapshot>();
        }

        public void EvaluateForHost(CCS_INpcMovementHost host)
        {
            if (!isInitialized || activeProfile == null || host == null || !host.HasIdentity || setSocialStates == null)
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

            bool isLeisureBlock = scheduleAvailable
                && scheduleSnapshot.CurrentBlockType == CCS_NpcScheduleBlockType.Leisure;
            PersistSocialState(host, isLeisureBlock, currentHour);
            RebuildGroupsForSettlement(host.SettlementId);
            RefreshHostPresentation(host);
        }

        public void RefreshAllSocialHosts()
        {
            HashSet<string> settlements = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(host =>
            {
                if (host != null && host.HasIdentity)
                {
                    EvaluateForHost(host);
                    if (!string.IsNullOrWhiteSpace(host.SettlementId))
                    {
                        settlements.Add(host.SettlementId);
                    }
                }
            });

            foreach (string settlementId in settlements)
            {
                RebuildGroupsForSettlement(settlementId);
            }
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

            RebuildGroupsForSettlement(settlementId);
        }

        public void RebuildGroupsAfterLoad(string settlementId)
        {
            RebuildGroupsForSettlement(settlementId);
            RefreshSettlement(settlementId);
        }

        private void PersistSocialState(
            CCS_INpcMovementHost host,
            bool isLeisureBlock,
            int currentHour)
        {
            CCS_NpcSocialState[] states =
                getSocialStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcSocialState>();
            CCS_NpcSocialState existing = CCS_NpcSocialValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcSocialState updated = existing ?? new CCS_NpcSocialState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId
            };

            if (!isLeisureBlock)
            {
                updated.groupId = string.Empty;
                updated.anchorId = string.Empty;
                updated.isSocializing = false;
                updated.lastEvaluatedHour = currentHour;
                setSocialStates.Invoke(
                    host.SettlementId,
                    CCS_NpcSocialValidationUtility.UpsertState(states, updated));
                return;
            }

            if (!CCS_NpcSocialValidationUtility.TryResolveNearestSocialAnchor(
                    activeProfile,
                    host,
                    out Vector3 anchorPosition,
                    out string anchorId,
                    out _))
            {
                updated.groupId = string.Empty;
                updated.anchorId = string.Empty;
                updated.isSocializing = false;
                updated.lastEvaluatedHour = currentHour;
                setSocialStates.Invoke(
                    host.SettlementId,
                    CCS_NpcSocialValidationUtility.UpsertState(states, updated));
                return;
            }

            updated.anchorId = anchorId;
            updated.groupId = CCS_NpcSocialValidationUtility.BuildGroupId(host.SettlementId, anchorId);
            updated.lastEvaluatedHour = currentHour;
            updated.isSocializing = !string.IsNullOrWhiteSpace(anchorId);

            setSocialStates.Invoke(
                host.SettlementId,
                CCS_NpcSocialValidationUtility.UpsertState(states, updated));
        }

        private void RebuildGroupsForSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            CCS_NpcSocialState[] states =
                getSocialStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcSocialState>();
            groupCache[settlementId] = CCS_NpcSocialValidationUtility.RebuildGroups(states);
        }

        private string ResolveAnchorDisplayName(string anchorId)
        {
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return string.Empty;
            }

            if (CCS_SettlementSocialRuntimeBridge.TryFindAnchor(anchorId, out CCS_SettlementSocialAnchor anchor)
                && anchor != null)
            {
                return anchor.DisplayName;
            }

            CCS_NpcSocialGatheringDefinition[] definitions = activeProfile?.GatheringDefinitions
                ?? Array.Empty<CCS_NpcSocialGatheringDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcSocialGatheringDefinition definition = definitions[index];
                if (definition != null
                    && string.Equals(definition.AnchorId, anchorId, StringComparison.OrdinalIgnoreCase))
                {
                    return definition.DisplayName;
                }
            }

            return anchorId;
        }

        private void BindRuntimeBridge()
        {
            CCS_NpcSocialRuntimeBridge.ResolveSocialSnapshot = (settlementId, npcIdentityId) =>
            {
                TryGetSocialSnapshot(settlementId, npcIdentityId, out CCS_NpcSocialSnapshot snapshot);
                return snapshot ?? CCS_NpcSocialSnapshot.Empty;
            };
            CCS_NpcSocialRuntimeBridge.ResolveGroupsForSettlement = GetGroupsForSettlement;
            CCS_NpcSocialRuntimeBridge.RefreshAllSocialHosts = RefreshAllSocialHosts;
            CCS_NpcSocialRuntimeBridge.TryResolveNearestSocialAnchorForHost = TryResolveNearestSocialAnchorForBridge;

            CCS_NpcSocialLabelBridge.ResolveSocialDisplayLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetSocialSnapshot(settlementId, npcIdentityId, out CCS_NpcSocialSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsSocializing)
                {
                    return string.Empty;
                }

                return "Socializing";
            };

            CCS_NpcSocialLabelBridge.ResolveSocialDebugLine = (settlementId, npcIdentityId) =>
            {
                if (!TryGetSocialSnapshot(settlementId, npcIdentityId, out CCS_NpcSocialSnapshot snapshot)
                    || snapshot == null
                    || !snapshot.IsValid)
                {
                    return string.Empty;
                }

                return snapshot.IsSocializing
                    ? $"Social: {snapshot.AnchorDisplayName} | Group {snapshot.ParticipantCount}"
                    : "Social: idle";
            };
        }

        private bool TryResolveNearestSocialAnchorForBridge(
            CCS_INpcMovementHost host,
            out Vector3 targetPosition,
            out string anchorId,
            out string anchorDisplayName)
        {
            targetPosition = Vector3.zero;
            anchorId = string.Empty;
            anchorDisplayName = string.Empty;
            if (host == null || activeProfile == null)
            {
                return false;
            }

            return CCS_NpcSocialValidationUtility.TryResolveNearestSocialAnchor(
                activeProfile,
                host,
                out targetPosition,
                out anchorId,
                out anchorDisplayName);
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
