// =============================================================================
// SCRIPT: CCS_ISystem
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Base contract for future CCS framework systems.
// PLACEMENT: Runtime assembly contract. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No implementation. Compiled by CCS.Core.Runtime.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_ISystem
    {
        bool IsInitialized { get; }

        void Initialize();

        void Shutdown();
    }
}
