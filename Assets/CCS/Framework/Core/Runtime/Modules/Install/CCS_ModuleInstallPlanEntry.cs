// =============================================================================
// SCRIPT: CCS_ModuleInstallPlanEntry
// CATEGORY: Core / Runtime / Modules / Install
// PURPOSE: Single ordered module installer entry for manual install plans.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No auto-discovery. Order is explicit and authoritative.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_ModuleInstallPlanEntry
    {
        #region Properties

        public CCS_IModuleInstaller Installer { get; }

        public string ModuleId { get; }

        #endregion

        #region Public Methods

        public CCS_ModuleInstallPlanEntry(CCS_IModuleInstaller installer)
        {
            Installer = installer;
            ModuleId = installer != null && installer.Module != null
                ? installer.Module.Metadata.ModuleId
                : string.Empty;
        }

        #endregion
    }
}
