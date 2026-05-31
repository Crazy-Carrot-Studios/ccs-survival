using System;
using System.Collections.Generic;
using System.IO;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveLoadService
// CATEGORY: Modules / SaveLoad / Runtime / Services
// PURPOSE: Creates, loads, enumerates, and deletes JSON save slots via registered saveables.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Framework-only at 0.6.0. No world/building/combat persistence yet.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveLoadService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SaveLoadService]";

        #region Variables

        private readonly CCS_SaveableRegistry saveableRegistry = new CCS_SaveableRegistry();

        private CCS_SaveLoadProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event SaveStartedHandler SaveStarted;
        public event SaveCompletedHandler SaveCompleted;
        public event LoadStartedHandler LoadStarted;
        public event LoadCompletedHandler LoadCompleted;
        public event SaveFailedHandler SaveFailed;
        public event LoadFailedHandler LoadFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SaveLoadProfile ActiveProfile => activeProfile;

        public int RegisteredSaveableCount => saveableRegistry.RegisteredSaveableCount;

        #endregion

        #region Public Methods

        public bool IsSaveableRegistered(string saveableId)
        {
            return saveableRegistry.TryGetSaveable(saveableId, out _);
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            CCS_SavePathUtility.EnsureSaveDirectoryExists();
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SaveLoadProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SaveLoadValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            CCS_SavePathUtility.EnsureSaveDirectoryExists();
            isInitialized = true;
        }

        public bool RegisterSaveable(CCS_ISaveable saveable)
        {
            return saveableRegistry.RegisterSaveable(saveable);
        }

        public bool UnregisterSaveable(CCS_ISaveable saveable)
        {
            return saveableRegistry.UnregisterSaveable(saveable);
        }

        public IReadOnlyList<CCS_SaveSlotData> EnumerateSaveSlots()
        {
            List<CCS_SaveSlotData> slots = new List<CCS_SaveSlotData>();
            if (!EnsureReadyForQueries())
            {
                return slots;
            }

            string saveRootDirectory = CCS_SavePathUtility.GetSaveRootDirectory();
            if (!Directory.Exists(saveRootDirectory))
            {
                return slots;
            }

            string[] saveFiles = Directory.GetFiles(saveRootDirectory, "*.json");
            for (int i = 0; i < saveFiles.Length; i++)
            {
                string filePath = saveFiles[i];
                if (!TryReadSaveGameData(filePath, out CCS_SaveGameData saveGameData))
                {
                    continue;
                }

                string slotId = Path.GetFileNameWithoutExtension(filePath);
                slots.Add(new CCS_SaveSlotData(
                    slotId,
                    CCS_SaveMetadata.FromSaveGameData(saveGameData, slotId),
                    filePath));
            }

            slots.Sort(CompareSaveSlotsByTimestampDescending);
            return slots;
        }

        public CCS_SaveLoadResult TryCreateSave(string slotId)
        {
            if (!EnsureReadyForMutation(out string readinessMessage))
            {
                return FailSave(slotId, string.Empty, readinessMessage);
            }

            CCS_SurvivalValidationResult slotValidation = CCS_SaveLoadValidationUtility.ValidateSlotId(slotId);
            if (!slotValidation.IsSuccess)
            {
                return FailSave(slotId, string.Empty, slotValidation.Message);
            }

            string sanitizedSlotId = CCS_SavePathUtility.SanitizeSlotId(slotId);
            if (WouldExceedMaxSaveSlots(sanitizedSlotId))
            {
                return FailSave(sanitizedSlotId, string.Empty, "Maximum save slot count reached.");
            }

            RaiseSaveStarted(sanitizedSlotId, string.Empty, "Save started.");

            CCS_SaveGameData saveGameData = BuildSaveGameData(sanitizedSlotId);
            string filePath = CCS_SavePathUtility.GetSaveFilePath(sanitizedSlotId);

            if (!TryWriteSaveGameData(filePath, saveGameData, out string writeMessage))
            {
                return FailSave(sanitizedSlotId, saveGameData.SaveId, writeMessage);
            }

            CCS_SaveLoadResult success = CCS_SaveLoadResult.Success("Save completed.", sanitizedSlotId);
            RaiseSaveCompleted(sanitizedSlotId, saveGameData.SaveId, success.Message);
            return success;
        }

        public CCS_SaveLoadResult TryLoadSave(string slotId)
        {
            if (!EnsureReadyForMutation(out string readinessMessage))
            {
                return FailLoad(slotId, string.Empty, readinessMessage);
            }

            CCS_SurvivalValidationResult slotValidation = CCS_SaveLoadValidationUtility.ValidateSlotId(slotId);
            if (!slotValidation.IsSuccess)
            {
                return FailLoad(slotId, string.Empty, slotValidation.Message);
            }

            string sanitizedSlotId = CCS_SavePathUtility.SanitizeSlotId(slotId);
            string filePath = CCS_SavePathUtility.GetSaveFilePath(sanitizedSlotId);
            if (!File.Exists(filePath))
            {
                return FailLoad(sanitizedSlotId, string.Empty, "Save slot file was not found.");
            }

            RaiseLoadStarted(sanitizedSlotId, string.Empty, "Load started.");

            if (!TryReadSaveGameData(filePath, out CCS_SaveGameData saveGameData))
            {
                return FailLoad(sanitizedSlotId, string.Empty, "Save slot file could not be parsed.");
            }

            CCS_SaveLoadResult versionValidation = ValidateSaveVersions(saveGameData);
            if (!versionValidation.IsSuccess)
            {
                return FailLoad(sanitizedSlotId, saveGameData.SaveId, versionValidation.Message);
            }

            saveableRegistry.RestoreAllModuleStates(saveGameData.GetModuleDataDictionary());

            CCS_SaveLoadResult success = CCS_SaveLoadResult.Success("Load completed.", sanitizedSlotId);
            RaiseLoadCompleted(sanitizedSlotId, saveGameData.SaveId, success.Message);
            return success;
        }

        public CCS_SaveLoadResult TryDeleteSaveSlot(string slotId)
        {
            if (!EnsureReadyForMutation(out string readinessMessage))
            {
                return CCS_SaveLoadResult.Failure(readinessMessage, slotId);
            }

            CCS_SurvivalValidationResult slotValidation = CCS_SaveLoadValidationUtility.ValidateSlotId(slotId);
            if (!slotValidation.IsSuccess)
            {
                return CCS_SaveLoadResult.Failure(slotValidation.Message, slotId);
            }

            string sanitizedSlotId = CCS_SavePathUtility.SanitizeSlotId(slotId);
            string filePath = CCS_SavePathUtility.GetSaveFilePath(sanitizedSlotId);
            if (!File.Exists(filePath))
            {
                return CCS_SaveLoadResult.Failure("Save slot file was not found.", sanitizedSlotId);
            }

            try
            {
                File.Delete(filePath);
                return CCS_SaveLoadResult.Success("Save slot deleted.", sanitizedSlotId);
            }
            catch (Exception exception)
            {
                return CCS_SaveLoadResult.Failure($"Failed to delete save slot: {exception.Message}", sanitizedSlotId);
            }
        }

        #endregion

        #region Private Methods

        private bool EnsureReadyForQueries()
        {
            return isInitialized && activeProfile != null;
        }

        private bool EnsureReadyForMutation(out string message)
        {
            if (!isInitialized)
            {
                message = "Save/load service is not initialized.";
                return false;
            }

            if (activeProfile == null)
            {
                message = "Save/load profile is not assigned.";
                return false;
            }

            if (!CCS_SavePathUtility.EnsureSaveDirectoryExists())
            {
                message = "Save directory could not be created.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool WouldExceedMaxSaveSlots(string slotId)
        {
            if (activeProfile == null)
            {
                return true;
            }

            string filePath = CCS_SavePathUtility.GetSaveFilePath(slotId);
            if (File.Exists(filePath))
            {
                return false;
            }

            return EnumerateSaveSlots().Count >= activeProfile.MaxSaveSlots;
        }

        private CCS_SaveGameData BuildSaveGameData(string slotId)
        {
            CCS_SaveGameData saveGameData = new CCS_SaveGameData
            {
                SaveId = Guid.NewGuid().ToString("N"),
                SlotId = slotId,
                TimestampUtc = DateTime.UtcNow.ToString("o"),
                Version = Application.version,
                ProfileVersion = activeProfile != null ? activeProfile.ProfileVersion : string.Empty,
                PlayerDataJson = string.Empty
            };

            saveGameData.SetModuleDataDictionary(saveableRegistry.CaptureAllModuleStates());
            return saveGameData;
        }

        private static bool TryWriteSaveGameData(string filePath, CCS_SaveGameData saveGameData, out string message)
        {
            message = string.Empty;
            if (saveGameData == null)
            {
                message = "Save game data is null.";
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(saveGameData, prettyPrint: true);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception exception)
            {
                message = $"Failed to write save file: {exception.Message}";
                return false;
            }
        }

        private static bool TryReadSaveGameData(string filePath, out CCS_SaveGameData saveGameData)
        {
            saveGameData = null;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                saveGameData = JsonUtility.FromJson<CCS_SaveGameData>(json);
                return saveGameData != null;
            }
            catch
            {
                saveGameData = null;
                return false;
            }
        }

        private CCS_SaveLoadResult ValidateSaveVersions(CCS_SaveGameData saveGameData)
        {
            if (saveGameData == null)
            {
                return CCS_SaveLoadResult.Failure("Save game data is null.");
            }

            if (string.IsNullOrWhiteSpace(saveGameData.Version))
            {
                return CCS_SaveLoadResult.Failure("Save file is missing version metadata.");
            }

            if (activeProfile != null
                && !string.IsNullOrWhiteSpace(saveGameData.ProfileVersion)
                && !string.Equals(saveGameData.ProfileVersion, activeProfile.ProfileVersion, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"{LogPrefix} Save profile version {saveGameData.ProfileVersion} differs from active profile {activeProfile.ProfileVersion}.");
            }

            return CCS_SaveLoadResult.Success("Save version validated.");
        }

        private static int CompareSaveSlotsByTimestampDescending(CCS_SaveSlotData left, CCS_SaveSlotData right)
        {
            DateTime leftTimestamp = ParseTimestamp(left?.Metadata?.timestampUtc);
            DateTime rightTimestamp = ParseTimestamp(right?.Metadata?.timestampUtc);
            return rightTimestamp.CompareTo(leftTimestamp);
        }

        private static DateTime ParseTimestamp(string timestampUtc)
        {
            if (DateTime.TryParse(timestampUtc, out DateTime parsedTimestamp))
            {
                return parsedTimestamp;
            }

            return DateTime.MinValue;
        }

        private CCS_SaveLoadResult FailSave(string slotId, string saveId, string message)
        {
            CCS_SaveLoadResult failure = CCS_SaveLoadResult.Failure(message, slotId);
            RaiseSaveFailed(slotId, saveId, message);
            return failure;
        }

        private CCS_SaveLoadResult FailLoad(string slotId, string saveId, string message)
        {
            CCS_SaveLoadResult failure = CCS_SaveLoadResult.Failure(message, slotId);
            RaiseLoadFailed(slotId, saveId, message);
            return failure;
        }

        private void RaiseSaveStarted(string slotId, string saveId, string message)
        {
            SaveStarted?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        private void RaiseSaveCompleted(string slotId, string saveId, string message)
        {
            SaveCompleted?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        private void RaiseLoadStarted(string slotId, string saveId, string message)
        {
            LoadStarted?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        private void RaiseLoadCompleted(string slotId, string saveId, string message)
        {
            LoadCompleted?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        private void RaiseSaveFailed(string slotId, string saveId, string message)
        {
            SaveFailed?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        private void RaiseLoadFailed(string slotId, string saveId, string message)
        {
            LoadFailed?.Invoke(new CCS_SaveLoadEventArgs(slotId, saveId, message));
        }

        #endregion
    }
}
