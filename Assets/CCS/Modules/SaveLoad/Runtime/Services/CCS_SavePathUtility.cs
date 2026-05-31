using System.IO;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SavePathUtility
// CATEGORY: Modules / SaveLoad / Runtime / Services
// PURPOSE: Resolves persistent save directory and file paths.
// PLACEMENT: Used by CCS_SaveLoadService for JSON read/write operations.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No encryption or cloud sync in 0.6.0 foundation.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public static class CCS_SavePathUtility
    {
        private const string SaveRootFolderName = "CCS_Survival";
        private const string SaveSubFolderName = "Saves";
        private const string SaveFileExtension = ".json";

        #region Public Methods

        public static string GetSaveRootDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SaveRootFolderName, SaveSubFolderName);
        }

        public static string GetSaveFilePath(string slotId)
        {
            string sanitizedSlotId = SanitizeSlotId(slotId);
            return Path.Combine(GetSaveRootDirectory(), sanitizedSlotId + SaveFileExtension);
        }

        public static bool EnsureSaveDirectoryExists()
        {
            string saveRootDirectory = GetSaveRootDirectory();
            if (Directory.Exists(saveRootDirectory))
            {
                return true;
            }

            Directory.CreateDirectory(saveRootDirectory);
            return Directory.Exists(saveRootDirectory);
        }

        public static string SanitizeSlotId(string slotId)
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                return string.Empty;
            }

            string trimmed = slotId.Trim();
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                trimmed = trimmed.Replace(invalidCharacter, '_');
            }

            return trimmed;
        }

        public static string GetShortDisplayPath(int maxLength = 52)
        {
            string fullPath = GetSaveRootDirectory();
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return string.Empty;
            }

            if (fullPath.Length <= maxLength)
            {
                return fullPath;
            }

            int tailLength = maxLength - 3;
            if (tailLength < 1)
            {
                return fullPath;
            }

            return "..." + fullPath.Substring(fullPath.Length - tailLength);
        }

        #endregion
    }
}
