using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcSocialValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Profile validation, state helpers, and nearest social anchor resolution.
// PLACEMENT: Used by CCS_NpcSocialService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — leisure blocks target nearest registered social anchor.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcSocialValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcSocialProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC social profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC social profile requires profileId.");
            }

            CCS_NpcSocialGatheringDefinition[] definitions = profile.GatheringDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC social profile requires gathering definitions.");
            }

            HashSet<string> anchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcSocialGatheringDefinition definition = definitions[index];
                if (definition == null
                    || string.IsNullOrWhiteSpace(definition.SettlementId)
                    || string.IsNullOrWhiteSpace(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail("NPC social gathering definition requires settlement and anchor ids.");
                }

                if (!anchorIds.Add(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate NPC social gathering anchor id '{definition.AnchorId}'.");
                }
            }

            if (profile.SocialArrivalTolerance <= 0f || profile.SocialArrivalTolerance > 3f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"NPC social arrival tolerance must be positive and <= 3m (found {profile.SocialArrivalTolerance:F2}).");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC social profile validated ({profile.ProfileId}). {definitions.Length} gathering definitions.");
        }

        public static bool TryResolveNearestSocialAnchor(
            CCS_NpcSocialProfile profile,
            CCS_INpcMovementHost host,
            out Vector3 targetPosition,
            out string anchorId,
            out string anchorDisplayName)
        {
            targetPosition = Vector3.zero;
            anchorId = string.Empty;
            anchorDisplayName = string.Empty;
            if (host == null || string.IsNullOrWhiteSpace(host.SettlementId))
            {
                return false;
            }

            Vector3 origin = host.MovementTransform != null
                ? host.MovementTransform.position
                : Vector3.zero;
            float bestDistance = float.MaxValue;
            string bestAnchorId = string.Empty;
            string bestDisplayName = string.Empty;
            Vector3 bestPosition = Vector3.zero;
            bool found = false;

            CCS_NpcSocialGatheringDefinition[] definitions = profile?.GatheringDefinitions ?? Array.Empty<CCS_NpcSocialGatheringDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcSocialGatheringDefinition definition = definitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, host.SettlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!CCS_SettlementSocialRuntimeBridge.TryFindAnchor(
                        definition.AnchorId,
                        out CCS_SettlementSocialAnchor anchor)
                    || anchor == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, anchor.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestAnchorId = anchor.AnchorId;
                    bestDisplayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                        ? anchor.DisplayName
                        : definition.DisplayName;
                    bestPosition = anchor.transform.position;
                    found = true;
                }
            }

            if (!found
                && CCS_SettlementSocialRuntimeBridge.TryGetFirstAnchorForSettlement(
                    host.SettlementId,
                    out CCS_SettlementSocialAnchor fallbackAnchor)
                && fallbackAnchor != null)
            {
                bestAnchorId = fallbackAnchor.AnchorId;
                bestDisplayName = fallbackAnchor.DisplayName;
                bestPosition = fallbackAnchor.transform.position;
                found = true;
            }

            if (!found)
            {
                return false;
            }

            anchorId = bestAnchorId;
            anchorDisplayName = bestDisplayName;
            targetPosition = bestPosition;
            return true;
        }

        public static string BuildGroupId(string settlementId, string anchorId)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(anchorId))
            {
                return string.Empty;
            }

            return $"ccs.survival.social.group.{settlementId}.{anchorId}";
        }

        public static CCS_NpcSocialState TryFindState(CCS_NpcSocialState[] states, string npcIdentityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcSocialState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcSocialState[] UpsertState(CCS_NpcSocialState[] states, CCS_NpcSocialState updated)
        {
            if (updated == null)
            {
                return states ?? Array.Empty<CCS_NpcSocialState>();
            }

            CCS_NpcSocialState[] source = states ?? Array.Empty<CCS_NpcSocialState>();
            List<CCS_NpcSocialState> list = new List<CCS_NpcSocialState>(source.Length + 1);
            bool replaced = false;
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcSocialState state = source[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, updated.npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(CloneState(updated));
                    replaced = true;
                }
                else if (state != null)
                {
                    list.Add(CloneState(state));
                }
            }

            if (!replaced)
            {
                list.Add(CloneState(updated));
            }

            return list.ToArray();
        }

        public static CCS_NpcSocialState[] CloneStates(CCS_NpcSocialState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return Array.Empty<CCS_NpcSocialState>();
            }

            CCS_NpcSocialState[] clones = new CCS_NpcSocialState[states.Length];
            for (int index = 0; index < states.Length; index++)
            {
                clones[index] = CloneState(states[index]);
            }

            return clones;
        }

        public static CCS_NpcSocialSnapshot BuildSnapshotFromState(
            CCS_NpcSocialState state,
            int participantCount,
            string anchorDisplayName)
        {
            if (state == null)
            {
                return CCS_NpcSocialSnapshot.Empty;
            }

            return new CCS_NpcSocialSnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                GroupId = state.groupId ?? string.Empty,
                AnchorId = state.anchorId ?? string.Empty,
                AnchorDisplayName = anchorDisplayName ?? string.Empty,
                ParticipantCount = participantCount,
                LastEvaluatedHour = state.lastEvaluatedHour,
                IsSocializing = state.isSocializing
            };
        }

        public static CCS_NpcSocialGroupSnapshot[] RebuildGroups(CCS_NpcSocialState[] states)
        {
            if (states == null || states.Length == 0)
            {
                return Array.Empty<CCS_NpcSocialGroupSnapshot>();
            }

            Dictionary<string, CCS_NpcSocialGroupSnapshot> groups =
                new Dictionary<string, CCS_NpcSocialGroupSnapshot>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcSocialState state = states[index];
                if (state == null || !state.isSocializing || string.IsNullOrWhiteSpace(state.groupId))
                {
                    continue;
                }

                if (!groups.TryGetValue(state.groupId, out CCS_NpcSocialGroupSnapshot group)
                    || group == null)
                {
                    group = new CCS_NpcSocialGroupSnapshot
                    {
                        GroupId = state.groupId,
                        SettlementId = state.settlementId ?? string.Empty,
                        AnchorId = state.anchorId ?? string.Empty,
                        ParticipantCount = 0
                    };
                    groups[state.groupId] = group;
                }

                group.ParticipantCount++;
            }

            CCS_NpcSocialGroupSnapshot[] results = new CCS_NpcSocialGroupSnapshot[groups.Count];
            groups.Values.CopyTo(results, 0);
            return results;
        }

        public static int CountParticipants(CCS_NpcSocialState[] states, string groupId)
        {
            if (states == null || string.IsNullOrWhiteSpace(groupId))
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcSocialState state = states[index];
                if (state != null
                    && state.isSocializing
                    && string.Equals(state.groupId, groupId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static CCS_NpcSocialState CloneState(CCS_NpcSocialState state)
        {
            if (state == null)
            {
                return new CCS_NpcSocialState();
            }

            return new CCS_NpcSocialState
            {
                npcIdentityId = state.npcIdentityId ?? string.Empty,
                settlementId = state.settlementId ?? string.Empty,
                groupId = state.groupId ?? string.Empty,
                anchorId = state.anchorId ?? string.Empty,
                lastEvaluatedHour = state.lastEvaluatedHour,
                isSocializing = state.isSocializing
            };
        }
    }
}
