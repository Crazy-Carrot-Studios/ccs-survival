using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalModuleBase
// CATEGORY: Survival / Runtime / Foundation / Modules
// PURPOSE: Abstract base for survival-owned modules with shared log and metadata conventions.
// PLACEMENT: Runtime assembly. Not attached to GameObjects. Inherit for survival module types.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No service or updatable registration. Downward dependency: Survival → Core only.
// =============================================================================

namespace CCS.Project
{
    public abstract class CCS_SurvivalModuleBase : CCS_ModuleBase
    {
        #region Variables

        private readonly bool survivalDebugLogs;

        #endregion

        #region Public Methods

        protected CCS_SurvivalModuleBase(
            CCS_ModuleMetadata metadata,
            string survivalLogCategory,
            bool enableDebugLogs = false)
            : base(metadata, enableDebugLogs)
        {
            SurvivalLogCategory = survivalLogCategory;
            survivalDebugLogs = enableDebugLogs;
        }

        #endregion

        #region Properties

        protected string SurvivalLogCategory { get; }

        #endregion

        #region Protected Methods

        protected void LogSurvival(string message)
        {
            CCS_Logger.Log(SurvivalLogCategory, message, survivalDebugLogs);
        }

        #endregion
    }
}
