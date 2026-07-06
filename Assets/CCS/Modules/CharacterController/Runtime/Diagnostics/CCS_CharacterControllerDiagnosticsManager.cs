using CCS.Modules.CharacterController.Local;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerDiagnosticsManager
// CATEGORY: Modules / CharacterController / Runtime / Diagnostics
// PURPOSE: Stripped-baseline validation scene controls for enemy, aim pose, and weapon visual.
// PLACEMENT: CCS_DiagnosticsManager on SCN_CCS_CharacterController_Validation only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.13 exposes only Enable Enemy, Enable Aim Pose, and Equip Weapon.
// =============================================================================

using UnityEngine.Scripting.APIUpdating;

namespace CCS.Modules.CharacterController.Diagnostics
{
    [MovedFrom(true, "CCS.Modules.CharacterController.Tests", "CCS.Modules.CharacterController.Tests.Runtime", "CCS_CharacterControllerTestingManager")]
    [DefaultExecutionOrder(-10)]
    public class CCS_CharacterControllerDiagnosticsManager : MonoBehaviour, CCS_IRevolverAimSetupPoseDebugSource
    {
        [Header("Stripped Baseline Controls")]
        [Tooltip("Validation only. Enables or disables the EnemyAI bandit in the stripped baseline scene.")]
        [SerializeField] private bool enableEnemy;

        [Tooltip("Validation only. Holds the revolver aim animation pose. Does not fire, damage, show reticle, use IK, or change ammo.")]
        [SerializeField] private bool enableAimPose;

        [Tooltip("Validation only. Shows the visual holstered/equipped revolver for aim-pose testing. Does not grant gameplay weapon ownership.")]
        [SerializeField] private bool equipWeapon = true;

        private bool previousEnableEnemy;
        private bool previousEnableAimPose;
        private bool previousEquipWeapon;
        private bool registriesInitialized;

        public static CCS_CharacterControllerDiagnosticsManager ActiveInstance { get; private set; }

        public bool EnableEnemy => enableEnemy;

        public bool EnableAimPose => enableAimPose;

        public bool EquipWeapon => equipWeapon;

        bool CCS_IRevolverAimSetupPoseDebugSource.EnableAimPose => enableAimPose;

        private void Awake()
        {
            ActiveInstance = this;
            CCS_RevolverAimSetupPoseDebugRegistry.Register(this);
            EnsureDiagnosticComponents();
            SyncDiagnosticsRegistries();
            CacheRegistryState();
        }

        private void OnDestroy()
        {
            CCS_RevolverAimSetupPoseDebugRegistry.Unregister(this);
            if (ActiveInstance == this)
            {
                ActiveInstance = null;
            }
        }

        private void LateUpdate()
        {
            if (!registriesInitialized
                || enableEnemy != previousEnableEnemy
                || enableAimPose != previousEnableAimPose
                || equipWeapon != previousEquipWeapon)
            {
                SyncDiagnosticsRegistries();
                CacheRegistryState();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            SyncDiagnosticsRegistries();
            CacheRegistryState();
        }
#endif

        public void SetEnableEnemy(bool enabled)
        {
            enableEnemy = enabled;
            SyncDiagnosticsRegistries();
            CacheRegistryState();
        }

        public void SetEnableAimPose(bool enabled)
        {
            enableAimPose = enabled;
            SyncDiagnosticsRegistries();
            CacheRegistryState();
        }

        public void SetEquipWeapon(bool enabled)
        {
            equipWeapon = enabled;
            SyncDiagnosticsRegistries();
            CacheRegistryState();
        }

        private void SyncDiagnosticsRegistries()
        {
            CCS_DiagnosticsEnemyBanditRegistry.EnableEnemy = enableEnemy;
            CCS_DiagnosticsEquipWeaponRegistry.EquipWeapon = equipWeapon;
        }

        private void CacheRegistryState()
        {
            previousEnableEnemy = enableEnemy;
            previousEnableAimPose = enableAimPose;
            previousEquipWeapon = equipWeapon;
            registriesInitialized = true;
        }

        private void EnsureDiagnosticComponents()
        {
            if (GetComponent<CCS_LocalPlayerOfflineBootstrapper>() == null)
            {
                gameObject.AddComponent<CCS_LocalPlayerOfflineBootstrapper>();
            }

            if (GetComponent<CCS_DiagnosticsEnemyBanditController>() == null)
            {
                gameObject.AddComponent<CCS_DiagnosticsEnemyBanditController>();
            }
        }
    }
}
