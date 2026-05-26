using CCS.Core;

// =============================================================================
// SCRIPT: CCS_ISurvivalService
// CATEGORY: Survival / Runtime / Foundation / Services
// PURPOSE: Marker contract for future survival-owned services registered on CCS_ServiceRegistry.
// PLACEMENT: Runtime interface. Not attached to GameObjects. No methods at foundation layer.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Distinguishes Survival services from Core-only services. No gameplay behavior.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_ISurvivalService : CCS_IService
    {
    }
}
