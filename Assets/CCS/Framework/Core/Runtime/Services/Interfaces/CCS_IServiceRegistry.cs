// =============================================================================
// SCRIPT: CCS_IServiceRegistry
// CATEGORY: Core / Runtime / Services
// PURPOSE: Contract for registering and resolving CCS services by interface type.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No static global instance. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IServiceRegistry
    {
        bool RegisterService<TService>(TService service) where TService : class, CCS_IService;

        bool UnregisterService<TService>() where TService : class, CCS_IService;

        bool TryGetService<TService>(out TService service) where TService : class, CCS_IService;

        bool HasService<TService>() where TService : class, CCS_IService;

        void Clear();
    }
}
