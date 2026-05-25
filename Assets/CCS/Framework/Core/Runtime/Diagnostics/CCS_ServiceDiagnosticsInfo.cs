using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_ServiceDiagnosticsInfo
// CATEGORY: Core / Runtime / Diagnostics
// PURPOSE: Read-only snapshot of service registry state for diagnostics reports.
// PLACEMENT: Runtime data type. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Allocated only when diagnostics are manually requested.
// =============================================================================

namespace CCS.Core
{
    public readonly struct CCS_ServiceDiagnosticsInfo
    {
        #region Properties

        public int RegisteredServiceCount { get; }

        public IReadOnlyList<string> ServiceTypeNames { get; }

        #endregion

        #region Public Methods

        public CCS_ServiceDiagnosticsInfo(int registeredServiceCount, IReadOnlyList<string> serviceTypeNames)
        {
            RegisteredServiceCount = registeredServiceCount;
            ServiceTypeNames = serviceTypeNames ?? System.Array.Empty<string>();
        }

        #endregion
    }
}
