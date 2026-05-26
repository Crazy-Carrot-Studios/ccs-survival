// =============================================================================
// SCRIPT: CCS_SurvivalRuntimeConstants
// CATEGORY: Survival / Runtime / Foundation / Diagnostics
// PURPOSE: Central survival-owned constants for module IDs, log categories, and diagnostic expectations.
// PLACEMENT: Static constants. Not attached to GameObjects. No runtime mechanics.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Feature-specific diagnostics may alias these values. No gameplay behavior.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalRuntimeConstants
    {
        public const string ModuleIdPrefix = "ccs.survival.";

        public const string CharacterModuleId = ModuleIdPrefix + "character";

        public const string SurvivalDiagnosticsLogCategory = "Survival Diagnostics";

        public const string SurvivalInstallerLogCategory = "Survival Installer";

        public const string SurvivalBootstrapLogCategory = "Survival Bootstrap";

        public const string CharacterLogCategory = "Survival Character";

        public const string CharacterInstallerLogCategory = "Survival Character Installer";

        public const int ExpectedSkeletonModuleCount = 1;
    }
}
