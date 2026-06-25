using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.Common;
using CCS.Modules.CharacterController.Editor.EquipmentFitStudio;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_AnimationFitStudioPreviewUtility
// CATEGORY: Modules / CharacterController / Editor / AnimationFitStudio
// PURPOSE: Loads editor preview player, equipped fit profile, and revolver visual for pose tuning.
// PLACEMENT: Editor utility used by Animation Fit Studio window.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses animation-specific DO_NOT_SAVE hierarchy. Never saves to scene or prefab.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.AnimationFitStudio
{
    public static class CCS_AnimationFitStudioPreviewUtility
    {
        public const string MissingEquippedProfileWarning =
            "No equipped revolver fit profile found. Preview weapon loaded at default hand socket transform.";

        public const string MissingWeaponVisualWarning =
            "Grip editing is available, but no revolver preview is loaded. Load weapon visual to judge finger wrap.";

        public static bool TryLoadPreview(
            CCS_AnimationFitStudioPreviewState previewState,
            AnimationClip aimClip,
            float poseTime,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (previewState == null)
            {
                errorMessage = "Preview state is not initialized.";
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                errorMessage = "Animation Fit Studio preview works in Edit Mode only.";
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                errorMessage = "Open a scene before loading the editor preview.";
                return false;
            }

            CCS_AnimationFitStudioCleanupUtility.CleanupPreviewObjectsInOpenScenes();
            previewState.Clear();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                errorMessage = "Missing test player prefab at "
                    + CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath;
                return false;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, activeScene) as GameObject;
            if (instance == null)
            {
                errorMessage = "Could not instantiate animation fit preview player.";
                return false;
            }

            instance.name = CCS_AnimationFitStudioConstants.PreviewPlayerObjectName;
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            instance.transform.SetPositionAndRotation(
                CCS_EquipmentFitStudioPreviewPlayerUtility.DefaultPreviewPlayerPosition,
                Quaternion.identity);
            PreparePreviewPlayerForEditMode(instance);

            previewState.PreviewPlayer = instance;
            previewState.PreviewAnimator = CCS_EquipmentFitStudioPosePreviewUtility.FindPlayerAnimator(instance);
            previewState.RightHandBonesFound = HasRightHandBones(previewState.PreviewAnimator);
            PreparePreviewAnimatorForPoseSampling(previewState);

            if (!TryAttachEquippedRevolverVisual(previewState, out errorMessage))
            {
                Object.DestroyImmediate(instance);
                previewState.Clear();
                return false;
            }

            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(activeScene);
            return true;
        }

        public static void RefreshWeaponAttachmentAfterPoseSample(CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState?.PreviewPlayer == null)
            {
                return;
            }

            TryAttachEquippedRevolverVisual(previewState, out _);
        }

        public static bool TryAttachEquippedRevolverVisual(
            CCS_AnimationFitStudioPreviewState previewState,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (previewState?.PreviewPlayer == null)
            {
                errorMessage = "Load preview player first.";
                return false;
            }

            DestroyExistingWeaponPreview(previewState.PreviewPlayer);

            CCS_EquipmentSocketRegistry registry =
                previewState.PreviewPlayer.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry == null
                || !registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                errorMessage = "Preview player is missing right hand socket.";
                return false;
            }

            previewState.EquippedFitProfile =
                CCS_EquipmentFitStudioRevolverFitUtility.LoadRevolverAttachmentFitProfile(
                    CCS_EquipmentConstants.HandSocketRightId);

            Transform attachmentRoot = CCS_EquipmentFitStudioTestAttachmentUtility.EnsureEditorAttachmentRoot(
                socketTransform,
                CCS_AnimationFitStudioConstants.WeaponAttachmentRootObjectName);
            previewState.WeaponAttachmentRoot = attachmentRoot;

            if (previewState.EquippedFitProfile != null
                && CCS_EquipmentFitStudioTestAttachmentUtility.TryGetSocketDefinition(
                    registry,
                    CCS_EquipmentConstants.HandSocketRightId,
                    out Vector3 definitionPosition,
                    out Vector3 definitionEuler,
                    out Vector3 definitionScale))
            {
                CCS_WeaponAttachmentFitProfileApplicator.ApplyProfileToAttachmentRoot(
                    socketTransform,
                    attachmentRoot,
                    previewState.EquippedFitProfile,
                    definitionPosition,
                    definitionEuler,
                    definitionScale);
                previewState.EquippedFitProfileApplied = true;
                previewState.ProfileWarningMessage = string.Empty;
            }
            else
            {
                attachmentRoot.localPosition = Vector3.zero;
                attachmentRoot.localRotation = Quaternion.identity;
                attachmentRoot.localScale = Vector3.one;
                previewState.EquippedFitProfileApplied = false;
                previewState.ProfileWarningMessage = MissingEquippedProfileWarning;
            }

            GameObject visualSourcePrefab = ResolveRevolverVisualSourcePrefab();
            if (visualSourcePrefab == null)
            {
                errorMessage = "Missing revolver visual source prefab.";
                previewState.WeaponVisualLoaded = false;
                return false;
            }

            GameObject weaponVisual = CCS_EquipmentFitStudioVisualSourceUtility.SpawnEditorVisualUnderSocket(
                attachmentRoot,
                visualSourcePrefab,
                CCS_AnimationFitStudioConstants.PreviewWeaponObjectName,
                hideInHierarchy: true);
            if (weaponVisual == null)
            {
                errorMessage = "Could not spawn revolver preview visual under attachment root.";
                previewState.WeaponVisualLoaded = false;
                return false;
            }

            weaponVisual.transform.localPosition = Vector3.zero;
            weaponVisual.transform.localRotation = Quaternion.identity;
            weaponVisual.transform.localScale = Vector3.one;

            previewState.PreviewWeaponVisual = weaponVisual;
            previewState.WeaponVisualLoaded = true;
            previewState.PreviewWeaponZeroed = IsZeroedLocalTransform(weaponVisual.transform);
            return true;
        }

        public static GameObject ResolveRevolverVisualSourcePrefab()
        {
            GameObject visualOnly = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879VisualOnlyPrefabPath);
            if (visualOnly != null)
            {
                return visualOnly;
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
        }

        public static Transform GetDefaultCameraFrameTarget(CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState?.PreviewWeaponVisual != null)
            {
                return previewState.PreviewWeaponVisual.transform;
            }

            if (previewState?.WeaponAttachmentRoot != null)
            {
                return previewState.WeaponAttachmentRoot;
            }

            if (previewState?.PreviewPlayer == null)
            {
                return null;
            }

            CCS_EquipmentSocketRegistry registry =
                previewState.PreviewPlayer.GetComponent<CCS_EquipmentSocketRegistry>();
            if (registry != null
                && registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out Transform socketTransform))
            {
                return socketTransform;
            }

            return previewState.PreviewPlayer.transform;
        }

        public static void DestroyPreviewArtifacts(
            CCS_AnimationFitStudioPreviewState previewState,
            CCS_EquipmentFitStudioPreviewCamera previewCamera)
        {
            CCS_AnimationFitStudioPoseUtility.StopAnimationModeIfNeeded();
            if (previewState != null)
            {
                CCS_AnimationFitStudioPlayablePreviewSampler.RestoreAnimatorController(previewState);
                previewState.Clear();
            }

            CCS_AnimationFitStudioCleanupUtility.CleanupAllPreviewArtifacts(previewCamera);
        }

        private static void PreparePreviewPlayerForEditMode(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            DisableBehaviour<CCS_PlayerEquipmentVisualController>(instance);
            DisableBehaviour<CCS_RevolverController>(instance);
            DisableBehaviour<CCS_RevolverArmReticleIK>(instance);
        }

        private static void PreparePreviewAnimatorForPoseSampling(CCS_AnimationFitStudioPreviewState previewState)
        {
            if (previewState?.PreviewAnimator == null)
            {
                return;
            }

            Animator animator = previewState.PreviewAnimator;
            if (previewState.StoredRuntimeAnimatorController == null && animator.runtimeAnimatorController != null)
            {
                previewState.StoredRuntimeAnimatorController = animator.runtimeAnimatorController;
            }

            CCS_RevolverAimPreviewPoseUtility.PrepareAnimatorForPreview(animator);
            previewState.FingerDiscovery = CCS_AnimationFitStudioFingerDiscoveryUtility.Discover(animator);
            previewState.FingerBonesFound = previewState.FingerDiscovery.AnyFingerBonesFound;
        }

        private static void DisableBehaviour<T>(GameObject root) where T : Behaviour
        {
            T[] behaviours = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviours[i].enabled = false;
                }
            }
        }

        private static void DestroyExistingWeaponPreview(GameObject previewPlayer)
        {
            if (previewPlayer == null)
            {
                return;
            }

            Transform[] transforms = previewPlayer.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (transform == null)
                {
                    continue;
                }

                if (transform.name == CCS_AnimationFitStudioConstants.WeaponAttachmentRootObjectName
                    || transform.name == CCS_AnimationFitStudioConstants.PreviewWeaponObjectName)
                {
                    Object.DestroyImmediate(transform.gameObject);
                }
            }
        }

        private static bool HasRightHandBones(Animator animator)
        {
            if (animator == null || !animator.isHuman)
            {
                return false;
            }

            return animator.GetBoneTransform(HumanBodyBones.RightHand) != null
                && animator.GetBoneTransform(HumanBodyBones.RightUpperArm) != null
                && animator.GetBoneTransform(HumanBodyBones.RightLowerArm) != null;
        }

        private static bool IsZeroedLocalTransform(Transform transform)
        {
            return transform != null
                && transform.localPosition == Vector3.zero
                && transform.localRotation == Quaternion.identity
                && transform.localScale == Vector3.one;
        }
    }
}
