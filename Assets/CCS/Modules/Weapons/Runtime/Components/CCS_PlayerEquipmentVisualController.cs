using System;
using System.Collections.Generic;
using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerEquipmentVisualController
// CATEGORY: Modules / Weapons / Runtime / Components
// PURPOSE: Runtime visual bridge — shows holstered/equipped revolver from saved fit profiles.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.8 profile-driven visuals only. Gameplay muzzle/hitscan unchanged.
//        Socket stays at definition; saved profile values apply to attachment root.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(120)]
    public sealed class CCS_PlayerEquipmentVisualController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private CCS_EquipmentSocketRegistry equipmentSocketRegistry;
        [SerializeField] private CCS_PlayerWeaponLoadout playerWeaponLoadout;
        [SerializeField] private CCS_CharacterAimLocomotionController aimLocomotionController;
        [SerializeField] private CCS_WeaponAttachmentFitProfile rightHipHolsterFitProfile;
        [SerializeField] private CCS_WeaponAttachmentFitProfile rightHandEquippedFitProfile;
        [SerializeField] private GameObject revolverVisualOnlyPrefab;

        [SerializeField] private CCS_WeaponCarryStateController weaponCarryStateController;

        [Header("Diagnostics")]
        [SerializeField] private bool debugRuntimeFitParity;
        [SerializeField] private bool debugEquipmentVisualProfileApplication;

        private Transform holsterAttachmentRoot;
        private Transform equippedAttachmentRoot;
        private Transform equippedAimConvergenceRoot;
        private CCS_RevolverVisualAimConvergence equippedAimConvergence;
        private GameObject holsteredVisualInstance;
        private GameObject equippedVisualInstance;
        private Transform equippedMuzzlePoint;
        private bool isAiming;
        private bool diagnosticsRevolverAimSetupPoseActive;
        private bool diagnosticsRevolverHandSocketPreviewActive;
        private Transform diagnosticsEquippedAttachmentRoot;
        private GameObject diagnosticsEquippedVisualInstance;
        private bool loggedEquippedFitParityThisAimSession;

        private const string DiagnosticsEquippedAttachmentRootObjectName =
            "CCS_DiagnosticsEquippedAttachmentRoot";
        private const string DiagnosticsEquippedVisualObjectName =
            "CCS_DiagnosticsEquippedVisual";

#if UNITY_EDITOR
        private bool editorAimFitOverrideActive;
        private bool editorForceEquippedVisual;
        private bool editorHideEquippedVisual;
#endif

        #endregion

        #region Properties

        public GameObject RevolverVisualOnlyPrefab => revolverVisualOnlyPrefab;

        public CCS_WeaponAttachmentFitProfile RightHipHolsterFitProfile => rightHipHolsterFitProfile;

        public CCS_WeaponAttachmentFitProfile RightHandEquippedFitProfile => rightHandEquippedFitProfile;

        public bool HasVisualBridgeWiring =>
            equipmentSocketRegistry != null
            && playerWeaponLoadout != null
            && aimLocomotionController != null
            && revolverVisualOnlyPrefab != null
            && rightHipHolsterFitProfile != null
            && rightHandEquippedFitProfile != null;

        public Transform CurrentEquippedMuzzlePoint => equippedMuzzlePoint;

        public bool HasEquippedMuzzlePoint => equippedMuzzlePoint != null;

        public Transform CurrentAimConvergenceRoot => equippedAimConvergenceRoot;

        public CCS_RevolverVisualAimConvergence CurrentAimConvergence => equippedAimConvergence;

        public bool HasEquippedAimConvergence => equippedAimConvergence != null;

#if UNITY_EDITOR
        public bool IsEditorAimFitOverrideActive => editorAimFitOverrideActive;

        public void SetEditorAimFitOverride(bool active)
        {
            editorAimFitOverrideActive = active;
            if (active)
            {
                editorForceEquippedVisual = true;
                editorHideEquippedVisual = false;
            }

            RefreshVisualState();
        }

        public void SetEditorForceEquippedVisual(bool showEquipped)
        {
            editorForceEquippedVisual = showEquipped;
            editorHideEquippedVisual = !showEquipped;
            RefreshVisualState();
        }
