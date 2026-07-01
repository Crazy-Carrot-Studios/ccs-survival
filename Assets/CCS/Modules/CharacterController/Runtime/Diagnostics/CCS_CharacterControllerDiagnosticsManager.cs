using System.Collections;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.CharacterController.Validation;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerDiagnosticsManager
// CATEGORY: Modules / CharacterController / Tests / Runtime / Managers
// PURPOSE: Central Master Test switchboard for debug toggles and one-shot reports.
// PLACEMENT: CCS_DiagnosticsManager on SCN_CCS_CharacterController_Validation only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Test-scene-only. Production Runtime must not depend on this assembly.
// =============================================================================

using UnityEngine.Scripting.APIUpdating;

using UnityEngine.Serialization;

namespace CCS.Modules.CharacterController.Diagnostics {
    [MovedFrom(true, "CCS.Modules.CharacterController.Tests", "CCS.Modules.CharacterController.Tests.Runtime", "CCS_CharacterControllerTestingManager")]
    [DefaultExecutionOrder(-10)]
    public class CCS_CharacterControllerDiagnosticsManager : MonoBehaviour, CCS_IRevolverAimSetupPoseDebugSource, CCS_IRevolverHandSocketPreviewDebugSource
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
        [Tooltip("For validation only. Holds revolver aim setup pose and right-hand visual preview without RMB. Does not fire, damage, change ammo, or grant weapon ownership.")]
        [FormerlySerializedAs("forceAimPresentation")]
        [SerializeField] private bool forceRevolverAimSetupPose;

        [Tooltip("For validation only. Shows the revolver visual on the right-hand socket without forcing aim animation, firing, ammo, damage, or gameplay ownership.")]
        [SerializeField] private bool forceRevolverHandSocketPreview;

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

        public static CCS_CharacterControllerDiagnosticsManager ActiveInstance { get; private set; }

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

        public bool EnableDamageDiagnostics => enableTestDamage;

        public bool EnableVisualDebugHelpers => enableVisualDebugHelpers;

        public bool ForceRevolverAimSetupPose => forceRevolverAimSetupPose;

        public bool ForceRevolverHandSocketPreview => forceRevolverHandSocketPreview;

        #endregion

        #region Unity Callbacks

        protected virtual void Awake()
        {
            ActiveInstance = this;
            CCS_RevolverAimSetupPoseDebugRegistry.Register(this);
            CCS_RevolverHandSocketPreviewDebugRegistry.Register(this);
            EnsureDiagnosticComponents();
            SyncAimPresentationDiagnosticsRegistry();
        }

        protected virtual void OnDestroy()
        {
            CCS_RevolverAimSetupPoseDebugRegistry.Unregister(this);
            CCS_RevolverHandSocketPreviewDebugRegistry.Unregister(this);
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
            SyncAimPresentationDiagnosticsRegistry();
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
            SyncAimPresentationDiagnosticsRegistry();
        }

        public void SetAnimationDiagnosticsEnabled(bool enabled)
        {
            enableAnimationDiagnostics = enabled;
        }

        public void SetInteractionDiagnosticsEnabled(bool enabled)
        {
            enableInteractionDiagnostics = enabled;
        }

        public void SetDamageDiagnosticsEnabled(bool enabled)
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
            builder.AppendLine("- ForceRevolverAimSetupPose: " + forceRevolverAimSetupPose);
            builder.AppendLine("- ForceRevolverHandSocketPreview: " + forceRevolverHandSocketPreview);
            builder.AppendLine();

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

        private void SyncAimPresentationDiagnosticsRegistry()
        {
            CCS_AimPresentationDiagnosticsRegistry.EnableReticleTransitionLogging = enableAimDiagnostics;
        }

        private void EnsureDiagnosticComponents()
        {
            if (GetComponent<CCS_CharacterCameraDebugReporter>() == null)
            {
                gameObject.AddComponent<CCS_CharacterCameraDebugReporter>();
            }

            if (GetComponent<CCS_LocalPlayerOfflineBootstrapper>() == null)
            {
                gameObject.AddComponent<CCS_LocalPlayerOfflineBootstrapper>();
            }

            if (GetComponent<CCS_PlayerDiagnosticsInputRouter>() == null)
            {
                gameObject.AddComponent<CCS_PlayerDiagnosticsInputRouter>();
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
            CCS_PlayerEquipmentVisualController equipmentVisualController =
                FindFirstObjectByType<CCS_PlayerEquipmentVisualController>();
            if (equipmentVisualController == null)
            {
                return false;
            }

            CCS_RevolverController revolverController =
                equipmentVisualController.GetComponent<CCS_RevolverController>();
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

            equipmentVisualController.SetDiagnosticsRevolverAimSetupPoseActive(forceRevolverAimSetupPose);
            equipmentVisualController.SetDiagnosticsRevolverHandSocketPreviewActive(forceRevolverHandSocketPreview);

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
