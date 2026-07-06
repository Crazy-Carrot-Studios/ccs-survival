using System.Collections.Generic;
using System.IO;
using CCS.Modules.AI;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Interaction;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_StrippedCharacterControllerBaselineValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.13 stripped Character Controller baseline assets and scene.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_StrippedCharacterControllerBaselineValidationUtility
    {
        private const int ExpectedRootMonoBehaviourCount =
            CCS_CharacterControllerConstants.StrippedBaselineExpectedRootMonoBehaviourCountMax;

        private static readonly string[] ForbiddenRootComponentTypeNames =
        {
            "CCS_RevolverController",
            "CCS_PlayerWeaponLoadout",
            "CCS_WeaponCarryStateController",
            "CCS_PlayerEquipmentVisualController",
            "CCS_NetworkInteractionScanner",
        };

        private static readonly string[] ForbiddenModelComponentTypeNames =
        {
            "CCS_SingleRevolverAimAnimator",
            "CCS_RevolverArmReticleIK",
            "CCS_RevolverBodyAimFollowController",
            "CCS_RevolverAimTargetResolver",
            "CCS_RevolverReticleAnimationEventReceiver",
            "CCS_MuzzleDrivenReticleController",
            "CCS_DualRevolverAimConvergenceRigPresenter",
            "CCS_DualRevolverArmAimBiasPresenter",
            "CCS_DualRevolverArmAimConstraintPresenter",
            "CCS_ManualArmRotationBiasPresenter",
        };

        private static readonly string[] ForbiddenPrefabObjectNames =
        {
            "WeaponReticle",
            CCS_EquipmentConstants.WeaponIkTargetsObjectName,
            CCS_EquipmentConstants.WeaponIkRigObjectName,
            CCS_EquipmentConstants.RightHandIkTargetObjectName,
            CCS_EquipmentConstants.RightElbowHintObjectName,
            CCS_EquipmentConstants.LeftHandIkTargetObjectName,
            CCS_EquipmentConstants.LeftElbowHintObjectName,
            CCS_EquipmentConstants.WeaponAimTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverAimRigRootObjectName,
            CCS_CharacterControllerConstants.DualRevolverAimRigLayerObjectName,
            "CCS_DualRevolverArmAimRigLayer",
            "CCS_DualRevolverArmAimRigTargets",
            "RightArmTwoBoneIK",
            "LeftArmTwoBoneIK",
            "RightHandTwoBoneIK",
            "LeftHandTwoBoneIK",
            "WeaponAimConstraint",
            CCS_CharacterControllerConstants.DualRevolverRightAimRigTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverLeftAimRigTargetObjectName,
            CCS_CharacterControllerConstants.DualRevolverRightElbowHintObjectName,
            CCS_CharacterControllerConstants.DualRevolverLeftElbowHintObjectName,
            CCS_CharacterControllerConstants.ManualArmRotationBiasRigLayerObjectName,
            CCS_CharacterControllerConstants.ManualArmAimRigLayerObjectName,
            CCS_CharacterControllerConstants.DualRevolverManualAimTargetsRootObjectName,
            CCS_CharacterControllerConstants.RightUpperArmRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.RightClavicleRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.LeftClavicleRotationBiasTestObjectName,
            CCS_CharacterControllerConstants.RightUpperArmRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.RightClavicleRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.LeftClavicleRotationBiasPoseObjectName,
            CCS_CharacterControllerConstants.RightUpperArmAimTestObjectName,
            CCS_CharacterControllerConstants.RightClavicleAimTestObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmAimTestObjectName,
            CCS_CharacterControllerConstants.LeftClavicleAimTestObjectName,
            CCS_CharacterControllerConstants.RightUpperArmAimSourceObjectName,
            CCS_CharacterControllerConstants.LeftUpperArmAimSourceObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRigObjectName,
            CCS_WeaponsConstants.RevolverArmReticleIkRootObjectName,
            CCS_WeaponsConstants.RightHandReticleIkTargetObjectName,
            CCS_WeaponsConstants.ReticleAimWorldTargetObjectName,
        };

        private static readonly string[] ForbiddenSceneObjectNames =
        {
            "CCS_TestPickupItemSpawner",
            "PF_CCS_TestInteractable_PickupItem",
            "PF_CCS_TestWeaponDamageTarget",
            "CCS_TestWeaponDamageTarget",
            CCS_WeaponsConstants.RevolverM1879WorldPickupInstanceName,
            "PF_CCS_RevolverM1879_WorldPickup",
        };

        public static CCS_SurvivalValidationResult ValidateStrippedCharacterControllerBaseline()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateFulldrawIdleReticleEventRemoved(failures);
            ValidateAnimatorControllerBaseline(failures);
            ValidateFitProfiles(failures);
            ValidateExperimentalLeftAimPreview(failures);
            ValidateNoProceduralArmConvergence(failures, warnings);
            ValidateEquipmentFitStudioPresent(failures);
            ValidateSharedDualAimPresentation(failures);
            ValidateSharedAimPresentationState(failures);
            ValidateAimCameraProfile(failures);
            ValidatePlayerPrefabStripped(failures);
            ValidateNoLegacyAimTargetResolver(failures);
            ValidateRootMonoBehaviourCount(failures);
            ValidateValidationSceneStripped(failures);
            ValidateDiagnosticsManagerStripped(failures);
            ValidateRequiredRuntimeTypes(failures);
            ValidateMissingScripts(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Stripped Character Controller baseline v0.7.13 validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateFulldrawIdleReticleEventRemoved(List<string> failures)
        {
            bool hasEvent = CCS_RevolverFulldrawIdleReticleEventBuilder.TryReadFulldrawIdleReticleEventTime(
                out _,
                out int matchingEventCount);
            AppendIfPresent(
                failures,
                hasEvent && matchingEventCount > 0,
                "Fulldraw_Idle reticle reveal animation event must be removed for stripped baseline.");
        }

        private static void ValidateAnimatorControllerBaseline(List<string> failures)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Missing player Animator Controller.");

            if (controller == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                controller.layers.Length == CCS_CharacterControllerConstants.StrippedBaselineAllowedAnimatorLayerNames.Length,
                "Animator must contain Base Layer, SingleRevolverUpperBody, and experimental SingleRevolverLeftUpperBody.");

            for (int i = 0; i < CCS_CharacterControllerConstants.StrippedBaselineAllowedAnimatorLayerNames.Length; i++)
            {
                string expectedLayer = CCS_CharacterControllerConstants.StrippedBaselineAllowedAnimatorLayerNames[i];
                AppendIfMissing(
                    failures,
                    i < controller.layers.Length && controller.layers[i].name == expectedLayer,
                    "Animator layer index " + i + " must be '" + expectedLayer + "'.");
            }

            HashSet<string> allowedParameters = new HashSet<string>(
                CCS_CharacterControllerConstants.StrippedBaselineAllowedAnimatorParameterNames);
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                string parameterName = controller.parameters[i].name;
                AppendIfPresent(
                    failures,
                    !allowedParameters.Contains(parameterName),
                    "Animator parameter '" + parameterName + "' is not approved for stripped baseline.");
            }
        }

        private static void ValidatePlayerPrefabStripped(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");
            if (prefab == null)
            {
                return;
            }

            for (int i = 0; i < ForbiddenRootComponentTypeNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    HasComponentByTypeName(prefab, ForbiddenRootComponentTypeNames[i]),
                    "Player root must not contain " + ForbiddenRootComponentTypeNames[i] + ".");
            }

            AppendIfMissing(
                failures,
                prefab.GetComponent<CCS_RevolverAimPresentationGate>() != null,
                "Player root must contain CCS_RevolverAimPresentationGate.");

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            AppendIfMissing(failures, modelRoot != null, "Player prefab must contain Model root.");
            if (modelRoot == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                modelRoot.GetComponent<CCS_RevolverAimLayerAnimator>() != null,
                "Model must contain CCS_RevolverAimLayerAnimator.");
            AppendIfMissing(
                failures,
                modelRoot.GetComponent<CCS_PlayerHolsteredRevolverVisualPresenter>() != null,
                "Model must contain CCS_PlayerHolsteredRevolverVisualPresenter.");
            AppendIfMissing(
                failures,
                modelRoot.GetComponent<CCS_PlayerEquippedRevolverAimVisualPresenter>() != null,
                "Model must contain CCS_PlayerEquippedRevolverAimVisualPresenter.");
            AppendIfMissing(
                failures,
                modelRoot.GetComponent<CCS_SharedDualAimPointPresenter>() != null,
                "Model must contain CCS_SharedDualAimPointPresenter.");
            AppendIfPresent(
                failures,
                HasComponentByTypeName(modelRoot.gameObject, "CCS_DualRevolverAimConvergenceRigPresenter"),
                "Model must not contain CCS_DualRevolverAimConvergenceRigPresenter.");
            AppendIfPresent(
                failures,
                HasComponentByTypeName(modelRoot.gameObject, "CCS_DualRevolverArmAimBiasPresenter"),
                "Model must not contain CCS_DualRevolverArmAimBiasPresenter.");
            AppendIfPresent(
                failures,
                HasComponentByTypeName(modelRoot.gameObject, "CCS_DualRevolverArmAimConstraintPresenter"),
                "Model must not contain CCS_DualRevolverArmAimConstraintPresenter.");
            AppendIfPresent(
                failures,
                HasComponentByTypeName(prefab, "CCS_DualRevolverAimConvergenceRigPresenter"),
                "Convergence rig presenter must not be on player root.");

            for (int i = 0; i < ForbiddenModelComponentTypeNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    HasComponentByTypeName(modelRoot.gameObject, ForbiddenModelComponentTypeNames[i]),
                    "Model must not contain " + ForbiddenModelComponentTypeNames[i] + ".");
            }

            for (int i = 0; i < ForbiddenPrefabObjectNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    FindChildByName(prefab.transform, ForbiddenPrefabObjectNames[i]) != null,
                    "Player prefab must not contain object '" + ForbiddenPrefabObjectNames[i] + "'.");
            }

            CCS_CharacterAimLocomotionController aimLocomotion =
                prefab.GetComponent<CCS_CharacterAimLocomotionController>();
            CCS_RevolverAimPresentationGate gate = prefab.GetComponent<CCS_RevolverAimPresentationGate>();
            if (aimLocomotion != null && gate != null)
            {
                SerializedObject serializedAim = new SerializedObject(aimLocomotion);
                SerializedProperty gateProperty = serializedAim.FindProperty("weaponAimGateComponent");
                AppendIfMissing(
                    failures,
                    gateProperty != null && gateProperty.objectReferenceValue == gate,
                    "Aim locomotion must reference CCS_RevolverAimPresentationGate.");
            }
            else
            {
                AppendIfMissing(failures, false, "Aim locomotion and presentation gate must both be present on player root.");
            }
        }

        private static void ValidateNoLegacyAimTargetResolver(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            AppendIfPresent(
                failures,
                HasComponentByTypeName(prefab, "CCS_RevolverAimTargetResolver"),
                "Player prefab must not contain CCS_RevolverAimTargetResolver.");

            Transform rootAiming = prefab.transform.Find(CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName);
            AppendIfPresent(
                failures,
                rootAiming != null,
                "Player root must not contain legacy Aiming aim-target resolver object.");

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            if (modelRoot == null)
            {
                return;
            }

            Transform modelAiming = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            AppendIfMissing(
                failures,
                modelAiming != null,
                "Model must contain Aiming container for passive visual aim reference.");
            AppendIfMissing(
                failures,
                modelAiming != null
                    && modelAiming.Find(CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName) != null,
                "Model/Aiming must contain CCS_DualRevolverVisualAimReference.");
            AppendIfPresent(
                failures,
                modelAiming != null
                    && modelAiming.Find(CCS_CharacterControllerConstants.DualRevolverManualAimTargetsRootObjectName) != null,
                "Legacy CCS_DualRevolverManualAimTargets must not remain on production prefab.");
        }

        private static void ValidateRootMonoBehaviourCount(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            int rootMonoBehaviourCount = prefab.GetComponents<MonoBehaviour>().Length;
            AppendIfPresent(
                failures,
                rootMonoBehaviourCount > ExpectedRootMonoBehaviourCount,
                "Player root MonoBehaviour count exceeds stripped baseline maximum (max "
                + ExpectedRootMonoBehaviourCount
                + ", found "
                + rootMonoBehaviourCount
                + ").");
        }

        private static void ValidateValidationSceneStripped(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            AppendIfMissing(failures, scene.IsValid(), "Could not open validation scene.");
            if (!scene.IsValid())
            {
                return;
            }

            for (int i = 0; i < ForbiddenSceneObjectNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    FindSceneObjectByName(scene, ForbiddenSceneObjectNames[i]) != null,
                    "Validation scene must not contain " + ForbiddenSceneObjectNames[i] + ".");
            }

            CCS_AIBanditSpawner spawner = Object.FindAnyObjectByType<CCS_AIBanditSpawner>();
            if (spawner != null)
            {
                SerializedObject serializedSpawner = new SerializedObject(spawner);
                SerializedProperty autoSpawnProperty = serializedSpawner.FindProperty("autoSpawnOnStart");
                AppendIfPresent(
                    failures,
                    autoSpawnProperty != null && autoSpawnProperty.boolValue,
                    "CCS_AIBanditSpawner autoSpawnOnStart must be disabled in stripped validation scene.");
            }

            CCS_DiagnosticsEnemyBanditController diagnosticsController =
                Object.FindAnyObjectByType<CCS_DiagnosticsEnemyBanditController>();
            AppendIfMissing(
                failures,
                diagnosticsController != null,
                "Validation scene must contain CCS_DiagnosticsEnemyBanditController on diagnostics manager.");
        }

        private static void ValidateDiagnosticsManagerStripped(List<string> failures)
        {
            CCS_CharacterControllerDiagnosticsManager manager =
                Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>();
            AppendIfMissing(failures, manager != null, "Validation scene must contain CCS_DiagnosticsManager.");
            if (manager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            string[] forbiddenSerializedFields =
            {
                "enableRecordingAmbience",
                "ambientAudioPlaylist",
                "applyOnStart",
                "applyInEditorWhenChanged",
                "forceRevolverAimSetupPose",
                "forceRevolverHandSocketPreview",
                "enableDualHolsteredRevolvers",
                "spawnEnemyBandit",
                "enableVerboseLogs",
                "enableCameraDiagnostics",
                "enableAimDiagnostics",
                "enableAnimationDiagnostics",
                "enableInteractionDiagnostics",
                "enableTestDamage",
                "enableVisualDebugHelpers",
                "debugTestingManager",
                "enableArmToReticleIK",
                "enableVisualAimConvergence",
                "reticleMode",
            };

            for (int i = 0; i < forbiddenSerializedFields.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    serializedManager.FindProperty(forbiddenSerializedFields[i]) != null,
                    "Diagnostics manager must not expose " + forbiddenSerializedFields[i] + " in stripped baseline.");
            }

            SerializedProperty enableEnemyProperty = serializedManager.FindProperty("enableEnemy");
            SerializedProperty enableAimPoseProperty = serializedManager.FindProperty("enableAimPose");
            SerializedProperty equipWeaponProperty = serializedManager.FindProperty("equipWeapon");
            AppendIfMissing(failures, enableEnemyProperty != null, "Diagnostics manager must expose enableEnemy.");
            AppendIfMissing(failures, enableAimPoseProperty != null, "Diagnostics manager must expose enableAimPose.");
            AppendIfMissing(failures, equipWeaponProperty != null, "Diagnostics manager must expose equipWeapon.");
            AppendIfMissing(
                failures,
                enableEnemyProperty == null || !enableEnemyProperty.boolValue,
                "Diagnostics manager enableEnemy must default to false.");
            AppendIfMissing(
                failures,
                enableAimPoseProperty == null || !enableAimPoseProperty.boolValue,
                "Diagnostics manager enableAimPose must default to false.");
            AppendIfMissing(
                failures,
                equipWeaponProperty != null && equipWeaponProperty.boolValue,
                "Diagnostics manager equipWeapon must default to true.");

            AppendIfPresent(
                failures,
                manager.GetComponent<CCS_CharacterCameraDebugReporter>() != null,
                "Diagnostics manager must not contain CCS_CharacterCameraDebugReporter in stripped baseline.");
            AppendIfPresent(
                failures,
                manager.GetComponent<CCS_PlayerDiagnosticsInputRouter>() != null,
                "Diagnostics manager must not contain CCS_PlayerDiagnosticsInputRouter in stripped baseline.");
        }

        private static void ValidateFitProfiles(List<string> failures)
        {
            CCS_WeaponAttachmentFitProfile rightHandProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.RightHandEquippedFitPath);
            AppendIfMissing(
                failures,
                rightHandProfile != null,
                "Missing CCS_RevolverM1879_RightHandEquipped_Fit profile.");
            if (rightHandProfile != null)
            {
                AppendIfMissing(
                    failures,
                    Vector3Approximately(
                        rightHandProfile.SocketLocalPosition,
                        CCS_CharacterControllerConstants.StrippedBaselineRightHandEquippedFitPosition),
                    "Right-hand equipped fit profile position must match stripped baseline tuned values.");
                AppendIfMissing(
                    failures,
                    Vector3Approximately(
                        rightHandProfile.SocketLocalEulerAngles,
                        CCS_CharacterControllerConstants.StrippedBaselineRightHandEquippedFitEuler),
                    "Right-hand equipped fit profile euler must match stripped baseline tuned values.");
                AppendIfMissing(
                    failures,
                    Vector3Approximately(rightHandProfile.SocketLocalScale, Vector3.one),
                    "Right-hand equipped fit profile scale must be (1, 1, 1).");
            }

            CCS_WeaponAttachmentFitProfile leftHandProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_RevolverFitProfilePaths.LeftHandEquippedFitPath);
            AppendIfMissing(
                failures,
                leftHandProfile != null,
                "Missing CCS_RevolverM1879_LeftHandEquipped_Fit experimental profile.");
            if (leftHandProfile != null)
            {
                AppendIfMissing(
                    failures,
                    leftHandProfile.SocketId == CCS_EquipmentConstants.HandSocketLeftId,
                    "Left-hand equipped fit profile must target CCS_HandSocket_Left.");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab != null)
            {
                AppendIfMissing(
                    failures,
                    FindChildByName(prefab.transform, CCS_EquipmentConstants.HandSocketLeftId) != null,
                    "Player prefab must contain CCS_HandSocket_Left for experimental dual preview.");
            }
        }

        private static void ValidateExperimentalLeftAimPreview(List<string> failures)
        {
            AvatarMask leftMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                CCS_CharacterControllerConstants.RevolverAimLeftArmMaskPath);
            AppendIfMissing(failures, leftMask != null, "Missing experimental left-arm aim Avatar Mask.");
            AppendIfMissing(
                failures,
                CCS_RevolverUpperBodyLeftArmAimMaskUtility.ValidateMaskConfiguration(leftMask),
                "Left-arm aim mask must include left arm only (no right arm).");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return;
            }

            int leftLayerIndex = -1;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name
                    == CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyLayerName)
                {
                    leftLayerIndex = i;
                    break;
                }
            }

            AppendIfMissing(
                failures,
                leftLayerIndex >= 0,
                "Animator must contain experimental SingleRevolverLeftUpperBody layer.");

            if (leftLayerIndex < 0)
            {
                return;
            }

            AnimatorControllerLayer leftLayer = controller.layers[leftLayerIndex];
            AppendIfMissing(
                failures,
                leftLayer.avatarMask == leftMask,
                "SingleRevolverLeftUpperBody must use AM_CCS_Revolver_UpperBodyLeftArm_Aim mask.");
            AppendIfMissing(
                failures,
                leftLayer.blendingMode == AnimatorLayerBlendingMode.Override,
                "SingleRevolverLeftUpperBody must use Override blending.");
            AppendIfMissing(
                failures,
                !leftLayer.iKPass,
                "SingleRevolverLeftUpperBody must not enable IK pass.");

            AnimatorStateMachine stateMachine = leftLayer.stateMachine;
            if (stateMachine == null)
            {
                return;
            }

            HashSet<string> allowedStates = new HashSet<string>(
                CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyAllowedStateNames);
            ChildAnimatorState[] childStates = stateMachine.states;
            for (int i = 0; i < childStates.Length; i++)
            {
                string stateName = childStates[i].state != null ? childStates[i].state.name : string.Empty;
                AppendIfPresent(
                    failures,
                    !allowedStates.Contains(stateName),
                    "SingleRevolverLeftUpperBody contains unapproved state '" + stateName + "'.");
            }

            for (int i = 0; i < CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyAllowedStateNames.Length; i++)
            {
                string expectedState =
                    CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyAllowedStateNames[i];
                bool found = false;
                for (int s = 0; s < childStates.Length; s++)
                {
                    if (childStates[s].state != null && childStates[s].state.name == expectedState)
                    {
                        found = true;
                        break;
                    }
                }

                AppendIfMissing(
                    failures,
                    found,
                    "SingleRevolverLeftUpperBody must contain state '" + expectedState + "'.");
            }
        }

        private static bool Vector3Approximately(Vector3 actual, Vector3 expected)
        {
            float tolerance = CCS_CharacterControllerConstants.StrippedBaselineFitComparisonTolerance;
            return Mathf.Abs(actual.x - expected.x) <= tolerance
                && Mathf.Abs(actual.y - expected.y) <= tolerance
                && Mathf.Abs(actual.z - expected.z) <= tolerance;
        }

        private static void ValidateAimCameraProfile(List<string> failures)
        {
            CCS_CharacterCameraProfile aimProfile = AssetDatabase.LoadAssetAtPath<CCS_CharacterCameraProfile>(
                CCS_CharacterControllerConstants.AimCameraProfilePath);
            AppendIfMissing(
                failures,
                aimProfile != null,
                "Missing CCS_CharacterCameraProfile_AimOverShoulder aim camera profile.");
            if (aimProfile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                Mathf.Approximately(
                    aimProfile.ThirdPersonCameraDistance,
                    CCS_CharacterControllerConstants.AimCameraDistanceTuned),
                "Aim over-shoulder camera distance must match stripped baseline tuned value.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(
                    aimProfile.ThirdPersonCameraSide,
                    CCS_CharacterControllerConstants.AimCameraSideTuned),
                "Aim over-shoulder camera side must match stripped baseline screenshot tuning.");
            AppendIfMissing(
                failures,
                Vector3Approximately(
                    aimProfile.ThirdPersonShoulderOffset,
                    new Vector3(
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetXTuned,
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetYTuned,
                        CCS_CharacterControllerConstants.AimCameraShoulderOffsetZTuned)),
                "Aim over-shoulder shoulder offset must match stripped baseline screenshot values.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(
                    aimProfile.ThirdPersonVerticalArmLength,
                    CCS_CharacterControllerConstants.AimCameraVerticalArmLengthTuned),
                "Aim over-shoulder vertical arm length must match stripped baseline screenshot value.");
            AppendIfMissing(
                failures,
                Mathf.Approximately(
                    aimProfile.FollowDampingX,
                    CCS_CharacterControllerConstants.AimCameraFollowDampingXTuned)
                    && Mathf.Approximately(
                        aimProfile.FollowDampingY,
                        CCS_CharacterControllerConstants.AimCameraFollowDampingYTuned)
                    && Mathf.Approximately(
                        aimProfile.FollowDampingZ,
                        CCS_CharacterControllerConstants.AimCameraFollowDampingZTuned),
                "Aim over-shoulder follow damping must match stripped baseline screenshot values.");
        }

        private static void ValidateSharedAimPresentationState(List<string> failures)
        {
            const string gateSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_RevolverAimPresentationGate.cs";
            const string cameraSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_CharacterCameraController.cs";
            const string aimAnimatorSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverAimLayerAnimator.cs";
            const string equippedVisualSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_PlayerEquippedRevolverAimVisualPresenter.cs";
            const string sharedAimPointSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SharedDualAimPointPresenter.cs";

            AppendIfMissing(failures, File.Exists(gateSourcePath), "Missing CCS_RevolverAimPresentationGate source.");
            AppendIfMissing(
                failures,
                File.Exists(cameraSourcePath),
                "Missing CCS_CharacterCameraController source.");

            if (File.Exists(gateSourcePath))
            {
                string gateSource = File.ReadAllText(gateSourcePath);
                AppendIfMissing(
                    failures,
                    gateSource.Contains("IsAimPresentationActive"),
                    "CCS_RevolverAimPresentationGate must expose IsAimPresentationActive.");
                AppendIfMissing(
                    failures,
                    gateSource.Contains("AimHeld"),
                    "CCS_RevolverAimPresentationGate must read RMB aim input from input provider.");
                AppendIfMissing(
                    failures,
                    gateSource.Contains("EnableAimPose"),
                    "CCS_RevolverAimPresentationGate must read diagnostics Enable Aim Pose.");
                AppendIfPresent(
                    failures,
                    gateSource.Contains("WantsAimOverShoulderCamera => false"),
                    "CCS_RevolverAimPresentationGate must not hard-disable camera aim over shoulder.");
            }

            if (File.Exists(cameraSourcePath))
            {
                string cameraSource = File.ReadAllText(cameraSourcePath);
                AppendIfMissing(
                    failures,
                    cameraSource.Contains("WantsAimOverShoulderCamera"),
                    "CCS_CharacterCameraController must read WantsAimOverShoulderCamera from carry state source.");
                AppendIfPresent(
                    failures,
                    cameraSource.Contains("CCS_RevolverController"),
                    "CCS_CharacterCameraController must not depend on CCS_RevolverController for aim zoom.");
            }

            if (File.Exists(aimAnimatorSourcePath))
            {
                string aimAnimatorSource = File.ReadAllText(aimAnimatorSourcePath);
                AppendIfMissing(
                    failures,
                    aimAnimatorSource.Contains("IsAimPresentationActive"),
                    "CCS_RevolverAimLayerAnimator must read shared IsAimPresentationActive.");
            }

            if (File.Exists(equippedVisualSourcePath))
            {
                string equippedVisualSource = File.ReadAllText(equippedVisualSourcePath);
                AppendIfMissing(
                    failures,
                    equippedVisualSource.Contains("IsAimPresentationActive"),
                    "CCS_PlayerEquippedRevolverAimVisualPresenter must read shared IsAimPresentationActive.");
            }

            if (File.Exists(sharedAimPointSourcePath))
            {
                string sharedAimPointSource = File.ReadAllText(sharedAimPointSourcePath);
                AppendIfMissing(
                    failures,
                    sharedAimPointSource.Contains("IsAimPresentationActive"),
                    "CCS_SharedDualAimPointPresenter must read shared IsAimPresentationActive.");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_RevolverAimPresentationGate gate = prefab.GetComponent<CCS_RevolverAimPresentationGate>();
            AppendIfMissing(
                failures,
                gate != null,
                "Player root must contain shared CCS_RevolverAimPresentationGate.");
            AppendIfPresent(
                failures,
                prefab.GetComponent("CCS_RevolverController") != null,
                "Player root must not contain CCS_RevolverController for stripped baseline camera aim.");

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            if (modelRoot == null || gate == null)
            {
                return;
            }

            ValidatePresenterReferencesGate(
                failures,
                modelRoot.GetComponent<CCS_RevolverAimLayerAnimator>(),
                gate,
                "aimPresentationInputComponent",
                "CCS_RevolverAimLayerAnimator");
            ValidatePresenterReferencesGate(
                failures,
                modelRoot.GetComponent<CCS_PlayerEquippedRevolverAimVisualPresenter>(),
                gate,
                "aimPresentationInputComponent",
                "CCS_PlayerEquippedRevolverAimVisualPresenter");
            ValidatePresenterReferencesGate(
                failures,
                modelRoot.GetComponent<CCS_SharedDualAimPointPresenter>(),
                gate,
                "aimPresentationInputComponent",
                "CCS_SharedDualAimPointPresenter");

            CCS_CharacterAimLocomotionController aimLocomotion =
                prefab.GetComponent<CCS_CharacterAimLocomotionController>();
            if (aimLocomotion != null)
            {
                SerializedObject serializedAim = new SerializedObject(aimLocomotion);
                SerializedProperty gateProperty = serializedAim.FindProperty("weaponAimGateComponent");
                AppendIfMissing(
                    failures,
                    gateProperty != null && gateProperty.objectReferenceValue == gate,
                    "CCS_CharacterAimLocomotionController must reference CCS_RevolverAimPresentationGate.");
            }
        }

        private static void ValidatePresenterReferencesGate(
            List<string> failures,
            Component presenter,
            CCS_RevolverAimPresentationGate gate,
            string propertyName,
            string presenterName)
        {
            if (presenter == null)
            {
                return;
            }

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            SerializedProperty inputProperty = serializedPresenter.FindProperty(propertyName);
            AppendIfMissing(
                failures,
                inputProperty != null && inputProperty.objectReferenceValue == gate,
                presenterName + " must reference CCS_RevolverAimPresentationGate for shared aim presentation.");
        }

        private static readonly string[] ManualRigWeightOverrideScriptTypeNames =
        {
            "CCS_DualRevolverAimConvergenceRigPresenter",
            "CCS_DualRevolverArmAimBiasPresenter",
            "CCS_DualRevolverArmAimConstraintPresenter",
            "CCS_ManualArmRotationBiasPresenter",
        };

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private static void ValidateNoProceduralArmConvergence(List<string> failures, List<string> warnings)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            if (modelRoot == null)
            {
                return;
            }

            for (int i = 0; i < ForbiddenPrefabObjectNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    FindChildByName(prefab.transform, ForbiddenPrefabObjectNames[i]) != null,
                    "Player prefab must not contain procedural rig object '" + ForbiddenPrefabObjectNames[i] + "'.");
            }

            for (int i = 0; i < ManualRigWeightOverrideScriptTypeNames.Length; i++)
            {
                AppendIfPresent(
                    failures,
                    HasComponentByTypeName(prefab, ManualRigWeightOverrideScriptTypeNames[i]),
                    "Player prefab must not contain procedural arm script '" + ManualRigWeightOverrideScriptTypeNames[i] + "'.");
            }

            AppendIfPresent(
                failures,
                File.Exists(CCS_CharacterControllerConstants.ManualDualRevolverArmRotationBiasProfilePath),
                "Unused CCS_ManualDualRevolverArmRotationBiasProfile asset must not remain in stripped baseline.");

            MultiAimConstraint[] multiAimConstraints = prefab.GetComponentsInChildren<MultiAimConstraint>(true);
            AppendIfPresent(
                failures,
                multiAimConstraints.Length > 0,
                "Production player prefab must not contain active MultiAimConstraint procedural arm convergence.");

            MultiRotationConstraint[] multiRotationConstraints =
                prefab.GetComponentsInChildren<MultiRotationConstraint>(true);
            AppendIfPresent(
                failures,
                multiRotationConstraints.Length > 0,
                "Production player prefab must not contain active MultiRotationConstraint procedural arm convergence.");

            TwoBoneIKConstraint[] twoBoneConstraints = prefab.GetComponentsInChildren<TwoBoneIKConstraint>(true);
            AppendIfPresent(
                failures,
                twoBoneConstraints.Length > 0,
                "Production player prefab must not contain active TwoBoneIKConstraint dual revolver convergence.");

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                string objectName = transforms[i].name;
                if (objectName.EndsWith("Aim_Test")
                    || objectName.EndsWith("RotationBias_Test")
                    || objectName.EndsWith("RotationBiasPose")
                    || objectName.EndsWith("UpperArmAimSource")
                    || objectName.EndsWith("ClavicleAimSource"))
                {
                    AppendIfPresent(
                        failures,
                        true,
                        "Player prefab must not contain procedural test object '" + objectName + "'.");
                    break;
                }
            }

            Animator animator = modelRoot.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                RigBuilder rigBuilder = animator.GetComponent<RigBuilder>();
                AppendIfPresent(
                    failures,
                    rigBuilder != null,
                    "Kevin Animator must not contain RigBuilder for stripped authored-animation baseline.");

                if (rigBuilder != null)
                {
                    SerializedObject serializedRigBuilder = new SerializedObject(rigBuilder);
                    SerializedProperty layersProperty = serializedRigBuilder.FindProperty("m_RigLayers");
                    AppendIfPresent(
                        failures,
                        layersProperty != null && layersProperty.arraySize > 0,
                        "RigBuilder must not contain active procedural convergence rig layers.");
                }
            }

            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            Transform visualReferenceRoot = aimingRoot != null
                ? aimingRoot.Find(CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName)
                : null;
            Transform sharedAimPoint = visualReferenceRoot != null
                ? visualReferenceRoot.Find(CCS_CharacterControllerConstants.SharedDualAimPointObjectName)
                : null;

            AppendIfMissing(
                failures,
                visualReferenceRoot != null,
                "Model/Aiming must contain CCS_DualRevolverVisualAimReference.");
            AppendIfMissing(
                failures,
                sharedAimPoint != null,
                "Model/Aiming/CCS_DualRevolverVisualAimReference must contain CCS_SharedDualAimPoint.");
            AppendIfPresent(
                failures,
                aimingRoot != null
                    && aimingRoot.Find(CCS_CharacterControllerConstants.DualRevolverManualAimTargetsRootObjectName) != null,
                "Legacy CCS_DualRevolverManualAimTargets must not remain.");

            int activeSharedAimPointCount = CountActiveChildrenByName(
                prefab.transform,
                CCS_CharacterControllerConstants.SharedDualAimPointObjectName);
            AppendIfMissing(
                failures,
                activeSharedAimPointCount == 1,
                "Player prefab must contain exactly one active shared aim point transform (found "
                    + activeSharedAimPointCount
                    + ").");

            warnings.Add("Dual revolver aim pose remains experimental; future v0.7.14 should author a dual aim clip.");
            warnings.Add("Left mirrored aim layer may need replacement with authored dual-revolver animation.");
            warnings.Add("Future convergence requires authored dual aim clip, not runtime procedural rigging.");
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfPresent(
                failures,
                Directory.Exists(AnimationFitStudioRoot),
                "Animation Fit Studio must remain absent.");
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio must remain present.");
        }

        private static int CountActiveChildrenByName(Transform searchRoot, string objectName)
        {
            int count = 0;
            Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName && transforms[i].gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidateSharedDualAimPresentation(List<string> failures)
        {
            string sharedAimPointSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SharedDualAimPointPresenter.cs";
            string reticleSourcePath =
                "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SharedDualAimReticlePresenter.cs";

            AppendIfMissing(failures, File.Exists(sharedAimPointSourcePath), "Missing CCS_SharedDualAimPointPresenter source.");
            AppendIfMissing(failures, File.Exists(reticleSourcePath), "Missing CCS_SharedDualAimReticlePresenter source.");

            if (File.Exists(sharedAimPointSourcePath))
            {
                string sharedAimPointSource = File.ReadAllText(sharedAimPointSourcePath);
                AppendIfPresent(
                    failures,
                    sharedAimPointSource.Contains("CCS_DualRevolverArmAimBiasProfile"),
                    "SharedDualAimPointPresenter must not depend on automatic arm bias profile.");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            Transform modelRoot = CCS_PlayerModelRootUtility.FindModelRoot(prefab.transform);
            if (modelRoot == null)
            {
                return;
            }

            Transform aimingRoot = modelRoot.Find(CCS_CharacterControllerConstants.DualRevolverAimAimingObjectName);
            Transform visualReferenceRoot = aimingRoot != null
                ? aimingRoot.Find(CCS_CharacterControllerConstants.DualRevolverVisualAimReferenceObjectName)
                : null;
            AppendIfMissing(
                failures,
                visualReferenceRoot != null
                    && visualReferenceRoot.Find(CCS_CharacterControllerConstants.SharedDualAimPointObjectName) != null,
                "Model/Aiming/CCS_DualRevolverVisualAimReference must contain CCS_SharedDualAimPoint.");

            Transform hudRoot = prefab.transform.Find(CCS_WeaponsConstants.WeaponHudRootName);
            AppendIfMissing(
                failures,
                hudRoot != null && hudRoot.GetComponent<CCS_SharedDualAimReticlePresenter>() != null,
                "WeaponHudRoot must contain CCS_SharedDualAimReticlePresenter.");
            AppendIfMissing(
                failures,
                hudRoot != null
                    && hudRoot.Find(CCS_CharacterControllerConstants.SharedDualAimReticleObjectName) != null,
                "WeaponHudRoot must contain CCS_SharedDualAimReticle UI object.");

            AppendIfPresent(
                failures,
                FindChildByName(prefab.transform, "WeaponReticle") != null,
                "Legacy WeaponReticle must not return; use CCS_SharedDualAimReticle only.");
        }

        private static void ValidateTwoBoneIkConstraint(
            List<string> failures,
            Transform constraintTransform,
            Animator animator,
            HumanBodyBones rootBone,
            HumanBodyBones midBone,
            HumanBodyBones tipBone,
            Transform expectedTarget,
            Transform expectedHint,
            string constraintName)
        {
            AppendIfMissing(
                failures,
                constraintTransform != null,
                "Convergence rig must contain " + constraintName + " constraint.");
            if (constraintTransform == null)
            {
                return;
            }

            TwoBoneIKConstraint constraint = constraintTransform.GetComponent<TwoBoneIKConstraint>();
            AppendIfMissing(
                failures,
                constraint != null,
                constraintName + " must contain TwoBoneIKConstraint.");

            if (constraint == null || animator == null)
            {
                return;
            }

            TwoBoneIKConstraintData data = constraint.data;
            AppendIfMissing(
                failures,
                data.root == animator.GetBoneTransform(rootBone),
                constraintName + " root bone must reference " + rootBone + ".");
            AppendIfMissing(
                failures,
                data.mid == animator.GetBoneTransform(midBone),
                constraintName + " mid bone must reference " + midBone + ".");
            AppendIfMissing(
                failures,
                data.tip == animator.GetBoneTransform(tipBone),
                constraintName + " tip bone must reference " + tipBone + ".");
            AppendIfMissing(
                failures,
                data.target == expectedTarget,
                constraintName + " target must reference the configured hand rig target.");
            AppendIfMissing(
                failures,
                data.hint == expectedHint,
                constraintName + " hint must reference the configured elbow hint.");
            AppendIfMissing(
                failures,
                constraint.enabled,
                constraintName + " must remain enabled for runtime convergence evaluation.");
        }

        private static void ValidateWeaponVisualsStayOnHandSockets(
            List<string> failures,
            Transform prefabRoot,
            Transform rigRoot)
        {
            Transform rightHandSocket = FindChildByName(prefabRoot, "CCS_HandSocket_Right");
            Transform leftHandSocket = FindChildByName(prefabRoot, "CCS_HandSocket_Left");
            AppendIfMissing(
                failures,
                rightHandSocket != null && leftHandSocket != null,
                "Player prefab must contain right and left hand sockets for equipped weapon visuals.");

            if (rigRoot == null)
            {
                return;
            }

            Transform rightTarget = rigRoot.Find(CCS_CharacterControllerConstants.DualRevolverRightAimRigTargetObjectName);
            Transform leftTarget = rigRoot.Find(CCS_CharacterControllerConstants.DualRevolverLeftAimRigTargetObjectName);
            AppendIfMissing(
                failures,
                rightTarget == null || rightTarget.childCount == 0,
                "Right hand rig target must not parent weapon visuals directly.");
            AppendIfMissing(
                failures,
                leftTarget == null || leftTarget.childCount == 0,
                "Left hand rig target must not parent weapon visuals directly.");
        }

        private static void ValidateRequiredRuntimeTypes(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_PlayerHolsteredRevolverVisualPresenter.cs"),
                "Missing CCS_PlayerHolsteredRevolverVisualPresenter runtime script.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/CharacterController/Runtime/CCS_DiagnosticsEquipWeaponRegistry.cs"),
                "Missing CCS_DiagnosticsEquipWeaponRegistry.");
            AppendIfMissing(
                failures,
                File.Exists("Assets/CCS/Modules/CharacterController/Runtime/Diagnostics/CCS_DiagnosticsEnemyBanditController.cs"),
                "Missing CCS_DiagnosticsEnemyBanditController.");
        }

        private static void ValidateMissingScripts(List<string> failures)
        {
            AppendValidationFailures(
                failures,
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts());
        }

        private static void CollectDeferredWarnings(List<string> warnings)
        {
            warnings.Add("Gameplay fire, damage, ammo, and weapon ownership remain removed in stripped baseline.");
            warnings.Add("Equip Weapon controls visual holster/equipped revolver only.");
            warnings.Add("Enemy spawn is diagnostics-controlled via Enable Enemy and defaults to off.");
            warnings.Add("Experimental left-arm aim preview uses mirrored Animator states and provisional left-hand fit.");
            warnings.Add("Shared aim presentation state drives RMB, diagnostics Enable Aim Pose, camera zoom, aim layers, shared aim point, and weapon visuals.");
            warnings.Add("Aim over-shoulder camera framing may still need manual tuning in validation scene.");
            warnings.Add("v0.7.13 uses authored animation only; procedural arm convergence was removed.");
            warnings.Add("Shared reticle and shared aim point are passive visual references only.");
        }

        private static bool HasComponentByTypeName(GameObject root, string typeName)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().Name == typeName)
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindChildByName(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && transforms[i].name == objectName)
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private static GameObject FindSceneObjectByName(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < transforms.Length; t++)
                {
                    if (transforms[t] != null && transforms[t].name == objectName)
                    {
                        return transforms[t].gameObject;
                    }
                }
            }

            return null;
        }

        private static void AppendValidationFailures(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> target, bool condition, string message)
        {
            if (!condition)
            {
                target.Add(message);
            }
        }

        private static void AppendIfPresent(List<string> target, bool condition, string message)
        {
            if (condition)
            {
                target.Add(message);
            }
        }
    }
}
