using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalCharacterModuleInstaller
// CATEGORY: Survival / Runtime / Character / Modules
// PURPOSE: Survival-owned installer for the character module using Core module installer pattern.
// PLACEMENT: Invoked from CCS_SurvivalInstaller during survival bootstrap sequencing.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Does not create gameplay systems. Downward dependency: Survival → Core only.
// =============================================================================

namespace CCS.Survival.Character
{
    public sealed class CCS_SurvivalCharacterModuleInstaller : CCS_ModuleInstallerBase
    {
        #region Variables

        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_SurvivalCharacterModuleInstaller(bool enableDebugLogs)
            : base(new CCS_SurvivalCharacterModule(enableDebugLogs), enableDebugLogs)
        {
            this.enableDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnBeforeInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(
                CCS_SurvivalCharacterDiagnostics.InstallerLogCategory,
                "Character module installer before install (skeleton).",
                enableDebugLogs);
            return CCS_Result.Success();
        }

        protected override CCS_Result OnAfterInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_Logger.Log(
                CCS_SurvivalCharacterDiagnostics.InstallerLogCategory,
                "Character module installer after install (skeleton).",
                enableDebugLogs);
            return CCS_Result.Success();
        }

        protected override string GetLogCategory()
        {
            return CCS_SurvivalCharacterDiagnostics.InstallerLogCategory;
        }

        #endregion
    }
}
