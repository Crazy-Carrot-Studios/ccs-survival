using System;
using CCS.Core;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcMovementService
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Schedule-driven transform movement for workforce placeholders and service reps.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — no NavMesh, Rigidbody, or CharacterController.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcMovementService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_NpcMovementService]";

        private CCS_NpcMovementProfile activeProfile;
        private Func<string, CCS_NpcMovementState[]> getMovementStates;
        private Action<string, CCS_NpcMovementState[]> setMovementStates;
        private Func<string, CCS_NpcIdentityState[]> getIdentityStates;
        private Action<string, CCS_NpcIdentityState[]> setIdentityStates;
        private Func<int> resolveCurrentHour;
        private Func<CCS_SettlementHousingProfile> housingProfileResolver;
        private CCS_NpcScheduleService scheduleService;
        private Action<CCS_INpcMovementHost> activityHostUpdated;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcMovementProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcMovementProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_NpcMovementValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindMovementStateAccessors(
            Func<string, CCS_NpcMovementState[]> getter,
            Action<string, CCS_NpcMovementState[]> setter)
        {
            getMovementStates = getter;
            setMovementStates = setter;
            BindRuntimeBridge();
        }

        public void BindIdentityStateAccessors(
            Func<string, CCS_NpcIdentityState[]> getter,
            Action<string, CCS_NpcIdentityState[]> setter)
        {
            getIdentityStates = getter;
            setIdentityStates = setter;
        }

        public void BindScheduleHourResolver(Func<int> resolver)
        {
            resolveCurrentHour = resolver;
        }

        public void BindHousingProfileResolver(Func<CCS_SettlementHousingProfile> resolver)
        {
            housingProfileResolver = resolver;
        }

        public void BindScheduleService(CCS_NpcScheduleService service)
        {
            scheduleService = service;
        }

        public void BindActivityHostUpdatedCallback(Action<CCS_INpcMovementHost> callback)
        {
            activityHostUpdated = callback;
        }

        public bool TryGetMovementSnapshot(string settlementId, string npcIdentityId, out CCS_NpcMovementSnapshot snapshot)
        {
            snapshot = CCS_NpcMovementSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            CCS_NpcMovementState state = CCS_NpcMovementValidationUtility.TryFindState(
                getMovementStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcMovementState>(),
                npcIdentityId);
            if (state == null)
            {
                return false;
            }

            snapshot = CCS_NpcMovementValidationUtility.BuildSnapshotFromState(state);
            return snapshot.IsValid;
        }

        public void ResyncAllFromSchedule()
        {
            CCS_NpcMovementRuntimeBridge.ForEachHost(ProcessHostScheduleResync);
        }

        public void RefreshAllMovement()
        {
            ResyncAllFromSchedule();
        }

        public void RefreshSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            CCS_NpcMovementRuntimeBridge.ForEachHost(host =>
            {
                if (host != null
                    && string.Equals(host.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    ProcessHostScheduleResync(host);
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

        public void Tick(float deltaTime)
        {
            if (!isInitialized || activeProfile == null || deltaTime <= 0f)
            {
                return;
            }

            int currentHour = resolveCurrentHour?.Invoke() ?? 12;
            CCS_NpcMovementRuntimeBridge.ForEachHost(host => ProcessHostMovement(host, currentHour, deltaTime));
        }

        private void BindRuntimeBridge()
        {
            CCS_NpcMovementRuntimeBridge.ResolveMovementSnapshot = (settlementId, npcIdentityId) =>
            {
                TryGetMovementSnapshot(settlementId, npcIdentityId, out CCS_NpcMovementSnapshot snapshot);
                return snapshot;
            };
            CCS_NpcMovementRuntimeBridge.RefreshAllMovement = RefreshAllMovement;
        }

        private void ProcessHostScheduleResync(CCS_INpcMovementHost host)
        {
            if (host == null || !host.HasIdentity || setMovementStates == null)
            {
                return;
            }

            EnsureHomeAssignment(host);
            int currentHour = resolveCurrentHour?.Invoke() ?? 12;
            string homeHousingId = ResolveHomeHousingId(host);
            ResolveMovementTarget(
                host,
                homeHousingId,
                currentHour,
                out _,
                out string targetAnchorId,
                out CCS_NpcScheduleBlockType blockType,
                out bool usesSchedule);

            CCS_NpcMovementStatus travelStatus = usesSchedule
                ? CCS_NpcScheduleValidationUtility.ResolveTravelStatus(blockType)
                : CCS_NpcMovementValidationUtility.ResolveTravelStatus(
                    CCS_NpcMovementValidationUtility.IsWorkPeriod(currentHour, activeProfile));

            CCS_NpcMovementState[] states =
                getMovementStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcMovementState>();
            CCS_NpcMovementState existing = CCS_NpcMovementValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcMovementState updated = existing ?? new CCS_NpcMovementState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId
            };

            updated.workplaceAnchorId = host.WorkforceAnchorId ?? string.Empty;
            updated.homeHousingId = homeHousingId ?? string.Empty;
            updated.targetAnchorId = targetAnchorId ?? string.Empty;
            updated.movementStatus = (int)travelStatus;

            setMovementStates.Invoke(
                host.SettlementId,
                CCS_NpcMovementValidationUtility.UpsertState(states, updated));
            activityHostUpdated?.Invoke(host);
        }

        private void ProcessHostMovement(CCS_INpcMovementHost host, int currentHour, float deltaTime)
        {
            if (host == null || !host.HasIdentity || host.MovementTransform == null || setMovementStates == null)
            {
                return;
            }

            EnsureHomeAssignment(host);
            string homeHousingId = ResolveHomeHousingId(host);
            if (!ResolveMovementTarget(
                    host,
                    homeHousingId,
                    currentHour,
                    out Vector3 targetPosition,
                    out string targetAnchorId,
                    out CCS_NpcScheduleBlockType blockType,
                    out bool usesSchedule))
            {
                return;
            }

            bool isWorkPeriod = usesSchedule
                ? CCS_NpcScheduleValidationUtility.IsWorkLikeBlock(blockType)
                : CCS_NpcMovementValidationUtility.IsWorkPeriod(currentHour, activeProfile);

            Vector3 currentPosition = host.MovementTransform.position;
            float distance = CCS_NpcMovementValidationUtility.ResolveHorizontalDistance(
                currentPosition,
                targetPosition);
            CCS_NpcMovementStatus status;
            if (distance <= activeProfile.ArrivalTolerance)
            {
                status = usesSchedule
                    ? CCS_NpcScheduleValidationUtility.ResolveArrivalStatus(blockType)
                    : CCS_NpcMovementValidationUtility.ResolveArrivalStatus(isWorkPeriod);
                CCS_NpcMovementRuntimeBridge.ApplyMovementTransform(
                    host,
                    new Vector3(targetPosition.x, currentPosition.y, targetPosition.z),
                    host.MovementTransform.eulerAngles.y);
                CCS_NpcMovementRuntimeBridge.ApplyIdleRotation(
                    host,
                    activeProfile.IdleRotationSpeed,
                    deltaTime);
            }
            else
            {
                status = usesSchedule
                    ? CCS_NpcScheduleValidationUtility.ResolveTravelStatus(blockType)
                    : CCS_NpcMovementValidationUtility.ResolveTravelStatus(isWorkPeriod);
                if (CCS_NpcMovementValidationUtility.TryStepTowardTarget(
                        currentPosition,
                        targetPosition,
                        activeProfile.MoveSpeed,
                        deltaTime,
                        out Vector3 nextPosition,
                        out float targetYaw))
                {
                    CCS_NpcMovementRuntimeBridge.ApplyMovementTransform(host, nextPosition, targetYaw);
                }
            }

            PersistMovementState(host, status, targetAnchorId, homeHousingId);
            activityHostUpdated?.Invoke(host);
        }

        private bool ResolveMovementTarget(
            CCS_INpcMovementHost host,
            string homeHousingId,
            int currentHour,
            out Vector3 targetPosition,
            out string targetAnchorId,
            out CCS_NpcScheduleBlockType blockType,
            out bool usesSchedule)
        {
            targetPosition = Vector3.zero;
            targetAnchorId = string.Empty;
            blockType = CCS_NpcScheduleBlockType.Unknown;
            usesSchedule = scheduleService != null
                && scheduleService.IsInitialized
                && scheduleService.TryEvaluateForHost(
                    host,
                    currentHour,
                    out blockType,
                    out _,
                    out targetAnchorId)
                && !CCS_NpcScheduleValidationUtility.UsesLegacyWorkHourFallback(blockType);

            if (usesSchedule)
            {
                return CCS_NpcScheduleValidationUtility.TryResolveTargetForBlock(
                    host,
                    homeHousingId,
                    blockType,
                    out targetPosition,
                    out targetAnchorId,
                    out _);
            }

            bool isWorkPeriod = CCS_NpcMovementValidationUtility.IsWorkPeriod(currentHour, activeProfile);
            return CCS_NpcMovementValidationUtility.TryResolveTargetPosition(
                host,
                homeHousingId,
                isWorkPeriod,
                out targetPosition,
                out targetAnchorId);
        }

        private void PersistMovementState(
            CCS_INpcMovementHost host,
            CCS_NpcMovementStatus status,
            string targetAnchorId,
            string homeHousingId)
        {
            CCS_NpcMovementState[] states =
                getMovementStates?.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcMovementState>();
            CCS_NpcMovementState existing = CCS_NpcMovementValidationUtility.TryFindState(states, host.NpcIdentityId);
            CCS_NpcMovementState updated = existing ?? new CCS_NpcMovementState
            {
                npcIdentityId = host.NpcIdentityId,
                settlementId = host.SettlementId
            };

            updated.movementStatus = (int)status;
            updated.targetAnchorId = targetAnchorId ?? string.Empty;
            updated.workplaceAnchorId = host.WorkforceAnchorId ?? string.Empty;
            updated.homeHousingId = homeHousingId ?? string.Empty;
            setMovementStates.Invoke(
                host.SettlementId,
                CCS_NpcMovementValidationUtility.UpsertState(states, updated));
        }

        private void EnsureHomeAssignment(CCS_INpcMovementHost host)
        {
            if (setIdentityStates == null || getIdentityStates == null)
            {
                return;
            }

            string resolvedHome = ResolveHomeHousingId(host);
            if (string.IsNullOrWhiteSpace(resolvedHome))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(host.HomeHousingId)
                && string.Equals(host.HomeHousingId, resolvedHome, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            CCS_NpcIdentityState[] states =
                getIdentityStates.Invoke(host.SettlementId) ?? Array.Empty<CCS_NpcIdentityState>();
            CCS_NpcIdentityState identityState =
                CCS_NpcIdentityValidationUtility.TryFindState(states, host.NpcIdentityId);
            if (identityState == null)
            {
                return;
            }

            if (string.Equals(identityState.homeHousingId, resolvedHome, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            identityState.homeHousingId = resolvedHome;
            setIdentityStates.Invoke(host.SettlementId, states);
            CCS_NpcRuntimeBridge.RefreshSettlementIdentities(host.SettlementId);
        }

        private string ResolveHomeHousingId(CCS_INpcMovementHost host)
        {
            if (!string.IsNullOrWhiteSpace(host.HomeHousingId))
            {
                return host.HomeHousingId;
            }

            CCS_SettlementHousingProfile housingProfile = housingProfileResolver?.Invoke();
            if (housingProfile == null)
            {
                return string.Empty;
            }

            return CCS_NpcMovementValidationUtility.ResolveHomeHousingId(
                host.SettlementId,
                (CCS_SettlementPopulationCategory)host.WorkforceCategoryValue,
                housingProfile);
        }
    }
}
