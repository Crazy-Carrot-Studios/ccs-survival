// =============================================================================
// SCRIPT: CCS_SurvivalTestRuntimeFlags
// CATEGORY: Survival / Runtime / Development / Testing
// PURPOSE: Dev-only runtime flags mirrored from CCS_SurvivalTestToggleProfile for lightweight checks.
// PLACEMENT: Static flags updated by profile apply or editor testing menu. Not persisted.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No gameplay automation yet. Future modules read flags without direct profile references.
// =============================================================================

namespace CCS.Survival.Development
{
    public static class CCS_SurvivalTestRuntimeFlags
    {
        #region Variables

        private static bool enableDevelopmentDiagnostics;
        private static bool enableTraversalRouteTests;
        private static bool enableSurvivalSimulationTests;
        private static bool enableInventorySmokeTests;

        #endregion

        #region Properties

        public static bool EnableDevelopmentDiagnostics => enableDevelopmentDiagnostics;

        public static bool EnableTraversalRouteTests => enableTraversalRouteTests;

        public static bool EnableSurvivalSimulationTests => enableSurvivalSimulationTests;

        public static bool EnableInventorySmokeTests => enableInventorySmokeTests;

        #endregion

        #region Public Methods

        public static void ApplyFromProfile(CCS_SurvivalTestToggleProfile profile)
        {
            if (profile == null)
            {
                Reset();
                return;
            }

            enableDevelopmentDiagnostics = profile.EnableDevelopmentDiagnostics;
            enableTraversalRouteTests = profile.EnableTraversalRouteTests;
            enableSurvivalSimulationTests = profile.EnableSurvivalSimulationTests;
            enableInventorySmokeTests = profile.EnableInventorySmokeTests;
        }

        public static void Reset()
        {
            enableDevelopmentDiagnostics = false;
            enableTraversalRouteTests = false;
            enableSurvivalSimulationTests = false;
            enableInventorySmokeTests = false;
        }

        #endregion
    }
}
