// =============================================================================
// SCRIPT: CCS_EnvironmentSaveData
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Versioned save payload for global environment simulation state.
// PLACEMENT: Serialized by CCS_EnvironmentEffectsService.CaptureState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Restored after weather in ModuleRestoreOrder.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    [System.Serializable]
    public sealed class CCS_EnvironmentSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public float ambientTemperature;

        public float wetness;

        public float exposure;

        #endregion
    }
}
