using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioValidationUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Validates Fit Studio assets, cleanup, and existing equipment foundation.
// PLACEMENT: Editor validator invoked from batch and Save/Validate tab.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Fails if preview objects remain in scenes or prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateEquipmentFitStudioFoundation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.EquipmentFitStudioSettingsPath),
                "Missing CCS_EquipmentFitStudioSettings.asset.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingProfileRootPath),
                "Missing Profiles/EquipmentFitting folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingIkProfileFolderPath),
                "Missing Profiles/EquipmentFitting/IK folder.");
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.EquipmentFittingHandPoseFolderPath),
                "Missing Profiles/EquipmentFitting/HandPoses folder.");

            CCS_EquipmentFitStudioSettings settings = AssetDatabase.LoadAssetAtPath<CCS_EquipmentFitStudioSettings>(
                CCS_EquipmentConstants.EquipmentFitStudioSettingsPath);
            if (settings != null)
            {
                AppendIfMissing(
                    failures,
                    settings.DefaultSocketProfile != null,
                    "Equipment Fit Studio settings must assign defaultSocketProfile.");
                AppendIfMissing(
                    failures,
                    settings.NudgePositionSmall > 0f && settings.NudgePositionLarge > 0f,
                    "Equipment Fit Studio nudge position values must be greater than zero.");
                AppendIfMissing(
                    failures,
                    settings.NudgeRotationSmall > 0f && settings.NudgeRotationLarge > 0f,
                    "Equipment Fit Studio nudge rotation values must be greater than zero.");
                AppendIfMissing(
                    failures,
                    settings.PreviewCameraNearClip > 0f && settings.PreviewCameraFarClip > settings.PreviewCameraNearClip,
                    "Equipment Fit Studio preview camera clip planes must be valid.");
            }

            ValidatePreviewObjectCleanup(failures);
            ValidateWorldPickupPreviewSource(failures);
            AppendResult(failures, CCS_EquipmentSocketValidationUtility.ValidateAnimationRiggingPackageInstalled());
            AppendResult(failures, CCS_EquipmentSocketValidationUtility.ValidateDefaultEquipmentSocketProfile());

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab != null)
            {
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerEquipmentSocketFoundation(playerPrefab));
                AppendResult(
                    failures,
                    CCS_EquipmentSocketValidationUtility.ValidatePlayerWeaponIkFoundation(playerPrefab));
                ValidatePlayerIkWeightsDefault(failures, playerPrefab);
            }

            ValidateRevolverM1879FitProfilePack(failures);
            ValidateRevolverFitProfilePersistence(failures);
            ValidateNoProductionWeaponVisualArtifacts(failures);
            AppendResult(failures, CCS_EquipmentFitStudioCaptureUtility.ValidateCaptureSaveWorkflowRouting());
            AppendResult(failures, CCS_EquipmentFitStudioWorkflowSessionUtility.ValidateActiveTargetWorkflowRouting());
            AppendResult(failures, CCS_EquipmentFitStudioPosePreviewUtility.ValidatePosePreviewFoundation());
            AppendResult(failures, CCS_EquipmentFitStudioPreviewPlayerUtility.ValidatePreviewPlayerFoundation());
            ValidateEditFitPreviewModeFoundation(failures);
            AppendResult(failures, CCS_EquipmentFitStudioFitTargetRoutingUtility.ValidateFitTargetRoutingFoundation());
            ValidateEditorOnlyRevampFoundation(failures);
            AppendResult(failures, CCS_EquipmentFitStudioEquippedPoseGuidanceUtility.ValidateEquippedPoseGuidanceFoundation());
            ValidateRuntimeBridgeFoundation(failures);
            ValidateDeferredIkFoundation(failures);
            AppendIfMissing(
                failures,
                !CCS_EquipmentFitStudioImGuiUtility.HasLayoutError,
                "Equipment Fit Studio must not log IMGUI EndLayoutGroup errors.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Equipment Fit Studio foundation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverM1879FitProfilePackOnly()
        {
            List<string> failures = new List<string>();
            ValidateRevolverM1879FitProfilePack(failures);
            ValidatePreviewObjectCleanup(failures);
            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Revolver M1879 fit profile pack validated.");
        }

        public static bool SceneContainsPreviewObjects()
        {
            return SceneContainsEditorTemporaryObjects();
        }

        public static bool SceneContainsEditorTemporaryObjects()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    if (ContainsEditorTemporaryObjectRecursive(roots[r].transform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static void ValidateRevolverM1879FitProfilePack(List<string> failures)
        {
            AppendIfMissing(
                failures,
                Directory.Exists(CCS_EquipmentConstants.RevolverM1879FitProfileFolderPath),
                "Missing Profiles/EquipmentFitting/RevolverM1879 folder.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath),
                "Missing CCS_RevolverM1879_RightHipHolster_Fit.asset.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath),
                "Missing CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879AimIkPosePath),
                "Missing CCS_RevolverM1879_AimIKPose.asset.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_EquipmentConstants.RevolverM1879RightHandGripPosePath),
                "Missing CCS_RevolverM1879_RightHandGripPose.asset.");

            ValidateAttachmentFitProfile(
                failures,
                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath,
                CCS_EquipmentConstants.HolsterSocketRightHipId);
            ValidateAttachmentFitProfile(
                failures,
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath,
                CCS_EquipmentConstants.HandSocketRightId);
            ValidateRightHipHolsterProfileTuned(failures);
            ValidateIkPoseProfile(failures, CCS_EquipmentConstants.RevolverM1879AimIkPosePath);
            ValidateHandPoseProfile(failures, CCS_EquipmentConstants.RevolverM1879RightHandGripPosePath);
            ValidateNoPreviewItemsUnderPlayerSockets(failures);
        }

        private static void ValidateRevolverFitProfilePersistence(List<string> failures)
        {
            CCS_WeaponAttachmentFitProfile holsterFromDisk =
                CCS_EquipmentFitProfilePersistenceUtility.LoadHolsterProfileFromDisk();
            CCS_WeaponAttachmentFitProfile equippedFromDisk =
                CCS_EquipmentFitProfilePersistenceUtility.LoadEquippedProfileFromDisk();
            AppendIfMissing(
                failures,
                holsterFromDisk != null,
                "Right hip holster fit profile could not be loaded from disk.");
            AppendIfMissing(
                failures,
                equippedFromDisk != null,
                "Right hand equipped fit profile could not be loaded from disk.");

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            CCS_PlayerEquipmentVisualController visualController =
                playerPrefab.GetComponent<CCS_PlayerEquipmentVisualController>();
            if (visualController == null)
            {
                failures.Add("Test player must contain CCS_PlayerEquipmentVisualController for fit profile validation.");
                return;
            }

            AppendIfMissing(
                failures,
                visualController.RightHipHolsterFitProfile != null,
                "Equipment visual controller must reference right hip holster fit profile.");
            AppendIfMissing(
                failures,
                visualController.RightHandEquippedFitProfile != null,
                "Equipment visual controller must reference right hand equipped fit profile.");

            if (visualController.RightHipHolsterFitProfile != null)
            {
                string holsterPath = AssetDatabase.GetAssetPath(visualController.RightHipHolsterFitProfile);
                AppendIfMissing(
                    failures,
                    holsterPath == CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath,
                    "Equipment visual controller must reference CCS_RevolverM1879_RightHipHolster_Fit.asset.");
            }

            if (visualController.RightHandEquippedFitProfile != null)
            {
                string equippedPath = AssetDatabase.GetAssetPath(visualController.RightHandEquippedFitProfile);
                AppendIfMissing(
                    failures,
                    equippedPath == CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath,
                    "Equipment visual controller must reference CCS_RevolverM1879_RightHandEquipped_Fit.asset.");
            }

            string visualControllerPath =
                CCS_WeaponsConstants.ModuleRootPath + "/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            if (File.Exists(visualControllerPath))
            {
                string visualSource = File.ReadAllText(visualControllerPath);
                AppendIfMissing(
                    failures,
                    visualSource.Contains("ReloadFitProfilesFromDisk"),
                    "Equipment visual controller must reload fit profiles from disk in editor.");
                AppendIfMissing(
                    failures,
                    visualSource.Contains("CCS_WeaponAttachmentFitProfileApplicator"),
                    "Equipment visual controller must apply saved profiles through attachment-root applicator.");
                AppendIfMissing(
                    failures,
                    !visualSource.Contains("0.11f, -0.04f, 0.05f"),
                    "Equipment visual controller must not hardcode holster seed/default fit values.");
            }

            string builderPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_RevolverM1879FitProfileBuilder.cs";
            if (File.Exists(builderPath))
            {
                string builderSource = File.ReadAllText(builderPath);
                AppendIfMissing(
                    failures,
                    builderSource.Contains("Preserved existing revolver fit profile values"),
                    "Revolver fit profile builder must preserve existing tuned profile assets.");
            }
        }

        private static void ValidateRightHipHolsterProfileTuned(List<string> failures)
        {
            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHipHolsterFitPath);
            if (profile == null)
            {
                return;
            }

            Vector3 seedPosition = CCS_EquipmentFitProfilePersistenceUtility.RightHipHolsterSeedPosition;
            Vector3 seedEuler = CCS_EquipmentFitProfilePersistenceUtility.RightHipHolsterSeedEuler;
            bool matchesSeed =
                CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(profile.SocketLocalPosition, seedPosition)
                && CCS_EquipmentFitStudioPendingChange.VectorsApproximatelyEqual(
                    profile.SocketLocalEulerAngles,
                    seedEuler);
            AppendIfMissing(
                failures,
                !matchesSeed,
                "Right hip holster fit profile still matches builder seed values. Save tuned Fit Studio values before release.");
        }

        private static void ValidateAttachmentFitProfile(List<string> failures, string assetPath, string expectedSocketId)
        {
            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(assetPath);
            AppendIfMissing(failures, profile != null, "Missing attachment fit profile: " + assetPath);
            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                profile.WeaponId == CCS_EquipmentConstants.RevolverM1879WeaponId,
                assetPath + " weaponId must be " + CCS_EquipmentConstants.RevolverM1879WeaponId + ".");
            AppendIfMissing(
                failures,
                !string.IsNullOrEmpty(profile.CharacterRigId),
                assetPath + " characterRigId must not be empty.");
            AppendIfMissing(
                failures,
                profile.CharacterRigId == CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId,
                assetPath + " characterRigId must be " + CCS_EquipmentConstants.TestPlayerCc3BasePlusRigId + ".");
            AppendIfMissing(
                failures,
                profile.SocketId == expectedSocketId,
                assetPath + " socketId must be " + expectedSocketId + ".");
            AppendIfMissing(
                failures,
                profile.SocketLocalScale != Vector3.zero,
                assetPath + " socketLocalScale must not be zero.");
        }

        private static void ValidateIkPoseProfile(List<string> failures, string assetPath)
        {
            CCS_WeaponIKPoseProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponIKPoseProfile>(assetPath);
            AppendIfMissing(failures, profile != null, "Missing IK pose profile: " + assetPath);
            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                profile.WeaponId == CCS_EquipmentConstants.RevolverM1879WeaponId,
                assetPath + " weaponId must be " + CCS_EquipmentConstants.RevolverM1879WeaponId + ".");
            AppendIfMissing(
                failures,
                !string.IsNullOrEmpty(profile.CharacterRigId),
                assetPath + " characterRigId must not be empty.");
            AppendIfMissing(
                failures,
                profile.PoseId == CCS_EquipmentConstants.RevolverM1879AimPoseId,
                assetPath + " poseId must be " + CCS_EquipmentConstants.RevolverM1879AimPoseId + ".");
            AppendIfMissing(failures, profile.RigWeight == 0f, assetPath + " rigWeight must default to 0.");
            AppendIfMissing(failures, profile.RightHandIKWeight == 0f, assetPath + " rightHandIKWeight must default to 0.");
            AppendIfMissing(failures, profile.LeftHandIKWeight == 0f, assetPath + " leftHandIKWeight must default to 0.");
            AppendIfMissing(failures, profile.AimWeight == 0f, assetPath + " aimWeight must default to 0.");
        }

        private static void ValidateHandPoseProfile(List<string> failures, string assetPath)
        {
            CCS_HandPoseDefinition profile = AssetDatabase.LoadAssetAtPath<CCS_HandPoseDefinition>(assetPath);
            AppendIfMissing(failures, profile != null, "Missing hand pose profile: " + assetPath);
            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                profile.WeaponId == CCS_EquipmentConstants.RevolverM1879WeaponId,
                assetPath + " weaponId must be " + CCS_EquipmentConstants.RevolverM1879WeaponId + ".");
            AppendIfMissing(
                failures,
                !string.IsNullOrEmpty(profile.CharacterRigId),
                assetPath + " characterRigId must not be empty.");
            AppendIfMissing(
                failures,
                profile.PoseId == CCS_EquipmentConstants.RevolverM1879RightHandGripPoseId,
                assetPath + " poseId must be " + CCS_EquipmentConstants.RevolverM1879RightHandGripPoseId + ".");
            AppendIfMissing(
                failures,
                profile.HandSide == CCS_HandPoseSide.Right,
                assetPath + " handSide must be Right.");
            AppendIfMissing(
                failures,
                !string.IsNullOrEmpty(profile.Notes) && profile.Notes.Contains("Foundation"),
                assetPath + " must include foundation-only hand pose notes.");
        }

        private static void ValidateNoPreviewItemsUnderPlayerSockets(List<string> failures)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            CCS_EquipmentSocketAnchor[] anchors = playerPrefab.GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true);
            for (int i = 0; i < anchors.Length; i++)
            {
                CCS_EquipmentSocketAnchor anchor = anchors[i];
                if (anchor == null)
                {
                    continue;
                }

                for (int c = 0; c < anchor.transform.childCount; c++)
                {
                    Transform child = anchor.transform.GetChild(c);
                    if (CCS_EquipmentFitStudioCleanupUtility.IsEditorTemporaryObjectName(child.name))
                    {
                        failures.Add("Player prefab socket " + anchor.SocketId + " must not contain editor temporary objects.");
                    }
                }
            }
        }

        private static void ValidateNoProductionWeaponVisualArtifacts(List<string> failures)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = playerPrefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName == "CCS_RevolverWeaponVisualFeedback")
                {
                    failures.Add("Player prefab must not contain legacy CCS_RevolverWeaponVisualFeedback.");
                }
            }

            AppendResult(failures, CCS_WeaponsValidationUtility.ValidateRevolverOwnershipAndMuzzleContract());
        }

        private static void ValidatePreviewObjectCleanup(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !SceneContainsEditorTemporaryObjects(),
                "Open scenes must not contain editor preview or test attachment objects.");
            AppendIfMissing(
                failures,
                !PrefabContainsEditorTemporaryObjects(
                    CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath),
                "Networked test player prefab must not contain editor temporary objects.");
            AppendIfMissing(
                failures,
                !PrefabContainsEditorTemporaryObjects(CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath),
                "World pickup prefab must not contain editor temporary objects.");
        }

        private static void ValidateWorldPickupPreviewSource(List<string> failures)
        {
            GameObject worldPickup = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_WeaponsConstants.RevolverM1879WorldPickupPrefabPath);
            if (worldPickup == null)
            {
                failures.Add("Missing world pickup prefab for preview source validation.");
                return;
            }

            Transform modelRoot = worldPickup.transform.Find(CCS_WeaponsConstants.RevolverModelRootObjectName);
            Transform revolverVisual = modelRoot != null
                ? modelRoot.Find(CCS_WeaponsConstants.RevolverMaterializedVisualChildName)
                : null;
            AppendIfMissing(failures, modelRoot != null, "World pickup must contain ModelRoot.");
            AppendIfMissing(failures, revolverVisual != null, "World pickup must contain ModelRoot/RevolverVisual.");
            AppendIfMissing(
                failures,
                worldPickup.transform.Find("RevolverMesh") == null,
                "World pickup must not contain top-level RevolverMesh.");
        }

        private static void ValidatePlayerIkWeightsDefault(List<string> failures, GameObject playerPrefab)
        {
            Transform visualRoot = FindDeepChild(playerPrefab.transform, CCS_EquipmentConstants.VisualRootObjectName);
            if (visualRoot == null)
            {
                return;
            }

            Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)
                ?.GetComponent<Rig>();
            if (rig != null && rig.weight != 0f)
            {
                failures.Add("Player Rig_WeaponIK weight must default to 0.");
            }

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null && constraints[i].weight != 0f)
                {
                    failures.Add("Player IK constraint weights must default to 0.");
                    break;
                }
            }
        }

        private static bool PrefabContainsEditorTemporaryObjects(string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                return false;
            }

            string text = File.ReadAllText(prefabPath);
            for (int i = 0; i < CCS_EquipmentConstants.EditorTemporaryObjectNames.Length; i++)
            {
                if (text.Contains("m_Name: " + CCS_EquipmentConstants.EditorTemporaryObjectNames[i]))
                {
                    return true;
                }
            }

            for (int i = 0; i < CCS_EquipmentConstants.RuntimeTemporaryObjectNames.Length; i++)
            {
                if (text.Contains("m_Name: " + CCS_EquipmentConstants.RuntimeTemporaryObjectNames[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsEditorTemporaryObjectRecursive(Transform root)
        {
            if (root == null)
            {
                return false;
            }

            if (CCS_EquipmentFitStudioCleanupUtility.IsEditorTemporaryObjectName(root.name)
                || CCS_EquipmentFitStudioCleanupUtility.IsRuntimeTemporaryObjectName(root.name))
            {
                return true;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                if (ContainsEditorTemporaryObjectRecursive(root.GetChild(i)))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateEditFitPreviewModeFoundation(List<string> failures)
        {
            string previewPlayerUtilityPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioPreviewPlayerUtility.cs";
            AppendIfMissing(failures, File.Exists(previewPlayerUtilityPath), "Missing CCS_EquipmentFitStudioPreviewPlayerUtility.");

            bool includesPreviewPlayerName = false;
            for (int i = 0; i < CCS_EquipmentConstants.EditorTemporaryObjectNames.Length; i++)
            {
                if (CCS_EquipmentConstants.EditorTemporaryObjectNames[i]
                    == CCS_EquipmentConstants.EditorFitPreviewPlayerObjectName)
                {
                    includesPreviewPlayerName = true;
                    break;
                }
            }

            AppendIfMissing(
                failures,
                includesPreviewPlayerName,
                "Editor temporary object list must include the editor preview player name.");

            AppendIfMissing(
                failures,
                CCS_EquipmentFitStudioPreviewPlayerUtility.FindExistingPreviewPlayer() == null,
                "Open scenes must not contain editor preview player before validation.");

            AppendIfMissing(
                failures,
                CCS_EquipmentFitStudioPosePreviewUtility.GetDefaultPosePreviewForSocket(
                    CCS_EquipmentConstants.HolsterSocketRightHipId)
                    == CCS_EquipmentFitStudioPosePreviewMode.Neutral,
                "Holstered Item must default to Neutral pose preview.");

            AppendIfMissing(
                failures,
                CCS_EquipmentFitStudioPosePreviewUtility.GetDefaultPosePreviewForSocket(
                    CCS_EquipmentConstants.HandSocketRightId)
                    == CCS_EquipmentFitStudioPosePreviewMode.RevolverAim,
                "Equipped Item must default to Revolver Aim pose preview.");
        }

        private static void ValidateEditorOnlyRevampFoundation(List<string> failures)
        {
            string revampPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.Revamp.cs";
            AppendIfMissing(failures, File.Exists(revampPath), "Missing CCS_EquipmentFitStudioWindow.Revamp.cs.");

            string fitTargetPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioFitTarget.cs";
            AppendIfMissing(failures, File.Exists(fitTargetPath), "Missing CCS_EquipmentFitStudioFitTarget.cs.");

            string routingPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioFitTargetRoutingUtility.cs";
            AppendIfMissing(failures, File.Exists(routingPath), "Missing CCS_EquipmentFitStudioFitTargetRoutingUtility.");

            string autoLoadPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioAutoLoadUtility.cs";
            AppendIfMissing(failures, File.Exists(autoLoadPath), "Missing CCS_EquipmentFitStudioAutoLoadUtility.");

            if (File.Exists(revampPath))
            {
                string revampSource = File.ReadAllText(revampPath);
                string windowPath =
                    CCS_CharacterControllerConstants.ModuleRootPath
                    + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";
                if (File.Exists(windowPath))
                {
                    string windowSource = File.ReadAllText(windowPath);
                    AppendIfMissing(
                        failures,
                        windowSource.Contains("MinWindowWidth = 1200f"),
                        "Fit Studio minimum window width must be 1200.");
                    AppendIfMissing(
                        failures,
                        windowSource.Contains("MinWindowHeight = 700f"),
                        "Fit Studio minimum window height must be 700.");
                    AppendIfMissing(
                        failures,
                        windowSource.Contains("DefaultWindowWidth = 1450f"),
                        "Fit Studio default window width must be 1450.");
                    AppendIfMissing(
                        failures,
                        windowSource.Contains("DefaultWindowHeight = 820f"),
                        "Fit Studio default window height must be 820.");
                }

                AppendIfMissing(
                    failures,
                    revampSource.Contains("Editor Mode Only • Profile Tuning"),
                    "Fit Studio header must state Editor Mode Only • Profile Tuning.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Fit Target"),
                    "Fit Studio revamp must expose Fit Target as the first control.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("FitTargetRoutingUtility.FitTargetLabels"),
                    "Fit Studio revamp must bind Fit Target dropdown labels.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Finger and palm IK are deferred"),
                    "Fit Studio revamp must document deferred finger/palm IK.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Equipment Fit Studio works in Editor Mode only."),
                    "Play Mode must show a single editor-only message.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Exit Play Mode to edit equipment fit profiles."),
                    "Play Mode message must direct users to exit Play Mode.");
                AppendIfMissing(
                    failures,
                    !revampSource.Contains("Runtime Verification (Read-Only)"),
                    "Play Mode must not show runtime verification sections.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Save Profile"),
                    "Fit Studio revamp must expose Save Profile action.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Attachment / Profile Offset"),
                    "Fit Studio revamp must edit attachment/profile offset only.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Preview Zeroed"),
                    "Fit Studio must expose preview zeroed status chips.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Fit Target Default"),
                    "Camera controls must live outside the preview viewport.");
                AppendIfMissing(
                    failures,
                    revampSource.Contains("Orbit: Left Drag"),
                    "Fit Studio must document preview camera mouse controls.");
                AppendIfMissing(
                    failures,
                    !revampSource.Contains("DrawRevampedCameraPresetOverlay"),
                    "Fit Studio must not draw camera preset buttons inside the viewport.");
                AppendIfMissing(
                    failures,
                    !revampSource.Contains("Force Aim"),
                    "Fit Studio primary workflow must not expose Force Aim controls.");
                AppendIfMissing(
                    failures,
                    !revampSource.Contains("Play Mode Aim Fit"),
                    "Fit Studio primary workflow must not expose Play Mode Aim Fit.");
            }

            string cameraPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioPreviewCamera.cs";
            if (File.Exists(cameraPath))
            {
                string cameraSource = File.ReadAllText(cameraPath);
                AppendIfMissing(
                    failures,
                    cameraSource.Contains("isDraggingLook"),
                    "Preview camera must support right-drag look rotation.");
                AppendIfMissing(
                    failures,
                    cameraSource.Contains("KeyCode.F"),
                    "Preview camera must support F key framing.");
                AppendIfMissing(
                    failures,
                    cameraSource.Contains("WeaponCloseUp"),
                    "Preview camera must expose Weapon Close-Up preset.");
            }
        }

        private static void ValidateRuntimeBridgeFoundation(List<string> failures)
        {
            string visualPath =
                "Assets/CCS/Modules/Weapons/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";
            AppendIfMissing(failures, File.Exists(visualPath), "Missing CCS_PlayerEquipmentVisualController runtime bridge.");

            if (File.Exists(visualPath))
            {
                string source = File.ReadAllText(visualPath);
                AppendIfMissing(
                    failures,
                    source.Contains("CCS_WeaponAttachmentFitProfile"),
                    "Runtime visual bridge must apply weapon attachment fit profiles.");
            }
        }

        private static void ValidateDeferredIkFoundation(List<string> failures)
        {
            string ikDiagnosticsPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioIkDiagnosticsUtility.cs";
            AppendIfMissing(failures, File.Exists(ikDiagnosticsPath), "Missing CCS_EquipmentFitStudioIkDiagnosticsUtility.");

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS.Modules.CharacterController.Tests.CCS_TestPlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (playerPrefab == null)
            {
                return;
            }

            Transform visualRoot = FindDeepChild(playerPrefab.transform, CCS_EquipmentConstants.VisualRootObjectName);
            Animator animator = visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
            if (animator == null)
            {
                return;
            }

            Rig rig = FindDeepChild(animator.transform, CCS_EquipmentConstants.WeaponIkRigObjectName)?.GetComponent<Rig>();
            if (rig != null && rig.weight != 0f)
            {
                failures.Add("Player prefab Rig_WeaponIK weight must default to 0.");
            }

            TwoBoneIKConstraint[] constraints = animator.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            for (int i = 0; i < constraints.Length; i++)
            {
                if (constraints[i] != null && constraints[i].weight != 0f)
                {
                    failures.Add("Player prefab IK constraint weights must default to 0.");
                    break;
                }
            }
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }

        #endregion
    }
}
