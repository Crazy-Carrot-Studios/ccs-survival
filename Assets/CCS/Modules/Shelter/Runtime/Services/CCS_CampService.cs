using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_CampService : CCS_ISurvivalService
    {
        private CCS_CampDefinition activeProfile;
        private CCS_FrontierShelterService frontierShelterService;
        private CCS_BuildingService buildingService;
        private System.Func<Vector3, float, bool> bedrollProximityQuery;
        private CCS_ShelterService shelterService;
        private Vector3 subjectPosition;
        private bool hasSubjectPosition;
        private CCS_CampSnapshot currentSnapshot = CCS_CampSnapshot.Empty;
        private CCS_CampSaveState savedState = new CCS_CampSaveState();
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_CampSnapshot CurrentSnapshot => currentSnapshot;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_CampDefinition profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            RecalculateCamp();
        }

        public void BindFrontierShelterService(CCS_FrontierShelterService service)
        {
            frontierShelterService = service;
            RecalculateCamp();
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            buildingService = service;
            RecalculateCamp();
        }

        public void BindBedrollProximityQuery(System.Func<Vector3, float, bool> query)
        {
            bedrollProximityQuery = query;
            RecalculateCamp();
        }

        public void BindShelterService(CCS_ShelterService service)
        {
            shelterService = service;
            RecalculateCamp();
        }

        public void SetSubjectPosition(Vector3 position)
        {
            subjectPosition = position;
            hasSubjectPosition = true;
            RecalculateCamp();
        }

        public void Tick(float deltaTime)
        {
            if (hasSubjectPosition)
            {
                RecalculateCamp();
            }
        }

        public float GetSleepBonusMultiplier()
        {
            if (currentSnapshot.CampTier >= CCS_CampTier.TemporaryCamp)
            {
                return 1f + GetActiveShelterSleepBonus();
            }

            return 1f;
        }

        public float GetWeatherProtectionPercent()
        {
            return GetActiveShelterWeatherProtection();
        }

        public void RecalculateCamp()
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableCampTracking)
            {
                currentSnapshot = CCS_CampSnapshot.Empty;
                return;
            }

            float radius = activeProfile.CampDetectionRadius;
            bool hasShelter = HasShelterInRadius(radius);
            bool hasCampfire = HasCampfireInRadius(radius);
            bool hasBedroll = HasBedrollInRadius(radius);
            Vector3 campCenter = subjectPosition;

            CCS_CampTier tier = CCS_CampTier.None;
            if (hasShelter && hasCampfire && hasBedroll)
            {
                tier = CCS_CampTier.TemporaryCamp;
            }

            bool ownsCamp = tier != CCS_CampTier.None;
            string ownerId = ownsCamp ? savedState.campOwnerId : string.Empty;
            if (ownsCamp && string.IsNullOrWhiteSpace(ownerId))
            {
                ownerId = "ccs.survival.camp.player";
                savedState.campOwnerId = ownerId;
            }

            savedState.campTier = (int)tier;
            savedState.hasShelter = hasShelter;
            savedState.hasCampfire = hasCampfire;
            savedState.hasBedroll = hasBedroll;
            savedState.ownsCamp = ownsCamp;
            savedState.campCenterX = campCenter.x;
            savedState.campCenterY = campCenter.y;
            savedState.campCenterZ = campCenter.z;

            currentSnapshot = new CCS_CampSnapshot(
                tier,
                hasShelter,
                hasCampfire,
                hasBedroll,
                ownsCamp,
                ownerId,
                campCenter,
                tier == CCS_CampTier.TemporaryCamp
                    ? "Temporary frontier camp established."
                    : "Camp requirements incomplete.");
        }

        public CCS_CampSaveState CaptureState()
        {
            return savedState ?? new CCS_CampSaveState();
        }

        public void RestoreState(CCS_CampSaveState state)
        {
            savedState = state ?? new CCS_CampSaveState();
            RecalculateCamp();
        }

        private bool HasShelterInRadius(float radius)
        {
            if (frontierShelterService == null || !frontierShelterService.IsInitialized)
            {
                return false;
            }

            IReadOnlyList<CCS_FrontierShelterInstance> shelters = frontierShelterService.GetRegisteredShelters();
            for (int index = 0; index < shelters.Count; index++)
            {
                CCS_FrontierShelterInstance shelter = shelters[index];
                if (shelter != null && IsWithinRadius(shelter.WorldPosition, radius))
                {
                    return true;
                }
            }

            return shelterService != null
                && shelterService.IsInitialized
                && shelterService.GetSnapshot().IsSheltered;
        }

        private bool HasCampfireInRadius(float radius)
        {
            if (buildingService == null || !buildingService.IsInitialized)
            {
                return false;
            }

            string campfirePieceId = activeProfile.CampfirePieceId;
            IReadOnlyList<CCS_BuildingInstance> instances = buildingService.GetPlacedInstances();
            for (int index = 0; index < instances.Count; index++)
            {
                CCS_BuildingInstance instance = instances[index];
                if (instance != null
                    && string.Equals(instance.PieceId, campfirePieceId, System.StringComparison.OrdinalIgnoreCase)
                    && IsWithinRadius(instance.Position, radius))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBedrollInRadius(float radius)
        {
            if (bedrollProximityQuery == null)
            {
                return false;
            }

            return bedrollProximityQuery.Invoke(subjectPosition, radius);
        }

        private bool IsWithinRadius(Vector3 worldPosition, float radius)
        {
            if (!hasSubjectPosition)
            {
                return false;
            }

            return Vector3.Distance(subjectPosition, worldPosition) <= radius;
        }

        private float GetActiveShelterSleepBonus()
        {
            if (frontierShelterService == null)
            {
                return 0f;
            }

            float best = 0f;
            IReadOnlyList<CCS_FrontierShelterInstance> shelters = frontierShelterService.GetRegisteredShelters();
            for (int index = 0; index < shelters.Count; index++)
            {
                CCS_FrontierShelterInstance shelter = shelters[index];
                if (shelter?.ShelterDefinition == null)
                {
                    continue;
                }

                if (!IsWithinRadius(shelter.WorldPosition, activeProfile.CampDetectionRadius))
                {
                    continue;
                }

                if (shelter.ShelterDefinition.SleepBonus > best)
                {
                    best = shelter.ShelterDefinition.SleepBonus;
                }
            }

            return best;
        }

        private float GetActiveShelterWeatherProtection()
        {
            if (frontierShelterService == null)
            {
                return 0f;
            }

            float best = 0f;
            IReadOnlyList<CCS_FrontierShelterInstance> shelters = frontierShelterService.GetRegisteredShelters();
            for (int index = 0; index < shelters.Count; index++)
            {
                CCS_FrontierShelterInstance shelter = shelters[index];
                if (shelter?.ShelterDefinition == null)
                {
                    continue;
                }

                if (!IsWithinRadius(shelter.WorldPosition, activeProfile.CampDetectionRadius))
                {
                    continue;
                }

                if (shelter.ShelterDefinition.WeatherProtectionPercent > best)
                {
                    best = shelter.ShelterDefinition.WeatherProtectionPercent;
                }
            }

            return best;
        }
    }
}
