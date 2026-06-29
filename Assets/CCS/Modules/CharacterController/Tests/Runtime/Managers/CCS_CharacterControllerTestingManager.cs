using System.Collections;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerTestingManager
// CATEGORY: Modules / CharacterController / Tests / Runtime / Managers
// PURPOSE: Central Master Test switchboard for debug toggles and one-shot reports.
// PLACEMENT: CCS_TestingManager on SCN_CCS_CharacterController_MasterTest only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Test-scene-only. Production Runtime must not depend on this assembly.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    [DefaultExecutionOrder(-10)]
    public class CCS_CharacterControllerTestingManager : MonoBehaviour
    {
        #region Variables

        [Header("Recording")]
        [Tooltip("Enables quiet background ambience for recording gameplay sections.")]
        [SerializeField] private bool enableRecordingAmbience;

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

        [Header("Central Debug Toggles")]
        [SerializeField] private bool enableVerboseLogs;

        [SerializeField] private bool enableCameraDiagnostics;

        [SerializeField] private bool enableAimDiagnostics;

        [SerializeField] private bool enableAnimationDiagnostics;

        [SerializeField] private bool enableInteractionDiagnostics;

        [SerializeField] private bool enableTestDamage;

        [SerializeField] private bool enableVisualDebugHelpers;

        [Header("Debug")]
        [SerializeField] private bool debugTestingManager;

        private bool appliedAimVisualSettingsToPlayer;

        #endregion

        #region Properties

        public static CCS_CharacterControllerTestingManager ActiveInstance { get; private set; }

        public bool EnableRecordingAmbience => enableRecordingAmbience;

        public bool EnableArmToReticleIk => enableArmToReticleIK;

        public bool EnableVisualAimConvergence => enableVisualAimConvergence;

        public CCS_AimReticleMode ReticleMode => reticleMode;

        public bool EnableReticleClamp => enableReticleClamp;

        public float MaxReticleDriftPixels => maxReticleDriftPixels;

        public bool EnableThirdPersonAimPitchBlend => enableThirdPersonAimPitchBlend;

        public bool EnableAimDebugRays => enableAimDebugRays;

        public bool EnableVerboseLogs => enableVerboseLogs;

        public bool EnableCameraDiagnostics => enableCameraDiagnostics;

        public bool EnableAimDiagnostics => enableAimDiagnostics;

        public bool EnableAnimationDiagnostics => enableAnimationDiagnostics;

        public bool EnableInteractionDiagnostics => enableInteractionDiagnostics;

        public bool EnableTestDamage => enableTestDamage;

        public bool EnableVisualDebugHelpers => enableVisualDebugHelpers;

        #endregion

        #region Unity Callbacks

        protected virtual void Awake()
        {
            ActiveInstance = this;
            EnsureDiagnosticComponents();
        }

        protected virtual void OnDestroy()
        {
            if (ActiveInstance == this)
            {
                ActiveInstance = null;
            }
        }

        protected virtual void Start()
        {
            if (applyOnStart)
            {
                ApplyTestingSettings();
                StartCoroutine(ApplyAimVisualSettingsWhenPlayerReady());
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
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

        public void SetVerboseLogsEnabled(bool enabled)
        {
            enableVerboseLogs = enabled;
        }

        public void SetCameraDiagnosticsEnabled(bool enabled)
        {
            enableCameraDiagnostics = enabled;
        }

        public void SetAimDiagnosticsEnabled(bool enabled)
        {
            enableAimDiagnostics = enabled;
        }

        public void SetAnimationDiagnosticsEnabled(bool enabled)
        {
            enableAnimationDiagnostics = enabled;
        }

        public void SetInteractionDiagnosticsEnabled(bool enabled)
        {
            enableInteractionDiagnostics = enabled;
        }

        public void SetTestDamageEnabled(bool enabled)
        {
            enableTestDamage = enabled;
        }

        public void SetVisualDebugHelpersEnabled(bool enabled)
        {
            enableVisualDebugHelpers = enabled;
        }

        public void WriteOneShotReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CCS Character Controller Master Test Report");
            builder.AppendLine();
            builder.AppendLine("Generated: " + System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            builder.AppendLine();
            builder.AppendLine("## Manager Toggles");
            builder.AppendLine("- VerboseLogs: " + enableVerboseLogs);
            builder.AppendLine("- CameraDiagnostics: " + enableCameraDiagnostics);
            builder.AppendLine("- AimDiagnostics: " + enableAimDiagnostics);
            builder.AppendLine("- AnimationDiagnostics: " + enableAnimationDiagnostics);
            builder.AppendLine("- InteractionDiagnostics: " + enableInteractionDiagnostics);
            builder.AppendLine("- TestDamage: " + enableTestDamage);
            builder.AppendLine("- VisualDebugHelpers: " + enableVisualDebugHelpers);
            builder.AppendLine("- RecordingAmbience: " + enableRecordingAmbience);
            builder.AppendLine("- AimDebugRays: " + enableAimDebugRays);
            builder.AppendLine();

            CCS_RevolverUpperBodyAnimatorDebugReporter animationReporter =
                GetComponent<CCS_RevolverUpperBodyAnimatorDebugReporter>();
            if (animationReporter != null)
            {
                builder.AppendLine("## Animation");
                builder.AppendLine(animationReporter.BuildReportSection());
                builder.AppendLine();
            }

            CCS_CharacterCameraDebugReporter cameraReporter = GetComponent<CCS_CharacterCameraDebugReporter>();
            if (cameraReporter != null)
            {
                builder.AppendLine("## Camera");
                builder.AppendLine(cameraReporter.BuildReportSection());
                builder.AppendLine();
            }

            string reportDirectory = ResolveReportDirectoryPath();
            Directory.CreateDirectory(reportDirectory);
            string reportPath = Path.Combine(
                reportDirectory,
                "CCS_MasterTest_OneShotReport.md");
            File.WriteAllText(reportPath, builder.ToString(), Encoding.UTF8);
            Debug.Log("[Character Controller Testing Manager] Wrote one-shot report to " + reportPath, this);
        }

        #endregion

        #region Protected Methods

        protected void LogVerbose(string message)
        {
            if (enableVerboseLogs || debugTestingManager)
            {
                Debug.Log("[Character Controller Testing Manager] " + message, this);
            }
        }

        #endregion

        #region Private Methods

        private static string ResolveReportDirectoryPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, "Logs", "CharacterController", "TestingReports");
        }

        private void EnsureDiagnosticComponents()
        {
            if (GetComponent<CCS_RevolverUpperBodyAnimatorDebugReporter>() == null)
            {
                gameObject.AddComponent<CCS_RevolverUpperBodyAnimatorDebugReporter>();
            }

            if (GetComponent<CCS_CharacterCameraDebugReporter>() == null)
            {
                gameObject.AddComponent<CCS_CharacterCameraDebugReporter>();
            }

            if (GetComponent<CCS_MasterTestPlayerOfflineBootstrapper>() == null)
            {
                gameObject.AddComponent<CCS_MasterTestPlayerOfflineBootstrapper>();
            }

            if (GetComponent<CCS_TestPlayerAttributeDebugInputRouter>() == null)
            {
                gameObject.AddComponent<CCS_TestPlayerAttributeDebugInputRouter>();
            }
        }

        private void ApplyRecordingAmbienceSettings()
        {
            if (ambientAudioPlaylist == null)
            {
                if (debugTestingManager)
                {
                    Debug.LogWarning(
                        "[Character Controller Testing Manager] Ambient audio playlist reference is missing.",
                        this);
                }

                return;
            }

            ambientAudioPlaylist.SetPlaylistEnabled(enableRecordingAmbience);
            LogVerbose(
                "Applied recording ambience = "
                + enableRecordingAmbience
                + " volume="
                + ambientAudioPlaylist.Volume.ToString("0.##"));
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
                    "[Character Controller Testing Manager] Timed out waiting for spawned test player to apply aim visual settings.",
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

            if (!appliedAimVisualSettingsToPlayer)
            {
                LogVerbose("Applied aim visual settings to spawned test player.");
            }

            appliedAimVisualSettingsToPlayer = true;
            return true;
        }

        #endregion
    }
}
