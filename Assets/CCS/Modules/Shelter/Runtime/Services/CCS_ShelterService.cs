using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterService
// CATEGORY: Modules / Shelter / Runtime / Services
// PURPOSE: Authoritative local shelter protection state with events and save/load.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Volume shelter plus building contribution coverage in 0.8.5.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public sealed class CCS_ShelterService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_ShelterService]";
        private const float MaxWetnessProtection = 1f;
        private const float MaxExposureProtection = 1f;
        private const float MaxTemperatureProtection = 2f;

        #region Variables

        private readonly CCS_ShelterState volumeShelterState = new CCS_ShelterState();
        private readonly CCS_ShelterState buildingShelterState = new CCS_ShelterState();
        private readonly List<CCS_ShelterVolume> registeredVolumes = new List<CCS_ShelterVolume>();
        private readonly List<CCS_BuildingShelterContribution> buildingContributions =
            new List<CCS_BuildingShelterContribution>();

        private CCS_ShelterProfile activeProfile;
        private CCS_BuildingService buildingService;
        private Vector3 subjectPosition;
        private bool hasSubjectPosition;
        private bool isInitialized;

        #endregion

        #region Events

        public event ShelterEnteredHandler ShelterEntered;
        public event ShelterExitedHandler ShelterExited;
        public event ShelterChangedHandler ShelterChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_ShelterProfile ActiveProfile => activeProfile;

        public string SaveableId => CCS_SaveLoadSaveableIds.GlobalShelter;

        public bool IsSheltered => GetSnapshot().IsSheltered;

        public bool IsBuildingShelterActive => buildingShelterState.IsSheltered;

        public int BuildingShelterContributionCount => buildingContributions.Count;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_ShelterProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_ShelterValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            volumeShelterState.Clear();
            buildingShelterState.Clear();
            buildingContributions.Clear();
            isInitialized = true;
            RaiseShelterChanged("Shelter service initialized.");
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            UnbindBuildingService();
            buildingService = service;

            if (buildingService == null || !buildingService.IsInitialized)
            {
                return;
            }

            buildingService.BuildingShelterContributionsChanged += HandleBuildingContributionsChanged;
            SyncBuildingContributions();
        }

        public void SetSubjectPosition(Vector3 position)
        {
            subjectPosition = position;
            hasSubjectPosition = true;
            RecalculateBuildingShelter();
        }

        public void SetBuildingContributions(IReadOnlyList<CCS_BuildingShelterContribution> contributions)
        {
            buildingContributions.Clear();

            if (contributions != null)
            {
                for (int index = 0; index < contributions.Count; index++)
                {
                    CCS_BuildingShelterContribution contribution = contributions[index];
                    if (contribution != null)
                    {
                        buildingContributions.Add(contribution);
                    }
                }
            }

            RecalculateBuildingShelter();
        }

        public void RecalculateBuildingShelter()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            bool wasBuildingSheltered = buildingShelterState.IsSheltered;
            buildingShelterState.Clear();

            if (hasSubjectPosition && buildingContributions.Count > 0)
            {
                float bestWetnessProtection = 0f;
                float bestExposureProtection = 0f;
                float bestTemperatureProtection = 0f;
                string bestShelterId = string.Empty;

                for (int index = 0; index < buildingContributions.Count; index++)
                {
                    CCS_BuildingShelterContribution contribution = buildingContributions[index];
                    float coverageRadius = contribution.CoverageRadius;
                    if (coverageRadius <= 0f)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(subjectPosition, contribution.WorldPosition);
                    if (distance > coverageRadius)
                    {
                        continue;
                    }

                    if (contribution.WetnessProtection > bestWetnessProtection)
                    {
                        bestWetnessProtection = contribution.WetnessProtection;
                    }

                    if (contribution.ExposureProtection > bestExposureProtection)
                    {
                        bestExposureProtection = contribution.ExposureProtection;
                    }

                    if (contribution.TemperatureProtection > bestTemperatureProtection)
                    {
                        bestTemperatureProtection = contribution.TemperatureProtection;
                    }

                    if (string.IsNullOrWhiteSpace(bestShelterId))
                    {
                        bestShelterId = contribution.BuildingInstanceId;
                    }
                }

                if (bestWetnessProtection > 0f
                    || bestExposureProtection > 0f
                    || bestTemperatureProtection > 0f)
                {
                    buildingShelterState.ApplyShelter(
                        bestShelterId,
                        ClampWetnessProtection(bestWetnessProtection),
                        ClampExposureProtection(bestExposureProtection),
                        ClampTemperatureProtection(bestTemperatureProtection),
                        1f);
                }
            }

            if (buildingShelterState.IsSheltered != wasBuildingSheltered
                || buildingContributions.Count > 0)
            {
                RaiseShelterChanged("Building shelter protection recalculated.");
            }
        }

        public void RegisterVolume(CCS_ShelterVolume shelterVolume)
        {
            if (shelterVolume == null || registeredVolumes.Contains(shelterVolume))
            {
                return;
            }

            registeredVolumes.Add(shelterVolume);
        }

        public void UnregisterVolume(CCS_ShelterVolume shelterVolume)
        {
            if (shelterVolume == null)
            {
                return;
            }

            registeredVolumes.Remove(shelterVolume);

            if (volumeShelterState.IsSheltered
                && volumeShelterState.ActiveShelterId == shelterVolume.ShelterId)
            {
                ExitShelter("Active shelter volume unregistered.");
            }
        }

        public bool EnterShelter(CCS_ShelterVolume shelterVolume)
        {
            if (!EnsureInitialized() || shelterVolume == null)
            {
                return false;
            }

            return EnterVolumeShelter(
                shelterVolume.ShelterId,
                shelterVolume.WetnessProtection,
                shelterVolume.ExposureProtection,
                shelterVolume.TemperatureProtection,
                shelterVolume.ProtectionMultiplier,
                $"Entered shelter '{shelterVolume.DisplayName}'.");
        }

        public bool EnterShelterWithProfileDefaults(string shelterId, string message = null)
        {
            if (!EnsureInitialized() || activeProfile == null)
            {
                return false;
            }

            return EnterVolumeShelter(
                shelterId,
                activeProfile.DefaultWetnessProtection,
                activeProfile.DefaultExposureProtection,
                activeProfile.DefaultTemperatureProtection,
                activeProfile.DefaultProtectionMultiplier,
                message ?? $"Entered shelter '{shelterId}' using profile defaults.");
        }

        public bool EnterShelter(
            string shelterId,
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection,
            float protectionMultiplier,
            string message = null)
        {
            return EnterVolumeShelter(
                shelterId,
                wetnessProtection,
                exposureProtection,
                temperatureProtection,
                protectionMultiplier,
                message);
        }

        public bool ExitShelter(string message = null)
        {
            if (!EnsureInitialized() || !volumeShelterState.IsSheltered)
            {
                return false;
            }

            volumeShelterState.Clear();
            CCS_ShelterEventArgs eventArgs = CreateEventArgs(message ?? "Exited shelter.");
            ShelterExited?.Invoke(eventArgs);
            ShelterChanged?.Invoke(eventArgs);
            return true;
        }

        public CCS_ShelterSnapshot GetSnapshot()
        {
            if (!EnsureInitialized())
            {
                return CCS_ShelterSnapshot.Empty;
            }

            bool isSheltered = volumeShelterState.IsSheltered || buildingShelterState.IsSheltered;
            string activeShelterId = volumeShelterState.IsSheltered
                ? volumeShelterState.ActiveShelterId
                : buildingShelterState.ActiveShelterId;

            return new CCS_ShelterSnapshot(
                isSheltered,
                activeShelterId,
                Mathf.Max(volumeShelterState.WetnessProtection, buildingShelterState.WetnessProtection),
                Mathf.Max(volumeShelterState.ExposureProtection, buildingShelterState.ExposureProtection),
                Mathf.Max(volumeShelterState.TemperatureProtection, buildingShelterState.TemperatureProtection),
                volumeShelterState.IsSheltered
                    ? volumeShelterState.ProtectionMultiplier
                    : buildingShelterState.ProtectionMultiplier,
                buildingShelterState.IsSheltered,
                buildingContributions.Count);
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_ShelterSaveData());
            }

            CCS_ShelterSaveData saveData = new CCS_ShelterSaveData
            {
                saveDataVersion = CCS_ShelterSaveData.CurrentSaveDataVersion,
                activeShelterId = volumeShelterState.ActiveShelterId,
                isSheltered = volumeShelterState.IsSheltered,
                wetnessProtection = volumeShelterState.WetnessProtection,
                exposureProtection = volumeShelterState.ExposureProtection,
                temperatureProtection = volumeShelterState.TemperatureProtection,
                protectionMultiplier = volumeShelterState.ProtectionMultiplier
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreState(string stateJson)
        {
            if (!EnsureInitialized())
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because service is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                volumeShelterState.Clear();
                RecalculateBuildingShelter();
                RaiseShelterChanged("Shelter restore cleared state.");
                return;
            }

            CCS_ShelterSaveData saveData = JsonUtility.FromJson<CCS_ShelterSaveData>(stateJson);
            if (saveData == null)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because payload could not be parsed.");
                return;
            }

            if (saveData.saveDataVersion <= 0)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because saveDataVersion is missing.");
                return;
            }

            volumeShelterState.Clear();

            if (saveData.isSheltered)
            {
                EnterVolumeShelter(
                    saveData.activeShelterId,
                    saveData.wetnessProtection,
                    saveData.exposureProtection,
                    saveData.temperatureProtection,
                    saveData.protectionMultiplier,
                    "Shelter restored from save.",
                    raiseVolumeEnteredEvent: false);
            }

            SyncBuildingContributions();
            RecalculateBuildingShelter();
        }

        #endregion

        #region Private Methods

        private bool EnterVolumeShelter(
            string shelterId,
            float wetnessProtection,
            float exposureProtection,
            float temperatureProtection,
            float protectionMultiplier,
            string message,
            bool raiseVolumeEnteredEvent = true)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(shelterId))
            {
                return false;
            }

            CCS_SurvivalValidationResult validation = CCS_ShelterValidationUtility.ValidateProtectionValues(
                wetnessProtection,
                exposureProtection,
                protectionMultiplier);

            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} EnterShelter rejected: {validation.Message}");
                return false;
            }

            bool wasVolumeSheltered = volumeShelterState.IsSheltered;
            volumeShelterState.ApplyShelter(
                shelterId,
                wetnessProtection,
                exposureProtection,
                temperatureProtection,
                protectionMultiplier);

            CCS_ShelterEventArgs eventArgs = CreateEventArgs(message ?? $"Entered shelter '{shelterId}'.");

            if (raiseVolumeEnteredEvent && !wasVolumeSheltered)
            {
                ShelterEntered?.Invoke(eventArgs);
            }

            ShelterChanged?.Invoke(eventArgs);
            return true;
        }

        private void SyncBuildingContributions()
        {
            if (buildingService != null && buildingService.IsInitialized)
            {
                SetBuildingContributions(buildingService.GetShelterContributions());
                return;
            }

            SetBuildingContributions(System.Array.Empty<CCS_BuildingShelterContribution>());
        }

        private void HandleBuildingContributionsChanged(CCS_BuildingShelterContributionsChangedEventArgs eventArgs)
        {
            SyncBuildingContributions();
        }

        private void UnbindBuildingService()
        {
            if (buildingService == null)
            {
                return;
            }

            buildingService.BuildingShelterContributionsChanged -= HandleBuildingContributionsChanged;
            buildingService = null;
        }

        private bool EnsureInitialized()
        {
            if (isInitialized && activeProfile != null)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
        }

        private static float ClampWetnessProtection(float value)
        {
            return Mathf.Clamp(value, 0f, MaxWetnessProtection);
        }

        private static float ClampExposureProtection(float value)
        {
            return Mathf.Clamp(value, 0f, MaxExposureProtection);
        }

        private static float ClampTemperatureProtection(float value)
        {
            return Mathf.Clamp(value, 0f, MaxTemperatureProtection);
        }

        private CCS_ShelterEventArgs CreateEventArgs(string message)
        {
            return new CCS_ShelterEventArgs(GetSnapshot(), message);
        }

        private void RaiseShelterChanged(string message)
        {
            ShelterChanged?.Invoke(CreateEventArgs(message));
        }

        #endregion
    }
}
