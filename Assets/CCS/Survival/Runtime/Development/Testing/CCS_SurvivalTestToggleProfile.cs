using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalTestToggleProfile
// CATEGORY: Survival / Runtime / Development / Testing
// PURPOSE: ScriptableObject dev-only test toggles for future automated and manual validation runs.
// PLACEMENT: Assets/CCS/Survival/Settings/Development/Testing/ (future). Editor/dev builds only.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Reserved categories: Traversal, Simulation, Inventory, SaveLoad. No automation in 0.3.6.
// =============================================================================

namespace CCS.Survival.Development
{
    [CreateAssetMenu(
        fileName = "CCS_SurvivalTestToggleProfile",
        menuName = "CCS/Survival/Development/Test Toggle Profile")]
    public sealed class CCS_SurvivalTestToggleProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Development Toggles")]
        [Tooltip("When true, verbose development diagnostics may be emitted.")]
        [SerializeField] private bool enableDevelopmentDiagnostics;

        [Header("Traversal Tests (Reserved)")]
        [Tooltip("Reserved: automated traversal route validation under Runtime/Development/Testing/Traversal/.")]
        [SerializeField] private bool enableTraversalRouteTests;

        [Header("Survival Simulation Tests (Reserved)")]
        [Tooltip("Reserved: survival pressure simulation under Runtime/Development/Testing/Simulation/.")]
        [SerializeField] private bool enableSurvivalSimulationTests;

        [Header("Inventory Tests (Reserved)")]
        [Tooltip("Reserved: inventory smoke tests under Runtime/Development/Testing/Inventory/.")]
        [SerializeField] private bool enableInventorySmokeTests;

        [Header("Save/Load Tests (Reserved)")]
        [Tooltip("Reserved: save/load round-trip tests under Runtime/Development/Testing/SaveLoad/.")]
        [SerializeField] private bool enableSaveLoadTests;

        #endregion

        #region Properties

        public bool EnableDevelopmentDiagnostics => enableDevelopmentDiagnostics;

        public bool EnableTraversalRouteTests => enableTraversalRouteTests;

        public bool EnableSurvivalSimulationTests => enableSurvivalSimulationTests;

        public bool EnableInventorySmokeTests => enableInventorySmokeTests;

        public bool EnableSaveLoadTests => enableSaveLoadTests;

        #endregion

        #region Public Methods

        public void ApplyToRuntimeFlags()
        {
            CCS_SurvivalTestRuntimeFlags.ApplyFromProfile(this);
        }

        #endregion
    }
}
