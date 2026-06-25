using System.Collections;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AmbientAudioPlaylist
// CATEGORY: Project / Runtime / Audio
// PURPOSE: Sequential 2D ambience playlist with fade in/out for recording scenes.
// PLACEMENT: CCS_AmbientAudio on SCN_CCS_CharacterController_MasterTest.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Plays clips in order and loops. Controlled by CCS_MasterTestSceneTestingManager.
// =============================================================================

namespace CCS.Project
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class CCS_AmbientAudioPlaylist : MonoBehaviour
    {
        #region Variables

        [SerializeField] private AudioClip[] playlist;
        [SerializeField, Range(0f, 1f)] private float volume = CCS_ProjectAudioConstants.MasterTestRecordingPlaylistDefaultVolume;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool repeatPlaylist = true;
        [SerializeField] private float fadeInSeconds = 1.5f;
        [SerializeField] private float fadeOutSeconds = 1.5f;
        [SerializeField] private float secondsBetweenClips = 0.25f;
        [SerializeField] private bool debugPlaylist;

        private AudioSource audioSource;
        private Coroutine playbackCoroutine;
        private Coroutine fadeCoroutine;
        private int currentClipIndex;
        private bool playlistEnabled;
        private bool warnedMissingClips;
        private string lastDebugClipName = string.Empty;

        #endregion

        #region Properties

        public bool IsPlaylistEnabled => playlistEnabled;

        public float Volume => volume;

        public float CurrentAudioSourceVolume => audioSource != null ? audioSource.volume : 0f;

        public bool IsAudioSourcePlaying => audioSource != null && audioSource.isPlaying;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            ConfigureAudioSource();
        }

        private void Start()
        {
            if (playOnStart && playlistEnabled)
            {
                PlayPlaylist();
            }
        }

        private void OnDisable()
        {
            StopPlaylistImmediate();
        }

        #endregion

        #region Public Methods

        public void SetPlaylistEnabled(bool enabled)
        {
            EnsureAudioSource();
            playlistEnabled = enabled;
            if (!enabled)
            {
                StopPlaylist();
                return;
            }

            audioSource.mute = false;
            audioSource.volume = volume;
            if (playbackCoroutine == null && !audioSource.isPlaying)
            {
                PlayPlaylist();
            }
        }

        public void PlayPlaylist()
        {
            EnsureAudioSource();
            if (!playlistEnabled)
            {
                return;
            }

            if (playbackCoroutine != null)
            {
                return;
            }

            if (!HasPlayableClips())
            {
                if (!warnedMissingClips)
                {
                    warnedMissingClips = true;
                    if (debugPlaylist)
                    {
                        Debug.LogWarning("[Ambient Audio Playlist] No playable clips assigned.", this);
                    }
                }

                return;
            }

            audioSource.mute = false;
            audioSource.volume = volume;
            playbackCoroutine = StartCoroutine(PlayPlaylistRoutine());
        }

        public void StopPlaylist()
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }

            if (audioSource == null)
            {
                return;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (audioSource.isPlaying && fadeOutSeconds > 0f)
            {
                fadeCoroutine = StartCoroutine(FadeVolumeRoutine(audioSource.volume, 0f, fadeOutSeconds, stopAfterFade: true));
            }
            else
            {
                StopPlaylistImmediate();
            }
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null && playlistEnabled)
            {
                audioSource.volume = volume;
            }
        }

        #endregion

        #region Private Methods

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                ConfigureAudioSource();
            }
        }

        private void ConfigureAudioSource()
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 128;
            audioSource.dopplerLevel = 0f;
            audioSource.reverbZoneMix = 0f;
            audioSource.mute = false;
            audioSource.volume = playlistEnabled ? volume : 0f;
        }

        private bool HasPlayableClips()
        {
            if (playlist == null || playlist.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < playlist.Length; i++)
            {
                if (playlist[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator PlayPlaylistRoutine()
        {
            while (playlistEnabled)
            {
                AudioClip clip = GetNextPlayableClip();
                if (clip == null)
                {
                    break;
                }

                LogClipStartOnce(clip);
                audioSource.clip = clip;
                audioSource.mute = false;
                audioSource.volume = volume;
                audioSource.Play();

                if (fadeInSeconds > 0f)
                {
                    yield return FadeVolumeRoutine(0f, volume, fadeInSeconds, stopAfterFade: false);
                }
                else
                {
                    audioSource.volume = volume;
                }

                float holdDuration = Mathf.Max(0f, clip.length - fadeOutSeconds);
                if (holdDuration > 0f)
                {
                    yield return WaitForSecondsOrDisable(holdDuration);
                }

                if (!playlistEnabled)
                {
                    break;
                }

                if (fadeOutSeconds > 0f)
                {
                    yield return FadeVolumeRoutine(audioSource.volume, 0f, fadeOutSeconds, stopAfterFade: true);
                }
                else
                {
                    StopPlaylistImmediate();
                }

                if (!playlistEnabled)
                {
                    break;
                }

                if (secondsBetweenClips > 0f)
                {
                    yield return WaitForSecondsOrDisable(secondsBetweenClips);
                }

                if (!repeatPlaylist)
                {
                    break;
                }
            }

            playbackCoroutine = null;
        }

        private void LogClipStartOnce(AudioClip clip)
        {
            if (!debugPlaylist || clip == null || clip.name == lastDebugClipName)
            {
                return;
            }

            lastDebugClipName = clip.name;
            int clipNumber = Mathf.Clamp(currentClipIndex, 1, Mathf.Max(1, playlist != null ? playlist.Length : 1));
            Debug.Log(
                "[Ambient Audio] Playing clip "
                + clipNumber
                + "/"
                + (playlist != null ? playlist.Length : 0)
                + ": "
                + clip.name
                + " volume="
                + volume.ToString("0.##"),
                this);
        }

        private AudioClip GetNextPlayableClip()
        {
            if (playlist == null || playlist.Length == 0)
            {
                return null;
            }

            int attempts = playlist.Length;
            while (attempts-- > 0)
            {
                if (currentClipIndex >= playlist.Length)
                {
                    currentClipIndex = 0;
                }

                AudioClip clip = playlist[currentClipIndex];
                currentClipIndex++;
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private IEnumerator WaitForSecondsOrDisable(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!playlistEnabled)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FadeVolumeRoutine(float fromVolume, float toVolume, float duration, bool stopAfterFade)
        {
            if (audioSource == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                audioSource.volume = toVolume;
                if (stopAfterFade)
                {
                    StopPlaylistImmediate();
                }

                fadeCoroutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (!playlistEnabled && toVolume > fromVolume)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                audioSource.volume = Mathf.Lerp(fromVolume, toVolume, t);
                yield return null;
            }

            audioSource.volume = toVolume;
            if (stopAfterFade)
            {
                StopPlaylistImmediate();
            }

            fadeCoroutine = null;
        }

        private void StopPlaylistImmediate()
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
            audioSource.volume = 0f;
            lastDebugClipName = string.Empty;
        }

        #endregion
    }
}
