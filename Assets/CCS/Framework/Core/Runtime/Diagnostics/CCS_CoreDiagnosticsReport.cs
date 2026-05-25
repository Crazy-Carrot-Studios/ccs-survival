using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_CoreDiagnosticsReport
// CATEGORY: Core / Runtime / Diagnostics
// PURPOSE: Aggregated read-only diagnostics snapshot for CCS Core runtime systems.
// PLACEMENT: Built on demand by CCS_RuntimeHost. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No gameplay data. No per-frame allocation. Manual request only.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_CoreDiagnosticsReport
    {
        #region Properties

        public bool IsRuntimeHostInitialized { get; }

        public bool IsRuntimeHostShutdown { get; }

        public int RegisteredModuleCount { get; }

        public IReadOnlyList<CCS_ModuleDiagnosticsInfo> Modules { get; }

        public CCS_ServiceDiagnosticsInfo Services { get; }

        public CCS_UpdateLoopDiagnosticsInfo UpdateLoop { get; }

        public int EventSubscriptionCount { get; }

        public int BootstrapInstallerCount { get; }

        #endregion

        #region Public Methods

        public CCS_CoreDiagnosticsReport(
            bool isRuntimeHostInitialized,
            bool isRuntimeHostShutdown,
            int registeredModuleCount,
            IReadOnlyList<CCS_ModuleDiagnosticsInfo> modules,
            CCS_ServiceDiagnosticsInfo services,
            CCS_UpdateLoopDiagnosticsInfo updateLoop,
            int eventSubscriptionCount,
            int bootstrapInstallerCount)
        {
            IsRuntimeHostInitialized = isRuntimeHostInitialized;
            IsRuntimeHostShutdown = isRuntimeHostShutdown;
            RegisteredModuleCount = registeredModuleCount;
            Modules = modules ?? System.Array.Empty<CCS_ModuleDiagnosticsInfo>();
            Services = services;
            UpdateLoop = updateLoop;
            EventSubscriptionCount = eventSubscriptionCount;
            BootstrapInstallerCount = bootstrapInstallerCount;
        }

        #endregion
    }
}
