using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveLoadProfile
// CATEGORY: Modules / SaveLoad / Runtime / Profiles
// PURPOSE: Tuning profile for auto-save rules and save slot limits.
// PLACEMENT: Assets/CCS/Survival/Profiles/SaveLoad/ (project shell configuration).
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Auto-save execution deferred until future milestone wiring.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    [CreateAssetMenu(
        fileName = "CCS_SaveLoadProfile",
        menuName = "CCS/Survival/Save Load/Save Load Profile")]
    public sealed class CCS_SaveLoadProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Auto Save (Deferred)")]
        [Tooltip("When enabled, future systems may trigger timed auto saves.")]
        [SerializeField] private bool autoSaveEnabled;

        [Tooltip("Seconds between auto saves when auto save is enabled.")]
        [SerializeField] private float autoSaveIntervalSeconds = 300f;

        [Header("Slots")]
        [Tooltip("Maximum number of save slots allowed by profile rules.")]
        [SerializeField] private int maxSaveSlots = 10;

        #endregion

        #region Properties

        public bool AutoSaveEnabled => autoSaveEnabled;

        public float AutoSaveIntervalSeconds => autoSaveIntervalSeconds;

        public int MaxSaveSlots => maxSaveSlots;

        #endregion
    }
}