#endif

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ReloadFitProfilesFromDisk();
        }

        private void OnEnable()
        {
            ReloadFitProfilesFromDisk();
            Subscribe();
            RefreshVisualState();
        }

        private void OnDisable()
        {
            Unsubscribe();
            diagnosticsRevolverAimSetupPoseActive = false;
            diagnosticsRevolverHandSocketPreviewActive = false;
            HideDiagnosticsEquippedPreview();
            DestroyRuntimeVisuals();
#if UNITY_EDITOR
            editorAimFitOverrideActive = false;
            editorForceEquippedVisual = false;
            editorHideEquippedVisual = false;
#endif
        }

        #endregion

        #region Public Methods

        public void RefreshVisualState()
        {
            ReloadFitProfilesFromDisk();

#if UNITY_EDITOR
            if (Application.isPlaying && (editorAimFitOverrideActive || editorForceEquippedVisual || editorHideEquippedVisual))
            {
                if (editorHideEquippedVisual && !editorForceEquippedVisual)
                {
                    HideEquippedVisual();
                    if (!editorAimFitOverrideActive)
                    {
                        if (playerWeaponLoadout != null && playerWeaponLoadout.HasRevolver)
                        {
                            ShowHolsteredVisual();
                        }
                    }
                    else
                    {
                        HideHolsteredVisual();
                    }

                    return;
                }

                HideHolsteredVisual();
                ShowEquippedVisual();
                return;
            }
#endif

            if (ShouldShowDiagnosticsEquippedVisualPreview())
            {
                HideHolsteredVisual();
                SuppressGameplayEquippedVisualForDiagnostics();
                bool previewShown = ShowDiagnosticsEquippedPreview();
                if (!previewShown)
                {
                    RestoreGameplayEquippedVisualAfterDiagnostics();
                }

                return;
            }

            HideDiagnosticsEquippedPreview();

            if (!PlayerOwnsRevolverForVisuals())
            {
                HideHolsteredVisual();
                HideEquippedVisual();
                return;
            }

            CCS_WeaponCarryState carryState = ResolveCarryState();
            switch (carryState)
            {
                case CCS_WeaponCarryState.Aiming:
                case CCS_WeaponCarryState.EquippedInHands:
                    HideHolsteredVisual();
                    ShowEquippedVisual();
                    break;
                case CCS_WeaponCarryState.Holstered:
                    HideEquippedVisual();
                    ShowHolsteredVisual();
                    break;
                default:
                    HideHolsteredVisual();
                    HideEquippedVisual();
                    break;
            }
        }

        private CCS_WeaponCarryState ResolveCarryState()
        {
            if (weaponCarryStateController != null)
            {
                return weaponCarryStateController.CarryState;
            }

            if (isAiming)
            {
                return CCS_WeaponCarryState.Aiming;
            }

            return playerWeaponLoadout != null && playerWeaponLoadout.HasRevolver
                ? CCS_WeaponCarryState.Holstered
                : CCS_WeaponCarryState.None;
        }

        private bool PlayerOwnsRevolverForVisuals()
        {
            if (playerWeaponLoadout != null && playerWeaponLoadout.HasRevolver)
            {
                return true;
            }

            return ResolveCarryState() != CCS_WeaponCarryState.None;
        }

        public void SetDiagnosticsRevolverAimSetupPoseActive(bool active)
        {
            if (diagnosticsRevolverAimSetupPoseActive == active)
            {
                return;
            }

            diagnosticsRevolverAimSetupPoseActive = active;
            RefreshVisualState();
            LogDiagnosticsPreviewToggle("Force Revolver Aim Setup Pose", active);
        }

        public void SetDiagnosticsRevolverHandSocketPreviewActive(bool active)
        {
            if (diagnosticsRevolverHandSocketPreviewActive == active)
            {
                return;
            }

            diagnosticsRevolverHandSocketPreviewActive = active;
            RefreshVisualState();
            LogDiagnosticsPreviewToggle("Force Revolver Hand Socket Preview", active);
        }

        public bool IsDiagnosticsRevolverAimSetupPoseActive => diagnosticsRevolverAimSetupPoseActive;

        public bool IsDiagnosticsRevolverHandSocketPreviewActive => diagnosticsRevolverHandSocketPreviewActive;

        private bool ShouldShowDiagnosticsEquippedVisualPreview()
        {
            return diagnosticsRevolverAimSetupPoseActive || diagnosticsRevolverHandSocketPreviewActive;
        }

        public void SetVisualAimConvergenceActive(bool active)
        {
            equippedAimConvergence?.SetConvergenceActive(active);
        }

        public void TickEquippedVisualAimConvergence(
            Camera aimCamera,
            Vector2 reticleViewportPoint,
            Transform fallbackMuzzle,
            float maxRange,
            LayerMask hitMask,
            Transform ignoreRoot,
            CCS_RevolverVisualAimConvergenceSettings settings,
            bool drawDebug)
        {
            if (equippedAimConvergence == null || equippedVisualInstance == null || !equippedVisualInstance.activeSelf)
            {
                return;
            }

            equippedAimConvergence.ApplySettings(settings);
            equippedAimConvergence.TickConvergence(
                aimCamera,
                reticleViewportPoint,
                fallbackMuzzle,
                maxRange,
                hitMask,
                ignoreRoot,
                drawDebug);
            equippedMuzzlePoint = equippedAimConvergence.MuzzlePoint;
        }

        #endregion

        #region Private Methods

        private void ReloadFitProfilesFromDisk()
        {
#if UNITY_EDITOR
            rightHipHolsterFitProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.RightHipHolsterFitPath);
            rightHandEquippedFitProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.RightHandEquippedFitPath);
