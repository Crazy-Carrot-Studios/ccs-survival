using CCS.Core;

// =============================================================================
// SCRIPT: CCS_ISurvivalVitalsTestModeService
// CATEGORY: Survival / Runtime / Survival / Interfaces
// PURPOSE: Dev/test contract for traversal-driven vitals isolation on the bootstrap host.
// PLACEMENT: Implemented by CCS_SurvivalModule. Optional resolve from CCS_ServiceRegistry.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Prefer CCS_SurvivalTraversalValidationLifecycleEvent when avoiding service lookups.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_ISurvivalVitalsTestModeService : CCS_ISurvivalService
    {
        bool IsTraversalValidationActive { get; }

        bool IsTraversalVitalsIsolationActive { get; }

        void NotifyTraversalValidationActive(bool isActive);
    }
}
