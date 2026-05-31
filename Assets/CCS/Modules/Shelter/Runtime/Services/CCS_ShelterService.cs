using System.Collections.Generic;
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
// NOTES: No building placement, weather mutation, or survival stat mutation in 0.7.5.
// =============================================================================

namespace CCS.Modules.Shelter
{
    public sealed class CCS_ShelterService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_ShelterService]";

        #region Variables

        private readonly CCS_ShelterState shelterState = new CCS_ShelterState();
        private readonly List<CCS_ShelterVolume> registeredVolumes = new List<CCS_ShelterVolume>();

        private CCS_ShelterProfile activeProfile;
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

        public bool IsSheltered => shelterState.IsSheltered;

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
            shelterState.Clear();
            isInitialized = true;
            RaiseShelterChanged("Shelter service initialized.");
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

            if (shelterState.IsSheltered
                && shelterState.ActiveShelterId == shelterVolume.ShelterId)
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

            return EnterShelter(
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

            return EnterShelter(
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

            bool wasSheltered = shelterState.IsSheltered;
            shelterState.ApplyShelter(
                shelterId,
                wetnessProtection,
                exposureProtection,
                temperatureProtection,
                protectionMultiplier);

            CCS_ShelterEventArgs eventArgs = CreateEventArgs(message ?? $"Entered shelter '{shelterId}'.");

            if (!wasSheltered)
            {
                ShelterEntered?.Invoke(eventArgs);
            }

            ShelterChanged?.Invoke(eventArgs);
            return true;
        }

        public bool ExitShelter(string message = null)
        {
            if (!EnsureInitialized() || !shelterState.IsSheltered)
            {
                return false;
            }

            shelterState.Clear();
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

            return shelterState.CreateSnapshot();
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
                activeShelterId = shelterState.ActiveShelterId,
                isSheltered = shelterState.IsSheltered,
                wetnessProtection = shelterState.WetnessProtection,
                exposureProtection = shelterState.ExposureProtection,
                temperatureProtection = shelterState.TemperatureProtection,
                protectionMultiplier = shelterState.ProtectionMultiplier
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
                ExitShelter("Shelter restore cleared state.");
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

            if (!saveData.isSheltered)
            {
                ExitShelter("Shelter restore applied unsheltered state.");
                return;
            }

            EnterShelter(
                saveData.activeShelterId,
                saveData.wetnessProtection,
                saveData.exposureProtection,
                saveData.temperatureProtection,
                saveData.protectionMultiplier,
                "Shelter restored from save.");
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            if (isInitialized && activeProfile != null)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
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
