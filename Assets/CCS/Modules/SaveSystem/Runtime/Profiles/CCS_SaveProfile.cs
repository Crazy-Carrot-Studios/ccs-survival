using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveProfile
// CATEGORY: Modules / SaveSystem / Runtime / Profiles
// PURPOSE: Tuning profile for unified survival save file rules and auto-save.
// PLACEMENT: Assets/CCS/Survival/Profiles/SaveSystem/CCS_DefaultSaveProfile.asset
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Persists to Application.persistentDataPath/CCS_Survival_Save.json at 1.0.1.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    [CreateAssetMenu(
        fileName = "CCS_SaveProfile",
        menuName = "CCS/Survival/Save System/Save Profile")]
    public sealed class CCS_SaveProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Save File")]
        [Tooltip("JSON file name written under Application.persistentDataPath.")]
        [SerializeField] private string saveFileName = "CCS_Survival_Save.json";

        [Header("Auto Save")]
        [Tooltip("When enabled, CCS_SaveService writes saves on a timer.")]
        [SerializeField] private bool autoSaveEnabled;

        [Tooltip("Seconds between automatic saves when auto save is enabled.")]
        [SerializeField] private float autoSaveIntervalSeconds = 120f;

        [Header("Diagnostics")]
        [Tooltip("Emit categorized save/load debug logs.")]
        [SerializeField] private bool enableDebugLogging;

        #endregion

        #region Properties

        public string SaveFileName => saveFileName;

        public bool AutoSaveEnabled => autoSaveEnabled;

        public float AutoSaveIntervalSeconds => autoSaveIntervalSeconds;

        public bool EnableDebugLogging => enableDebugLogging;

        #endregion
    }
}
