using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Profile validation, block evaluation, target resolution, and state helpers.
// PLACEMENT: Used by CCS_NpcScheduleService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — movement integration via block-to-target mapping.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcScheduleValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcScheduleProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC schedule profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC schedule profile requires profileId.");
            }

            if (string.IsNullOrWhiteSpace(profile.FallbackScheduleId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC schedule profile requires fallbackScheduleId.");
            }

            if (!profile.TryGetDefinition(profile.FallbackScheduleId, out _))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"NPC schedule fallbackScheduleId '{profile.FallbackScheduleId}' is not defined.");
            }

            CCS_NpcScheduleDefinition[] definitions = profile.ScheduleDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC schedule profile requires at least one definition.");
            }

            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcScheduleDefinition definition = definitions[index];
                CCS_SurvivalValidationResult definitionResult = ValidateDefinition(definition, profile);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }
            }

            CCS_NpcScheduleRoleMapping[] mappings = profile.RoleMappings;
            for (int index = 0; index < mappings.Length; index++)
            {
                CCS_NpcScheduleRoleMapping mapping = mappings[index];
                if (mapping == null || string.IsNullOrWhiteSpace(mapping.ScheduleId))
                {
                    return CCS_SurvivalValidationResult.Fail("NPC schedule role mapping requires scheduleId.");
                }

                if (!profile.TryGetDefinition(mapping.ScheduleId, out _))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"NPC schedule role mapping references unknown schedule '{mapping.ScheduleId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC schedule profile validated ({profile.ProfileId}). {definitions.Length} definitions.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(
            CCS_NpcScheduleDefinition definition,
            CCS_NpcScheduleProfile profile)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ScheduleId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC schedule definition requires scheduleId.");
            }

            CCS_NpcScheduleBlock[] blocks = definition.Blocks;
            if (blocks.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"NPC schedule '{definition.ScheduleId}' requires at least one block.");
            }

            for (int left = 0; left < blocks.Length; left++)
            {
                CCS_NpcScheduleBlock leftBlock = blocks[left];
                if (leftBlock == null || leftBlock.BlockType == CCS_NpcScheduleBlockType.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"NPC schedule '{definition.ScheduleId}' contains an invalid block.");
                }

                if (leftBlock.StartHour == leftBlock.EndHour)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"NPC schedule '{definition.ScheduleId}' block start/end hours must differ.");
                }

                for (int right = left + 1; right < blocks.Length; right++)
                {
                    CCS_NpcScheduleBlock rightBlock = blocks[right];
                    if (rightBlock != null && BlocksOverlap(leftBlock, rightBlock))
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            $"NPC schedule '{definition.ScheduleId}' blocks overlap ({leftBlock.BlockType}/{rightBlock.BlockType}).");
                    }
                }
            }

            if (!HasValidCoverage(blocks, profile.GapFallbackBlockType))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"NPC schedule '{definition.ScheduleId}' does not cover 24h and gap fallback is invalid.");
            }

            return CCS_SurvivalValidationResult.Pass($"NPC schedule definition validated ({definition.ScheduleId}).");
        }

        public static string ResolveScheduleIdForHost(
            CCS_NpcScheduleProfile profile,
            CCS_INpcMovementHost host,
            CCS_NpcIdentityState identityState)
        {
            if (profile == null || host == null)
            {
                return string.Empty;
            }

            if (host.IsServiceRepresentative)
            {
                string representativeSchedule = ResolveMappedScheduleId(
                    profile,
                    identityState?.ResolvedRoleType ?? CCS_NpcRoleType.Unknown,
                    host.BusinessId,
                    (CCS_SettlementPopulationCategory)host.WorkforceCategoryValue,
                    true);
                if (!string.IsNullOrWhiteSpace(representativeSchedule))
                {
                    return representativeSchedule;
                }
            }

            CCS_NpcRoleType roleType = identityState?.ResolvedRoleType ?? CCS_NpcRoleType.Unknown;
            CCS_SettlementPopulationCategory category =
                identityState?.ResolvedWorkforceCategory
                ?? (CCS_SettlementPopulationCategory)host.WorkforceCategoryValue;

            string mappedSchedule = ResolveMappedScheduleId(
                profile,
                roleType,
                host.BusinessId,
                category,
                host.IsServiceRepresentative);
            if (!string.IsNullOrWhiteSpace(mappedSchedule))
            {
                return mappedSchedule;
            }

            return profile.FallbackScheduleId ?? string.Empty;
        }

        public static bool TryResolveBlockTypeAtHour(
            CCS_NpcScheduleDefinition definition,
            CCS_NpcScheduleBlockType gapFallbackBlockType,
            int currentHour,
            out CCS_NpcScheduleBlockType blockType)
        {
            blockType = CCS_NpcScheduleBlockType.Unknown;
            if (definition == null)
            {
                blockType = gapFallbackBlockType;
                return blockType != CCS_NpcScheduleBlockType.Unknown;
            }

            int hour = ClampHour(currentHour);
            CCS_NpcScheduleBlock[] blocks = definition.Blocks;
            for (int index = 0; index < blocks.Length; index++)
            {
                CCS_NpcScheduleBlock block = blocks[index];
                if (block != null && IsHourWithinBlock(hour, block))
                {
                    blockType = block.BlockType;
                    return true;
                }
            }

            blockType = gapFallbackBlockType;
            return blockType != CCS_NpcScheduleBlockType.Unknown;
        }

        public static CCS_NpcScheduleTargetKind ResolveTargetKind(CCS_NpcScheduleBlockType blockType)
        {
            switch (blockType)
            {
                case CCS_NpcScheduleBlockType.Sleep:
                case CCS_NpcScheduleBlockType.Home:
                    return CCS_NpcScheduleTargetKind.Housing;
                case CCS_NpcScheduleBlockType.Work:
                    return CCS_NpcScheduleTargetKind.Workplace;
                case CCS_NpcScheduleBlockType.Service:
                    return CCS_NpcScheduleTargetKind.ServicePoint;
                case CCS_NpcScheduleBlockType.Break:
                case CCS_NpcScheduleBlockType.Leisure:
                    return CCS_NpcScheduleTargetKind.SettlementCenter;
                case CCS_NpcScheduleBlockType.Idle:
                    return CCS_NpcScheduleTargetKind.CurrentAnchor;
                default:
                    return CCS_NpcScheduleTargetKind.Idle;
            }
        }

        public static bool TryResolveTargetForBlock(
            CCS_INpcMovementHost host,
            string homeHousingId,
            CCS_NpcScheduleBlockType blockType,
            out Vector3 targetPosition,
            out string targetId,
            out CCS_NpcScheduleTargetKind targetKind)
        {
            targetPosition = Vector3.zero;
            targetId = string.Empty;
            targetKind = ResolveTargetKind(blockType);
            if (host == null)
            {
                return false;
            }

            switch (targetKind)
            {
                case CCS_NpcScheduleTargetKind.Housing:
                    if (CCS_NpcMovementValidationUtility.TryResolveHomePosition(
                            host.SettlementId,
                            homeHousingId,
                            out targetPosition))
                    {
                        if (CCS_SettlementHousingRuntimeBridge.TryFindAnchorForHousing(
                                host.SettlementId,
                                homeHousingId,
                                out CCS_SettlementHousingAnchor housingAnchor)
                            && housingAnchor != null)
                        {
                            targetId = housingAnchor.AnchorId;
                        }

                        return true;
                    }

                    break;
                case CCS_NpcScheduleTargetKind.Workplace:
                    if (CCS_NpcMovementValidationUtility.TryResolveWorkplacePosition(host, out targetPosition))
                    {
                        targetId = host.WorkforceAnchorId ?? string.Empty;
                        return true;
                    }

                    break;
                case CCS_NpcScheduleTargetKind.ServicePoint:
                    if (TryResolveServicePointTarget(host, out targetPosition, out targetId))
                    {
                        return true;
                    }

                    if (CCS_NpcMovementValidationUtility.TryResolveWorkplacePosition(host, out targetPosition))
                    {
                        targetId = host.WorkforceAnchorId ?? string.Empty;
                        return true;
                    }

                    break;
                case CCS_NpcScheduleTargetKind.SettlementCenter:
                    if (TryResolveSettlementCenterTarget(host, out targetPosition, out targetId))
                    {
                        return true;
                    }

                    break;
                case CCS_NpcScheduleTargetKind.CurrentAnchor:
                case CCS_NpcScheduleTargetKind.Idle:
                    if (host.MovementTransform != null)
                    {
                        targetPosition = host.MovementTransform.position;
                        targetId = host.WorkforceAnchorId ?? string.Empty;
                        return true;
                    }

                    break;
            }

            if (host.MovementTransform != null)
            {
                targetPosition = host.MovementTransform.position;
                targetId = host.WorkforceAnchorId ?? string.Empty;
                targetKind = CCS_NpcScheduleTargetKind.Idle;
                return true;
            }

            return false;
        }

        public static bool IsWorkLikeBlock(CCS_NpcScheduleBlockType blockType)
        {
            return blockType == CCS_NpcScheduleBlockType.Work
                || blockType == CCS_NpcScheduleBlockType.Service;
        }

        public static bool IsHomeLikeBlock(CCS_NpcScheduleBlockType blockType)
        {
            return blockType == CCS_NpcScheduleBlockType.Home
                || blockType == CCS_NpcScheduleBlockType.Sleep;
        }

        public static CCS_NpcMovementStatus ResolveTravelStatus(CCS_NpcScheduleBlockType blockType)
        {
            if (IsWorkLikeBlock(blockType))
            {
                return CCS_NpcMovementStatus.TravelingToWork;
            }

            if (IsHomeLikeBlock(blockType))
            {
                return CCS_NpcMovementStatus.TravelingHome;
            }

            return CCS_NpcMovementStatus.Idle;
        }

        public static CCS_NpcMovementStatus ResolveArrivalStatus(CCS_NpcScheduleBlockType blockType)
        {
            if (IsWorkLikeBlock(blockType))
            {
                return CCS_NpcMovementStatus.Working;
            }

            if (IsHomeLikeBlock(blockType))
            {
                return CCS_NpcMovementStatus.AtHome;
            }

            return CCS_NpcMovementStatus.Idle;
        }

        public static bool UsesLegacyWorkHourFallback(CCS_NpcScheduleBlockType blockType)
        {
            return blockType == CCS_NpcScheduleBlockType.Unknown;
        }

        public static CCS_NpcScheduleState[] CloneStates(CCS_NpcScheduleState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcScheduleState>();
            }

            CCS_NpcScheduleState[] clone = new CCS_NpcScheduleState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcScheduleState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcScheduleState()
                    : new CCS_NpcScheduleState
                    {
                        npcIdentityId = entry.npcIdentityId ?? string.Empty,
                        settlementId = entry.settlementId ?? string.Empty,
                        activeScheduleId = entry.activeScheduleId ?? string.Empty,
                        currentBlockType = entry.currentBlockType,
                        currentTargetKind = entry.currentTargetKind,
                        currentTargetId = entry.currentTargetId ?? string.Empty,
                        lastEvaluatedHour = entry.lastEvaluatedHour
                    };
            }

            return clone;
        }

        public static CCS_NpcScheduleState TryFindState(CCS_NpcScheduleState[] states, string npcIdentityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcScheduleState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcScheduleState[] UpsertState(
            CCS_NpcScheduleState[] states,
            CCS_NpcScheduleState updatedState)
        {
            if (updatedState == null || string.IsNullOrWhiteSpace(updatedState.npcIdentityId))
            {
                return states ?? Array.Empty<CCS_NpcScheduleState>();
            }

            CCS_NpcScheduleState[] working = states ?? Array.Empty<CCS_NpcScheduleState>();
            for (int index = 0; index < working.Length; index++)
            {
                CCS_NpcScheduleState existing = working[index];
                if (existing != null
                    && string.Equals(existing.npcIdentityId, updatedState.npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    working[index] = updatedState;
                    return working;
                }
            }

            CCS_NpcScheduleState[] expanded = new CCS_NpcScheduleState[working.Length + 1];
            Array.Copy(working, expanded, working.Length);
            expanded[working.Length] = updatedState;
            return expanded;
        }

        public static CCS_NpcScheduleSnapshot BuildSnapshotFromState(CCS_NpcScheduleState state)
        {
            if (state == null)
            {
                return CCS_NpcScheduleSnapshot.Empty;
            }

            return new CCS_NpcScheduleSnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                ActiveScheduleId = state.activeScheduleId ?? string.Empty,
                CurrentBlockType = Enum.IsDefined(typeof(CCS_NpcScheduleBlockType), state.currentBlockType)
                    ? (CCS_NpcScheduleBlockType)state.currentBlockType
                    : CCS_NpcScheduleBlockType.Unknown,
                CurrentTargetKind = Enum.IsDefined(typeof(CCS_NpcScheduleTargetKind), state.currentTargetKind)
                    ? (CCS_NpcScheduleTargetKind)state.currentTargetKind
                    : CCS_NpcScheduleTargetKind.Unknown,
                CurrentTargetId = state.currentTargetId ?? string.Empty,
                LastEvaluatedHour = state.lastEvaluatedHour
            };
        }

        private static string ResolveMappedScheduleId(
            CCS_NpcScheduleProfile profile,
            CCS_NpcRoleType roleType,
            string businessId,
            CCS_SettlementPopulationCategory workforceCategory,
            bool isServiceRepresentative)
        {
            CCS_NpcScheduleRoleMapping[] mappings = profile.RoleMappings;
            for (int index = 0; index < mappings.Length; index++)
            {
                CCS_NpcScheduleRoleMapping mapping = mappings[index];
                if (mapping != null
                    && mapping.Matches(roleType, businessId, workforceCategory, isServiceRepresentative))
                {
                    return mapping.ScheduleId;
                }
            }

            return string.Empty;
        }

        private static bool TryResolveServicePointTarget(
            CCS_INpcMovementHost host,
            out Vector3 targetPosition,
            out string targetId)
        {
            targetPosition = Vector3.zero;
            targetId = string.Empty;
            if (host == null || string.IsNullOrWhiteSpace(host.SettlementId) || string.IsNullOrWhiteSpace(host.BusinessId))
            {
                return false;
            }

            if (!CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    host.SettlementId,
                    host.BusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot representativeSnapshot)
                || representativeSnapshot == null
                || !representativeSnapshot.IsValid)
            {
                return false;
            }

            if (!CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(
                    representativeSnapshot.ServicePointId,
                    out CCS_SettlementServicePoint servicePoint)
                || servicePoint == null)
            {
                return false;
            }

            targetPosition = servicePoint.transform.position;
            targetId = representativeSnapshot.ServicePointId ?? string.Empty;
            return true;
        }

        private static bool TryResolveSettlementCenterTarget(
            CCS_INpcMovementHost host,
            out Vector3 targetPosition,
            out string targetId)
        {
            targetPosition = Vector3.zero;
            targetId = string.Empty;
            if (host == null || string.IsNullOrWhiteSpace(host.SettlementId))
            {
                return false;
            }

            if (CCS_PopulationPresenceRuntimeBridge.TryGetFirstAnchorPositionForSettlement(
                    host.SettlementId,
                    out targetPosition,
                    out targetId))
            {
                return true;
            }

            if (CCS_PopulationPresenceRuntimeBridge.TryFindAnchor(
                    host.WorkforceAnchorId,
                    out CCS_PopulationPresenceAnchor workforceAnchor)
                && workforceAnchor != null)
            {
                targetPosition = workforceAnchor.transform.position;
                targetId = workforceAnchor.AnchorId;
                return true;
            }

            return false;
        }

        private static bool HasValidCoverage(CCS_NpcScheduleBlock[] blocks, CCS_NpcScheduleBlockType gapFallback)
        {
            bool[] covered = new bool[24];
            for (int index = 0; index < blocks.Length; index++)
            {
                CCS_NpcScheduleBlock block = blocks[index];
                if (block == null)
                {
                    continue;
                }

                MarkBlockCoverage(block, covered);
            }

            for (int hour = 0; hour < covered.Length; hour++)
            {
                if (!covered[hour] && gapFallback == CCS_NpcScheduleBlockType.Unknown)
                {
                    return false;
                }
            }

            return true;
        }

        private static void MarkBlockCoverage(CCS_NpcScheduleBlock block, bool[] covered)
        {
            int start = ClampHour(block.StartHour);
            int end = ClampHour(block.EndHour);
            if (start < end)
            {
                for (int hour = start; hour < end; hour++)
                {
                    covered[hour] = true;
                }

                return;
            }

            for (int hour = start; hour < 24; hour++)
            {
                covered[hour] = true;
            }

            for (int hour = 0; hour < end; hour++)
            {
                covered[hour] = true;
            }
        }

        private static bool BlocksOverlap(CCS_NpcScheduleBlock left, CCS_NpcScheduleBlock right)
        {
            bool[] leftCoverage = new bool[24];
            bool[] rightCoverage = new bool[24];
            MarkBlockCoverage(left, leftCoverage);
            MarkBlockCoverage(right, rightCoverage);
            for (int hour = 0; hour < 24; hour++)
            {
                if (leftCoverage[hour] && rightCoverage[hour])
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsHourWithinBlock(int hour, CCS_NpcScheduleBlock block)
        {
            int start = ClampHour(block.StartHour);
            int end = ClampHour(block.EndHour);
            if (start < end)
            {
                return hour >= start && hour < end;
            }

            return hour >= start || hour < end;
        }

        private static int ClampHour(int hour)
        {
            if (hour < 0)
            {
                return 0;
            }

            return hour > 23 ? 23 : hour;
        }
    }
}
