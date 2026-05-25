using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BootstrapRunner
// CATEGORY: Core / Runtime / Systems / Bootstrap
// PURPOSE: Orchestrates bootstrap installers into the CCS runtime host.
// PLACEMENT: Owned by CCS_RuntimeHost. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No auto-run yet. No reflection or scene discovery. No UnityEditor.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_BootstrapRunner
    {
        private const string LogCategory = "Bootstrap Runner";

        #region Variables

        private readonly List<CCS_IBootstrapInstaller> bootstrapInstallers;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_BootstrapRunner()
            : this(false)
        {
        }

        public CCS_BootstrapRunner(bool enableDebugLogs)
        {
            bootstrapInstallers = new List<CCS_IBootstrapInstaller>();
            this.enableDebugLogs = enableDebugLogs;
        }

        public bool RegisterInstaller(CCS_IBootstrapInstaller installer)
        {
            if (!CCS_Validation.IsObjectValid(installer))
            {
                return false;
            }

            if (bootstrapInstallers.Contains(installer))
            {
                return false;
            }

            bootstrapInstallers.Add(installer);
            CCS_Logger.Log(LogCategory, "Registered bootstrap installer.", enableDebugLogs);
            return true;
        }

        public bool UnregisterInstaller(CCS_IBootstrapInstaller installer)
        {
            if (!bootstrapInstallers.Remove(installer))
            {
                return false;
            }

            CCS_Logger.Log(LogCategory, "Unregistered bootstrap installer.", enableDebugLogs);
            return true;
        }

        public void Run(CCS_RuntimeHost runtimeHost)
        {
            if (!CCS_Validation.IsObjectValid(runtimeHost))
            {
                return;
            }

            for (int index = 0; index < bootstrapInstallers.Count; index++)
            {
                bootstrapInstallers[index].Install(runtimeHost);
            }

            CCS_Logger.Log(LogCategory, "Bootstrap installers executed.", enableDebugLogs);
        }

        public void Clear()
        {
            bootstrapInstallers.Clear();
            CCS_Logger.Log(LogCategory, "Cleared bootstrap installers.", enableDebugLogs);
        }

        public int GetInstallerCount()
        {
            return bootstrapInstallers.Count;
        }

        #endregion
    }
}
