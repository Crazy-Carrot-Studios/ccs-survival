using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcMovementValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Profile validation, schedule evaluation, anchor resolution, and state helpers.
// PLACEMENT: Used by CCS_NpcMovementService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — schedule uses profile work/home hours with time-of-day input.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcMovementValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcMovementProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC movement profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC movement profile requires profileId.");
            }

            if (profile.MoveSpeed < 1.5f || profile.MoveSpeed > 2f)
            {
                return CCS_SurvivalValidationResult.Warn(
                    $"NPC movement speed {profile.MoveSpeed:F2} m/s is outside recommended 1.5–2.0 range.");
            }

            if (profile.ArrivalTolerance <= 0f || profile.ArrivalTolerance > 1f)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"NPC movement arrival tolerance must be positive and <= 1m (found {profile.ArrivalTolerance:F2}).");
            }

            int workStart = ClampHour(profile.WorkStartHour);
            int workEnd = ClampHour(profile.WorkEndHour);
            if (workStart == workEnd)
            {
                return CCS_SurvivalValidationResult.Fail("NPC movement workStartHour and workEndHour must differ.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC movement profile validated ({profile.ProfileId}). Work {workStart:00}:00–{workEnd:00}:00.");
        }

        public static bool IsWorkPeriod(int currentHour, CCS_NpcMovementProfile profile)
        {
            if (profile == null)
            {
                return true;
            }

            int hour = ClampHour(currentHour);
            int workStart = ClampHour(profile.WorkStartHour);
            int workEnd = ClampHour(profile.WorkEndHour);
            if (workStart < workEnd)
            {
                return hour >= workStart && hour < workEnd;
            }

            return hour >= workStart || hour < workEnd;
        }

        public static CCS_NpcMovementState[] CloneStates(CCS_NpcMovementState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_NpcMovementState>();
            }

            CCS_NpcMovementState[] clone = new CCS_NpcMovementState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_NpcMovementState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_NpcMovementState()
                    : new CCS_NpcMovementState
                    {
                        npcIdentityId = entry.npcIdentityId ?? string.Empty,
                        settlementId = entry.settlementId ?? string.Empty,
                        movementStatus = entry.movementStatus,
                        targetAnchorId = entry.targetAnchorId ?? string.Empty,
                        workplaceAnchorId = entry.workplaceAnchorId ?? string.Empty,
                        homeHousingId = entry.homeHousingId ?? string.Empty
                    };
            }

            return clone;
        }

        public static CCS_NpcMovementState TryFindState(CCS_NpcMovementState[] states, string npcIdentityId)
        {
            if (states == null || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_NpcMovementState state = states[index];
                if (state != null
                    && string.Equals(state.npcIdentityId, npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static CCS_NpcMovementState[] UpsertState(
            CCS_NpcMovementState[] states,
            CCS_NpcMovementState updatedState)
        {
            if (updatedState == null || string.IsNullOrWhiteSpace(updatedState.npcIdentityId))
            {
                return states ?? Array.Empty<CCS_NpcMovementState>();
            }

            CCS_NpcMovementState[] working = states ?? Array.Empty<CCS_NpcMovementState>();
            for (int index = 0; index < working.Length; index++)
            {
                CCS_NpcMovementState existing = working[index];
                if (existing != null
                    && string.Equals(existing.npcIdentityId, updatedState.npcIdentityId, StringComparison.OrdinalIgnoreCase))
                {
                    working[index] = updatedState;
                    return working;
                }
            }

            CCS_NpcMovementState[] expanded = new CCS_NpcMovementState[working.Length + 1];
            Array.Copy(working, expanded, working.Length);
            expanded[working.Length] = updatedState;
            return expanded;
        }

        public static CCS_NpcMovementSnapshot BuildSnapshotFromState(CCS_NpcMovementState state)
        {
            if (state == null)
            {
                return CCS_NpcMovementSnapshot.Empty;
            }

            return new CCS_NpcMovementSnapshot
            {
                NpcIdentityId = state.npcIdentityId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                Status = (CCS_NpcMovementStatus)state.movementStatus,
                TargetAnchorId = state.targetAnchorId ?? string.Empty,
                WorkplaceAnchorId = state.workplaceAnchorId ?? string.Empty,
                HomeHousingId = state.homeHousingId ?? string.Empty
            };
        }

        public static CCS_NpcMovementState BuildStateFromSnapshot(CCS_NpcMovementSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return new CCS_NpcMovementState();
            }

            return new CCS_NpcMovementState
            {
                npcIdentityId = snapshot.NpcIdentityId,
                settlementId = snapshot.SettlementId,
                movementStatus = (int)snapshot.Status,
                targetAnchorId = snapshot.TargetAnchorId ?? string.Empty,
                workplaceAnchorId = snapshot.WorkplaceAnchorId ?? string.Empty,
                homeHousingId = snapshot.HomeHousingId ?? string.Empty
            };
        }

        public static string ResolveHomeHousingId(
            string settlementId,
            CCS_SettlementPopulationCategory workforceCategory,
            CCS_SettlementHousingProfile housingProfile)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || housingProfile == null)
            {
                return string.Empty;
            }

            CCS_SettlementHousingDefinition preferred = null;
            CCS_SettlementHousingDefinition fallback = null;
            CCS_SettlementHousingDefinition[] definitions = housingProfile.HousingDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition definition = definitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (definition.WorkforceAffinity != workforceCategory)
                {
                    continue;
                }

                fallback = definition;
                if (CCS_SettlementHousingRuntimeBridge.ResolveHousingStatus(settlementId, definition.HousingId)
                    == CCS_SettlementHousingStatus.Active)
                {
                    preferred = definition;
                    break;
                }
            }

            CCS_SettlementHousingDefinition resolved = preferred ?? fallback;
            return resolved?.HousingId ?? string.Empty;
        }

        public static bool TryResolveWorkplacePosition(
            CCS_INpcMovementHost host,
            out Vector3 workplacePosition)
        {
            workplacePosition = Vector3.zero;
            if (host == null || string.IsNullOrWhiteSpace(host.SettlementId))
            {
                return false;
            }

            if (host.IsServiceRepresentative
                && !string.IsNullOrWhiteSpace(host.BusinessId)
                && CCS_NpcServiceRepresentativeRuntimeBridge.TryGetActiveRepresentativeSnapshot(
                    host.SettlementId,
                    host.BusinessId,
                    out CCS_NpcServiceRepresentativeSnapshot representativeSnapshot)
                && representativeSnapshot != null
                && representativeSnapshot.IsValid
                && CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(
                    representativeSnapshot.ServicePointId,
                    out CCS_SettlementServicePoint servicePoint)
                && servicePoint != null)
            {
                workplacePosition = servicePoint.transform.position;
                return true;
            }

            if (CCS_PopulationPresenceRuntimeBridge.TryFindAnchor(
                    host.WorkforceAnchorId,
                    out CCS_PopulationPresenceAnchor workforceAnchor)
                && workforceAnchor != null)
            {
                workplacePosition = workforceAnchor.transform.position;
                return true;
            }

            if (host.MovementTransform != null)
            {
                workplacePosition = host.MovementTransform.position;
                return true;
            }

            return false;
        }

        public static bool TryResolveHomePosition(
            string settlementId,
            string homeHousingId,
            out Vector3 homePosition)
        {
            homePosition = Vector3.zero;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(homeHousingId))
            {
                return false;
            }

            if (CCS_SettlementHousingRuntimeBridge.TryFindAnchorForHousing(
                    settlementId,
                    homeHousingId,
                    out CCS_SettlementHousingAnchor housingAnchor)
                && housingAnchor != null)
            {
                homePosition = housingAnchor.transform.position;
                return true;
            }

            return false;
        }

        public static bool TryResolveTargetPosition(
            CCS_INpcMovementHost host,
            string homeHousingId,
            bool isWorkPeriod,
            out Vector3 targetPosition,
            out string targetAnchorId)
        {
            targetPosition = Vector3.zero;
            targetAnchorId = string.Empty;
            if (host == null)
            {
                return false;
            }

            if (isWorkPeriod)
            {
                if (!TryResolveWorkplacePosition(host, out targetPosition))
                {
                    if (host.MovementTransform != null)
                    {
                        targetPosition = host.MovementTransform.position;
                    }

                    return host.MovementTransform != null;
                }

                targetAnchorId = host.WorkforceAnchorId ?? string.Empty;
                return true;
            }

            if (TryResolveHomePosition(host.SettlementId, homeHousingId, out targetPosition))
            {
                if (CCS_SettlementHousingRuntimeBridge.TryFindAnchorForHousing(
                        host.SettlementId,
                        homeHousingId,
                        out CCS_SettlementHousingAnchor housingAnchor)
                    && housingAnchor != null)
                {
                    targetAnchorId = housingAnchor.AnchorId;
                }

                return true;
            }

            if (TryResolveWorkplacePosition(host, out targetPosition))
            {
                targetAnchorId = host.WorkforceAnchorId ?? string.Empty;
                return true;
            }

            if (host.MovementTransform != null)
            {
                targetPosition = host.MovementTransform.position;
                return true;
            }

            return false;
        }

        public static CCS_NpcMovementStatus ResolveArrivalStatus(bool isWorkPeriod)
        {
            return isWorkPeriod ? CCS_NpcMovementStatus.Working : CCS_NpcMovementStatus.AtHome;
        }

        public static CCS_NpcMovementStatus ResolveTravelStatus(bool isWorkPeriod)
        {
            return isWorkPeriod ? CCS_NpcMovementStatus.TravelingToWork : CCS_NpcMovementStatus.TravelingHome;
        }

        public static float ResolveHorizontalDistance(Vector3 fromPosition, Vector3 toPosition)
        {
            fromPosition.y = 0f;
            toPosition.y = 0f;
            return Vector3.Distance(fromPosition, toPosition);
        }

        public static bool TryStepTowardTarget(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float moveSpeed,
            float deltaTime,
            out Vector3 nextPosition,
            out float targetYawDegrees)
        {
            nextPosition = currentPosition;
            targetYawDegrees = 0f;
            Vector3 flatDelta = targetPosition - currentPosition;
            flatDelta.y = 0f;
            if (flatDelta.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float step = moveSpeed * deltaTime;
            if (flatDelta.magnitude <= step)
            {
                nextPosition = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
            }
            else
            {
                Vector3 direction = flatDelta.normalized;
                nextPosition = currentPosition + direction * step;
            }

            targetYawDegrees = Mathf.Atan2(flatDelta.x, flatDelta.z) * Mathf.Rad2Deg;
            return true;
        }

        public static bool IsTravelingStatus(CCS_NpcMovementStatus status)
        {
            return status == CCS_NpcMovementStatus.TravelingToWork
                || status == CCS_NpcMovementStatus.TravelingHome;
        }

        public static bool IsIdleAtDestinationStatus(CCS_NpcMovementStatus status)
        {
            return status == CCS_NpcMovementStatus.Working
                || status == CCS_NpcMovementStatus.AtHome
                || status == CCS_NpcMovementStatus.Idle;
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
