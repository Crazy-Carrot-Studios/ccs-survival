// =============================================================================
// SCRIPT: CCS_SurvivalCharacterDiagnostics
// CATEGORY: Survival / Runtime / Character / Diagnostics
// PURPOSE: Central diagnostic labels and module identity constants for the survival character layer.
// PLACEMENT: Static utility. Not attached to GameObjects. No runtime mechanics.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No player controller, movement, or authority implementation at milestone 0.3.0.
// =============================================================================

namespace CCS.Survival.Character
{
    public static class CCS_SurvivalCharacterDiagnostics
    {
        public const string LogCategory = "Survival Character";

        public const string InstallerLogCategory = "Survival Character Installer";

        public const string ModuleId = "ccs.survival.character";
    }
}
