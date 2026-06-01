using CCS.Survival;
using System.IO;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveValidationUtility
// CATEGORY: Modules / SaveSystem / Runtime / Validation
// PURPOSE: Runtime-safe validation for save profiles and save file paths.
// PLACEMENT: Used by CCS_SaveService and editor validators.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Editor-safe; no UnityEditor references.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public static class CCS_SaveValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SaveProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Save profile is null.");
            }

            if (string.IsNullOrWhiteSpace(profile.SaveFileName))
            {
                return CCS_SurvivalValidationResult.Fail("Save profile file name is empty.");
            }

            if (profile.AutoSaveEnabled && profile.AutoSaveIntervalSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Auto save interval must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Save profile validated.");
        }

        public static string ResolveSaveFilePath(CCS_SaveProfile profile)
        {
            string fileName = profile != null && !string.IsNullOrWhiteSpace(profile.SaveFileName)
                ? profile.SaveFileName
                : "CCS_Survival_Save.json";

            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static CCS_SurvivalValidationResult ValidateSaveFilePath(string saveFilePath)
        {
            if (string.IsNullOrWhiteSpace(saveFilePath))
            {
                return CCS_SurvivalValidationResult.Fail("Save file path is empty.");
            }

            string directory = Path.GetDirectoryName(saveFilePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return CCS_SurvivalValidationResult.Fail("Save file directory could not be resolved.");
            }

            return CCS_SurvivalValidationResult.Pass("Save file path is valid.");
        }

        public static bool TryRoundTripSerialize(CCS_SaveData saveData, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (saveData == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            string json = JsonUtility.ToJson(saveData, true);
            CCS_SaveData restored = JsonUtility.FromJson<CCS_SaveData>(json);
            if (restored == null)
            {
                errorMessage = "Round-trip deserialization returned null.";
                return false;
            }

            if (restored.saveVersion != saveData.saveVersion)
            {
                errorMessage = "Round-trip save version mismatch.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
