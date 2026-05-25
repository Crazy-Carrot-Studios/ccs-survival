// =============================================================================
// SCRIPT: CCS_IService
// CATEGORY: Core / Runtime / Services
// PURPOSE: Base contract for future CCS framework services.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No implementation. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IService
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}
