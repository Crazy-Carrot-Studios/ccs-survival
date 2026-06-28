// =============================================================================
// SCRIPT: CCS_ProjectAudioConstants
// CATEGORY: Project / Runtime / Audio
// PURPOSE: Paths and defaults for project-owned ambience and recording audio.
// PLACEMENT: Static constants. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Plays continuously on hosting/mode-select; Master Test gameplay has no ambient music.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_ProjectAudioConstants
    {
        public const string AmbienceRootPath = "Assets/CCS/Project/Audio/Ambience";

        public const string WesternGame2ClipPath = AmbienceRootPath + "/CCS Western Game 2.mp3";

        public const string WesternTheme7ClipPath = AmbienceRootPath + "/CCS_Western_Theme 7.mp3";

        public const string MultiplayerHostingScenePath =
            "Assets/CCS/Scenes/Network/SCN_CCS_MultiplayerHosting.unity";

        public const string HostingAmbientAudioObjectName = "CCS_HostingAmbientAudio";

        public const string MasterTestAmbientAudioObjectName = "CCS_AmbientAudio";

        public const string MasterTestTestingManagerObjectName = "CCS_TestingManager";

        public const float MasterTestRecordingPlaylistDefaultVolume = 0.10f;

        public const float HostingAmbientPlaylistDefaultVolume = 0.10f;

        public const float MasterTestRecordingPlaylistMaxValidatedVolume = 0.12f;
    }
}