#endif
        }

        private void EnsureFitProfilesResolved()
        {
            ReloadFitProfilesFromDisk();
        }

        private bool ShowDiagnosticsEquippedPreview()
        {
            if (equipmentSocketRegistry == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: missing CCS_PlayerEquipmentVisualController equipment socket registry.");
                return false;
            }

            equipmentSocketRegistry.RefreshSocketRegistry();

            if (!equipmentSocketRegistry.TryGetSocketAnchor(
                    CCS_EquipmentConstants.HandSocketRightId,
                    out CCS_EquipmentSocketAnchor socketAnchor)
                || socketAnchor == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: missing equipment socket anchor for CCS_HandSocket_Right.");
                return false;
            }

            Transform socketTransform = socketAnchor.SocketTransform;
            if (socketTransform == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: CCS_HandSocket_Right transform is missing.");
                return false;
            }

            if (IsIkOnlyAttachmentTransform(socketTransform))
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: resolved parent '"
                    + BuildTransformPath(socketTransform)
                    + "' is an IK target. Weapon preview must attach to CCS_HandSocket_Right.");
                return false;
            }

            EnsureFitProfilesResolved();
            CCS_WeaponAttachmentFitProfile profile = rightHandEquippedFitProfile;
            if (profile == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: missing right-hand equipped fit profile "
                    + "(CCS_RevolverM1879_RightHandEquipped_Fit).");
                return false;
            }

            if (revolverVisualOnlyPrefab == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: missing revolver visual-only prefab reference.");
                return false;
            }

            if (!TryGetSocketDefinitionBaseline(
                    CCS_EquipmentConstants.HandSocketRightId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: missing right hand socket definition baseline.");
                return false;
            }

            diagnosticsEquippedAttachmentRoot = EnsureAttachmentRoot(
                socketTransform,
                DiagnosticsEquippedAttachmentRootObjectName);
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                diagnosticsEquippedAttachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);

            diagnosticsEquippedVisualInstance = EnsureDiagnosticsVisualInstance(diagnosticsEquippedAttachmentRoot);
            if (diagnosticsEquippedVisualInstance == null)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: could not instantiate diagnostics revolver visual.");
                return false;
            }

            diagnosticsEquippedVisualInstance.SetActive(true);
            if (!diagnosticsEquippedVisualInstance.activeInHierarchy)
            {
                Debug.LogWarning(
                    "[Equipment Visual] Diagnostics preview failed: diagnostics revolver visual instance is inactive in hierarchy.");
                return false;
            }

            Debug.Log(
                "[Equipment Visual] Diagnostics equipped preview shown.\n"
                + "- Preview type: diagnostics-only (no gameplay ownership)\n"
                + "- Right-hand socket path: "
                + BuildTransformPath(socketTransform)
                + "\n- Visual source prefab: "
                + revolverVisualOnlyPrefab.name
                + "\n- Fit profile: CCS_RevolverM1879_RightHandEquipped_Fit\n"
                + "- Final parent path: "
                + BuildTransformPath(diagnosticsEquippedVisualInstance.transform.parent)
                + "\n- Final local position: "
                + FormatVector3(diagnosticsEquippedVisualInstance.transform.localPosition)
                + "\n- Final local rotation: "
                + FormatVector3(diagnosticsEquippedVisualInstance.transform.localEulerAngles)
                + "\n- Final local scale: "
                + FormatVector3(diagnosticsEquippedVisualInstance.transform.localScale));
            return true;
        }

        private void HideDiagnosticsEquippedPreview()
        {
            if (diagnosticsEquippedVisualInstance != null)
            {
                diagnosticsEquippedVisualInstance.SetActive(false);
            }

            DestroyVisualInstance(ref diagnosticsEquippedVisualInstance);
            DestroyAttachmentRoot(ref diagnosticsEquippedAttachmentRoot);
            RestoreSocketDefinition(CCS_EquipmentConstants.HandSocketRightId);
        }

        private void SuppressGameplayEquippedVisualForDiagnostics()
        {
            if (equippedVisualInstance != null)
            {
                equippedVisualInstance.SetActive(false);
            }
        }

        private void RestoreGameplayEquippedVisualAfterDiagnostics()
        {
            if (!PlayerOwnsRevolverForVisuals())
            {
                return;
            }

            CCS_WeaponCarryState carryState = ResolveCarryState();
            if (carryState == CCS_WeaponCarryState.Aiming
                || carryState == CCS_WeaponCarryState.EquippedInHands)
            {
                ShowEquippedVisual();
            }
        }

        private GameObject EnsureDiagnosticsVisualInstance(Transform attachmentRoot)
        {
            Transform existing = attachmentRoot.Find(DiagnosticsEquippedVisualObjectName);
            if (existing != null)
            {
                ZeroLocalTransform(existing);
                return existing.gameObject;
            }

            GameObject instance = Instantiate(revolverVisualOnlyPrefab, attachmentRoot);
            instance.name = DiagnosticsEquippedVisualObjectName;
            ZeroLocalTransform(instance.transform);
            StripRuntimeGameplayComponents(instance);
            return instance;
        }

        private static bool IsIkOnlyAttachmentTransform(Transform transform)
        {
            if (transform == null)
            {
                return false;
            }

            string objectName = transform.name;
            if (objectName.Contains("HandSocket"))
            {
                return false;
            }

            return objectName.Contains("IKTarget")
                || objectName == CCS_EquipmentConstants.WeaponAimTargetObjectName
                || objectName == CCS_EquipmentConstants.WeaponIkTargetsObjectName
                || objectName == CCS_EquipmentConstants.RightHandIkTargetObjectName
                || objectName == "MuzzlePoint"
                || objectName == "RightHandReticleIKTarget";
        }

        private static string BuildTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return "(null)";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private void LogDiagnosticsPreviewToggle(string toggleLabel, bool active)
        {
            if (!active)
            {
                Debug.Log("[Equipment Visual] " + toggleLabel + " deactivated. Diagnostics preview cleared.");
                return;
            }

            if (diagnosticsEquippedVisualInstance != null && diagnosticsEquippedVisualInstance.activeInHierarchy)
            {
                return;
            }

            Debug.LogWarning(
                "[Equipment Visual] "
                + toggleLabel
                + " activated but diagnostics preview is not visible. See prior preview failure warnings.");
        }

        private void Subscribe()
        {
            if (playerWeaponLoadout != null)
            {
                playerWeaponLoadout.WeaponGranted += HandleWeaponGranted;
                playerWeaponLoadout.RevolverGranted += HandleRevolverGranted;
            }

            if (aimLocomotionController != null)
            {
                aimLocomotionController.AimMovementActiveChanged += HandleAimMovementActiveChanged;
                isAiming = aimLocomotionController.IsFirearmAimCameraActive;
            }

            if (weaponCarryStateController != null)
            {
                weaponCarryStateController.CarryStateChanged += HandleCarryStateChanged;
            }
        }

        private void Unsubscribe()
        {
            if (playerWeaponLoadout != null)
            {
                playerWeaponLoadout.WeaponGranted -= HandleWeaponGranted;
                playerWeaponLoadout.RevolverGranted -= HandleRevolverGranted;
            }

            if (aimLocomotionController != null)
            {
                aimLocomotionController.AimMovementActiveChanged -= HandleAimMovementActiveChanged;
            }

            if (weaponCarryStateController != null)
            {
                weaponCarryStateController.CarryStateChanged -= HandleCarryStateChanged;
            }
        }

        private void HandleCarryStateChanged(CCS_WeaponCarryState carryState)
        {
            RefreshVisualState();
        }

        private void HandleWeaponGranted()
        {
            RefreshVisualState();
        }

        private void HandleRevolverGranted()
        {
            RefreshVisualState();
        }

        private void HandleAimMovementActiveChanged(bool active)
        {
            isAiming = active;
            if (active)
            {
                loggedEquippedFitParityThisAimSession = false;
            }

            RefreshVisualState();

            if (active && debugRuntimeFitParity && !loggedEquippedFitParityThisAimSession)
            {
                LogEquippedFitParityOnce();
            }
        }

        private void ShowHolsteredVisual()
        {
            if (!TryGetSocket(CCS_EquipmentConstants.HolsterSocketRightHipId, out Transform socketTransform))
            {
                return;
            }

            CCS_WeaponAttachmentFitProfile profile = rightHipHolsterFitProfile;
            if (profile == null)
            {
                Debug.LogError(
                    "[Equipment Visual] Missing right hip holster fit profile; cannot show revolver holster.");
                return;
            }

            if (!TryGetSocketDefinitionBaseline(
                    CCS_EquipmentConstants.HolsterSocketRightHipId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                Debug.LogError(
                    "[Equipment Visual] Missing right hip holster socket definition; cannot show revolver holster.");
                return;
            }

            holsterAttachmentRoot = EnsureAttachmentRoot(
                socketTransform,
                CCS_EquipmentConstants.RuntimeHolsterAttachmentRootObjectName);
            bool wasHolsteredActive = holsteredVisualInstance != null && holsteredVisualInstance.activeSelf;
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                holsterAttachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);
            if (debugEquipmentVisualProfileApplication && !wasHolsteredActive)
            {
                LogHolsterProfileApplication(profile);
            }

            holsteredVisualInstance = EnsureVisualInstance(
                holsterAttachmentRoot,
                CCS_EquipmentConstants.RuntimeHolsteredVisualObjectName);
            if (holsteredVisualInstance != null)
            {
                holsteredVisualInstance.SetActive(true);
            }
        }

        private void HideHolsteredVisual()
        {
            if (holsteredVisualInstance != null)
            {
                holsteredVisualInstance.SetActive(false);
            }

            if (holsterAttachmentRoot != null)
            {
                CCS_WeaponAttachmentFitProfileApplicator.ResetAttachmentRoot(holsterAttachmentRoot);
            }

            RestoreSocketDefinition(CCS_EquipmentConstants.HolsterSocketRightHipId);
        }

        private void ShowEquippedVisual()
        {
            if (!TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                return;
            }

            CCS_WeaponAttachmentFitProfile profile = rightHandEquippedFitProfile;
            if (profile == null)
            {
                Debug.LogError(
                    "[Equipment Visual] Missing right hand equipped fit profile; cannot show equipped revolver.");
                return;
            }

            if (!TryGetSocketDefinitionBaseline(
                    CCS_EquipmentConstants.HandSocketRightId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                Debug.LogError(
                    "[Equipment Visual] Missing right hand socket definition; cannot show equipped revolver.");
                return;
            }

            equippedAttachmentRoot = EnsureAttachmentRoot(
                socketTransform,
                CCS_EquipmentConstants.RuntimeEquippedAttachmentRootObjectName);
            bool wasEquippedActive = equippedVisualInstance != null && equippedVisualInstance.activeSelf;
            CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                socketTransform,
                equippedAttachmentRoot,
                profile,
                definitionPosition,
                definitionEuler,
                definitionScale);
            if (debugEquipmentVisualProfileApplication && !wasEquippedActive)
            {
                LogEquippedProfileApplication(profile);
            }

            equippedAimConvergenceRoot = EnsureEquippedAimConvergenceRoot(equippedAttachmentRoot);
            CleanupLegacyEquippedVisualLayouts(equippedAttachmentRoot, equippedAimConvergenceRoot);
            equippedAimConvergence?.SetConvergenceActive(false);

            equippedVisualInstance = EnsureVisualInstance(
                equippedAttachmentRoot,
                CCS_EquipmentConstants.RuntimeEquippedVisualObjectName);

            if (equippedVisualInstance != null)
            {
                equippedVisualInstance.SetActive(true);
                equippedMuzzlePoint = ResolveEquippedMuzzlePoint(equippedVisualInstance.transform);
                equippedAimConvergence?.BindEquippedVisual(equippedVisualInstance.transform);
            }
            else
            {
                equippedMuzzlePoint = null;
                equippedAimConvergence?.BindEquippedVisual(null);
            }
        }

        private void HideEquippedVisual()
        {
            equippedMuzzlePoint = null;
            equippedAimConvergence?.SetConvergenceActive(false);
            equippedAimConvergence?.ResetConvergenceRotation();
            if (equippedVisualInstance != null)
            {
                equippedVisualInstance.SetActive(false);
            }

            if (equippedAimConvergenceRoot != null)
            {
                equippedAimConvergenceRoot.localRotation = Quaternion.identity;
            }

            if (equippedAttachmentRoot != null)
            {
                CCS_WeaponAttachmentFitProfileApplicator.ResetAttachmentRoot(equippedAttachmentRoot);
            }

            RestoreSocketDefinition(CCS_EquipmentConstants.HandSocketRightId);
        }

        private Transform EnsureEquippedAimConvergenceRoot(Transform attachmentRoot)
        {
            equippedAimConvergenceRoot = EnsureAttachmentRoot(
                attachmentRoot,
                CCS_EquipmentConstants.RuntimeEquippedAimConvergenceRootObjectName);
            ZeroLocalTransform(equippedAimConvergenceRoot);

            equippedAimConvergence = equippedAimConvergenceRoot.GetComponent<CCS_RevolverVisualAimConvergence>();
            if (equippedAimConvergence == null)
            {
                equippedAimConvergence = equippedAimConvergenceRoot.gameObject.AddComponent<CCS_RevolverVisualAimConvergence>();
            }

            return equippedAimConvergenceRoot;
        }

        private static void CleanupLegacyEquippedVisualLayouts(Transform attachmentRoot, Transform convergenceRoot)
        {
            if (attachmentRoot == null || convergenceRoot == null)
            {
                return;
            }

            Transform convergenceVisual = convergenceRoot.Find(
                CCS_EquipmentConstants.RuntimeEquippedVisualObjectName);
            if (convergenceVisual != null)
            {
                Destroy(convergenceVisual.gameObject);
            }
        }

        private static Transform EnsureAttachmentRoot(Transform socketTransform, string attachmentRootName)
        {
            Transform existing = socketTransform.Find(attachmentRootName);
            if (existing == null)
            {
                GameObject rootObject = new GameObject(attachmentRootName);
                existing = rootObject.transform;
                existing.SetParent(socketTransform, false);
            }

            return existing;
        }

        private GameObject EnsureVisualInstance(Transform attachmentRoot, string visualInstanceName)
        {
            if (revolverVisualOnlyPrefab == null)
            {
                Debug.LogWarning("[Equipment Visual] Missing revolver visual-only prefab reference.");
                return null;
            }

            Transform existing = attachmentRoot.Find(visualInstanceName);
            if (existing != null)
            {
                ZeroLocalTransform(existing);
                return existing.gameObject;
            }

            GameObject instance = Instantiate(revolverVisualOnlyPrefab, attachmentRoot);
            instance.name = visualInstanceName;
            ZeroLocalTransform(instance.transform);
            StripRuntimeGameplayComponents(instance);
            return instance;
        }

        private void RestoreSocketDefinition(string socketId)
        {
            if (!TryGetSocket(socketId, out Transform socketTransform))
            {
                return;
            }

            if (!TryGetSocketDefinitionBaseline(
                    socketId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                return;
            }

            socketTransform.localPosition = definitionPosition;
            socketTransform.localRotation = Quaternion.Euler(definitionEuler);
            socketTransform.localScale = definitionScale;
        }

        private bool TryGetSocketDefinitionBaseline(
            string socketId,
            out Vector3 position,
            out Vector3 euler,
            out Vector3 scale)
        {
            position = Vector3.zero;
            euler = Vector3.zero;
            scale = Vector3.one;

            CCS_EquipmentSocketProfile socketProfile = equipmentSocketRegistry?.EquipmentSocketProfile;
            if (socketProfile == null)
            {
                return false;
            }

            IReadOnlyList<CCS_EquipmentSocketDefinition> definitions = socketProfile.SocketDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                CCS_EquipmentSocketDefinition definition = definitions[i];
                if (definition != null && definition.SocketId == socketId)
                {
                    position = definition.LocalPosition;
                    euler = definition.LocalEulerAngles;
                    scale = definition.LocalScale;
                    return true;
                }
            }

            return false;
        }

        private void LogHolsterProfileApplication(CCS_WeaponAttachmentFitProfile profile)
        {
            Debug.Log(
                "[Equipment Visual] Applying ccs.weapon.revolver.m1879 holster fit:\n"
                + "Position="
                + FormatVector3(profile.SocketLocalPosition)
                + "\nRotation="
                + FormatVector3(profile.SocketLocalEulerAngles)
                + "\nScale="
                + FormatVector3(profile.SocketLocalScale)
                + "\nProfile=CCS_RevolverM1879_RightHipHolster_Fit");

            Debug.Log(
                "[Equipment Visual] Revolver holstered using profile CCS_RevolverM1879_RightHipHolster_Fit Position="
                + FormatCompactVector3(profile.SocketLocalPosition)
                + " Rotation="
                + FormatCompactVector3(profile.SocketLocalEulerAngles));
        }

        private static void LogEquippedProfileApplication(CCS_WeaponAttachmentFitProfile profile)
        {
            Debug.Log(
                "[Equipment Visual] Applying ccs.weapon.revolver.m1879 equipped fit:\n"
                + "Position="
                + FormatVector3(profile.SocketLocalPosition)
                + "\nRotation="
                + FormatVector3(profile.SocketLocalEulerAngles)
                + "\nScale="
                + FormatVector3(profile.SocketLocalScale)
                + "\nProfile=CCS_RevolverM1879_RightHandEquipped_Fit");
        }

        private void LogEquippedFitParityOnce()
        {
            loggedEquippedFitParityThisAimSession = true;
            CCS_WeaponAttachmentFitProfile profile = rightHandEquippedFitProfile;
            if (profile == null || equippedAttachmentRoot == null)
            {
                Debug.LogWarning("[Equipment Visual] Runtime Equipped Fit Parity skipped: missing profile or attachment root.");
                return;
            }

            Transform visualTransform = equippedVisualInstance != null ? equippedVisualInstance.transform : null;
            bool visualZeroed = visualTransform != null
                && visualTransform.localPosition == Vector3.zero
                && visualTransform.localRotation == Quaternion.identity
                && visualTransform.localScale == Vector3.one;

            bool attachmentMatchesProfile = false;
            if (TryGetSocketDefinitionBaseline(
                    CCS_EquipmentConstants.HandSocketRightId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                attachmentMatchesProfile = CCS_WeaponAttachmentFitProfileApplicator.AttachmentRootMatchesProfile(
                    equippedAttachmentRoot,
                    profile,
                    definitionPosition,
                    definitionEuler,
                    definitionScale);
            }

            Debug.Log(
                "[Equipment Visual] Runtime Equipped Fit Parity\n"
                + "Profile:\n"
                + "  Position="
                + FormatVector3(profile.SocketLocalPosition)
                + "\n  Rotation="
                + FormatVector3(profile.SocketLocalEulerAngles)
                + "\n  Scale="
                + FormatVector3(profile.SocketLocalScale)
                + "\nRuntime Attachment Root:\n"
                + "  LocalPosition="
                + FormatVector3(equippedAttachmentRoot.localPosition)
                + "\n  LocalEuler="
                + FormatVector3(equippedAttachmentRoot.localEulerAngles)
                + "\n  LocalScale="
                + FormatVector3(equippedAttachmentRoot.localScale)
                + "\nRuntime Visual Child:\n"
                + "  LocalPosition="
                + (visualTransform != null ? FormatVector3(visualTransform.localPosition) : "(missing)")
                + "\n  LocalRotation="
                + (visualTransform != null ? FormatVector3(visualTransform.localEulerAngles) : "(missing)")
                + "\n  LocalScale="
                + (visualTransform != null ? FormatVector3(visualTransform.localScale) : "(missing)")
                + "\nParity:\n"
                + "  AttachmentRootMatchesProfile="
                + attachmentMatchesProfile
                + "\n  VisualZeroed="
                + visualZeroed
                + "\n  VisualAimConvergenceActive=false");
        }

        private static Transform ResolveEquippedMuzzlePoint(Transform visualRoot)
        {
            if (visualRoot == null)
            {
                return null;
            }

            Transform muzzle = visualRoot.Find("MuzzlePoint");
            if (muzzle == null)
            {
                muzzle = visualRoot.Find("FitGuides/MuzzlePoint");
            }

            if (muzzle == null)
            {
                Transform[] children = visualRoot.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null && children[i].name == "MuzzlePoint")
                    {
                        muzzle = children[i];
                        break;
                    }
                }
            }

            return muzzle;
        }

        private static string FormatVector3(Vector3 value)
        {
            return "("
                + value.x.ToString("0.00")
                + ","
                + value.y.ToString("0.00")
                + ","
                + value.z.ToString("0.00")
                + ")";
        }

        private static string FormatCompactVector3(Vector3 value)
        {
            return "("
                + value.x.ToString("0.00")
                + ","
                + value.y.ToString("0.00")
                + ","
                + value.z.ToString("0.00")
                + ")";
        }

        private static void ZeroLocalTransform(Transform target)
        {
            if (target == null)
            {
                return;
            }

            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        private static void StripRuntimeGameplayComponents(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    Destroy(colliders[i]);
                }
            }

            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null)
                {
                    Destroy(behaviour);
                }
            }
        }

        private bool TryGetSocket(string socketId, out Transform socketTransform)
        {
            socketTransform = null;
            if (equipmentSocketRegistry == null || string.IsNullOrEmpty(socketId))
            {
                return false;
            }

            return equipmentSocketRegistry.TryGetSocket(socketId, out socketTransform);
        }

        private void DestroyRuntimeVisuals()
        {
            HideHolsteredVisual();
            HideEquippedVisual();
            HideDiagnosticsEquippedPreview();

            DestroyVisualInstance(ref holsteredVisualInstance);
            DestroyVisualInstance(ref equippedVisualInstance);
            DestroyVisualInstance(ref diagnosticsEquippedVisualInstance);
            DestroyAttachmentRoot(ref equippedAimConvergenceRoot);
            equippedAimConvergence = null;
            DestroyAttachmentRoot(ref holsterAttachmentRoot);
            DestroyAttachmentRoot(ref equippedAttachmentRoot);
        }

        private static void DestroyVisualInstance(ref GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            Destroy(instance);
            instance = null;
        }

        private static void DestroyAttachmentRoot(ref Transform attachmentRoot)
        {
            if (attachmentRoot == null)
            {
                return;
            }

            Destroy(attachmentRoot.gameObject);
            attachmentRoot = null;
        }

        private void ResolveReferences()
        {
            if (equipmentSocketRegistry == null)
            {
                equipmentSocketRegistry = GetComponent<CCS_EquipmentSocketRegistry>();
            }

            if (playerWeaponLoadout == null)
            {
                playerWeaponLoadout = GetComponent<CCS_PlayerWeaponLoadout>();
            }

            if (aimLocomotionController == null)
            {
                aimLocomotionController = GetComponent<CCS_CharacterAimLocomotionController>();
            }

            if (weaponCarryStateController == null)
            {
                weaponCarryStateController = GetComponent<CCS_WeaponCarryStateController>();
            }
        }

        #endregion
    }
}
