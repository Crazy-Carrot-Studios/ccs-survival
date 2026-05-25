// =============================================================================
// SCRIPT: CCS_IBootstrapInstaller
// CATEGORY: Core / Runtime / Systems / Bootstrap
// PURPOSE: Defines how services and systems install into the CCS runtime host.
// PLACEMENT: Runtime assembly contract. Implemented by module/core installers.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Interface only. No MonoBehaviour. No auto-run in this foundation phase.
// =============================================================================

namespace CCS.Core
{
    public interface CCS_IBootstrapInstaller
    {
        void Install(CCS_RuntimeHost runtimeHost);
    }
}
