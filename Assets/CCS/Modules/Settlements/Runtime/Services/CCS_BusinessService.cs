using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Frontier business profile host, snapshots, and activation events.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — evaluation driven by CCS_WorldSimulationService.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BusinessService]";

        private CCS_BusinessProfile activeProfile;
        private Func<string, CCS_BusinessSnapshot> snapshotResolver;
        private bool isInitialized;

        public event Action<CCS_BusinessActivatedEventArgs> BusinessActivated;

        public event Action<CCS_BusinessDeactivatedEventArgs> BusinessDeactivated;

        public bool IsInitialized => isInitialized;

        public CCS_BusinessProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_BusinessProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindBusinessSnapshotResolver(Func<string, CCS_BusinessSnapshot> resolver)
        {
            snapshotResolver = resolver;
        }

        public bool TryGetBusinessSnapshot(string settlementId, out CCS_BusinessSnapshot snapshot)
        {
            snapshot = CCS_BusinessSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || snapshotResolver == null)
            {
                return false;
            }

            snapshot = snapshotResolver.Invoke(settlementId) ?? CCS_BusinessSnapshot.Empty;
            return snapshot.IsValid;
        }

        public void DispatchBusinessEvaluation(
            CCS_BusinessSnapshot snapshot,
            IReadOnlyList<CCS_BusinessType> activated,
            IReadOnlyList<CCS_BusinessType> deactivated)
        {
            if (snapshot == null || !snapshot.IsValid || activeProfile == null)
            {
                return;
            }

            if (activated != null)
            {
                for (int index = 0; index < activated.Count; index++)
                {
                    CCS_BusinessType businessType = activated[index];
                    activeProfile.TryGetDefinition(businessType, out CCS_BusinessDefinition definition);
                    BusinessActivated?.Invoke(new CCS_BusinessActivatedEventArgs
                    {
                        Snapshot = snapshot,
                        BusinessType = businessType,
                        BusinessId = definition?.BusinessId ?? string.Empty
                    });
                }
            }

            if (deactivated != null)
            {
                for (int index = 0; index < deactivated.Count; index++)
                {
                    CCS_BusinessType businessType = deactivated[index];
                    activeProfile.TryGetDefinition(businessType, out CCS_BusinessDefinition definition);
                    BusinessDeactivated?.Invoke(new CCS_BusinessDeactivatedEventArgs
                    {
                        Snapshot = snapshot,
                        BusinessType = businessType,
                        BusinessId = definition?.BusinessId ?? string.Empty
                    });
                }
            }
        }
    }
}
