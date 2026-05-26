// =============================================================================
// SCRIPT: CCS_ISurvivalAuthority
// CATEGORY: Survival / Runtime / Character / Authority
// PURPOSE: Contract for future ownership authority of a survival character (identity, control, save, network signals).
// PLACEMENT: Implemented by future authority components/services. No implementation in this milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Future-facing ownership signals only. See CCS_SurvivalFrameworkFutureMarkers.MultiplayerAuthorityIntegration.
// =============================================================================

namespace CCS.Survival
{
    public interface CCS_ISurvivalAuthority
    {
        #region Properties

        // Stable save/network identity key for this authority (not a scene object or asset path).
        string AuthorityId { get; }

        // Human-readable label for diagnostics and UI; not authoritative for save identity.
        string DisplayName { get; }

        // True when this process owns local decision-making for the authority (future local player / host).
        bool IsLocalAuthority { get; }

        // True when a human player is intended to drive intent for this authority (future input routing).
        bool IsPlayerControlled { get; }

        // True when authority metadata is ready for future network ownership handoff (no netcode dependency today).
        bool IsNetworkAuthorityReady { get; }

        #endregion
    }
}
