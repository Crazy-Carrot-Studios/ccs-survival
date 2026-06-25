using System.Collections;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MasterTestSceneTestingManager
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Scene-level test switchboard for Master Test recording and debug toggles.
// PLACEMENT: CCS_TestingManager on SCN_CCS_CharacterController_MasterTest.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Test-scene-only. Not part of production gameplay systems.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [DefaultExecutionOrder(-10)]
    public sealed class CCS_MasterTestSceneTestingManager : MonoBehaviour
    {
        #region Variables

        [Header("Recording")]
        [Tooltip("Enables quiet background ambience for recording gameplay sections.")]
        [SerializeField] private bool enableRecordingAmbience = true;

        [Header("References")]
        [SerializeField] private CCS_AmbientAudioPlaylist ambientAudioPlaylist;

        [SerializeField] private bool applyOnStart = true;

        [SerializeField] private bool applyInEditorWhenChanged = true;

        [Header("Aiming Visual Tests")]
        [Tooltip("Pulls arm/hand toward reticle via Animation Rigging. Default OFF for stable FitTest pose.")]
        [SerializeField] private bool enableArmToReticleIK;

        [Tooltip("Rotates equipped gun toward reticle after hand fit is applied. Default OFF.")]
        [SerializeField] private bool enableVisualAimConvergence;

        [Tooltip("Hybrid camera center with clamped muzzle drift. Default for stable screen-safe reticle.")]
        [SerializeField] private CCS_AimReticleMode reticleMode =
            CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift;

        [SerializeField] private bool enableReticleClamp = true;

        [SerializeField] private float maxReticleDriftPixels =
            CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault;

        [Tooltip("Blends upper-body aim pose by camera pitch while third-person aiming.")]
        [SerializeField] private bool enableThirdPersonAimPitchBlend = true;

        [Tooltip("Draws camera/muzzle aim debug rays while aiming.")]
        [SerializeField] private bool enableAimDebugRays;

        [Header("Debug")]
        [SerializeField] private bool debugTestingManager;

        private bool appliedAimVisualSettingsToPlayer;

        #endregion

        #region Properties

        public bool EnableRecordingAmbience => enableRecordingAmbience;

        public bool EnableArmToReticleIk => enableArmToReticleIK;

        public bool EnableVisualAimConvergence => enableVisualAimConvergence;

        public CCS_AimReticleMode ReticleMode => reticleMode;

        public bool EnableReticleClamp => enableReticleClamp;

        public float MaxReticleDriftPixels => maxReticleDriftPixels;

        public bool EnableThirdPersonAimPitchBlend => enableThirdPersonAimPitchBlend;

        public bool EnableAimDebugRays => enableAimDebugRays;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyTestingSettings();
                StartCoroutine(ApplyAimVisualSettingsWhenPlayerReady());
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!applyInEditorWhenChanged || !Application.isPlaying)
            {
                return;
            }

            ApplyTestingSettings();
            ApplyAimVisualSettingsToSpawnedPlayer();
        }
#endif

        #endregion

        #region Public Methods

        public void ApplyTestingSettings()
        {
            ApplyRecordingAmbienceSettings();
            ApplyAimVisualSettingsToSpawnedPlayer();
        }

        public void SetRecordingAmbienceEnabled(bool enabled)
        {
            enableRecordingAmbience = enabled;
            ApplyRecordingAmbienceSettings();
        }

        public void SetArmToReticleIkEnabled(bool enabled)
        {
            enableArmToReticleIK = enabled;
            ApplyAimVisualSettingsToSpawnedPlayer();
        }

        public void SetReticleMode(CCS_AimReticleMode mode)
        {
            reticleMode = mode;
            ApplyAimVisualSettingsToSpawnedPlayer();
        }

        #endregion

        #region Private Methods

        private void ApplyRecordingAmbienceSettings()
        {
            if (ambientAudioPlaylist == null)
            {
                if (debugTestingManager)
                {
                    Debug.LogWarning("[Master Test Testing Manager] Ambient audio playlist reference is missing.", this);
                }

                return;
            }

            ambientAudioPlaylist.SetPlaylistEnabled(enableRecordingAmbience);
            if (debugTestingManager)
            {
                Debug.Log(
                    "[Master Test Testing Manager] Applied recording ambience = "
                    + enableRecordingAmbience
                    + " volume="
                    + ambientAudioPlaylist.Volume.ToString("0.##"),
                    this);
            }
        }

        private IEnumerator ApplyAimVisualSettingsWhenPlayerReady()
        {
            const int maxFrames = 240;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (ApplyAimVisualSettingsToSpawnedPlayer())
                {
                    yield break;
                }

                yield return null;
            }

            if (debugTestingManager)
            {
                Debug.LogWarning(
                    "[Master Test Testing Manager] Timed out waiting for spawned test player to apply aim visual settings.",
                    this);
            }
        }

        private bool ApplyAimVisualSettingsToSpawnedPlayer()
        {
            CCS_RevolverController revolverController = FindFirstObjectByType<CCS_RevolverController>();
            if (revolverController == null)
            {
                return false;
            }

            revolverController.ConfigureAimVisualTestSettings(
                enableArmToReticleIK,
                enableVisualAimConvergence,
                reticleMode,
                enableReticleClamp,
                maxReticleDriftPixels,
                false,
                enableAimDebugRays);

            CCS_RevolverBodyAimFollowController bodyAimFollow =
                revolverController.GetComponentInChildren<CCS_RevolverBodyAimFollowController>(true);
            bodyAimFollow?.SetBodyAimFollowEnabled(true);

            CCS_RevolverUpperBodyAnimator upperBodyAnimator =
                revolverController.GetComponentInChildren<CCS_RevolverUpperBodyAnimator>(true);
            _ = upperBodyAnimator;

            if (!appliedAimVisualSettingsToPlayer && debugTestingManager)
            {
                Debug.Log(
                    "[Master Test Testing Manager] Applied aim visual settings to spawned test player.",
                    this);
            }

            appliedAimVisualSettingsToPlayer = true;
            return true;
        }

        #endregion
    }
}
