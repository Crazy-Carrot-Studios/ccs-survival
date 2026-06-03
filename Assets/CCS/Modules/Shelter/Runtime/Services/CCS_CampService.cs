using System;
using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_CampService : CCS_ISurvivalService
    {
        private const string DefaultCampOwnerId = "ccs.survival.camp.player";

        private CCS_CampDefinition activeProfile;
        private CCS_FrontierShelterService frontierShelterService;
        private CCS_FrontierHomesteadStructureService homesteadStructureService;
        private Func<Vector3, float, bool> storageProximityQuery;
        private Func<Vector3, float, bool> mountPresenceQuery;
        private Func<Vector3, float, bool> ranchStructureProximityQuery;
        private Func<Vector3, string> landClaimIdQuery;
        private CCS_BuildingService buildingService;
        private Func<Vector3, float, bool> bedrollProximityQuery;
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

        public void BindHomesteadStructureService(CCS_FrontierHomesteadStructureService service)
        {
            homesteadStructureService = service;
            RecalculateCamp();
        }

        public void BindStorageProximityQuery(Func<Vector3, float, bool> query)
        {
            storageProximityQuery = query;
            RecalculateCamp();
        }

        public void BindMountPresenceQuery(Func<Vector3, float, bool> query)
        {
            mountPresenceQuery = query;
            RecalculateCamp();
        }

        public void BindRanchStructureProximityQuery(Func<Vector3, float, bool> query)
        {
            ranchStructureProximityQuery = query;
            RecalculateCamp();
        }

        public void BindLandClaimQuery(Func<Vector3, string> query)
        {
            landClaimIdQuery = query;
            RecalculateCamp();
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            buildingService = service;
            RecalculateCamp();
        }

        public void BindBedrollProximityQuery(Func<Vector3, float, bool> query)
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
            bool hasStorage = HasStorageInRadius(radius);
            bool hasWorkArea = HasCampStructureInRadius(radius, CCS_CampStructureKind.WorkArea);
            bool hasSawTable = HasCampStructureInRadius(radius, CCS_CampStructureKind.SawTable);
            bool hasCharcoalKiln = HasCampStructureInRadius(radius, CCS_CampStructureKind.CharcoalKiln);
            bool hasPrimitiveForge = HasCampStructureInRadius(radius, CCS_CampStructureKind.PrimitiveForge);
            Vector3 campCenter = subjectPosition;

            Dictionary<CCS_CampStructureKind, bool> presence = CCS_CampTierEvaluationUtility.CreatePresenceMap();
            presence[CCS_CampStructureKind.Shelter] = hasShelter;
            presence[CCS_CampStructureKind.Campfire] = hasCampfire;
            presence[CCS_CampStructureKind.Bedroll] = hasBedroll;
            presence[CCS_CampStructureKind.Storage] = hasStorage;
            presence[CCS_CampStructureKind.WorkArea] = hasWorkArea;
            presence[CCS_CampStructureKind.SawTable] = hasSawTable;
            presence[CCS_CampStructureKind.CharcoalKiln] = hasCharcoalKiln;
            presence[CCS_CampStructureKind.PrimitiveForge] = hasPrimitiveForge;
            bool hasHorsePresence = hasSubjectPosition
                && mountPresenceQuery != null
                && mountPresenceQuery(campCenter, radius);
            presence[CCS_CampStructureKind.Stable] = hasHorsePresence;
            bool hasLivestockStructure = hasSubjectPosition
                && ranchStructureProximityQuery != null
                && ranchStructureProximityQuery(campCenter, radius);
            presence[CCS_CampStructureKind.Livestock] = hasLivestockStructure;

            CCS_CampTier tier = activeProfile.CampTierProfile != null
                ? CCS_CampTierEvaluationUtility.EvaluateHighestTier(activeProfile.CampTierProfile, presence)
                : EvaluateLegacyTier(
                    hasShelter,
                    hasCampfire,
                    hasBedroll,
                    hasStorage,
                    hasWorkArea,
                    hasPrimitiveForge);

            bool ownsCamp = tier != CCS_CampTier.None;
            string ownerId = ownsCamp ? savedState.campOwnerId : string.Empty;
            if (ownsCamp && string.IsNullOrWhiteSpace(ownerId))
            {
                ownerId = DefaultCampOwnerId;
                savedState.campOwnerId = ownerId;
            }

            if (ownsCamp && savedState.campCreationTimeUtcTicks <= 0L)
            {
                savedState.campCreationTimeUtcTicks = DateTime.UtcNow.Ticks;
            }

            List<string> structuresPresent = BuildStructuresPresentList(
                hasShelter,
                hasCampfire,
                hasBedroll,
                hasStorage,
                hasWorkArea,
                hasSawTable,
                hasCharcoalKiln,
                hasPrimitiveForge,
                hasHorsePresence,
                hasLivestockStructure);

            savedState.campTier = (int)tier;
            savedState.hasShelter = hasShelter;
            savedState.hasCampfire = hasCampfire;
            savedState.hasBedroll = hasBedroll;
            savedState.hasStorage = hasStorage;
            savedState.hasWorkArea = hasWorkArea;
            savedState.hasSawTable = hasSawTable;
            savedState.hasCharcoalKiln = hasCharcoalKiln;
            savedState.hasPrimitiveForge = hasPrimitiveForge;
            savedState.ownsCamp = ownsCamp;
            savedState.campCenterX = campCenter.x;
            savedState.campCenterY = campCenter.y;
            savedState.campCenterZ = campCenter.z;
            savedState.landClaimId = landClaimIdQuery != null
                ? landClaimIdQuery.Invoke(campCenter) ?? string.Empty
                : savedState.landClaimId ?? string.Empty;
            savedState.structuresPresent = structuresPresent.ToArray();

            currentSnapshot = new CCS_CampSnapshot(
                tier,
                hasShelter,
                hasCampfire,
                hasBedroll,
                hasStorage,
                hasWorkArea,
                hasSawTable,
                hasCharcoalKiln,
                hasPrimitiveForge,
                ownsCamp,
                ownerId,
                savedState.campCreationTimeUtcTicks,
                campCenter,
                structuresPresent,
                savedState.landClaimId,
                BuildTierMessage(tier));
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

        private static CCS_CampTier EvaluateLegacyTier(
            bool hasShelter,
            bool hasCampfire,
            bool hasBedroll,
            bool hasStorage,
            bool hasWorkArea,
            bool hasPrimitiveForge)
        {
            if (!hasShelter || !hasCampfire || !hasBedroll)
            {
                return CCS_CampTier.None;
            }

            if (!hasStorage)
            {
                return CCS_CampTier.TemporaryCamp;
            }

            if (!hasWorkArea)
            {
                return CCS_CampTier.FrontierCamp;
            }

            if (!hasPrimitiveForge)
            {
                return CCS_CampTier.FrontierHomestead;
            }

            return CCS_CampTier.IndustrialHomestead;
        }

        private static List<string> BuildStructuresPresentList(
            bool hasShelter,
            bool hasCampfire,
            bool hasBedroll,
            bool hasStorage,
            bool hasWorkArea,
            bool hasSawTable,
            bool hasCharcoalKiln,
            bool hasPrimitiveForge,
            bool hasHorsePresence,
            bool hasLivestockStructure)
        {
            List<string> structures = new List<string>(10);
            if (hasShelter)
            {
                structures.Add(CCS_CampStructureKind.Shelter.ToString());
            }

            if (hasCampfire)
            {
                structures.Add(CCS_CampStructureKind.Campfire.ToString());
            }

            if (hasBedroll)
            {
                structures.Add(CCS_CampStructureKind.Bedroll.ToString());
            }

            if (hasStorage)
            {
                structures.Add(CCS_CampStructureKind.Storage.ToString());
            }

            if (hasWorkArea)
            {
                structures.Add(CCS_CampStructureKind.WorkArea.ToString());
            }

            if (hasSawTable)
            {
                structures.Add(CCS_CampStructureKind.SawTable.ToString());
            }

            if (hasCharcoalKiln)
            {
                structures.Add(CCS_CampStructureKind.CharcoalKiln.ToString());
            }

            if (hasPrimitiveForge)
            {
                structures.Add(CCS_CampStructureKind.PrimitiveForge.ToString());
            }

            if (hasHorsePresence)
            {
                structures.Add(CCS_CampStructureKind.Stable.ToString());
            }

            if (hasLivestockStructure)
            {
                structures.Add(CCS_CampStructureKind.Livestock.ToString());
            }

            return structures;
        }

        private static string BuildTierMessage(CCS_CampTier tier)
        {
            return tier switch
            {
                CCS_CampTier.TemporaryCamp => "Temporary frontier camp established.",
                CCS_CampTier.FrontierCamp => "Frontier camp established with storage.",
                CCS_CampTier.FrontierHomestead => "Frontier homestead established.",
                CCS_CampTier.IndustrialHomestead => "Industrial homestead established.",
                _ => "Camp requirements incomplete."
            };
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
                    && string.Equals(instance.PieceId, campfirePieceId, StringComparison.OrdinalIgnoreCase)
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

        private bool HasStorageInRadius(float radius)
        {
            if (storageProximityQuery != null)
            {
                return storageProximityQuery.Invoke(subjectPosition, radius);
            }

            return false;
        }

        private bool HasCampStructureInRadius(float radius, CCS_CampStructureKind structureKind)
        {
            return homesteadStructureService != null
                && homesteadStructureService.IsInitialized
                && homesteadStructureService.HasCampStructureInRadius(subjectPosition, radius, structureKind);
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
