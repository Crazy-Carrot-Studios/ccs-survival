using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcActivityValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Profile validation, schedule/movement mapping, and state helpers.
// PLACEMENT: Used by CCS_NpcActivityService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — traveling overrides block-derived activity while moving.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcActivityValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcActivityProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC activity profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC activity profile requires profileId.");
            }

            CCS_NpcActivityBlockMapping[] mappings = profile.BlockMappings;
            if (mappings.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("NPC activity profile requires block mappings.");
            }

            for (int index = 0; index < mappings.Length; index++)
            {
                CCS_NpcActivityBlockMapping mapping = mappings[index];
                if (mapping == null
                    || mapping.ScheduleBlockType == CCS_NpcScheduleBlockType.Unknown
                    || mapping.ActivityType == CCS_NpcActivityType.None)
                {
                    return CCS_SurvivalValidationResult.Fail("NPC activity profile contains an invalid block mapping.");
                }
            }

            if (!ContainsBlockMapping(profile, CCS_NpcScheduleBlockType.Work)
                || !ContainsBlockMapping(profile, CCS_NpcScheduleBlockType.Service)
                || !ContainsBlockMapping(profile, CCS_NpcScheduleBlockType.Home)
                || !ContainsBlockMapping(profile, CCS_NpcScheduleBlockType.Sleep))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "NPC activity profile missing required work/service/home/sleep mappings.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC activity profile validated ({profile.ProfileId}). {mappings.Length} block mappings.");
        }

        public static CCS_NpcActivityType ResolveActivity(
            CCS_NpcActivityProfile profile,
            CCS_NpcScheduleBlockType scheduleBlockType,
            CCS_NpcMovementStatus movementStatus,
            bool scheduleAvailable,
            bool movementAvailable)
        {
            if (movementAvailable && IsTravelingMovementStatus(movementStatus))
            {
                return CCS_NpcActivityType.Traveling;
            }

            if (scheduleAvailable && profile != null)
            {
                if (profile.TryGetActivityForBlock(scheduleBlockType, out CCS_NpcActivityType mappedActivity))
                {
                    return mappedActivity;
                }

                return profile.ScheduleMissingFallbackActivity;
            }

            if (movementAvailable)
            {
                return ResolveActivityFromMovementOnly(movementStatus, profile?.MovementMissingFallbackActivity
                    ?? CCS_NpcActivityType.Idle);
            }

            return profile?.ScheduleMissingFallbackActivity ?? CCS_NpcActivityType.None;
        }

        public static CCS_NpcActivityType ResolveDefaultActivityForBlock(CCS_NpcScheduleBlockType blockType)
        {
            switch (blockType)
            {
                case CCS_NpcScheduleBlockType.Sleep:
                    return CCS_NpcActivityType.Sleeping;
                case CCS_NpcScheduleBlockType.Home:
                case CCS_NpcScheduleBlockType.Break:
                    return CCS_NpcActivityType.Resting;
                case CCS_NpcScheduleBlockType.Work:
                    return CCS_NpcActivityType.Working;
                case CCS_NpcScheduleBlockType.Service:
                    return CCS_NpcActivityType.Serving;
                case CCS_NpcScheduleBlockType.Leisure:
                    return CCS_NpcActivityType.Leisure;
                case CCS_NpcScheduleBlockType.Idle:
                    return CCS_NpcActivityType.Idle;
                default:
                    return CCS_NpcActivityType.None;
            }
        }

        public static bool IsTravelingMovementStatus(CCS_NpcMovementStatus movementStatus)
        {
            return movementStatus == CCS_NpcMovementStatus.TravelingToWork
                || movementStatus == CCS_NpcMovementStatus.TravelingHome;
        }

        public static CCS_NpcActivityState[] CloneStates(CCS_NpcActivityState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcActivityState>();
            }

            CCS_NpcActivityState[] clone = new CCS_NpcActivityState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcActivityState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcActivityState()
                    : new CCS_NpcActivityState
                    {
                        npcIdentityId = entry.npcIdentityId ?? string.Empty,
                        settlementId = entry.settlementId ?? string.Empty,
                        currentActivityType = entry.currentActivityType,
                        lastEvaluatedHour = entry.lastEvaluatedHour
                    };
            }

            return clone;
        }

        public static CCS_NpcActivityState TryFindState(CCS_NpcActivityState[] states, string npcIdentityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcActivityState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcActivityState[] UpsertState(
            CCS_NpcActivityState[] states,
            CCS_NpcActivityState updatedState)
        {
            if (updatedState == null || string.IsNullOrWhiteSpace(updatedState.npcIdentityId))
            {
                return states ?? Array.Empty<CCS_NpcActivityState>();
            }

            CCS_NpcActivityState[] working = states ?? Array.Empty<CCS_NpcActivityState>();
            for (int index = 0; index < working.Length; index++)
            {
                CCS_NpcActivityState existing = working[index];
                if (existing != null
                    && string.Equals(existing.npcIdentityId, updatedState.npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    working[index] = updatedState;
                    return working;
                }
            }

            CCS_NpcActivityState[] expanded = new CCS_NpcActivityState[working.Length + 1];
            Array.Copy(working, expanded, working.Length);
            expanded[working.Length] = updatedState;
            return expanded;
        }

        public static CCS_NpcActivitySnapshot BuildSnapshotFromState(CCS_NpcActivityState state)
        {
            if (state == null)
            {
                return CCS_NpcActivitySnapshot.Empty;
            }

            return new CCS_NpcActivitySnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                CurrentActivityType = Enum.IsDefined(typeof(CCS_NpcActivityType), state.currentActivityType)
                    ? (CCS_NpcActivityType)state.currentActivityType
                    : CCS_NpcActivityType.None,
                LastEvaluatedHour = state.lastEvaluatedHour
            };
        }

        private static bool ContainsBlockMapping(CCS_NpcActivityProfile profile, CCS_NpcScheduleBlockType blockType)
        {
            CCS_NpcActivityBlockMapping[] mappings = profile.BlockMappings;
            for (int index = 0; index < mappings.Length; index++)
            {
                CCS_NpcActivityBlockMapping mapping = mappings[index];
                if (mapping != null && mapping.ScheduleBlockType == blockType)
                {
                    return true;
                }
            }

            return false;
        }

        private static CCS_NpcActivityType ResolveActivityFromMovementOnly(
            CCS_NpcMovementStatus movementStatus,
            CCS_NpcActivityType fallbackActivity)
        {
            switch (movementStatus)
            {
                case CCS_NpcMovementStatus.TravelingToWork:
                    return CCS_NpcActivityType.Traveling;
                case CCS_NpcMovementStatus.Working:
                    return CCS_NpcActivityType.Working;
                case CCS_NpcMovementStatus.TravelingHome:
                    return CCS_NpcActivityType.Traveling;
                case CCS_NpcMovementStatus.AtHome:
                    return CCS_NpcActivityType.Resting;
                case CCS_NpcMovementStatus.Idle:
                    return CCS_NpcActivityType.Idle;
                default:
                    return fallbackActivity;
            }
        }
    }
}
