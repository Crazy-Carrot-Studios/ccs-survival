using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Editor.AnimationFitStudio;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates player Animator Controller uses only CCS-owned animation clips.
// PLACEMENT: Called from master test validator and animation isolation menu.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.4 — validates revolver upper-body isolation and RevolverUpperBody layer wiring.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorControllerAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath),
                "Missing player Animator Controller at "
                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + ".");

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.ContentAnimationsRootPath),
                "Missing Content/Animations folder at "
                + CCS_CharacterControllerConstants.ContentAnimationsRootPath
                + ".");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            HashSet<Motion> visitedMotions = new HashSet<Motion>();
            List<AnimationClip> clips = new List<AnimationClip>();
            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                CollectAnimationClips(controller.layers[layerIndex].stateMachine, visitedMotions, clips);
            }

            AppendIfMissing(
                failures,
                clips.Count > 0,
                "Player Animator Controller resolved zero animation clips.");

            string allowedRoot = NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath);
            for (int i = 0; i < clips.Count; i++)
            {
                AnimationClip clip = clips[i];
                if (clip == null)
                {
                    continue;
                }

                string clipPath = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(clipPath))
                {
                    failures.Add("Player Animator Controller references unresolved animation clip.");
                    continue;
                }

                string normalizedClipPath = NormalizeAssetPath(clipPath);
                if (!normalizedClipPath.StartsWith(allowedRoot))
                {
                    failures.Add(
                        "Player Animator Controller references non-CCS animation clip: "
                        + normalizedClipPath);
                }

                if (IsVendorFbxSubAssetPath(normalizedClipPath))
                {
                    failures.Add(
                        "Player Animator Controller references vendor FBX sub-asset: "
                        + normalizedClipPath);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Player Animator Controller uses CCS-owned animation clips only.");
        }

        public static CCS_SurvivalValidationResult ValidateAimLocomotionAnimatorParameters()
        {
            List<string> failures = new List<string>();

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");

            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter),
                "Player Animator Controller must define IsAimingMovementMode.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorAimMoveXParameter),
                "Player Animator Controller must define AimMoveX.");
            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorAimMoveYParameter),
                "Player Animator Controller must define AimMoveY.");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player Animator Controller aim strafe parameters validated.");
        }

        public static CCS_SurvivalValidationResult ValidateAimStrafeAnimationIsolation()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.AimStrafeAnimationsPath),
                "Missing AimStrafe animation folder at "
                + CCS_CharacterControllerConstants.AimStrafeAnimationsPath
                + ".");

            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath,
                requireLoopTime: true);
            ValidateRequiredAimStrafeClipAsset(
                failures,
                CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath,
                requireLoopTime: true);

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AnimatorState aimState = FindState(
                controller.layers[0].stateMachine,
                CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName);
            AppendIfMissing(
                failures,
                aimState != null,
                "Player Animator Controller must define AimStrafe_Locomotion state.");

            if (aimState == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            BlendTree blendTree = aimState.motion as BlendTree;
            AppendIfMissing(
                failures,
                blendTree != null,
                "AimStrafe_Locomotion must use a 2D blend tree.");
            if (blendTree == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                blendTree.blendType == BlendTreeType.FreeformDirectional2D
                    || blendTree.blendType == BlendTreeType.SimpleDirectional2D,
                "AimStrafe_Locomotion blend tree must be 2D directional.");
            AppendIfMissing(
                failures,
                blendTree.blendParameter == CCS_CharacterControllerConstants.AnimatorAimMoveXParameter,
                "AimStrafe blend tree must use AimMoveX.");
            AppendIfMissing(
                failures,
                blendTree.blendParameterY == CCS_CharacterControllerConstants.AnimatorAimMoveYParameter,
                "AimStrafe blend tree must use AimMoveY.");

            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(0f, 1f),
                CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(0f, -1f),
                CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(-1f, 0f),
                CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath);
            ValidateBlendTreeMotionAt(
                failures,
                blendTree,
                new Vector2(1f, 0f),
                CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath);

            ChildMotion[] children = blendTree.children;
            bool hasIdleCenter = false;
            string idlePath = CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim";
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].motion is AnimationClip clip
                    && Vector2.Distance(children[i].position, Vector2.zero) <= 0.001f
                    && NormalizeAssetPath(AssetDatabase.GetAssetPath(clip)) == NormalizeAssetPath(idlePath))
                {
                    hasIdleCenter = true;
                    break;
                }
            }

            AppendIfMissing(
                failures,
                hasIdleCenter,
                "AimStrafe blend tree must include CCS idle at (0,0).");

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Aim strafe animation isolation validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverUpperBodyAnimationIsolation()
        {
            return ValidateSimplifiedRevolverAimController();
        }

        public static CCS_SurvivalValidationResult ValidateSimplifiedRevolverAimController()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                Directory.Exists(CCS_CharacterControllerConstants.RevolverAimAnimationsPath),
                "Missing revolver aim animation folder at "
                + CCS_CharacterControllerConstants.RevolverAimAnimationsPath
                + ".");
            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverIdleToAimClipPath,
                requireLoopTime: false);
            ValidateRequiredRevolverClipAsset(
                failures,
                CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
                requireLoopTime: true);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath),
                "Missing right-arm aim mask at "
                + CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath
                + ".");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Player Animator Controller is missing.");
            if (controller == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter),
                "Player Animator Controller must define RevolverAimHeld.");
            AppendIfMissing(
                failures,
                !HasAnimatorParameter(controller, CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter),
                "Player Animator Controller must not use RevolverAimPitch in simplified aim flow.");

            int revolverLayerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            int obsoleteLayerIndex = FindLayerIndex(
                controller,
                CCS_CharacterControllerConstants.AnimatorRevolverAimUpperBodyLayerNameObsolete);
            AppendIfMissing(
                failures,
                revolverLayerIndex >= 0,
                "Player Animator Controller must define RevolverUpperBody layer.");
            AppendIfMissing(
                failures,
                obsoleteLayerIndex < 0,
                "Player Animator Controller must not keep obsolete Revolver Aim Upper Body layer.");
            AppendIfMissing(
                failures,
                CountRevolverAimLayers(controller) == 1,
                "Player Animator Controller must contain exactly one revolver aim layer.");
            AppendIfMissing(
                failures,
                controller.layers.Length == 3,
                "Player Animator Controller must contain exactly three layers: Base Layer, RevolverUpperBody, Interaction.");

            if (revolverLayerIndex < 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendIfMissing(
                failures,
                revolverLayerIndex == 1,
                "RevolverUpperBody must be layer index 1.");
            AppendIfMissing(
                failures,
                FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName) == 2,
                "Interaction must be layer index 2.");

            int aimLayerIndex = revolverLayerIndex;
            if (aimLayerIndex >= 0)
            {
                AnimatorControllerLayer aimLayer = controller.layers[aimLayerIndex];
                AvatarMask expectedMask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                    CCS_CharacterControllerConstants.RevolverAimRightArmMaskPath);
                AppendIfMissing(
                    failures,
                    aimLayer.avatarMask == expectedMask,
                    "RevolverUpperBody layer must use AM_CCS_Revolver_UpperBodyRightArm_Aim.mask.");
                AppendIfMissing(
                    failures,
                    CCS_RevolverUpperBodyRightArmAimMaskUtility.ValidateMaskConfiguration(expectedMask),
                    "Revolver aim mask must include upper body/right arm/right fingers and exclude left arm/legs/root.");

                AnimatorControllerLayer baseLayer = controller.layers[0];
                AppendIfMissing(
                    failures,
                    baseLayer.avatarMask == null,
                    "Base Layer must not use an Avatar Mask.");

                AnimatorStateMachine stateMachine = aimLayer.stateMachine;
                AnimatorState defaultState = stateMachine != null ? stateMachine.defaultState : null;
                AppendIfMissing(
                    failures,
                    defaultState != null
                    && defaultState.name == CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName,
                    "RevolverUpperBody default state must be NoAim.");
                AppendIfMissing(
                    failures,
                    stateMachine != null && stateMachine.states.Length == 4,
                    "RevolverUpperBody must contain exactly four states.");

                string[] forbiddenStates =
                {
                    CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName,
                    "Revolver_AimPitch_Blend",
                    "Revolver_Fire",
                    "Revolver_WW_Fire_Fanning_RH",
                    "Revolver_Reload",
                    CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName,
                    CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName,
                };
                for (int forbiddenIndex = 0; forbiddenIndex < forbiddenStates.Length; forbiddenIndex++)
                {
                    AppendIfMissing(
                        failures,
                        stateMachine == null
                        || FindState(stateMachine, forbiddenStates[forbiddenIndex]) == null,
                        "RevolverUpperBody must not contain state "
                        + forbiddenStates[forbiddenIndex]
                        + ".");
                }

                ValidateRevolverStateClip(failures, stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName, null);
                ValidateRevolverStateClip(
                    failures,
                    stateMachine,
                    CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName,
                    CCS_CharacterControllerConstants.RevolverIdleToAimClipPath);
                ValidateRevolverStateClip(
                    failures,
                    stateMachine,
                    CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName,
                    CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath);
                ValidateRevolverStateClip(
                    failures,
                    stateMachine,
                    CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName,
                    CCS_CharacterControllerConstants.RevolverIdleToAimClipPath);

                AnimatorState returnState = FindState(
                    stateMachine,
                    CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName);
                if (returnState != null)
                {
                    AppendIfMissing(
                        failures,
                        returnState.speed < 0f,
                        "Revolver_AimToIdle_Return must play IdleToAim in reverse (state speed < 0).");
                }

                AppendIfMissing(
                    failures,
                    FindState(stateMachine, CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName) == null,
                    "Simplified aim controller must not use Revolver_AimPitch_Blend.");
            }

            AppendIfMissing(
                failures,
                FindLayerIndex(controller, CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName) >= 0,
                "Player Animator Controller must define Interaction reserved layer.");

            string builderPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/Validation/CCS_RevolverAimSimplificationBuilder.cs";
            AppendIfMissing(
                failures,
                File.Exists(builderPath),
                "Missing CCS_RevolverAimSimplificationBuilder for simplified aim controller pass.");

            string runtimeAnimatorPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Runtime/Animation/CCS_RevolverUpperBodyAnimator.cs";
            if (File.Exists(runtimeAnimatorPath))
            {
                string runtimeAnimatorSource = File.ReadAllText(runtimeAnimatorPath);
                AppendIfMissing(
                    failures,
                    runtimeAnimatorSource.Contains("AnimatorRevolverUpperBodyLayerName"),
                    "CCS_RevolverUpperBodyAnimator must drive AnimatorRevolverUpperBodyLayerName.");
                AppendIfMissing(
                    failures,
                    !runtimeAnimatorSource.Contains("AnimatorRevolverAimUpperBodyLayerNameObsolete"),
                    "CCS_RevolverUpperBodyAnimator must not reference obsolete Revolver Aim Upper Body layer.");
                AppendIfMissing(
                    failures,
                    runtimeAnimatorSource.Contains("SetAnimatorBoolIfPresent"),
                    "CCS_RevolverUpperBodyAnimator must guard Animator parameter writes.");
                AppendIfMissing(
                    failures,
                    runtimeAnimatorSource.Contains("CurrentAimPhase"),
                    "CCS_RevolverUpperBodyAnimator must expose CurrentAimPhase for reticle gating.");
                AppendIfMissing(
                    failures,
                    runtimeAnimatorSource.Contains("ResolveTargetLayerWeight"),
                    "CCS_RevolverUpperBodyAnimator must blend aim layer weight by phase.");
            }

            AppendValidationFailures(failures, ValidateFullDrawClipPreservedAfterBuilder());

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Simplified revolver aim controller validated.");
        }

        public static CCS_SurvivalValidationResult ValidateFullDrawClipPreservedAfterBuilder()
        {
            List<string> failures = new List<string>();
            string builderPath = CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/Validation/CCS_RevolverAimSimplificationBuilder.cs";
            AppendIfMissing(
                failures,
                File.Exists(builderPath),
                "Missing CCS_RevolverAimSimplificationBuilder for FullDraw preservation validation.");

            if (File.Exists(builderPath))
            {
                string builderSource = File.ReadAllText(builderPath);
                AppendIfMissing(
                    failures,
                    builderSource.Contains("File.Exists(destinationPath)"),
                    "Builder must skip clip migration when controller FullDraw clip already exists.");
            }

            string clipPath = CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            AppendIfMissing(
                failures,
                clip != null,
                "Missing controller FullDraw clip at " + clipPath + ".");
            if (clip == null)
            {
                return failures.Count > 0
                    ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                    : CCS_SurvivalValidationResult.Pass("FullDraw clip preservation skipped (clip missing).");
            }

            AppendIfMissing(
                failures,
                CCS_AnimationFitStudioClipCurveModeUtility.DetectClipCurveMode(clip)
                    == CCS_AnimationFitStudioClipCurveMode.HumanoidMuscleCurves,
                "FullDraw controller clip must use Humanoid muscle curves.");

            string hashBefore = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(clip);
            CCS_RevolverAimSimplificationBuilder.EnsureRevolverAimSimplificationPass();
            AnimationClip clipAfter = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            AppendIfMissing(
                failures,
                clipAfter != null,
                "Controller FullDraw clip missing after builder pass.");
            if (clipAfter != null)
            {
                string hashAfter = CCS_AnimationFitStudioCurveHashUtility.ComputeCurveHash(clipAfter);
                AppendIfMissing(
                    failures,
                    hashBefore == hashAfter,
                    "Builder/scene validation reverted FullDraw clip curve hash.");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("FullDraw clip curve hash preserved after builder pass.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverWildWestDefaultAimRuntime(GameObject prefabRoot = null)
        {
            return ValidateSimplifiedRevolverAimController();
        }


        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorUsesExpectedController(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            List<string> failures = new List<string>();
            Animator animator = prefabRoot.GetComponentInChildren<Animator>(true);
            AppendIfMissing(failures, animator != null, "Player prefab must contain an Animator.");
            if (animator == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AnimatorController expectedController = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(
                failures,
                expectedController != null,
                "Expected player Animator Controller asset is missing at "
                + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + ".");

            RuntimeAnimatorController assignedController = animator.runtimeAnimatorController;
            AppendIfMissing(
                failures,
                assignedController != null,
                "Player prefab Animator must assign AC_CCS_Player_Locomotion_StarterAssets.");

            if (assignedController != null && expectedController != null)
            {
                RuntimeAnimatorController resolvedController = assignedController;
                if (assignedController is AnimatorOverrideController overrideController)
                {
                    resolvedController = overrideController.runtimeAnimatorController;
                }

                AppendIfMissing(
                    failures,
                    resolvedController == expectedController,
                    "Player prefab Animator must use "
                    + CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                    + " (found "
                    + AssetDatabase.GetAssetPath(resolvedController)
                    + ").");
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player prefab Animator Controller assignment validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRevolverWildWestHardReplaceAimRuntime(GameObject prefabRoot = null)
        {
            return ValidateSimplifiedRevolverAimController();
        }

        public static CCS_SurvivalValidationResult ValidateNoInvectorRuntimeReferences()
        {
            List<string> failures = new List<string>();

            AppendIfMissing(
                failures,
                !Directory.Exists("Assets/Invector-3rdPersonController"),
                "Legacy Invector import folder must be removed from Assets/Invector-3rdPersonController.");

            string[] runtimeAssetGuids = AssetDatabase.FindAssets(
                "t:AnimatorController t:Prefab t:Scene",
                new[] { "Assets/CCS" });
            for (int i = 0; i < runtimeAssetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(runtimeAssetGuids[i]);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                if (assetPath.StartsWith("Assets/VendorSource/"))
                {
                    continue;
                }

                string assetText = File.ReadAllText(assetPath);
                if (assetText.Contains("Invector-3rdPersonController"))
                {
                    failures.Add("Runtime CCS asset references legacy Invector path: " + assetPath);
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("No runtime references to legacy Invector assets.");
        }

        public static CCS_SurvivalValidationResult ValidatePlayerRevolverUpperBodyAnimator(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            List<string> failures = new List<string>();

            CCS_RevolverUpperBodyAnimator upperBodyAnimator =
                prefabRoot.GetComponentInChildren<CCS_RevolverUpperBodyAnimator>(true);
            AppendIfMissing(
                failures,
                upperBodyAnimator != null,
                "Player prefab must contain CCS_RevolverUpperBodyAnimator on VisualRoot.");

            if (upperBodyAnimator == null)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            AppendValidationFailures(failures, ValidateRevolverWildWestHardReplaceAimRuntime(prefabRoot));
            AppendValidationFailures(failures, ValidatePlayerAnimatorUsesExpectedController(prefabRoot));

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player prefab revolver upper-body animator validated.");
        }

        private static void AppendValidationFailures(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            failures.Add(result.Message);
        }

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorRootMotionDisabled(GameObject prefabRoot)
        {
            if (prefabRoot == null)
            {
                return CCS_SurvivalValidationResult.Fail("Player prefab reference is null.");
            }

            Animator[] animators = prefabRoot.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null || !animator.isActiveAndEnabled && animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                if (animator.applyRootMotion)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Player Animator must have Apply Root Motion disabled on "
                        + animator.gameObject.name
                        + ".");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Player Animator root motion is disabled.");
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredRevolverClipAsset(
            List<string> failures,
            string clipAssetPath,
            bool requireLoopTime)
        {
            AppendIfMissing(
                failures,
                File.Exists(clipAssetPath),
                "Missing required Revolver clip asset: " + clipAssetPath + ".");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                return;
            }

            string normalizedPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            AppendIfMissing(
                failures,
                normalizedPath.StartsWith(NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath)),
                "Revolver clip must live under Assets/CCS/: " + normalizedPath);
            AppendIfMissing(
                failures,
                !IsVendorFbxSubAssetPath(normalizedPath),
                "Revolver clip must not be a vendor FBX sub-asset: " + normalizedPath);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            AppendIfMissing(
                failures,
                settings.loopTime == requireLoopTime,
                "Revolver clip loop setting invalid for " + clipAssetPath + ".");
        }

        private static void ValidateRevolverAimPitchBlendState(
            List<string> failures,
            AnimatorStateMachine stateMachine)
        {
            AnimatorState pitchBlendState = FindState(
                stateMachine,
                CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName);
            AppendIfMissing(
                failures,
                pitchBlendState != null,
                "RevolverUpperBody layer must define "
                + CCS_CharacterControllerConstants.AnimatorRevolverAimPitchBlendStateName
                + ".");

            if (pitchBlendState == null)
            {
                return;
            }

            BlendTree blendTree = pitchBlendState.motion as BlendTree;
            AppendIfMissing(
                failures,
                blendTree != null
                    && blendTree.blendType == BlendTreeType.Simple1D
                    && blendTree.blendParameter
                        == CCS_CharacterControllerConstants.AnimatorRevolverAimPitchParameter,
                "Revolver_AimPitch_Blend must use a 1D blend tree driven by RevolverAimPitch.");

            if (blendTree == null || blendTree.children == null || blendTree.children.Length < 3)
            {
                AppendIfMissing(
                    failures,
                    false,
                    "Revolver_AimPitch_Blend must contain Down/Center/Up FitTest clips.");
                return;
            }

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimPitchDownFitTestClipPath),
                "Missing " + CCS_CharacterControllerConstants.RevolverAimPitchDownFitTestClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimPitchCenterFitTestClipPath),
                "Missing " + CCS_CharacterControllerConstants.RevolverAimPitchCenterFitTestClipPath);
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimPitchUpFitTestClipPath),
                "Missing " + CCS_CharacterControllerConstants.RevolverAimPitchUpFitTestClipPath);
        }

        private static void ValidateRevolverStateClip(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string stateName,
            string expectedClipPath)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            AppendIfMissing(
                failures,
                state != null,
                "RevolverUpperBody layer must define state " + stateName + ".");

            if (state == null)
            {
                return;
            }

            if (expectedClipPath == null)
            {
                AppendIfMissing(
                    failures,
                    state.motion == null,
                    stateName + " must have no motion clip assigned.");
                return;
            }

            AppendIfMissing(
                failures,
                state.motion is AnimationClip,
                stateName + " must reference a CCS-owned animation clip.");

            if (state.motion is AnimationClip clip)
            {
                string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                AppendIfMissing(
                    failures,
                    clipPath == NormalizeAssetPath(expectedClipPath),
                    stateName + " must reference " + expectedClipPath + " (found " + clipPath + ").");
                AppendIfMissing(
                    failures,
                    !IsVendorFbxSubAssetPath(clipPath),
                    stateName + " must not reference vendor FBX sub-asset: " + clipPath);
            }
        }

        private static void ValidateRevolverStateClipMustNotEqual(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string stateName,
            string forbiddenClipPath)
        {
            AnimatorState state = FindState(stateMachine, stateName);
            if (state == null || !(state.motion is AnimationClip clip))
            {
                return;
            }

            string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            AppendIfMissing(
                failures,
                clipPath != NormalizeAssetPath(forbiddenClipPath),
                stateName + " must not reference legacy runtime clip " + forbiddenClipPath + " (found " + clipPath + ").");
        }

        private static void ValidateNoVendorRevolverClipsOnRevolverUpperBodyLayer(
            List<string> failures,
            AnimatorControllerLayer layer)
        {
            if (layer.stateMachine == null)
            {
                return;
            }

            CollectVendorClipAssignments(failures, layer.stateMachine, layer.name);
        }

        private static void CollectVendorClipAssignments(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string layerName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                if (state?.motion is not AnimationClip clip)
                {
                    continue;
                }

                string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                if (clipPath.Contains("YashMakesGames") || clipPath.Contains("Wild West Animation Pack"))
                {
                    failures.Add(
                        "Active Animator state "
                        + layerName
                        + "/"
                        + state.name
                        + " must use CCS-duplicated Wild West clips, not vendor path: "
                        + clipPath
                        + ".");
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                if (childMachines[i].stateMachine != null)
                {
                    CollectVendorClipAssignments(failures, childMachines[i].stateMachine, layerName);
                }
            }
        }

        private static void ValidateNoLegacyRevolverClipsOnController(
            List<string> failures,
            AnimatorController controller)
        {
            if (controller == null)
            {
                return;
            }

            string[] legacyClipPaths =
            {
                CCS_CharacterControllerConstants.RevolverAimIdleLegacyClipPath,
                CCS_CharacterControllerConstants.RevolverFireLegacyClipPath,
                CCS_CharacterControllerConstants.RevolverIdlePistolLegacyClipPath,
            };

            for (int layerIndex = 0; layerIndex < controller.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = controller.layers[layerIndex];
                if (layer.stateMachine == null)
                {
                    continue;
                }

                CollectLegacyClipAssignments(failures, layer.stateMachine, layer.name, legacyClipPaths);
            }
        }

        private static void CollectLegacyClipAssignments(
            List<string> failures,
            AnimatorStateMachine stateMachine,
            string layerName,
            string[] legacyClipPaths)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = states[i].state;
                if (state?.motion is not AnimationClip clip)
                {
                    continue;
                }

                string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                for (int legacyIndex = 0; legacyIndex < legacyClipPaths.Length; legacyIndex++)
                {
                    if (clipPath == NormalizeAssetPath(legacyClipPaths[legacyIndex]))
                    {
                        failures.Add(
                            "Active Animator state "
                            + layerName
                            + "/"
                            + state.name
                            + " must not reference legacy runtime clip "
                            + legacyClipPaths[legacyIndex]
                            + ".");
                    }
                }
            }

            ChildAnimatorStateMachine[] childMachines = stateMachine.stateMachines;
            for (int i = 0; i < childMachines.Length; i++)
            {
                if (childMachines[i].stateMachine != null)
                {
                    CollectLegacyClipAssignments(
                        failures,
                        childMachines[i].stateMachine,
                        layerName,
                        legacyClipPaths);
                }
            }
        }

        private static void ValidateRuntimeGuiNotCalledFromSimulationTick(
            List<string> failures,
            string sourcePath,
            string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
            {
                return;
            }

            string[] tickMethodNames = { "Update", "LateUpdate", "FixedUpdate" };
            for (int i = 0; i < tickMethodNames.Length; i++)
            {
                string methodName = tickMethodNames[i];
                if (!TryExtractMethodBody(sourceText, methodName, out string methodBody))
                {
                    continue;
                }

                AppendIfMissing(
                    failures,
                    !ContainsImmediateModeGuiCall(methodBody),
                    sourcePath
                    + " must not call immediate-mode GUI from "
                    + methodName
                    + "().");
            }
        }

        private static bool TryExtractMethodBody(string sourceText, string methodName, out string methodBody)
        {
            methodBody = string.Empty;
            string signature = "void " + methodName + "(";
            int methodIndex = sourceText.IndexOf(signature, System.StringComparison.Ordinal);
            if (methodIndex < 0)
            {
                return false;
            }

            int braceStart = sourceText.IndexOf('{', methodIndex);
            if (braceStart < 0)
            {
                return false;
            }

            int depth = 0;
            for (int i = braceStart; i < sourceText.Length; i++)
            {
                char current = sourceText[i];
                if (current == '{')
                {
                    depth++;
                }
                else if (current == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        methodBody = sourceText.Substring(braceStart, i - braceStart + 1);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ContainsImmediateModeGuiCall(string methodBody)
        {
            return methodBody.Contains("GUI.")
                || methodBody.Contains("GUILayout.")
                || methodBody.Contains("EditorGUI.");
        }

        private static void ValidateRevolverRuntimeEditedAimIdleBuilderWiring(List<string> failures)
        {
            string builderPath =
                "Assets/CCS/Modules/CharacterController/Editor/Validation/CCS_CharacterControllerAnimationIsolationBuilder.cs";
            AppendIfMissing(
                failures,
                File.Exists(builderPath),
                "Missing animation isolation builder at " + builderPath + ".");

            if (!File.Exists(builderPath))
            {
                return;
            }

            string builderSource = File.ReadAllText(builderPath);
            AppendIfMissing(
                failures,
                builderSource.Contains("EnsureRevolverAimPitchBlendWiring"),
                "Animation isolation builder must wire aim pitch blend via EnsureRevolverAimPitchBlendWiring.");
            AppendIfMissing(
                failures,
                builderSource.Contains("RevolverAimPitchCenterFitTestClipPath"),
                "Animation isolation builder must reference RevolverAimPitchCenterFitTestClipPath for pitch blend wiring.");
            AppendIfMissing(
                failures,
                builderSource.Contains("Revolver AimPitch blend requires Down/Center/Up FitTest clips"),
                "Animation isolation builder must fail loudly when aim pitch FitTest clips are missing.");
        }

        private static void ValidateWildWestAimClipHasUpperBodyMuscleCurves(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestRevolverAimIdleFullDrawClipPath),
                "Missing Wild West aim idle source clip at "
                + CCS_CharacterControllerConstants.WildWestRevolverAimIdleFullDrawClipPath
                + ".");

            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestRevolverRuntimeDefaultAimIdleClipPath),
                "Missing controller FullDraw clip at "
                + CCS_CharacterControllerConstants.WildWestRevolverRuntimeDefaultAimIdleClipPath
                + ". Open Animation Fit Studio and save Runtime FullDraw first.");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                CCS_CharacterControllerConstants.WildWestRevolverRuntimeDefaultAimIdleClipPath);
            AppendIfMissing(
                failures,
                clip != null,
                "Missing runtime Wild West aim idle FitTest clip at "
                + CCS_CharacterControllerConstants.WildWestRevolverRuntimeDefaultAimIdleClipPath
                + ".");

            if (clip == null)
            {
                return;
            }

            string[] requiredMuscleAttributes =
            {
                "Right Shoulder Down-Up",
                "Right Arm Down-Up",
                "RightHandT.x",
                "Chest Front-Back",
            };

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < requiredMuscleAttributes.Length; i++)
            {
                string requiredAttribute = requiredMuscleAttributes[i];
                bool found = false;
                for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
                {
                    if (bindings[bindingIndex].propertyName == requiredAttribute)
                    {
                        found = true;
                        break;
                    }
                }

                AppendIfMissing(
                    failures,
                    found,
                    "Wild West aim idle clip must contain muscle curve "
                    + requiredAttribute
                    + " for masked upper-body/right-arm playback.");
            }
        }

        private static int CountRevolverAimLayers(AnimatorController controller)
        {
            int count = 0;
            for (int i = 0; i < controller.layers.Length; i++)
            {
                string layerName = controller.layers[i].name;
                if (layerName == CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName
                    || layerName == CCS_CharacterControllerConstants.AnimatorRevolverAimUpperBodyLayerNameObsolete
                    || layerName == CCS_CharacterControllerConstants.AnimatorRevolverRightHandPreviewLayerName)
                {
                    count++;
                }
            }

            return count;
        }

        private static int FindLayerIndex(AnimatorController controller, string layerName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void ValidateRequiredAimStrafeClipAsset(
            List<string> failures,
            string clipAssetPath,
            bool requireLoopTime)
        {
            AppendIfMissing(
                failures,
                File.Exists(clipAssetPath),
                "Missing required AimStrafe clip asset: " + clipAssetPath + ".");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                return;
            }

            string normalizedPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
            AppendIfMissing(
                failures,
                normalizedPath.StartsWith(NormalizeAssetPath(CCS_CharacterControllerConstants.ContentAnimationsRootPath)),
                "AimStrafe clip must live under Assets/CCS/: " + normalizedPath);
            AppendIfMissing(
                failures,
                !IsVendorFbxSubAssetPath(normalizedPath),
                "AimStrafe clip must not be a vendor FBX sub-asset: " + normalizedPath);

            if (requireLoopTime)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                AppendIfMissing(
                    failures,
                    settings.loopTime,
                    "AimStrafe clip must be loopable: " + clipAssetPath);
            }
        }

        private static void ValidateBlendTreeMotionAt(
            List<string> failures,
            BlendTree blendTree,
            Vector2 expectedPosition,
            string expectedClipPath)
        {
            ChildMotion[] children = blendTree.children;
            for (int i = 0; i < children.Length; i++)
            {
                if (Vector2.Distance(children[i].position, expectedPosition) > 0.001f)
                {
                    continue;
                }

                if (children[i].motion is AnimationClip clip)
                {
                    string clipPath = NormalizeAssetPath(AssetDatabase.GetAssetPath(clip));
                    if (clipPath == NormalizeAssetPath(expectedClipPath))
                    {
                        return;
                    }

                    failures.Add(
                        "AimStrafe blend point "
                        + expectedPosition
                        + " must reference "
                        + expectedClipPath
                        + " (found "
                        + clipPath
                        + ").");
                    return;
                }
            }

            failures.Add(
                "AimStrafe blend tree missing motion at "
                + expectedPosition
                + " for "
                + expectedClipPath
                + ".");
        }

        private static bool IsVendorFbxSubAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string normalized = NormalizeAssetPath(assetPath);
            if (!normalized.StartsWith("Assets/CCS/"))
            {
                return normalized.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains(".fbx/");
            }

            return false;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            return null;
        }

        private static bool HasAnimatorParameter(AnimatorController controller, string parameterName)
        {
            for (int i = 0; i < controller.parameters.Length; i++)
            {
                if (controller.parameters[i].name == parameterName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CollectAnimationClips(
            AnimatorStateMachine stateMachine,
            HashSet<Motion> visitedMotions,
            List<AnimationClip> clips)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                CollectMotionClips(states[i].state != null ? states[i].state.motion : null, visitedMotions, clips);
            }

            ChildAnimatorStateMachine[] childStateMachines = stateMachine.stateMachines;
            for (int i = 0; i < childStateMachines.Length; i++)
            {
                if (childStateMachines[i].stateMachine != null)
                {
                    CollectAnimationClips(childStateMachines[i].stateMachine, visitedMotions, clips);
                }
            }
        }

        private static void CollectMotionClips(Motion motion, HashSet<Motion> visitedMotions, List<AnimationClip> clips)
        {
            if (motion == null || !visitedMotions.Add(motion))
            {
                return;
            }

            if (motion is AnimationClip clip)
            {
                clips.Add(clip);
                return;
            }

            if (motion is BlendTree blendTree)
            {
                ChildMotion[] children = blendTree.children;
                for (int i = 0; i < children.Length; i++)
                {
                    CollectMotionClips(children[i].motion, visitedMotions, clips);
                }
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/');
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
