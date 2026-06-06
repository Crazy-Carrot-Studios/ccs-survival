using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeService
// CATEGORY: Modules / NPCs / Runtime / Services
// PURPOSE: Assigns named NPC representatives to active businesses and syncs interactables.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — routes through CCS_SettlementServiceRouteResolver; no AI/dialogue.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcServiceRepresentativeService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_NpcServiceRepresentativeService]";

        private CCS_NpcServiceRepresentativeProfile activeProfile;
        private CCS_NpcIdentityService identityService;
        private Func<string, CCS_NpcServiceRepresentativeState[]> getRepresentativeStates;
        private Action<string, CCS_NpcServiceRepresentativeState[]> setRepresentativeStates;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_NpcServiceRepresentativeProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_NpcServiceRepresentativeProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_NpcServiceRepresentativeValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindIdentityService(CCS_NpcIdentityService npcIdentityService)
        {
            identityService = npcIdentityService;
        }

        public void BindRepresentativeStateAccessors(
            Func<string, CCS_NpcServiceRepresentativeState[]> getter,
            Action<string, CCS_NpcServiceRepresentativeState[]> setter)
        {
            getRepresentativeStates = getter;
            setRepresentativeStates = setter;
            BindRuntimeBridge();
        }

        public void HandleBusinessActivated(CCS_BusinessActivatedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            TrySyncRepresentative(
                eventArgs.Snapshot.SettlementId,
                eventArgs.BusinessId,
                eventArgs.BusinessType,
                true);
        }

        public void HandleBusinessDeactivated(CCS_BusinessDeactivatedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            TrySyncRepresentative(
                eventArgs.Snapshot.SettlementId,
                eventArgs.BusinessId,
                eventArgs.BusinessType,
                false);
        }

        public void RefreshSettlement(string settlementId)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            CCS_NpcServiceRepresentativeDefinition[] definitions = activeProfile.RepresentativeDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcServiceRepresentativeDefinition definition = definitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CCS_NpcServiceRepresentativeState state =
                    CCS_NpcServiceRepresentativeUtility.TryFindState(
                        getRepresentativeStates?.Invoke(settlementId),
                        definition.RepresentativeId);
                if (state != null && state.isActive)
                {
                    ApplyRepresentativePresentation(state);
                }
            }
        }

        public void RefreshAllRepresentatives()
        {
            if (activeProfile == null)
            {
                return;
            }

            HashSet<string> settlements = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CCS_NpcServiceRepresentativeDefinition[] definitions = activeProfile.RepresentativeDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_NpcServiceRepresentativeDefinition definition = definitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    settlements.Add(definition.SettlementId);
                }
            }

            foreach (string settlementId in settlements)
            {
                RefreshSettlement(settlementId);
            }
        }

        public bool TryGetRepresentativeSnapshot(
            string settlementId,
            string businessId,
            out CCS_NpcServiceRepresentativeSnapshot snapshot)
        {
            snapshot = CCS_NpcServiceRepresentativeSnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(businessId))
            {
                return false;
            }

            if (!activeProfile.TryGetDefinitionForBusiness(settlementId, businessId, out CCS_NpcServiceRepresentativeDefinition definition)
                && !activeProfile.TryGetDefinitionForBusiness(string.Empty, businessId, out definition))
            {
                return false;
            }

            string representativeId = string.IsNullOrWhiteSpace(definition.RepresentativeId)
                ? CCS_NpcServiceRepresentativeUtility.BuildRepresentativeId(settlementId, businessId)
                : definition.RepresentativeId;
            CCS_NpcServiceRepresentativeState state =
                CCS_NpcServiceRepresentativeUtility.TryFindState(
                    getRepresentativeStates?.Invoke(settlementId),
                    representativeId);
            if (state == null)
            {
                return false;
            }

            string displayName = ResolveDisplayNameForIdentity(state.assignedNpcIdentityId, settlementId);
            snapshot = CCS_NpcServiceRepresentativeUtility.BuildSnapshotFromState(state, displayName);
            return snapshot.IsValid;
        }

        private void BindRuntimeBridge()
        {
            CCS_NpcServiceRepresentativeRuntimeBridge.ResolveRepresentativeService = TryGetRepresentativeSnapshot;
            CCS_NpcServiceRepresentativeRuntimeBridge.ResolveAllActiveRepresentatives = settlementId =>
            {
                if (string.IsNullOrWhiteSpace(settlementId))
                {
                    return Array.Empty<CCS_NpcServiceRepresentativeSnapshot>();
                }

                List<CCS_NpcServiceRepresentativeSnapshot> snapshots = new List<CCS_NpcServiceRepresentativeSnapshot>();
                CCS_NpcServiceRepresentativeState[] states =
                    getRepresentativeStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcServiceRepresentativeState>();
                for (int index = 0; index < states.Length; index++)
                {
                    CCS_NpcServiceRepresentativeState state = states[index];
                    if (state == null || !state.isActive)
                    {
                        continue;
                    }

                    string displayName = ResolveDisplayNameForIdentity(state.assignedNpcIdentityId, settlementId);
                    snapshots.Add(CCS_NpcServiceRepresentativeUtility.BuildSnapshotFromState(state, displayName));
                }

                return snapshots.ToArray();
            };
            CCS_NpcServiceRepresentativeRuntimeBridge.SyncRepresentativeActor = SyncRepresentativeActor;
            CCS_NpcServiceRepresentativeRuntimeBridge.RefreshAllRepresentatives = RefreshAllRepresentatives;
        }

        private void TrySyncRepresentative(
            string settlementId,
            string businessId,
            CCS_BusinessType businessType,
            bool active)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(businessId))
            {
                return;
            }

            if (!activeProfile.TryGetDefinitionForBusiness(settlementId, businessId, out CCS_NpcServiceRepresentativeDefinition definition)
                && !activeProfile.TryGetDefinitionForBusiness(string.Empty, businessId, out definition))
            {
                return;
            }

            string representativeId = string.IsNullOrWhiteSpace(definition.RepresentativeId)
                ? CCS_NpcServiceRepresentativeUtility.BuildRepresentativeId(settlementId, businessId)
                : definition.RepresentativeId;
            CCS_NpcRoleType requiredRole = definition.RequiredRole != CCS_NpcRoleType.Unknown
                ? definition.RequiredRole
                : CCS_NpcServiceRepresentativeUtility.ResolveRoleForBusinessType(businessType);
            if (requiredRole == CCS_NpcRoleType.Unknown)
            {
                return;
            }

            string anchorId = CCS_NpcServiceRepresentativeUtility.BuildRepresentativeAnchorId(settlementId, businessId);
            string displayTitle = string.IsNullOrWhiteSpace(definition.DisplayTitle)
                ? CCS_NpcServiceRepresentativeUtility.ResolveDefaultTitle(
                    requiredRole,
                    identityService?.ActiveProfile as CCS_NpcIdentityProfile)
                : definition.DisplayTitle;

            string assignedIdentityId = string.Empty;
            string displayName = string.Empty;
            if (active && identityService != null && identityService.IsInitialized)
            {
                if (!identityService.TryResolveRepresentativeIdentity(
                        anchorId,
                        settlementId,
                        requiredRole,
                        businessId,
                        out CCS_NpcIdentitySnapshot identitySnapshot)
                    || identitySnapshot == null)
                {
                    return;
                }

                assignedIdentityId = identitySnapshot.NpcIdentityId;
                displayName = identitySnapshot.DisplayName;
            }

            CCS_NpcServiceRepresentativeAssignment assignment = new CCS_NpcServiceRepresentativeAssignment
            {
                representativeId = representativeId,
                settlementId = settlementId,
                businessId = businessId,
                servicePointId = definition.ServicePointId,
                requiredRole = (int)requiredRole,
                assignedNpcIdentityId = assignedIdentityId,
                displayTitle = displayTitle,
                isActive = active,
                fallbackToServicePoint = definition.FallbackToServicePoint
            };

            CCS_NpcServiceRepresentativeState[] states =
                getRepresentativeStates?.Invoke(settlementId) ?? Array.Empty<CCS_NpcServiceRepresentativeState>();
            CCS_NpcServiceRepresentativeState persisted =
                CCS_NpcServiceRepresentativeUtility.BuildStateFromAssignment(assignment);
            setRepresentativeStates?.Invoke(
                settlementId,
                CCS_NpcServiceRepresentativeUtility.UpsertState(states, persisted));

            if (active)
            {
                ApplyRepresentativePresentation(persisted, displayName);
            }
            else
            {
                ClearRepresentativePresentation(persisted);
            }
        }

        private void ApplyRepresentativePresentation(CCS_NpcServiceRepresentativeState state)
        {
            string displayName = ResolveDisplayNameForIdentity(state.assignedNpcIdentityId, state.settlementId);
            ApplyRepresentativePresentation(state, displayName);
        }

        private void ApplyRepresentativePresentation(CCS_NpcServiceRepresentativeState state, string displayName)
        {
            if (state == null)
            {
                return;
            }

            CCS_NpcServiceRepresentativeSnapshot snapshot =
                CCS_NpcServiceRepresentativeUtility.BuildSnapshotFromState(state, displayName);
            CCS_NpcServiceRepresentativeRuntimeBridge.RegisterDisplayName(state.representativeId, displayName);
            CCS_NpcServiceRepresentativeDebugHud.NotifyRepresentativeSnapshot(snapshot);
            SyncRepresentativeActor(state.settlementId, state.businessId, true, snapshot);
        }

        private void ClearRepresentativePresentation(CCS_NpcServiceRepresentativeState state)
        {
            if (state == null)
            {
                return;
            }

            CCS_NpcServiceRepresentativeSnapshot snapshot =
                CCS_NpcServiceRepresentativeUtility.BuildSnapshotFromState(state, string.Empty);
            snapshot.IsActive = false;
            SyncRepresentativeActor(state.settlementId, state.businessId, false, snapshot);
        }

        private bool SyncRepresentativeActor(
            string settlementId,
            string businessId,
            bool active,
            CCS_NpcServiceRepresentativeSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return false;
            }

            CCS_IPopulationPlaceholderIdentityHost host = null;
            if (CCS_NpcServiceRepresentativeUtility.TryFindHostByBusinessId(settlementId, businessId, out host)
                && host != null)
            {
                if (active)
                {
                    host.ApplyServiceRepresentativePresentation(snapshot.DisplayTitle);
                }
                else
                {
                    host.ClearServiceRepresentativePresentation();
                }

                ConfigureInteractable(host, snapshot, active);
                return true;
            }

            if (!CCS_SettlementServicePointRuntimeBridge.TryGetServicePoint(snapshot.ServicePointId, out CCS_SettlementServicePoint servicePoint)
                || servicePoint == null)
            {
                return false;
            }

            GameObject root = CCS_NpcServiceRepresentativeRuntimeBridge.EnsureSpawnedRepresentativeRoot(
                snapshot.RepresentativeId,
                servicePoint.transform,
                new Vector3(1.2f, 0f, 0f));
            if (root == null)
            {
                return false;
            }

            CCS_PopulationPlaceholderActor placeholder = root.GetComponent<CCS_PopulationPlaceholderActor>();
            if (placeholder == null)
            {
                GameObject actorObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                actorObject.name = $"CCS_ServiceRepresentativeActor_{businessId}";
                actorObject.transform.SetParent(root.transform, false);
                actorObject.transform.localScale = new Vector3(0.45f, 0.9f, 0.45f);
                Collider collider = actorObject.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.Destroy(collider);
                }

                placeholder = actorObject.AddComponent<CCS_PopulationPlaceholderActor>();
            }

            CCS_SettlementPopulationCategory workforceCategory =
                CCS_NpcServiceRepresentativeUtility.ResolveWorkforceCategory(snapshot.RequiredRole);
            placeholder.Configure(workforceCategory);
            placeholder.BindAnchorContext(
                CCS_NpcServiceRepresentativeUtility.BuildRepresentativeAnchorId(settlementId, businessId),
                0,
                settlementId,
                businessId);
            placeholder.ApplyIdentityData(
                snapshot.AssignedNpcIdentityId,
                snapshot.DisplayName,
                (int)snapshot.RequiredRole,
                snapshot.DisplayTitle,
                settlementId,
                businessId,
                (int)workforceCategory);
            if (active)
            {
                placeholder.ApplyServiceRepresentativePresentation(snapshot.DisplayTitle);
            }
            else
            {
                placeholder.ClearServiceRepresentativePresentation();
            }

            ConfigureInteractable(placeholder, snapshot, active);
            root.SetActive(active);
            return true;
        }

        private static void ConfigureInteractable(
            CCS_IPopulationPlaceholderIdentityHost host,
            CCS_NpcServiceRepresentativeSnapshot snapshot,
            bool active)
        {
            if (host is not MonoBehaviour behaviour)
            {
                return;
            }

            CCS_NpcServiceRepresentativeInteractable interactable =
                behaviour.GetComponent<CCS_NpcServiceRepresentativeInteractable>();
            if (interactable == null)
            {
                interactable = behaviour.gameObject.AddComponent<CCS_NpcServiceRepresentativeInteractable>();
            }

            interactable.Configure(snapshot, active);
        }

        private string ResolveDisplayNameForIdentity(string identityId, string settlementId)
        {
            if (identityService != null
                && identityService.TryGetDisplayNameForIdentity(settlementId, identityId, out string displayName))
            {
                return displayName;
            }

            return string.Empty;
        }
    }
}
