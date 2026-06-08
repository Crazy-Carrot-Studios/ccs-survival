// =============================================================================
// SCRIPT: CCS_SurvivalCharacterDiagnostics
// CATEGORY: Survival / Runtime / Character / Diagnostics
// PURPOSE: Character-layer diagnostic aliases for survival foundation constants.
// PLACEMENT: Static utility. Not attached to GameObjects. No runtime mechanics.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Canonical values live in CCS_SurvivalRuntimeConstants. No gameplay behavior.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_SurvivalCharacterDiagnostics
    {
        public const string LogCategory = CCS_SurvivalRuntimeConstants.CharacterLogCategory;

        public const string InstallerLogCategory = CCS_SurvivalRuntimeConstants.CharacterInstallerLogCategory;

        public const string ModuleId = CCS_SurvivalRuntimeConstants.CharacterModuleId;
    }
}
