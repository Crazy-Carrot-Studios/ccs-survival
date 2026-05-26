// =============================================================================
// SCRIPT: CCS_SurvivalFrameworkFutureMarkers
// CATEGORY: Survival / Runtime / Foundation / Diagnostics
// PURPOSE: Descriptive FUTURE integration markers for contributors (no placeholder systems).
// PLACEMENT: Reference constants for documentation and intentional TODO comments. Not executed at runtime.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Quality gate 0.3.5. Do not implement gameplay or netcode in foundation assemblies from these markers alone.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalFrameworkFutureMarkers
    {
        public const string MultiplayerAuthorityIntegration =
            "FUTURE: Multiplayer authority handoff implements CCS_ISurvivalAuthority in gameplay assemblies without netcode references in CCS.Survival.Runtime foundation.";

        public const string SaveSerializationAdapters =
            "FUTURE: Save serialization adapters map authority/profile/module IDs to persisted blobs; never persist Unity asset paths or scene object references.";

        public const string AvatarPossessionSystems =
            "FUTURE: Avatar possession binds CCS_ISurvivalAvatar to CCS_ISurvivalAuthority at runtime using stable IDs, not Transform or GameObject names.";

        public const string ProfileLoadingAbstraction =
            "FUTURE: Profile loading abstraction reads CCS_SurvivalProfileBase assets from explicit references or content pipelines (not Resources/Addressables in foundation).";

        public const string GameplayModuleRegistrationExpansion =
            "FUTURE: Additional gameplay modules register through CCS_SurvivalInstaller (or successors) with explicit install order and validation.";

        public const string StreamingWorldPartition =
            "FUTURE: Streaming/world-partition scenes must preserve one composition root per loaded survival context; scene names remain non-authoritative for save identity.";

        public const string FeatureDiagnosticsExtension =
            "FUTURE: Feature diagnostics extend CCS_SurvivalDiagnostics patterns with feature-owned log categories; keep Core diagnostics disabled in survival scenes.";
    }
}
