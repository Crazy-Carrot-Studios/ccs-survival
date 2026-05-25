using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalCharacterModule
// CATEGORY: Survival / Runtime / Character / Modules
// PURPOSE: Survival-owned character-layer module identity using Core module lifecycle contracts.
// PLACEMENT: Installed by CCS_SurvivalCharacterModuleInstaller via survival bootstrap sequencing.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Skeleton only. No movement, attributes, inventory, combat, AI, save, or multiplayer behavior.
// =============================================================================

namespace CCS.Survival.Character
{
    public sealed class CCS_SurvivalCharacterModule : CCS_ModuleBase
    {
        #region Variables

        private readonly bool characterDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SurvivalCharacterModule(bool enableDebugLogs)
            : base(
                new CCS_ModuleMetadata(
                    CCS_SurvivalCharacterDiagnostics.ModuleId,
                    "CCS Survival Character Module",
                    "0.3.0",
                    "Survival character-layer module identity skeleton."),
                enableDebugLogs)
        {
            characterDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnInitialize()
        {
            CCS_Logger.Log(
                CCS_SurvivalCharacterDiagnostics.LogCategory,
                "Character module initialize hook (skeleton).",
                characterDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(
                CCS_SurvivalCharacterDiagnostics.LogCategory,
                "Character module install hook (skeleton). No gameplay systems registered.",
                characterDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnUninstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(
                CCS_SurvivalCharacterDiagnostics.LogCategory,
                "Character module uninstall hook (skeleton).",
                characterDebugLogs);
            return CCS_Result.Success();
        }

        #endregion
    }
}
