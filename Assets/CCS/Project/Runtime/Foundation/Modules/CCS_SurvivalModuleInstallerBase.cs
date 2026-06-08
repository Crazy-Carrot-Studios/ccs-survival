using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalModuleInstallerBase
// CATEGORY: Survival / Runtime / Foundation / Modules
// PURPOSE: Abstract installer base for survival-owned modules with standardized log category behavior.
// PLACEMENT: Runtime assembly. Not attached to GameObjects. Inherit for survival module installers.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No gameplay systems created. Aligns with Core installer pipeline only.
// =============================================================================

namespace CCS.Project
{
    public abstract class CCS_SurvivalModuleInstallerBase : CCS_ModuleInstallerBase
    {
        #region Variables

        private readonly string survivalInstallerLogCategory;
        private readonly bool survivalInstallerDebugLogs;

        #endregion

        #region Public Methods

        protected CCS_SurvivalModuleInstallerBase(
            CCS_IModule module,
            string survivalInstallerLogCategory,
            bool enableDebugLogs = false)
            : base(module, enableDebugLogs)
        {
            this.survivalInstallerLogCategory = survivalInstallerLogCategory;
            survivalInstallerDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Protected Methods

        protected override string GetLogCategory()
        {
            return survivalInstallerLogCategory;
        }

        protected void LogSurvivalInstaller(string message)
        {
            CCS_Logger.Log(survivalInstallerLogCategory, message, survivalInstallerDebugLogs);
        }

        #endregion
    }
}
