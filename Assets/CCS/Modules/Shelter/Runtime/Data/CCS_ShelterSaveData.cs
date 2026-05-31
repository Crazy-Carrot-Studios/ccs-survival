// =============================================================================
// SCRIPT: CCS_ShelterSaveData
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Versioned save payload for global shelter state persistence.
// PLACEMENT: Serialized by CCS_ShelterService.CaptureState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Restored after weather and before environment in ModuleRestoreOrder.
// =============================================================================

namespace CCS.Modules.Shelter
{
    [System.Serializable]
    public sealed class CCS_ShelterSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public string activeShelterId = string.Empty;

        public bool isSheltered;

        public float wetnessProtection;

        public float exposureProtection;

        public float temperatureProtection;

        public float protectionMultiplier = 1f;

        #endregion
    }
}
