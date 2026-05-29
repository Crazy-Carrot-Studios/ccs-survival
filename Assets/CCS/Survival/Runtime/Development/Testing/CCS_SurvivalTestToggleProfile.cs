using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalTestToggleProfile
// CATEGORY: Survival / Runtime / Development / Testing
// PURPOSE: ScriptableObject dev-only test toggles for future automated and manual validation runs.
// PLACEMENT: Assets/CCS/Survival/Settings/Development/Testing/ (future). Editor/dev builds only.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No gameplay automation in 0.3.6. Prepares traversal, simulation, and inventory test flags.
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

        [Tooltip("Reserved for future traversal route validation.")]
        [SerializeField] private bool enableTraversalRouteTests;

        [Tooltip("Reserved for future survival simulation validation.")]
        [SerializeField] private bool enableSurvivalSimulationTests;

        [Tooltip("Reserved for future inventory smoke validation.")]
        [SerializeField] private bool enableInventorySmokeTests;

        #endregion

        #region Properties

        public bool EnableDevelopmentDiagnostics => enableDevelopmentDiagnostics;

        public bool EnableTraversalRouteTests => enableTraversalRouteTests;

        public bool EnableSurvivalSimulationTests => enableSurvivalSimulationTests;

        public bool EnableInventorySmokeTests => enableInventorySmokeTests;

        #endregion

        #region Public Methods

        public void ApplyToRuntimeFlags()
        {
            CCS_SurvivalTestRuntimeFlags.ApplyFromProfile(this);
        }

        #endregion
    }
}
