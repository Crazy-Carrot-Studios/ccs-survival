// =============================================================================
// SCRIPT: CCS_BuildingEventArgs
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event payload for building definition registration and state changes.
// PLACEMENT: Passed to building event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes registered definition count for HUD refresh.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingEventArgs
    {
        #region Public Methods

        public CCS_BuildingEventArgs(int registeredDefinitionCount, string message)
        {
            RegisteredDefinitionCount = registeredDefinitionCount < 0 ? 0 : registeredDefinitionCount;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public int RegisteredDefinitionCount { get; }

        public string Message { get; }

        #endregion
    }
}
