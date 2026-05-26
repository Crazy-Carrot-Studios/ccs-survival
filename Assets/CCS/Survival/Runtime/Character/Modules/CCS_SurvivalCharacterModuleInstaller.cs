using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalCharacterModuleInstaller
// CATEGORY: Survival / Runtime / Character / Modules
// PURPOSE: Survival-owned installer for the character module using survival foundation installer base.
// PLACEMENT: Invoked from CCS_SurvivalInstaller during survival bootstrap sequencing.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Does not create gameplay systems. Downward dependency: Survival → Core only.
// =============================================================================

namespace CCS.Survival
{
    public sealed class CCS_SurvivalCharacterModuleInstaller : CCS_SurvivalModuleInstallerBase
    {
        #region Public Methods

        public CCS_SurvivalCharacterModuleInstaller(bool enableDebugLogs)
            : base(
                new CCS_SurvivalCharacterModule(enableDebugLogs),
                CCS_SurvivalRuntimeConstants.CharacterInstallerLogCategory,
                enableDebugLogs)
        {
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnBeforeInstall(CCS_RuntimeHost runtimeHost)
        {
            LogSurvivalInstaller("Character module installer before install (skeleton).");
            return CCS_Result.Success();
        }

        protected override CCS_Result OnAfterInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_SurvivalValidationResult moduleValidation = CCS_SurvivalModuleValidationUtility.ValidateModule(Module);
            if (!moduleValidation.IsSuccess)
            {
                LogSurvivalInstaller($"Character module validation failed: {moduleValidation.Message}");
                return moduleValidation.ToCoreResult();
            }

            LogSurvivalInstaller("Character module installer after install (skeleton).");
            return CCS_Result.Success();
        }

        #endregion
    }
}
