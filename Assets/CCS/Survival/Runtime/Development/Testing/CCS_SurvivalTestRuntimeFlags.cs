// =============================================================================
// SCRIPT: CCS_SurvivalTestRuntimeFlags
// CATEGORY: Survival / Runtime / Development / Testing
// PURPOSE: Dev-only runtime flags mirrored from CCS_SurvivalTestToggleProfile for lightweight checks.
// PLACEMENT: Static flags updated by profile apply or editor testing menu. Not persisted.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Categories: Traversal, Simulation, Inventory, SaveLoad. No automation in 0.3.6.
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
        private static bool enableSaveLoadTests;

        #endregion

        #region Properties

        public static bool EnableDevelopmentDiagnostics => enableDevelopmentDiagnostics;

        public static bool EnableTraversalRouteTests => enableTraversalRouteTests;

        public static bool EnableSurvivalSimulationTests => enableSurvivalSimulationTests;

        public static bool EnableInventorySmokeTests => enableInventorySmokeTests;

        public static bool EnableSaveLoadTests => enableSaveLoadTests;

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
            enableSaveLoadTests = profile.EnableSaveLoadTests;
        }

        public static void Reset()
        {
            enableDevelopmentDiagnostics = false;
            enableTraversalRouteTests = false;
            enableSurvivalSimulationTests = false;
            enableInventorySmokeTests = false;
            enableSaveLoadTests = false;
        }

        #endregion
    }
}
