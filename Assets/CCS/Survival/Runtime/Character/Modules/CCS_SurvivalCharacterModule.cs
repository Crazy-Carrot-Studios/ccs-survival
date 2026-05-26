using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalCharacterModule
// CATEGORY: Survival / Runtime / Character / Modules
// PURPOSE: Survival-owned character-layer module identity using survival foundation module base.
// PLACEMENT: Installed by CCS_SurvivalCharacterModuleInstaller via survival bootstrap sequencing.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Skeleton only. No movement, attributes, inventory, combat, AI, save, or multiplayer behavior.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalCharacterModule : CCS_SurvivalModuleBase
    {
        #region Public Methods

        public CCS_SurvivalCharacterModule(bool enableDebugLogs)
            : base(
                new CCS_ModuleMetadata(
                    CCS_SurvivalRuntimeConstants.CharacterModuleId,
                    "CCS Survival Character Module",
                    "0.3.0",
                    "Survival character-layer module identity skeleton."),
                CCS_SurvivalRuntimeConstants.CharacterLogCategory,
                enableDebugLogs)
        {
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnInitialize()
        {
            LogSurvival("Character module initialize hook (skeleton).");
            return CCS_Result.Success();
        }

        protected override CCS_Result OnInstall(CCS_RuntimeHost runtimeHost)
        {
            LogSurvival("Character module install hook (skeleton). No gameplay systems registered.");
            return CCS_Result.Success();
        }

        protected override CCS_Result OnUninstall(CCS_RuntimeHost runtimeHost)
        {
            LogSurvival("Character module uninstall hook (skeleton).");
            return CCS_Result.Success();
        }

        #endregion
    }
}
